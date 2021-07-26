using System;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using RhinoInside.Revit.Convert.Geometry;
using RhinoInside.Revit.External.DB.Extensions;
using DB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components.Level
{
  public class DocumentLevels : ElementCollectorComponent
  {
    public override Guid ComponentGuid => new Guid("87715CAF-92A9-4B14-99E5-F8CCB2CC19BD");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    protected override DB.ElementFilter ElementFilter => new DB.ElementClassFilter(typeof(DB.Level));

    public DocumentLevels() : base
    (
      name: "Query Levels",
      nickname: "Levels",
      description: "Get all document levels",
      category: "Revit",
      subCategory: "Model"
    )
    { }

    protected override ParamDefinition[] Inputs => inputs;
    static readonly ParamDefinition[] inputs =
    {
      new ParamDefinition(new Parameters.Document(), ParamRelevance.Occasional),
      ParamDefinition.Create<Param_String>("Name", "N", "Level name", GH_ParamAccess.item, optional: true),
      ParamDefinition.Create<Parameters.ElevationInterval>("Elevation", "E", "Level elevation interval along z-axis", GH_ParamAccess.item, optional: true, relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Boolean>("Structural", "S", "Level is structural", GH_ParamAccess.item, optional: true, relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Boolean>("Building Story", "BS", "Level is building story", defaultValue: true, GH_ParamAccess.item, optional: true, relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Parameters.ElementFilter>("Filter", "F", "Filter", GH_ParamAccess.item, optional: true, relevance: ParamRelevance.Primary)
    };

    protected override ParamDefinition[] Outputs => outputs;
    static readonly ParamDefinition[] outputs =
    {
      ParamDefinition.Create<Parameters.Level>("Levels", "L", "Levels list", GH_ParamAccess.list)
    };

    protected override void TrySolveInstance(IGH_DataAccess DA)
    {
      if (!Parameters.Document.GetDataOrDefault(this, DA, "Document", out var doc))
        return;

      if (!Params.TryGetData(DA, "Name", out string name)) return;
      if (!Params.TryGetData(DA, "Elevation", out Interval? elevation, x => x.IsValid)) return;
      if (!Params.TryGetData(DA, "Structural", out bool? structural)) return;
      if (!Params.TryGetData(DA, "Building Story", out bool? buildingStory)) return;
      if (!Params.TryGetData(DA, "Filter", out DB.ElementFilter filter, x => x.IsValidObject)) return;

      using (var collector = new DB.FilteredElementCollector(doc))
      {
        var levelsCollector = collector.WherePasses(ElementFilter);

        if (filter is object)
          levelsCollector = levelsCollector.WherePasses(filter);

        if (name is string && TryGetFilterStringParam(DB.BuiltInParameter.DATUM_TEXT, ref name, out var nameFilter))
          levelsCollector = levelsCollector.WherePasses(nameFilter);

        if (structural.HasValue && TryGetFilterIntegerParam(DB.BuiltInParameter.LEVEL_IS_STRUCTURAL, structural.Value ? 1 : 0, out var structuralFilter))
          levelsCollector = levelsCollector.WherePasses(structuralFilter);

        if (buildingStory.HasValue && TryGetFilterIntegerParam(DB.BuiltInParameter.LEVEL_IS_BUILDING_STORY, buildingStory.Value ? 1 : 0, out var buildingStoryilter))
          levelsCollector = levelsCollector.WherePasses(buildingStoryilter);

        var levels = levelsCollector.Cast<DB.Level>();

        if (!string.IsNullOrEmpty(name))
          levels = levels.Where(x => x.Name.IsSymbolNameLike(name));

        if (elevation.HasValue)
        {
          var height = elevation.Value.InHostUnits() +
            doc.GetBasePointLocation(Params.Input<Parameters.ElevationInterval>("Elevation").ElevationBase).Z;

          levels = levels.Where(x => height.IncludesParameter(x.GetHeight(), false));
        }

        DA.SetDataList("Levels", levels);
      }
    }
  }
}
