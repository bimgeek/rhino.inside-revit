using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using Microsoft.Win32.SafeHandles;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.PlugIns;
using Rhino.Runtime.InProcess;
using RhinoInside.Revit.Convert.Geometry;
using RhinoInside.Revit.Convert.Units;
using RhinoInside.Revit.External.ApplicationServices.Extensions;
using static Rhino.RhinoMath;
using DB = Autodesk.Revit.DB;

namespace RhinoInside
{
  #region Guest
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
  public class GuestPlugInIdAttribute : Attribute
  {
    public readonly Guid PlugInId;
    public GuestPlugInIdAttribute(string plugInId) => PlugInId = Guid.Parse(plugInId);
  }

  public class CheckInArgs : EventArgs
  {
    public string Message { get; set; } = string.Empty;
    public bool ShowMessage { get; set; } = true;
  }

  public class CheckOutArgs : EventArgs
  {
    public string Message { get; set; } = string.Empty;
    public bool ShowMessage { get; set; } = true;
  }

  public enum GuestResult
  {
    Failed = int.MinValue,
    Cancelled = int.MinValue + 1,
    Nothing = 0,
    Succeeded = 1
  }

  public interface IGuest
  {
    string Name { get; }

    GuestResult EntryPoint(object sender, EventArgs args);
  }
  #endregion
}

namespace RhinoInside.Revit
{
  using Result = Autodesk.Revit.UI.Result;
  using TaskDialog = Autodesk.Revit.UI.TaskDialog;

  public static class Rhinoceros
  {
    #region Revit Interface
    static RhinoCore core;
    internal static readonly string SchemeName = $"Inside-Revit-{AddIn.Host.Services.VersionNumber}";
    internal static string[] StartupLog;

    internal static bool InitEto()
    {
      if (Eto.Forms.Application.Instance is null)
        new Eto.Forms.Application(Eto.Platforms.Wpf).Attach();

      return true;
    }

    internal static bool InitRhinoCommon()
    {
      var hostMainWindow = new WindowHandle(AddIn.Host.MainWindowHandle);

      // Save Revit window status
      bool wasEnabled = hostMainWindow.Enabled;
      var activeWindow = WindowHandle.ActiveWindow ?? hostMainWindow;

      // Disable Revit window
      hostMainWindow.Enabled = false;

      // Load RhinoCore
      try
      {
        var args = new List<string>();

        if (Settings.AddinOptions.Session.IsolateSettings)
        {
          args.Add($"/scheme={SchemeName}");
        }

        if (Settings.AddinOptions.Session.UseHostLanguage)
        {
          args.Add($"/language={AddIn.Host.Services.Language.ToLCID()}");
        }

        args.Add("/nosplash");
        //args.Add("/notemplate");

        if (Settings.DebugLogging.Current.Enabled)
        {
          args.Add("/captureprintcalls");
          args.Add("/stopwatch");
        }

        core = new RhinoCore(args.ToArray(), WindowStyle.Hidden);
      }
      catch (Exception e)
      {
        AddIn.ReportException(e, AddIn.Host, new string[0]);
        AddIn.CurrentStatus = AddIn.Status.Failed;
        return false;
      }
      finally
      {
        // Enable Revit window back
        hostMainWindow.Enabled = wasEnabled;
        WindowHandle.ActiveWindow = activeWindow;

        StartupLog = RhinoApp.CapturedCommandWindowStrings(true);
        RhinoApp.CommandWindowCaptureEnabled = false;
      }

      MainWindow = new WindowHandle(RhinoApp.MainWindowHandle());
      return External.ActivationGate.AddGateWindow(MainWindow.Handle);
    }

    internal static bool InitGrasshopper()
    {
      var PluginId = new Guid(0xB45A29B1, 0x4343, 0x4035, 0x98, 0x9E, 0x04, 0x4E, 0x85, 0x80, 0xD9, 0xCF);
      return PlugIn.PlugInExists(PluginId, out bool loaded, out bool loadProtected) & (loaded | !loadProtected);
    }

