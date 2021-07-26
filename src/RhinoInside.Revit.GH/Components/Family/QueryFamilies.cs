using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using RhinoInside.Revit.External.DB.Extensions;
using DB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components
{
  public class QueryFamilies : ElementCollectorComponent
  {
    public override Guid ComponentGuid => new Guid("B6C377BA-BC46-495C-8250-F09DB0219C91");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    protected override DB.ElementFilter ElementFilter => new DB.ElementIsElementTypeFilter(false);

    public QueryFamilies() : base
    (
      name: "Query Families",
      nickname: "Families",
      description: "Get document families list",
      category: "Revit",
      subCategory: "Family"
    )
    { }

    protected override ParamDefinition[] Inputs => inputs;
    static readonly ParamDefinition[] inputs =
    {
      new ParamDefinition(new Parameters.Document(), ParamRelevance.Occasional),
      ParamDefinition.Create<Parameters.Category>("Category", "C", string.Empty, GH_ParamAccess.item, optional: true),
      ParamDefinition.Create<Param_String>("Name", "N", string.Empty, GH_ParamAccess.item, optional: true),
      ParamDefinition.Create<Parameters.ElementFilter>("Filter", "F", "Filter", GH_ParamAccess.item, optional: true),
    };

    protected override ParamDefinition[] Outputs => outputs;
    static readonly ParamDefinition[] outputs =
    {
      ParamDefinition.Create<Param_String>("Families", "F", "Family list", GH_ParamAccess.list)
    };

    struct FamilyNameComparer : IEqualityComparer<DB.ElementType>
    {
      public  bool Equals(DB.ElementType x, DB.ElementType y)
      {
        if (ReferenceEquals(x, y))
          return true;

        var familyNameX = x.FamilyName;
        var isSystemTypeX = x is DB.FamilySymbol;

        var familyNameY = y.FamilyName;
        var isSystemTypeY = y is DB.FamilySymbol;

        return (isSystemTypeX == isSystemTypeY) && (familyNameX == familyNameY);
      }

      public int GetHashCode(DB.ElementType obj) => new
      {
        IsSystemType = obj is DB.FamilySymbol,
        FamilyName = obj?.FamilyName
      }.
      GetHashCode();
    }

    protected override void TrySolveInstance(IGH_DataAccess DA)
    {
      if (!Parameters.Document.GetDataOrDefault(this, DA, "Document", out var doc)) return;
      Params.GetData(DA, "Category", out Types.Category category);
      Params.GetData(DA, "Name", out string name);
      Params.GetData(DA, "Filter", out DB.ElementFilter filter);

      using (var collector = new DB.FilteredElementCollector(doc))
      {
        var elementCollector = collector.WherePasses(ElementFilter);

        if (category is object)
          elementCollector.WhereCategoryIdEqualsTo(category.Id);

        if (filter is object)
          elementCollector = elementCollector.WherePasses(filter);

        if (TryGetFilterStringParam(DB.BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM, ref name, out var nameFilter))
          elementCollector = elementCollector.WherePasses(nameFilter);

        var familiesSet = new HashSet<DB.ElementType>(elementCollector.Cast<DB.ElementType>(), default(FamilyNameComparer));

        var families = name is null ?
          familiesSet :
          familiesSet.Where(x => x.FamilyName.IsSymbolNameLike(name));

        DA.SetDataList("Families", families.Select(x => x.FamilyName));
      }
    }

    protected override string HtmlHelp_Source()
    {
      var nTopic = new Grasshopper.GUI.HTML.GH_HtmlFormatter(this)
      {
        Title = Name,
        Description =
        @"<p>This component returns all Families in the document filtered by Filter.</p>" +
        @"<p>You can also specify a name pattern as Name." +
        @"<p>Several kind of patterns are supported, the method used depends on the first pattern character:</p>" +
        @"<dl>" +
        @"<dt><b><</b></dt><dd>Starts with</dd>" +
        @"<dt><b>></b></dt><dd>Ends with</dd>" +
        @"<dt><b>?</b></dt><dd>Contains, same as a regular search</dd>" +
        @"<dt><b>:</b></dt><dd>Wildcards, see Microsoft.VisualBasic " + "<a target=\"_blank\" href=\"https://docs.microsoft.com/en-us/dotnet/visual-basic/language-reference/operators/like-operator#pattern-options\">LikeOperator</a></dd>" +
        @"<dt><b>;</b></dt><dd>Regular expresion, see " + "<a target=\"_blank\" href=\"https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference\">here</a> as reference</dd>" +
        @"</dl>" +
        @"Else it looks for an exact match.",
        ContactURI = AssemblyInfo.ContactURI,
        WebPageURI = AssemblyInfo.WebPageURI
      };

      return nTopic.HtmlFormat();
    }
  }
}
