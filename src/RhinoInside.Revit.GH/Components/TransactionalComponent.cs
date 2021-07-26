using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.UI.Events;
using GH_IO.Serialization;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using RhinoInside.Revit.GH.ElementTracking;
using DB = Autodesk.Revit.DB;
using DBX = RhinoInside.Revit.External.DB;

namespace RhinoInside.Revit.GH.Components
{
  class TransactionalComponentFailuresPreprocessor : DB.IFailuresPreprocessor
  {
    readonly IGH_ActiveObject ActiveObject;
    readonly IEnumerable<DB.FailureDefinitionId> FailureDefinitionIdsToFix;
    readonly bool FixUnhandledFailures;

    public TransactionalComponentFailuresPreprocessor
    (
      IGH_ActiveObject activeObject
    )
    {
      ActiveObject = activeObject;
      FailureDefinitionIdsToFix = default;
      FixUnhandledFailures = true;
    }

    public TransactionalComponentFailuresPreprocessor
    (
      IGH_ActiveObject activeObject,
      IEnumerable<DB.FailureDefinitionId> failureDefinitionIdsToFix,
      bool fixUnhandledFailures
    )
    {
      ActiveObject = activeObject;
      FailureDefinitionIdsToFix = failureDefinitionIdsToFix;
      FixUnhandledFailures = fixUnhandledFailures;
    }

    void AddRuntimeMessage(DB.FailureMessageAccessor error, bool? solved = null)
    {
      if (error.GetFailureDefinitionId() == DBX.ExternalFailures.TransactionFailures.SimulatedTransaction)
      {
        // Simulation signal is already reflected in the canvas changing the component color,
        // So it's up to the component show relevant information about what 'simulation' means.
        // As an example Purge component shows a remarks that reads like 'No elements were deleted'.
        //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, error.GetDescriptionText());

        return;
      }

      var level = GH_RuntimeMessageLevel.Remark;
      switch (error.GetSeverity())
      {
        case DB.FailureSeverity.Warning: level = GH_RuntimeMessageLevel.Warning; break;
        case DB.FailureSeverity.Error: level = GH_RuntimeMessageLevel.Error; break;
      }

      string solvedMark = string.Empty;
      if (error.GetSeverity() > DB.FailureSeverity.Warning)
      {
        switch (solved)
        {
          case false: solvedMark = "❌ "; break;
          case true: solvedMark = "✔ "; break;
        }
      }

      var description = error.GetDescriptionText();
      var text = string.IsNullOrEmpty(description) ?
        $"{solvedMark}{level} {{{error.GetFailureDefinitionId().Guid}}}" :
        $"{solvedMark}{description}";

      int idsCount = 0;
      foreach (var id in error.GetFailingElementIds())
        text += idsCount++ == 0 ? $" {{{id.IntegerValue}" : $", {id.IntegerValue}";
      if (idsCount > 0) text += "} ";

      ActiveObject.AddRuntimeMessage(level, text);
    }

    DB.FailureProcessingResult FixFailures(DB.FailuresAccessor failuresAccessor, IEnumerable<DB.FailureDefinitionId> failureIds)
    {
      foreach (var failureId in failureIds)
      {
        foreach (var error in failuresAccessor.GetFailureMessages().Where(x => x.GetFailureDefinitionId() == failureId))
        {
          if (!failuresAccessor.IsFailureResolutionPermitted(error))
            continue;

          // Don't try to fix two times same issue
          if (failuresAccessor.GetAttemptedResolutionTypes(error).Any())
            continue;

          AddRuntimeMessage(error, true);

          failuresAccessor.ResolveFailure(error);
        }

        if (failuresAccessor.CanCommitPendingTransaction())
          return DB.FailureProcessingResult.ProceedWithCommit;
      }

      return DB.FailureProcessingResult.Continue;
    }

    public virtual DB.FailureProcessingResult PreprocessFailures(DB.FailuresAccessor failuresAccessor)
    {
      if (!failuresAccessor.IsTransactionBeingCommitted())
        return DB.FailureProcessingResult.Continue;

      if (failuresAccessor.GetSeverity() >= DB.FailureSeverity.DocumentCorruption)
        return DB.FailureProcessingResult.ProceedWithRollBack;

      if (failuresAccessor.GetSeverity() >= DB.FailureSeverity.Error)
      {
        // Handled failures in order
        if (FailureDefinitionIdsToFix is IEnumerable<DB.FailureDefinitionId> failureDefinitionIdsToFix)
        {
          var result = FixFailures(failuresAccessor, failureDefinitionIdsToFix);
          if (result != DB.FailureProcessingResult.Continue)
            return result;
        }

        // Unhandled failures in incomming order
        if (FixUnhandledFailures)
        {
          var unhandledFailureDefinitionIds = failuresAccessor.GetFailureMessages().GroupBy(x => x.GetFailureDefinitionId()).Select(x => x.Key);
          var result = FixFailures(failuresAccessor, unhandledFailureDefinitionIds);
          if (result != DB.FailureProcessingResult.Continue)
            return result;
        }
      }

      var severity = failuresAccessor.GetSeverity();
      if (severity >= DB.FailureSeverity.Warning)
      {
        // Unsolved failures or warnings
        foreach (var error in failuresAccessor.GetFailureMessages().OrderBy(error => error.GetSeverity()))
          AddRuntimeMessage(error, false);

        failuresAccessor.DeleteAllWarnings();
      }

      if (severity >= DB.FailureSeverity.Error)
        return DB.FailureProcessingResult.ProceedWithRollBack;

      return DB.FailureProcessingResult.Continue;
    }
  }