    internal static Result Startup()
    {
      RhinoApp.MainLoop                         += MainLoop;

      RhinoDoc.NewDocument                      += OnNewDocument;
      RhinoDoc.EndOpenDocumentInitialViewUpdate += EndOpenDocumentInitialViewUpdate;

      Command.BeginCommand                      += BeginCommand;
      Command.EndCommand                        += EndCommand;

      // Alternative to /runscript= Rhino command line option
      RunScriptAsync
      (
        script:   Environment.GetEnvironmentVariable("RhinoInside_RunScript"),
        activate: AddIn.StartupMode == AddInStartupMode.AtStartup
      );

      // Add DefaultRenderAppearancePath to Rhino settings if missing
      {
        var DefaultRenderAppearancePath = System.IO.Path.Combine
        (
          Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86),
          "Autodesk Shared",
          "Materials",
          "Textures"
        );

        if (!Rhino.ApplicationSettings.FileSettings.GetSearchPaths().Any(x => x.Equals(DefaultRenderAppearancePath, StringComparison.OrdinalIgnoreCase)))
          Rhino.ApplicationSettings.FileSettings.AddSearchPath(DefaultRenderAppearancePath, -1);

        // TODO: Add also AdditionalRenderAppearancePaths content from Revit.ini if missing ??
      }

      // Reset document units
      if (string.IsNullOrEmpty(Rhino.ApplicationSettings.FileSettings.TemplateFile))
      {
        UpdateDocumentUnits(RhinoDoc.ActiveDoc);
        UpdateDocumentUnits(RhinoDoc.ActiveDoc, Revit.ActiveDBDocument);
      }

      // Load Guests
      CheckInGuests();

      return Result.Succeeded;
    }

    internal static Result Shutdown()
    {
      if (core is null)
        return Result.Failed;

      // Unload Guests
      CheckOutGuests();

      Command.EndCommand                        -= EndCommand;
      Command.BeginCommand                      -= BeginCommand;

      RhinoDoc.EndOpenDocumentInitialViewUpdate -= EndOpenDocumentInitialViewUpdate;
      RhinoDoc.NewDocument                      -= OnNewDocument;

      RhinoApp.MainLoop                         -= MainLoop;

      // Unload RhinoCore
      try
      {
        core.Dispose();
        core = null;
      }
      catch (Exception /*e*/)
      {
        //Debug.Fail(e.Source, e.Message);
        return Result.Failed;
      }

      return Result.Succeeded;
    }

    internal static WindowHandle MainWindow = WindowHandle.Zero;
    public static IntPtr MainWindowHandle => MainWindow.Handle;

    static bool idlePending = true;
    internal static void RaiseIdle() => core.RaiseIdle();

    internal static bool Run()
    {
      if (idlePending)
      {
        Revit.ActiveDBApplication?.PurgeReleasedAPIObjects();
        idlePending = core.DoIdle();
      }

      var active = core.DoEvents();
      if (active)
        idlePending = true;

      if (Revit.ProcessIdleActions())
        core.RaiseIdle();

      return active;
    }
    #endregion

    #region Guests
    class GuestInfo
    {
      public readonly Type ClassType;
      public IGuest Guest;
      public GuestResult CheckInResult;

      public GuestInfo(Type type) => ClassType = type;
    }
    static List<GuestInfo> guests;

    static void AppendExceptionExpandedContent(StringBuilder builder, Exception e)
    {
      switch (e)
      {
        case System.BadImageFormatException badImageFormatException:
          if (badImageFormatException.FileName != null) builder.AppendLine($"FileName: {badImageFormatException.FileName}");
          if (badImageFormatException.FusionLog != null) builder.AppendLine($"FusionLog: {badImageFormatException.FusionLog}");
          break;
        case System.IO.FileNotFoundException fileNotFoundException:
          if (fileNotFoundException.FileName != null) builder.AppendLine($"FileName: {fileNotFoundException.FileName}");
          if (fileNotFoundException.FusionLog != null) builder.AppendLine($"FusionLog: {fileNotFoundException.FusionLog}");
          break;
        case System.IO.FileLoadException fileLoadException:
          if (fileLoadException.FileName != null) builder.AppendLine($"FileName: {fileLoadException.FileName}");
          if (fileLoadException.FusionLog != null) builder.AppendLine($"FusionLog: {fileLoadException.FusionLog}");
          break;
      }
    }

