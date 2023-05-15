using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using ARDB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Parameters
{
  public class Truss : GraphicalElement<Types.Truss, ARDB.Structure.Truss>
  {
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override Guid ComponentGuid => new Guid("A3313847-9ACE-483D-BED8-5FA7CA2DE103");

    public Truss() : base("Truss", "Truss", "Contains a collection of Revit Truss elements", "Params", "Revit Elements") { }

  }
}
