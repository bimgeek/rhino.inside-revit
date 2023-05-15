using System;
using Grasshopper.Kernel;
using ARDB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Parameters
{
  public class BeamSystem : GraphicalElement<Types.BeamSystem, ARDB.BeamSystem>
  {
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override Guid ComponentGuid => new Guid("45CD3655-4658-4C35-99FB-9975A1128DDF");

    public BeamSystem() : base("Beam System", "Beam System", "Contains a collection of Revit Beam System elements", "Params", "Revit Elements") { }

  }
}