    internal static void CheckInGuests()
    {
      if (guests is object)
        return;

      var types = Type.EmptyTypes;
      try { types = Assembly.GetExecutingAssembly().GetTypes(); }
      catch (ReflectionTypeLoadException ex)
      { types = ex.Types.Where(x => x?.TypeInitializer is object).ToArray(); }

      // Look for Guests
      guests = types.
        Where(x => typeof(IGuest).IsAssignableFrom(x)).
        Where(x => !x.IsInterface && !x.IsAbstract && !x.ContainsGenericParameters).
        Select(x => new GuestInfo(x)).
        ToList();

      foreach (var guestInfo in guests)
      {
        if (guestInfo.Guest is object)
          continue;

        bool load = true;
        foreach (var guestPlugIn in guestInfo.ClassType.GetCustomAttributes(typeof(GuestPlugInIdAttribute), false).Cast<GuestPlugInIdAttribute>())
          load |= PlugIn.GetPlugInInfo(guestPlugIn.PlugInId).IsLoaded;

        if (!load)
          continue;

        string mainContent = string.Empty;
        string expandedContent = string.Empty;
        var checkInArgs = new CheckInArgs();
        try
        {
          guestInfo.Guest = Activator.CreateInstance(guestInfo.ClassType) as IGuest;
          guestInfo.CheckInResult = guestInfo.Guest.EntryPoint(default, checkInArgs);
        }
        catch (Exception e)
        {
          guestInfo.CheckInResult = GuestResult.Failed;

          var mainContentBuilder = new StringBuilder();
          var expandedContentBuilder = new StringBuilder();
          while (e.InnerException != default)
          {
            mainContentBuilder.AppendLine($"· {e.Message}");
            AppendExceptionExpandedContent(expandedContentBuilder, e);
            e = e.InnerException;
          }
          mainContentBuilder.AppendLine($"· {e.Message}");
          AppendExceptionExpandedContent(expandedContentBuilder, e);

          mainContent = mainContentBuilder.ToString();
          expandedContent = expandedContentBuilder.ToString();
        }

        if (guestInfo.CheckInResult == GuestResult.Failed)
        {
          var guestName = guestInfo.Guest?.Name ?? guestInfo.ClassType.Namespace;

          {
            var journalContent = new StringBuilder();
            journalContent.AppendLine($"{guestName} failed to load");
            journalContent.AppendLine(mainContent);
            journalContent.AppendLine(expandedContent);
            Revit.ActiveDBApplication.WriteJournalComment(journalContent.ToString(), false);
          }

          using
          (
            var taskDialog = new TaskDialog(AddIn.DisplayVersion)
            {
              Id = $"{MethodBase.GetCurrentMethod().DeclaringType.FullName}.{MethodBase.GetCurrentMethod().Name}",
              MainIcon = External.UI.TaskDialogIcons.IconError,
              AllowCancellation = false,
              MainInstruction = $"{guestName} failed to load",
              MainContent = mainContent +
                            Environment.NewLine + "Do you want to report this problem by email to tech@mcneel.com?",
              ExpandedContent = expandedContent,
              FooterText = "Press CTRL+C to copy this information to Clipboard",
              CommonButtons = Autodesk.Revit.UI.TaskDialogCommonButtons.Yes | Autodesk.Revit.UI.TaskDialogCommonButtons.Cancel,
              DefaultButton = Autodesk.Revit.UI.TaskDialogResult.Yes
            }
          )
          {
            if (taskDialog.Show() == Autodesk.Revit.UI.TaskDialogResult.Yes)
            {
              ErrorReport.SendEmail
              (
                Revit.ActiveUIApplication,
                taskDialog.MainInstruction,
                false,
                new string[]
                {
                  Revit.ActiveUIApplication.Application.RecordingJournalFilename
                }
              );
            }
          }
        }
      }
    }

    static void CheckOutGuests()
    {
      if (guests is null)
        return;

      foreach (var guestInfo in Enumerable.Reverse(guests))
      {
        if (guestInfo.Guest is null)
          continue;

        if (guestInfo.CheckInResult == GuestResult.Succeeded)
          continue;

        try { guestInfo.Guest.EntryPoint(default, new CheckOutArgs()); guestInfo.CheckInResult = GuestResult.Cancelled; }
        catch (Exception) { }
      }

      guests = null;
    }
    #endregion

    #region Document
    static void OnNewDocument(object sender, DocumentEventArgs e)
    {
      // If a new document is created without template it is updated from Revit.ActiveDBDocument
      Debug.Assert(string.IsNullOrEmpty(e.Document.TemplateFileUsed));

      if (e.Document is RhinoDoc)
      {
        UpdateDocumentUnits(e.Document);
        UpdateDocumentUnits(e.Document, Revit.ActiveDBDocument);
      }
    }