  public abstract class TransactionalComponent :
    ZuiComponent,
    DB.ITransactionFinalizer
  {
    protected TransactionalComponent(string name, string nickname, string description, string category, string subCategory)
    : base(name, nickname, description, category, subCategory) { }

    #region Transaction
    DB.TransactionStatus status = DB.TransactionStatus.Uninitialized;
    public DB.TransactionStatus Status
    {
      get => status;
      protected set => status = value;
    }

    protected DB.Transaction NewTransaction(DB.Document doc) => NewTransaction(doc, Name);
    protected DB.Transaction NewTransaction(DB.Document doc, string name)
    {
      var transaction = new DB.Transaction(doc, name);

      var options = transaction.GetFailureHandlingOptions();
      options = options.SetClearAfterRollback(true);
      options = options.SetDelayedMiniWarnings(false);
      options = options.SetForcedModalHandling(true);

      if(CreateFailuresPreprocessor() is DB.IFailuresPreprocessor preprocessor)
        options = options.SetFailuresPreprocessor(preprocessor);

      options = options.SetTransactionFinalizer(this);

      transaction.SetFailureHandlingOptions(options);

      return transaction;
    }

    protected DB.TransactionStatus CommitTransaction(DB.Document doc, DB.Transaction transaction)
    {
      // Disable Rhino UI if any warning-error dialog popup
      var uiApplication = Revit.ActiveUIApplication;
      External.UI.EditScope scope = null;
      EventHandler<DialogBoxShowingEventArgs> _ = null;
      try
      {
        uiApplication.DialogBoxShowing += _ = (sender, args) =>
        {
          if (scope is null)
            scope = new External.UI.EditScope(uiApplication);
        };

        if (transaction.GetStatus() == DB.TransactionStatus.Started)
        {
          return transaction.Commit();
        }
        else return transaction.RollBack();
      }
      finally
      {
        uiApplication.DialogBoxShowing -= _;

        if (scope is IDisposable disposable)
          disposable.Dispose();
      }
    }
    #endregion

    #region Attributes
    internal new class Attributes : ZuiAttributes
    {
      public Attributes(TransactionalComponent owner) : base(owner) { }

      protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
      {
        if (channel == GH_CanvasChannel.Objects && !Owner.Locked && Owner is TransactionalComponent component)
        {
          if (component.Status != DB.TransactionStatus.RolledBack && component.Status != DB.TransactionStatus.Uninitialized)
          {
            var palette = GH_CapsuleRenderEngine.GetImpliedPalette(Owner);
            if (palette == GH_Palette.Normal && !Owner.IsPreviewCapable)
              palette = GH_Palette.Hidden;

            // Errors and warnings should be refelected in Canvas
            if (palette == GH_Palette.Normal || palette == GH_Palette.Hidden)
            {
              var style = GH_CapsuleRenderEngine.GetImpliedStyle(palette, Selected, Owner.Locked, Owner.Hidden);
              var fill = style.Fill;
              var edge = style.Edge;
              var text = style.Text;

              switch (component.Status)
              {
                case DB.TransactionStatus.Uninitialized: palette = GH_Palette.Grey; break;
                case DB.TransactionStatus.Started: palette = GH_Palette.White; break;
                case DB.TransactionStatus.RolledBack:     /*palette = GH_Palette.Normal;*/ break;
                case DB.TransactionStatus.Committed: palette = GH_Palette.Black; break;
                case DB.TransactionStatus.Pending: palette = GH_Palette.Blue; break;
                case DB.TransactionStatus.Error: palette = GH_Palette.Pink; break;
                case DB.TransactionStatus.Proceed: palette = GH_Palette.Brown; break;
              }
              var replacement = GH_CapsuleRenderEngine.GetImpliedStyle(palette, Selected, Owner.Locked, Owner.Hidden);

              try
              {
                style.Edge = replacement.Edge;
                style.Fill = replacement.Fill;
                style.Text = replacement.Text;

                base.Render(canvas, graphics, channel);
              }
              finally
              {
                style.Fill = fill;
                style.Edge = edge;
                style.Text = text;
              }

              return;
            }
          }
        }

        base.Render(canvas, graphics, channel);
      }
    }

    public override void CreateAttributes() => m_attributes = new Attributes(this);
    #endregion

    // Setp 1.
    protected override void BeforeSolveInstance() => status = DB.TransactionStatus.Uninitialized;

    // Step 2.
    //protected override void TrySolveInstance(IGH_DataAccess DA) { }

    // Step 3.
    //protected override void AfterSolveInstance() {}

    #region IFailuresPreprocessor

    // Override to add handled failures to your component (Order is important).
    protected virtual IEnumerable<DB.FailureDefinitionId> FailureDefinitionIdsToFix => null;
    protected virtual bool FixUnhandledFailures => true;

    protected virtual DB.IFailuresPreprocessor CreateFailuresPreprocessor()
    {
      return new TransactionalComponentFailuresPreprocessor(this, FailureDefinitionIdsToFix, FixUnhandledFailures);
    }
    #endregion

    #region ITransactionFinalizer
    public virtual void OnCommitted(DB.Document document, string strTransactionName)
    {
      if (Status < DB.TransactionStatus.Pending)
        Status = DB.TransactionStatus.Committed;
    }

    public virtual void OnRolledBack(DB.Document document, string strTransactionName)
    {
      if (Status < DB.TransactionStatus.Pending)
        Status = DB.TransactionStatus.RolledBack;
    }
    #endregion
  }

