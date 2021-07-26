using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using RhinoInside.Revit.External.DB.Extensions;
using RhinoInside.Revit.External.UI.Extensions;

namespace RhinoInside.Revit.External.UI
{
  #region UIHostApplication
  public abstract class UIHostApplication : IDisposable
  {
    protected UIHostApplication() { }
    public abstract void Dispose();

    public static implicit operator UIHostApplication(UIApplication value) => new UIHostApplicationU(value);
    public static implicit operator UIHostApplication(UIControlledApplication value) => new UIHostApplicationC(value);

    public abstract object Value { get; }
    public abstract bool IsValid { get; }

    public abstract ApplicationServices.HostServices Services { get; }
    public abstract UIDocument ActiveUIDocument { get; set; }

    #region UI
    public abstract IntPtr MainWindowHandle { get; }
    #endregion

    #region Ribbon
    public abstract void CreateRibbonTab(string tabName);
    internal bool ActivateRibbonTab(string tabName)
    {
      foreach (var tab in Autodesk.Windows.ComponentManager.Ribbon.Tabs)
      {
        if (tab.Name == tabName)
        {
          tab.IsActive = true;
          return true;
        }
      }

      return false;
    }

    public abstract RibbonPanel CreateRibbonPanel(Tab tab, string panelName);
    public abstract RibbonPanel CreateRibbonPanel(string tabName, string panelName);
    public abstract IReadOnlyList<RibbonPanel> GetRibbonPanels(Tab tab);
    public abstract IReadOnlyList<RibbonPanel> GetRibbonPanels(string tabName);
    #endregion

    #region Addins
    public abstract AddInId ActiveAddInId { get; }
    public abstract void LoadAddIn(string fileName);
    public abstract ExternalApplicationArray LoadedApplications { get; }
    #endregion

    #region Commands
    public abstract bool CanPostCommand(RevitCommandId commandId);
    public abstract void PostCommand(RevitCommandId commandId);
    #endregion

    #region Events
    public abstract event EventHandler<IdlingEventArgs> Idling;
    public abstract event EventHandler<ViewActivatingEventArgs> ViewActivating;
    public abstract event EventHandler<ViewActivatedEventArgs> ViewActivated;
    #endregion
  }

  class UIHostApplicationC : UIHostApplication
  {
    readonly UIControlledApplication _app;
    public UIHostApplicationC(UIControlledApplication app) => _app = app;
    public override void Dispose() { }
    public override object Value => _app;
    public override bool IsValid => true;

    public override ApplicationServices.HostServices Services => new ApplicationServices.HostServicesC(_app.ControlledApplication);
    public override UIDocument ActiveUIDocument { get => default; set => throw new InvalidOperationException(); }

    #region UI
    public override IntPtr MainWindowHandle
    {
#if REVIT_2019
      get => _app.MainWindowHandle;
#else
      get => System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
#endif
    }
    #endregion

    #region Ribbon
    public override void CreateRibbonTab(string tabName) =>
      _app.CreateRibbonTab(tabName);

    public override RibbonPanel CreateRibbonPanel(Tab tab, string panelName) =>
      _app.CreateRibbonPanel(tab, panelName);

    public override RibbonPanel CreateRibbonPanel(string tabName, string panelName) =>
      _app.CreateRibbonPanel(tabName, panelName);

    public override IReadOnlyList<RibbonPanel> GetRibbonPanels(Tab tab) =>
      _app.GetRibbonPanels(tab);

    public override IReadOnlyList<RibbonPanel> GetRibbonPanels(string tabName) =>
      _app.GetRibbonPanels(tabName);
    #endregion

    #region Addins
    public override AddInId ActiveAddInId => _app.ActiveAddInId;
    public override void LoadAddIn(string fileName) => _app.LoadAddIn(fileName);
    public override ExternalApplicationArray LoadedApplications => _app.LoadedApplications;
    #endregion

    #region Commands
    public override bool CanPostCommand(RevitCommandId commandId) => false;
    public override void PostCommand(RevitCommandId commandId) => throw new InvalidOperationException();
    #endregion

    #region Events
    public override event EventHandler<IdlingEventArgs> Idling { add => _app.Idling += value; remove => _app.Idling -= value; }
    public override event EventHandler<ViewActivatingEventArgs> ViewActivating { add => _app.ViewActivating += value; remove => _app.ViewActivating -= value; }
    public override event EventHandler<ViewActivatedEventArgs> ViewActivated { add => _app.ViewActivated += value; remove => _app.ViewActivated -= value; }
    #endregion
  }