    static void EndOpenDocumentInitialViewUpdate(object sender, DocumentEventArgs e)
    {
      if (e.Document.IsOpening)
        AuditUnits(e.Document);
    }

    static void AuditTolerances(RhinoDoc doc)
    {
      if (doc is object)
      {
        var maxDistanceTolerance = UnitConverter.ConvertFromHostUnits(Revit.VertexTolerance, doc.ModelUnitSystem);
        if (doc.ModelAbsoluteTolerance > maxDistanceTolerance)
          doc.ModelAbsoluteTolerance = maxDistanceTolerance;

        var maxAngleTolerance = Revit.AngleTolerance;
        if (doc.ModelAngleToleranceRadians > maxAngleTolerance)
          doc.ModelAngleToleranceRadians = maxAngleTolerance;
      }
    }

    internal static void AuditUnits(RhinoDoc doc)
    {
      if (Command.InScriptRunnerCommand())
        return;

      if (Revit.ActiveUIDocument?.Document is DB.Document revitDoc)
      {
        var units = revitDoc.GetUnits();
        var RevitModelUnitSystem = units.ToUnitSystem(out var distanceDisplayPrecision);
        var GrasshopperModelUnitSystem = GH.Guest.ModelUnitSystem != UnitSystem.Unset ? GH.Guest.ModelUnitSystem : doc.ModelUnitSystem;
        if (doc.ModelUnitSystem != RevitModelUnitSystem || doc.ModelUnitSystem != GrasshopperModelUnitSystem)
        {
          var hasUnits = doc.ModelUnitSystem != UnitSystem.Unset && doc.ModelUnitSystem != UnitSystem.None;
          var expandedContent = doc.IsOpening ?
            $"The Rhino model you are opening is in {doc.ModelUnitSystem}{Environment.NewLine}Revit document '{revitDoc.Title}' length units are {RevitModelUnitSystem}" :
            string.Empty;

          using
          (
            var taskDialog = new TaskDialog("Units")
            {
              MainIcon = External.UI.TaskDialogIcons.IconInformation,
              TitleAutoPrefix = true,
              AllowCancellation = hasUnits,
              MainInstruction = hasUnits ? (doc.IsOpening ? "Model units mismatch." : "Model units mismatch warning.") : "Rhino model has no units.",
              MainContent = doc.IsOpening ? "What units do you want to use?" : $"Revit document '{revitDoc.Title}' length units are {RevitModelUnitSystem}." + (hasUnits ? $"{Environment.NewLine}Rhino is working in {doc.ModelUnitSystem}." : string.Empty),
              ExpandedContent = expandedContent,
              FooterText = "Current version: " + AddIn.DisplayVersion
            }
          )
          {
            if (!doc.IsOpening && hasUnits)
            {
#if REVIT_2020
              taskDialog.EnableDoNotShowAgain("RhinoInside.Revit.DocumentUnitsMismatch", true, "Do not show again");
#else
              // Without the ability of checking DoNotShowAgain this may be too anoying.
              return;
#endif
            }
            else
            {
              taskDialog.AddCommandLink(Autodesk.Revit.UI.TaskDialogCommandLinkId.CommandLink2, $"Use {RevitModelUnitSystem} like Revit", $"Scale Rhino model by {UnitConverter.Convert(1.0, doc.ModelUnitSystem, RevitModelUnitSystem)}");
              taskDialog.DefaultButton = Autodesk.Revit.UI.TaskDialogResult.CommandLink2;
            }

            if (hasUnits)
            {
              if (doc.IsOpening)
              {
                taskDialog.AddCommandLink(Autodesk.Revit.UI.TaskDialogCommandLinkId.CommandLink1, $"Continue in {doc.ModelUnitSystem}", $"Rhino and Grasshopper will work in {doc.ModelUnitSystem}");
                taskDialog.DefaultButton = Autodesk.Revit.UI.TaskDialogResult.CommandLink1;
              }
              else
              {
                taskDialog.CommonButtons = Autodesk.Revit.UI.TaskDialogCommonButtons.Ok;
                taskDialog.DefaultButton = Autodesk.Revit.UI.TaskDialogResult.Ok;
              }
            }

            if (GH.Guest.ModelUnitSystem != UnitSystem.Unset)
            {
              taskDialog.ExpandedContent += $"{Environment.NewLine}Documents opened in Grasshopper were working in {GH.Guest.ModelUnitSystem}";
              if (GrasshopperModelUnitSystem != doc.ModelUnitSystem && GrasshopperModelUnitSystem != RevitModelUnitSystem)
              {
                taskDialog.AddCommandLink(Autodesk.Revit.UI.TaskDialogCommandLinkId.CommandLink3, $"Adjust Rhino model to {GH.Guest.ModelUnitSystem} like Grasshopper", $"Scale Rhino model by {UnitConverter.Convert(1.0, doc.ModelUnitSystem, GH.Guest.ModelUnitSystem)}");
                taskDialog.DefaultButton = Autodesk.Revit.UI.TaskDialogResult.CommandLink3;
              }
            }

            switch (taskDialog.Show())
            {
            case Autodesk.Revit.UI.TaskDialogResult.CommandLink2:
                doc.ModelAngleToleranceRadians = Revit.AngleTolerance;
                doc.ModelDistanceDisplayPrecision = distanceDisplayPrecision;
                doc.ModelAbsoluteTolerance = UnitConverter.ConvertFromHostUnits(Revit.VertexTolerance, RevitModelUnitSystem);
                doc.AdjustModelUnitSystem(RevitModelUnitSystem, true);
                AdjustViewConstructionPlanes(doc);
              break;
              case Autodesk.Revit.UI.TaskDialogResult.CommandLink3:
                doc.ModelAngleToleranceRadians = Revit.AngleTolerance;
                doc.ModelDistanceDisplayPrecision = Clamp(Grasshopper.CentralSettings.FormatDecimalDigits, 0, 7);
                doc.ModelAbsoluteTolerance = UnitConverter.ConvertFromHostUnits(Revit.VertexTolerance, GH.Guest.ModelUnitSystem);
                doc.AdjustModelUnitSystem(GH.Guest.ModelUnitSystem, true);
                AdjustViewConstructionPlanes(doc);
                break;
              default:
                AuditTolerances(doc);
                break;
            }
          }
        }
      }
    }

