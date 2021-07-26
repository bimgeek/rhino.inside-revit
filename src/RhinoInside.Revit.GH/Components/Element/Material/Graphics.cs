using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using DB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components.Material
{
  public class MaterialGraphics : TransactionalChainComponent
  {
    public override Guid ComponentGuid => new Guid("8C5CD6FB-4F48-4F35-B0B8-42B5A3636B5C");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    protected override string IconTag => "G";

    public MaterialGraphics()
    : base
    (
      "Material Graphics",
      "Graphics",
      "Material Graphics Data.",
      "Revit",
      "Material"
    )
    { }

    protected override ParamDefinition[] Inputs => inputs;
    static readonly ParamDefinition[] inputs =
    {
      ParamDefinition.Create<Parameters.Material>("Material", "M"),

      ParamDefinition.Create<Param_Boolean>("Use Render Appearance", "R", optional: true, relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Colour>("Color", "C", optional: true, relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Number>("Transparency", "T", optional: true, relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Number>("Shininess", "SH", optional: true, relevance: ParamRelevance.Occasional),
      ParamDefinition.Create<Param_Number>("Smoothness", "SM", optional: true, relevance: ParamRelevance.Occasional),

      ParamDefinition.Create<Parameters.FillPatternElement>("Surface Foreground Pattern", "SFP", optional: true, relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Colour>("Surface Foreground Color", "SFC", optional: true, relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Parameters.FillPatternElement>("Surface Background Pattern", "SBP", optional: true, relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Colour>("Surface Background Color", "SBC", optional: true, relevance: ParamRelevance.Primary),

      ParamDefinition.Create<Parameters.FillPatternElement>("Cut Foreground Pattern", "SFP", optional: true, relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Colour>("Cut Foreground Color", "SFC", optional: true, relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Parameters.FillPatternElement>("Cut Background Pattern", "SBP", optional: true, relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Colour>("Cut Background Color", "SBC", optional: true, relevance: ParamRelevance.Primary),
    };

    protected override ParamDefinition[] Outputs => outputs;
    static readonly ParamDefinition[] outputs =
    {
      ParamDefinition.Create<Parameters.Material>("Material", "M"),

      ParamDefinition.Create<Param_Boolean>("Use Render Appearance", "R", relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Colour>("Color", "C", relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Number>("Transparency", "T", relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Number>("Shininess", "SH", relevance: ParamRelevance.Occasional),
      ParamDefinition.Create<Param_Number>("Smoothness", "SM", relevance: ParamRelevance.Occasional),

      ParamDefinition.Create<Parameters.FillPatternElement>("Surface Foreground Pattern", "SFP", relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Colour>("Surface Foreground Color", "SFC", relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Parameters.FillPatternElement>("Surface Background Pattern", "SBP", relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Colour>("Surface Background Color", "SBC", relevance: ParamRelevance.Primary),

      ParamDefinition.Create<Parameters.FillPatternElement>("Cut Foreground Pattern", "SFP", relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Colour>("Cut Foreground Color", "SFC", relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Parameters.FillPatternElement>("Cut Background Pattern", "SBP", relevance: ParamRelevance.Primary),
      ParamDefinition.Create<Param_Colour>("Cut Background Color", "SBC", relevance: ParamRelevance.Primary),
    };

    protected override void TrySolveInstance(IGH_DataAccess DA)
    {
      if (!Params.GetData(DA, "Material", out Types.Material material, x => x.IsValid))
        return;

      bool update = false;
      update |= Params.GetData(DA, "Use Render Appearance", out bool? appearance);
      update |= Params.GetData(DA, "Color", out System.Drawing.Color? color);
      update |= Params.GetData(DA, "Transparency", out double? transparency);
      update |= Params.GetData(DA, "Shininess", out double? shininess);
      update |= Params.GetData(DA, "Smoothness", out double? smoothness);

      update |= Params.GetData(DA, "Surface Foreground Pattern", out Types.FillPatternElement sfp);
      update |= Params.GetData(DA, "Surface Foreground Color", out System.Drawing.Color? sfc);
      update |= Params.GetData(DA, "Surface Background Pattern", out Types.FillPatternElement sbp);
      update |= Params.GetData(DA, "Surface Background Color", out System.Drawing.Color? sbc);

      update |= Params.GetData(DA, "Cut Foreground Pattern", out Types.FillPatternElement cfp);
      update |= Params.GetData(DA, "Cut Foreground Color", out System.Drawing.Color? cfc);
      update |= Params.GetData(DA, "Cut Background Pattern", out Types.FillPatternElement cbp);
      update |= Params.GetData(DA, "Cut Background Color", out System.Drawing.Color? cbc);

      if (update)
      {
        StartTransaction(material.Document);
        material.UseRenderAppearanceForShading = appearance;
        material.Color = color;
        material.Transparency = transparency;
        material.Shininess = shininess;
        material.Smoothness = smoothness;

        material.SurfaceForegroundPattern = sfp;
        material.SurfaceForegroundPatternColor = sfc;
        material.SurfaceBackgroundPattern = sbp;
        material.SurfaceBackgroundPatternColor = sbc;

        material.CutForegroundPattern = cfp;
        material.CutForegroundPatternColor = cfc;
        material.CutBackgroundPattern = cbp;
        material.CutBackgroundPatternColor = cbc;
      }

      Params.TrySetData(DA, "Material", () => material);

      Params.TrySetData(DA, "Use Render Appearance", () => material.UseRenderAppearanceForShading);
      Params.TrySetData(DA, "Color", () => material.Color);
      Params.TrySetData(DA, "Transparency", () => material.Transparency);
      Params.TrySetData(DA, "Shininess", () => material.Shininess);
      Params.TrySetData(DA, "Smoothness", () => material.Smoothness);

      Params.TrySetData(DA, "Surface Foreground Pattern", () => material.SurfaceForegroundPattern);
      Params.TrySetData(DA, "Surface Foreground Color", () => material.SurfaceForegroundPatternColor);
      Params.TrySetData(DA, "Surface Background Pattern", () => material.SurfaceBackgroundPattern);
      Params.TrySetData(DA, "Surface Background Color", () => material.SurfaceBackgroundPatternColor);

      Params.TrySetData(DA, "Cut Foreground Pattern", () => material.CutForegroundPattern);
      Params.TrySetData(DA, "Cut Foreground Color", () => material.CutForegroundPatternColor);
      Params.TrySetData(DA, "Cut Background Pattern", () => material.CutBackgroundPattern);
      Params.TrySetData(DA, "Cut Background Color", () => material.CutBackgroundPatternColor);
    }
  }
}

