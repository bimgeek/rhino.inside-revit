using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using RhinoInside.Revit.External.DB.Extensions;
using DB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components
{
  public class DocumentLinks : ElementCollectorComponent
  {
    public override Guid ComponentGuid => new Guid("EBCCFDD8-9F3B-44F4-A209-72D06C8082A5");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    protected override string IconTag => "L";
    protected override DB.ElementFilter ElementFilter => new DB.ElementClassFilter(typeof(DB.RevitLinkType));

    #region UI
    protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
    {
      base.AppendAdditionalComponentMenuItems(menu);

      var activeApp = Revit.ActiveUIApplication;
      var commandId = Autodesk.Revit.UI.RevitCommandId.LookupPostableCommandId(Autodesk.Revit.UI.PostableCommand.ManageLinks);
      Menu_AppendItem
      (
        menu, $"Manage Links…",
        (sender, arg) => External.UI.EditScope.PostCommand(activeApp, commandId),
        activeApp.CanPostCommand(commandId), false
      );
    }
    #endregion

    public DocumentLinks() : base
    (
      name: "Document Links",
      nickname: "Links",
      description: "Gets Revit documents that are linked into given document",
      category: "Revit",
      subCategory: "Document"
    )
    { }

    protected override ParamDefinition[] Inputs => inputs;
    static readonly ParamDefinition[] inputs =
    {
      new ParamDefinition(new Parameters.Document(), ParamRelevance.Occasional),
    };

    protected override ParamDefinition[] Outputs => outputs;
    static readonly ParamDefinition[] outputs =
    {
      ParamDefinition.Create<Parameters.Document>("Documents", "D", "Revit documents that are linked into given document", GH_ParamAccess.list)
    };

    protected override void TrySolveInstance(IGH_DataAccess DA)
    {
      if (!Parameters.Document.GetDataOrDefault(this, DA, "Document", out var doc))
        return;

      // Note: linked documents that are not loaded in Revit memory,
      // are not reported since no interaction can be done if not loaded
      var docs = new List<DB.Document>();
      using (var documents = Revit.ActiveDBApplication.Documents)
      {
        /* NOTES:
         * 1) On a cloud host model with links (that are also on cloud)
         *    .GetAllExternalFileReferences does not return the "File" references
         *    to the linked cloud models
         * 2) doc.PathName is not equal to DB.ExternalResourceReference.InSessionPath
         *    e.g. Same modle but reported paths are different. Respectively:
         *    "BIM 360://Default Test/Host_Model1.rvt"
         *    vs
         *    "BIM 360://Default Test/Project Files/Linked_Project2.rvt"
         */
        // get all external model references
        foreach (var id in DB.ExternalFileUtils.GetAllExternalFileReferences(doc))
        {
          // inspect the reference, and ...
          var reference = DB.ExternalFileUtils.GetExternalFileReference(doc, id);
          if (reference.ExternalFileReferenceType == DB.ExternalFileReferenceType.RevitLink)
          {
            // grab the model path
            var modelPath = reference.PathType == DB.PathType.Relative ? reference.GetAbsolutePath() : reference.GetPath();
            // look into the loaded documents and find the one with the same path
            if (documents.Cast<DB.Document>().Where(x => x.IsLinked && modelPath.IsEquivalent(x.GetModelPath())).FirstOrDefault() is DB.Document linkedDoc)
              // if found, add that to the output list
              docs.Add(linkedDoc);
          }
        }

#if REVIT_2020
        // if no document is reported using DB.ExternalFileUtils then links
        // are in the cloud. try getting linked documents from DB.RevitLinkType
        // element types inside the host model

        // find all the revit link types in the host model
        using (var collector = new DB.FilteredElementCollector(doc).OfClass(typeof(DB.RevitLinkType)))
        {
          foreach (var revitLinkType in collector)
          {
            // extract the path of external document that is wrapped by the revit link type
            var linkInfo = revitLinkType.GetExternalResourceReferences().FirstOrDefault();
            if (linkInfo.Key != null && linkInfo.Value.HasValidDisplayPath())
            {
              // stores custom info about the reference (project::model ids)
              var refInfo = linkInfo.Value.GetReferenceInformation();
              // try to grab the linked cloud doc id from the info dict
              string linkedDocId;
              if (refInfo.TryGetValue("LinkedModelModelId", out linkedDocId))
              {
                // look into the loaded documents and find the one with the same 'cloud' path
                var linkedDoc = documents.Cast<DB.Document>()
                                         .Where(x => x.IsLinked &&
                                                     x.GetCloudModelPath().GetModelGUID() == Guid.Parse(linkedDocId))
                                         .FirstOrDefault();
                // if found add that to the output list
                if (linkedDoc != null)
                  docs.Add(linkedDoc);
              }
            }
          }
        }
#endif

        DA.SetDataList("Documents", docs);
      }
    }
  }
}