    static void UpdateDocumentUnits(RhinoDoc rhinoDoc, DB.Document revitDoc = null)
    {
      bool docModified = rhinoDoc.Modified;
      try
      {
        if (revitDoc is null)
        {
          rhinoDoc.ModelUnitSystem = UnitSystem.None;
          rhinoDoc.ModelAbsoluteTolerance = Revit.VertexTolerance;
          rhinoDoc.ModelAngleToleranceRadians = Revit.AngleTolerance;
        }
        else if (rhinoDoc.ModelUnitSystem == UnitSystem.None)
        {
          var units = revitDoc.GetUnits();
          rhinoDoc.ModelUnitSystem = units.ToUnitSystem(out var distanceDisplayPrecision);
          rhinoDoc.ModelAngleToleranceRadians = Revit.AngleTolerance;
          rhinoDoc.ModelDistanceDisplayPrecision = distanceDisplayPrecision;
          rhinoDoc.ModelAbsoluteTolerance = UnitConverter.ConvertFromHostUnits(Revit.VertexTolerance, rhinoDoc.ModelUnitSystem);
          //switch (rhinoDoc.ModelUnitSystem)
          //{
          //  case UnitSystem.None: break;
          //  case UnitSystem.Feet:
          //  case UnitSystem.Inches:
          //    newDoc.ModelAbsoluteTolerance = UnitConverter.Convert(1.0 / 160.0, UnitSystem.Inches, newDoc.ModelUnitSystem);
          //    break;
          //  default:
          //    newDoc.ModelAbsoluteTolerance = UnitConverter.Convert(0.1, UnitSystem.Millimeters, newDoc.ModelUnitSystem);
          //    break;
          //}

          AdjustViewConstructionPlanes(rhinoDoc);
        }
      }
      finally
      {
        rhinoDoc.Modified = docModified;
      }
    }

    static void AdjustViewConstructionPlanes(RhinoDoc rhinoDoc)
    {
      if (!string.IsNullOrEmpty(rhinoDoc.TemplateFileUsed))
        return;

      if (rhinoDoc.IsCreating)
      {
        Revit.EnqueueIdlingAction(() => AdjustViewConstructionPlanes(rhinoDoc));
        return;
      }

      foreach (var view in rhinoDoc.Views)
        AdjustViewCPlane(view.MainViewport);
    }