  class UIHostApplicationU : UIHostApplication
  {
    readonly UIApplication _app;
    public UIHostApplicationU(UIApplication app) => _app = app;
    public override void Dispose() => _app.Dispose();
    public override object Value => _app.IsValidObject ? _app : default;
    public override bool IsValid => _app.IsValidObject;

    public override ApplicationServices.HostServices Services => new ApplicationServices.HostServicesU(_app.Application);
    public override UIDocument ActiveUIDocument
    {
      get => _app.ActiveUIDocument;
      set
      {
        if (value is null) throw new ArgumentNullException();
        if (value.Document.IsEquivalent(_app.ActiveUIDocument.Document)) return;

        Rhinoceros.InvokeInHostContext
        (
          () =>
          {
            var activeWindow = Microsoft.Win32.SafeHandles.WindowHandle.ActiveWindow;

            try
            {
              if (value.TryGetActiveGraphicalView(out var uiView))
              {
                using (var collector = new FilteredElementCollector(value.Document, uiView.ViewId))
                  value.ShowElements(collector.FirstElementId());
              }
            }
            finally { Microsoft.Win32.SafeHandles.WindowHandle.ActiveWindow = activeWindow; }
          }
        );
      }
    }

    #region UI
    public override IntPtr MainWindowHandle
    {
#if REVIT_2019
      get => _app.MainWindowHandle;
#else
      get => System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
#endif
    }
    #endregion

    #region Ribbon
    public override void CreateRibbonTab(string tabName) =>
      _app.CreateRibbonTab(tabName);

    public override RibbonPanel CreateRibbonPanel(Tab tab, string panelName) =>
      _app.CreateRibbonPanel(tab, panelName);

    public override RibbonPanel CreateRibbonPanel(string tabName, string panelName) =>
      _app.CreateRibbonPanel(tabName, panelName);

    public override IReadOnlyList<RibbonPanel> GetRibbonPanels(Tab tab) =>
      _app.GetRibbonPanels(tab);

    public override IReadOnlyList<RibbonPanel> GetRibbonPanels(string tabName) =>
      _app.GetRibbonPanels(tabName);
    #endregion

    #region Addins
    public override AddInId ActiveAddInId => _app.ActiveAddInId;
    public override void LoadAddIn(string fileName) => _app.LoadAddIn(fileName);
    public override ExternalApplicationArray LoadedApplications => _app.LoadedApplications;
    #endregion

    #region Commands
    public override bool CanPostCommand(RevitCommandId commandId) => _app.CanPostCommand(commandId);
    public override void PostCommand(RevitCommandId commandId) => _app.PostCommand(commandId);
    #endregion

    #region Events
    public override event EventHandler<IdlingEventArgs> Idling { add => _app.Idling += value; remove => _app.Idling -= value; }
    public override event EventHandler<ViewActivatingEventArgs> ViewActivating { add => _app.ViewActivating += value; remove => _app.ViewActivating -= value; }
    public override event EventHandler<ViewActivatedEventArgs> ViewActivated { add => _app.ViewActivated += value; remove => _app.ViewActivated -= value; }
    #endregion
  }
  #endregion
}

namespace System.Windows.Interop
{
  static class UIHostApplicationInterop
  {
    public static Window GetMainWindow(this RhinoInside.Revit.External.UI.UIHostApplication app)
    {
      if (app.MainWindowHandle != IntPtr.Zero)
      {
        if (Interop.HwndSource.FromHwnd(app.MainWindowHandle).RootVisual is Window window)
          return window;

        try { return new Interop.HwndTarget(app.MainWindowHandle).RootVisual as Window; }
        catch { }
      }

      return default;
    }
  }
}

namespace System.Windows.Forms.Interop
{
  static class UIHostApplicationInterop
  {
    public static IWin32Window GetMainWindow(this RhinoInside.Revit.External.UI.UIHostApplication app)
    {
      return app.MainWindowHandle != IntPtr.Zero ?
        NativeWindow.FromHandle(app.MainWindowHandle) :
        default;
    }
  }
}

namespace Eto.Forms.Interop
{
  static class UIHostApplicationInterop
  {
    public static Window GetMainWindow(this RhinoInside.Revit.External.UI.UIHostApplication app)
    {
      return app.MainWindowHandle != IntPtr.Zero ?
        new Form(new Wpf.Forms.HwndFormHandler(app.MainWindowHandle)) :
        default;
    }
  }
}