  public enum TransactionExtent
  {
    Default,
    Component,
    Instance,
    Scope,
  }

  public abstract class TransactionalChainComponent : TransactionalComponent, DBX.ITransactionNotification
  {
    protected TransactionalChainComponent(string name, string nickname, string description, string category, string subCategory)
    : base(name, nickname, description, category, subCategory)
    { }

    protected override bool AbortOnUnhandledException => TransactionExtent == TransactionExtent.Component;

    TransactionExtent transactionExtent = TransactionExtent.Default;
    protected TransactionExtent TransactionExtent
    {
      get => transactionExtent == TransactionExtent.Default ? TransactionExtent.Component : transactionExtent;
      set
      {
        if (Phase == GH_SolutionPhase.Computing)
          throw new InvalidOperationException();

        transactionExtent = value;
      }
    }

    DBX.TransactionChain chain;

    public DB.TransactionStatus StartTransaction(DB.Document document) => chain.Start(document);

    protected DB.TransactionStatus CommitTransaction()
    {
      if (TransactionExtent != TransactionExtent.Scope)
        throw new InvalidOperationException();

      try
      {
        if (chain.HasStarted())
         Status = chain.Commit();
      }
      catch (Exception e)
      {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"{e.Source}: {e.Message}");
      }

      return Status;
    }