    static void AdjustViewCPlane(RhinoViewport viewport)
    {
      if (viewport.ParentView?.Document is RhinoDoc rhinoDoc)
      {
        bool imperial = rhinoDoc.ModelUnitSystem == UnitSystem.Feet || rhinoDoc.ModelUnitSystem == UnitSystem.Inches;

        var modelGridSpacing = imperial ?
        UnitConverter.Convert(1.0, UnitSystem.Yards, rhinoDoc.ModelUnitSystem) :
        UnitConverter.Convert(1.0, UnitSystem.Meters, rhinoDoc.ModelUnitSystem);

        var modelSnapSpacing = imperial ?
        UnitConverter.Convert(3.0, UnitSystem.Feet, rhinoDoc.ModelUnitSystem) :
        UnitConverter.Convert(1.0, UnitSystem.Meters, rhinoDoc.ModelUnitSystem);

        var modelThickLineFrequency = imperial ? 6 : 5;

        var cplane = viewport.GetConstructionPlane();

        cplane.GridSpacing = modelGridSpacing;
        cplane.SnapSpacing = modelSnapSpacing;
        cplane.ThickLineFrequency = modelThickLineFrequency;

        viewport.SetConstructionPlane(cplane);

        var min = cplane.Plane.PointAt(-cplane.GridSpacing * cplane.GridLineCount, -cplane.GridSpacing * cplane.GridLineCount, 0.0);
        var max = cplane.Plane.PointAt(+cplane.GridSpacing * cplane.GridLineCount, +cplane.GridSpacing * cplane.GridLineCount, 0.0);
        var bbox = new BoundingBox(min, max);

        // Zoom to grid
        viewport.ZoomBoundingBox(bbox);

        // Adjust to extens in case There is anything in the viewports like Grasshopper previews.
        viewport.ZoomExtents();
      }
    }
    #endregion

    #region Rhino UI
    /*internal*/
    public static void InvokeInHostContext(Action action) => core.InvokeInHostContext(action);
    /*internal*/ public static T InvokeInHostContext<T>(Func<T> func) => core.InvokeInHostContext(func);

    public static bool Exposed
    {
      get => MainWindow.Visible && MainWindow.WindowStyle != ProcessWindowStyle.Minimized;
      set
      {
        MainWindow.Visible = value;

        if (value && MainWindow.WindowStyle == ProcessWindowStyle.Minimized)
          MainWindow.WindowStyle = ProcessWindowStyle.Normal;
      }
    }

    class ExposureSnapshot
    {
      readonly bool Visible             = MainWindow.Visible;
      readonly ProcessWindowStyle Style = MainWindow.WindowStyle;

      public void Restore()
      {
        MainWindow.WindowStyle          = Style;
        MainWindow.Visible              = Visible;
      }
    }
    static ExposureSnapshot QuiescentExposure;

    static void BeginCommand(object sender, CommandEventArgs e)
    {
      if (!Command.InScriptRunnerCommand())
      {
        // Capture Rhino Main Window exposure to restore it when user ends picking
        QuiescentExposure = new ExposureSnapshot();

        // Disable Revit Main Window while in Command
        Revit.MainWindow.Enabled = false;
      }
    }

    static void EndCommand(object sender, CommandEventArgs e)
    {
      if (!Command.InScriptRunnerCommand())
      {
        // Reenable Revit main window
        Revit.MainWindow.Enabled = true;

        if (MainWindow.WindowStyle != ProcessWindowStyle.Maximized)
        {
          // Restore Rhino Main Window exposure
          QuiescentExposure?.Restore();
          QuiescentExposure = null;
          RhinoApp.SetFocusToMainWindow();
        }
      }
    }

    static void MainLoop(object sender, EventArgs e)
    {
      if (!Command.InScriptRunnerCommand()) 
      {
        if (RhinoDoc.ActiveDoc is RhinoDoc rhinoDoc)
        {
          // Keep Rhino window exposed to user while in a get operation
          if (RhinoGet.InGet(rhinoDoc) && !Exposed)
          {
            if (RhinoGet.InGetObject(rhinoDoc) || RhinoGet.InGetPoint(rhinoDoc))
            {
              // If there is no floating viewport visible...
              if (!rhinoDoc.Views.Where(x => x.Floating).Any())
              {
                var cursorPosition = System.Windows.Forms.Cursor.Position;
                if (!OpenRevitViewport(cursorPosition.X - 400, cursorPosition.Y - 300))
                  Exposed = true;
              }
            }
            // If we are in a GetString or GetInt we need the command prompt.
            else Exposed = true;
          }
        }
      }
    }

