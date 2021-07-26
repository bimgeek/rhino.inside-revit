using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace RhinoInside.Revit.GH.Parameters
{
  public class DataObject<T> : Param<Types.DataObject<T>>
  {
    public override Guid ComponentGuid => new Guid("F25FAC7B-B338-4E12-A974-F2238E3B83C2");

    protected override Bitmap Icon => (Bitmap) Properties.Resources.ResourceManager.GetObject(typeof(T).Name);

    public override GH_Exposure Exposure => GH_Exposure.hidden;

    public DataObject() : base
    (
      name: "RevitAPIDataObject",
      nickname: "RevitAPIDataObject",
      description: "Wraps Types.DataObject",
      category: string.Empty,
      subcategory: string.Empty
    )
    { }
  }
}