    protected void RollBackTransaction()
    {
      if (TransactionExtent != TransactionExtent.Scope)
        throw new InvalidOperationException();

      try
      {
        if (chain.HasStarted())
          Status = chain.RollBack();
      }
      catch (Exception e)
      {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"{e.Source}: {e.Message}");
      }
    }

    // Setp 1.
    protected override void BeforeSolveInstance()
    {
      base.BeforeSolveInstance();

      chain = new DBX.TransactionChain
      (
        new DBX.TransactionHandlingOptions
        {
          FailuresPreprocessor = CreateFailuresPreprocessor(),
          TransactionNotification = this
        },
        Name
      );

    }

    // Step 2.
    protected sealed override void SolveInstance(IGH_DataAccess DA)
    {
      if (TransactionExtent == TransactionExtent.Component)
      {
        base.SolveInstance(DA);
      }
      else
      {
        try
        {
          base.SolveInstance(DA);

          if (chain.HasStarted())
            Status = chain.Commit();
        }
        catch
        {
          Status = chain.RollBack();
        }
        finally
        {
          switch (Status)
          {
            case DB.TransactionStatus.Uninitialized:
            case DB.TransactionStatus.Started:
            case DB.TransactionStatus.Committed:
              break;
            default:
              AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Transaction {Status} and aborted.");
              break;
          }
        }
      }
    }

    // Step 3.
    protected /*sealed*/ override void AfterSolveInstance()
    {
      using (chain)
      {
        try
        {
          if (chain.HasStarted())
            Status = IsAborted ? chain.RollBack() : chain.Commit();
        }
        catch (Exception e)
        {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"{e.Source}: {e.Message}");
        }
        finally
        {
          switch (Status)
          {
            case DB.TransactionStatus.Uninitialized:
            case DB.TransactionStatus.Started:
            case DB.TransactionStatus.Committed:
              break;
            default:
              AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Transaction {Status} and aborted.");
              ResetData();
              break;
          }
        }

        chain = default;
      }

      base.AfterSolveInstance();
    }

    #region DBX.ITransactionNotification
    External.UI.EditScope editScope = null;
    EventHandler<DialogBoxShowingEventArgs> dialogBoxShowing = null;

    // Step 2.1
    public virtual void OnStarted(DB.Document document) { }

    // Step 3.1
    public virtual void OnPrepare(IReadOnlyCollection<DB.Document> documents)
    {
      // Disable Rhino UI in case any warning-error dialog popups
      var activeApplication = Revit.ActiveUIApplication;
      activeApplication.DialogBoxShowing += dialogBoxShowing = (sender, args) =>
      {
        if (editScope is null)
          editScope = new External.UI.EditScope(activeApplication);
      };
    }

    // Step 3.2
    public virtual void OnDone(DB.TransactionStatus status)
    {
      Status = status;

      // Restore Rhino UI in case any warning-error dialog popups
      if(dialogBoxShowing != default)
      {
        Revit.ActiveUIApplication.DialogBoxShowing -= dialogBoxShowing;

        if (editScope is IDisposable disposable)
          disposable.Dispose();
      }
    }
    #endregion
  }

  public abstract class ElementTrackerComponent : TransactionalChainComponent, IGH_TrackingComponent
  {
    protected ElementTrackerComponent(string name, string nickname, string description, string category, string subCategory)
    : base(name, nickname, description, category, subCategory)
    { }

    protected virtual bool CurrentDocumentOnly
    {
      get
      {
        if(Params.Input<Parameters.Document>("Document") is IGH_Param document)
          return document.SourceCount == 0 && document.DataType == GH_ParamData.@void;

        return false;
      }
    }

    protected override void BeforeSolveInstance()
    {
      base.BeforeSolveInstance();

      var currentDocumentOnly = CurrentDocumentOnly;
      foreach (var output in Params.Output.OfType<IGH_TrackingParam>())
        output.OpenTrackingParam(currentDocumentOnly);
    }

    protected override void AfterSolveInstance()
    {
      foreach (var output in Params.Output.OfType<IGH_TrackingParam>())
        output.CloseTrackingParam();

      base.AfterSolveInstance();
    }

    #region IGH_TrackingComponent
    public TrackingMode TrackingMode { get; set; } = TrackingMode.Reconstruct;
    #endregion

    #region IO
    public override void AddedToDocument(GH_Document document)
    {
      if (ComponentVersion < new Version(0, 9, 0, 0))
        TrackingMode = TrackingMode.Reconstruct;

      base.AddedToDocument(document);
    }

    public override bool Read(GH_IReader reader)
    {
      if (!base.Read(reader))
        return false;

      int mode = (int) TrackingMode.Disabled;
      reader.TryGetInt32("TrackingMode", ref mode);
      TrackingMode = (TrackingMode) mode;

      return true;
    }

    public override bool Write(GH_IWriter writer)
    {
      if (!base.Write(writer))
        return false;

      if (TrackingMode != TrackingMode.Disabled)
        writer.SetInt32("TrackingMode", (int) TrackingMode);

      return true;
    }
    #endregion

    #region UI
    protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
    {
      base.AppendAdditionalComponentMenuItems(menu);

      if (TrackingMode != TrackingMode.NotApplicable)
      {
        if (Params.Output.OfType<ElementTracking.IGH_TrackingParam>().FirstOrDefault() is IGH_Param output)
        {
          Menu_AppendSeparator(menu);
          var tracking = Menu_AppendItem(menu, "Tracking Mode");

          var append = Menu_AppendItem(tracking.DropDown, "Disabled", (s, a) => { TrackingMode = TrackingMode.Disabled; ExpireSolution(true); }, true, TrackingMode == TrackingMode.Disabled);
          append.ToolTipText = $"No element tracking takes part in this mode, each solution will append a new {output.TypeName}." + Environment.NewLine +
                               $"The operation may fail if a {output.TypeName} with same name already exists.";

          var supersede = Menu_AppendItem(tracking.DropDown, "Supersede", (s, a) => { TrackingMode = TrackingMode.Supersede; ExpireSolution(true); }, true, TrackingMode == TrackingMode.Supersede);
          supersede.ToolTipText = $"A brand new {output.TypeName} will be created for each solution." + Environment.NewLine +
                                  $"{GH_Convert.ToPlural(output.TypeName)} created on previous iterations are deleted.";

          var reconstruct = Menu_AppendItem(tracking.DropDown, "Reconstruct", (s, a) => { TrackingMode = TrackingMode.Reconstruct; ExpireSolution(true); }, true, TrackingMode == TrackingMode.Reconstruct);
          reconstruct.ToolTipText = $"If suitable, the previous solution {output.TypeName} will be reconstructed from the input values;" + Environment.NewLine +
                                    "otherwise, a new one will be created.";
        }
      }
    }
    #endregion
  }
}