    public static void Show()
    {
      Exposed = true;
      MainWindow.BringToFront();
    }

    public static async void ShowAsync()
    {
      await External.ActivationGate.Yield();

      Show();
    }

    public static async void RunScriptAsync(string script, bool activate)
    {
      if (string.IsNullOrEmpty(script))
        return;

      await External.ActivationGate.Yield();

      if (activate)
        RhinoApp.SetFocusToMainWindow();

      RhinoApp.RunScript(script, false);
    }

    public static Result RunCommandAbout()
    {
      var docSerial = RhinoDoc.ActiveDoc.RuntimeSerialNumber;
      var result = RhinoApp.RunScript("!_About", false) ? Result.Succeeded : Result.Failed;

      if (result == Result.Succeeded && docSerial != RhinoDoc.ActiveDoc.RuntimeSerialNumber)
      {
        Exposed = true;
        return Result.Succeeded;
      }

      return Result.Cancelled;
    }

    public static Result RunCommandOptions()
    {
      return RhinoApp.RunScript("!_Options", false) ? Result.Succeeded : Result.Failed;
    }

    public static Result RunCommandPackageManager()
    {
      return RhinoApp.RunScript("!_PackageManager", false) ? Result.Succeeded : Result.Failed;
    }

    #region Open Viewport
    const string RevitViewName = "Revit";
    internal static bool OpenRevitViewport(int x, int y)
    {
      if (RhinoDoc.ActiveDoc is RhinoDoc rhinoDoc)
      {
        var view3D = rhinoDoc.Views.Where(v => v.MainViewport.Name == RevitViewName).FirstOrDefault();
        if (view3D is null)
        {
          if
          (
            rhinoDoc.Views.Add
            (
              RevitViewName, DefinedViewportProjection.Perspective,
              new System.Drawing.Rectangle(x, y, 800, 600),
              true
            ) is RhinoView rhinoView
          )
          {
            rhinoDoc.Views.ActiveView = rhinoView;

            AdjustViewCPlane(rhinoView.MainViewport);
            return true;
          }
          else return false;
        }
        else
        {
          rhinoDoc.Views.ActiveView = view3D;
          return view3D.BringToFront();
        }
      }

      return false;
    }

    public static async void RunCommandOpenViewportAsync()
    {
      var cursorPosition = System.Windows.Forms.Cursor.Position;

      await External.ActivationGate.Yield();
      OpenRevitViewport(cursorPosition.X + 50, cursorPosition.Y + 50);
    }
    #endregion

    /// <summary>
    /// Represents a Pseudo-modal loop.
    /// </summary>
    /// <remarks>
    /// This class implements <see cref="IDisposable"/> interface, it's been designed to be used in a using statement.
    /// </remarks>
    internal sealed class ModalScope : IDisposable
    {
      static bool wasExposed = false;
      readonly bool wasEnabled = Revit.MainWindow.Enabled;

      public ModalScope() => Revit.MainWindow.Enabled = false;

      void IDisposable.Dispose() => Revit.MainWindow.Enabled = wasEnabled;

      public Result Run(bool exposeMainWindow)
      {
        return Run(exposeMainWindow, !Keyboard.IsKeyDown(Key.LeftCtrl));
      }

      public Result Run(bool exposeMainWindow, bool restorePopups)
      {
        try
        {
          if (exposeMainWindow) Exposed = true;
          else if (restorePopups) Exposed = wasExposed || MainWindow.WindowStyle == ProcessWindowStyle.Minimized;

          if (restorePopups)
            MainWindow.ShowOwnedPopups();

          // Activate a Rhino window to keep the loop running
          var activePopup = MainWindow.ActivePopup;
          if (activePopup.IsInvalid || exposeMainWindow)
          {
            if (!Exposed)
              return Result.Cancelled;

            RhinoApp.SetFocusToMainWindow();
          }
          else
          {
            activePopup.BringToFront();
          }

          while (Rhinoceros.Run())
          {
            if (!Exposed && MainWindow.ActivePopup.IsInvalid)
              break;
          }

          return Result.Succeeded;
        }
        finally
        {
          wasExposed = Exposed;

          Revit.MainWindow.Enabled = true;
          WindowHandle.ActiveWindow = Revit.MainWindow;
          MainWindow.HideOwnedPopups();
          Exposed = false;
        }
      }
    }
    #endregion
  }
}
