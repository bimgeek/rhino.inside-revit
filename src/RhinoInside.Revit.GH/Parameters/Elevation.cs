using System;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using RhinoInside.Revit.External.DB.Extensions;
using DBX = RhinoInside.Revit.External.DB;

namespace RhinoInside.Revit.GH.Parameters
{
  public class Elevation : Param_Number
  {
    public override Guid ComponentGuid => new Guid("63F4A581-6065-4F90-BAD2-714DA8B97C08");
    public override GH_Exposure Exposure => GH_Exposure.hidden;
    public DBX.ElevationBase ElevationBase { get; set; } = DBX.ElevationBase.InternalOrigin;

    public Elevation()
    {
      Name = "Elevation";
      NickName = "E";
      Description = "Contains a collection of signed distances along Z axis";
      Category = "Params";
      SubCategory = "Revit Primitives";
    }

    public override bool Read(GH_IReader reader)
    {
      if (!base.Read(reader))
        return false;

      int elevationBase = (int) DBX.ElevationBase.InternalOrigin;
      reader.TryGetInt32("ElevationBase", ref elevationBase);
      ElevationBase = (DBX.ElevationBase) elevationBase;

      return true;
    }

    public override bool Write(GH_IWriter writer)
    {
      if (!base.Write(writer))
        return false;

      if (ElevationBase != DBX.ElevationBase.InternalOrigin)
        writer.SetInt32("Grouping", (int) ElevationBase);

      return true;
    }

    public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
    {
      Menu_AppendWireDisplay(menu);
      //this.Menu_AppendConnect(menu);
      Menu_AppendDisconnectWires(menu);

      Menu_AppendPrincipalParameter(menu);
      Menu_AppendReverseParameter(menu);
      Menu_AppendFlattenParameter(menu);
      Menu_AppendGraftParameter(menu);
      Menu_AppendSimplifyParameter(menu);
      Menu_AppendElevationBase(menu);

      if (Kind == GH_ParamKind.floating || Kind == GH_ParamKind.input)
      {
        Menu_AppendSeparator(menu);
        if (Menu_CustomSingleValueItem() is ToolStripMenuItem single)
        {
          single.Enabled &= SourceCount == 0;
          menu.Items.Add(single);
        }
        else Menu_AppendPromptOne(menu);

        if (Menu_CustomMultiValueItem() is ToolStripMenuItem more)
        {
          more.Enabled &= SourceCount == 0;
          menu.Items.Add(more);
        }
        else Menu_AppendPromptMore(menu);

        Menu_AppendManageCollection(menu);
        Menu_AppendSeparator(menu);
        Menu_AppendDestroyPersistent(menu);
        Menu_AppendInternaliseData(menu);

        if (Exposure != GH_Exposure.hidden)
          Menu_AppendExtractParameter(menu);
      }
    }

    protected void Menu_AppendElevationBase(ToolStripDropDown menu)
    {
      if (Kind <= GH_ParamKind.floating)
        return;

      var Base = Menu_AppendItem(menu, "Elevation Base") as ToolStripMenuItem;

      Base.Checked = ElevationBase != DBX.ElevationBase.InternalOrigin;
      Menu_AppendItem(Base.DropDown, "Internal Origin", (s, a) => Menu_ElevationBase(DBX.ElevationBase.InternalOrigin), true, ElevationBase == DBX.ElevationBase.InternalOrigin);
      Menu_AppendItem(Base.DropDown, "Project Base Point", (s, a) => Menu_ElevationBase(DBX.ElevationBase.ProjectBasePoint), true, ElevationBase == DBX.ElevationBase.ProjectBasePoint);
      Menu_AppendItem(Base.DropDown, "Survey Point", (s, a) => Menu_ElevationBase(DBX.ElevationBase.SurveyPoint), true, ElevationBase == DBX.ElevationBase.SurveyPoint);
    }

    private void Menu_ElevationBase(DBX.ElevationBase value)
    {
      RecordUndoEvent("Set: Elevation Base");

      ElevationBase = value;

      OnObjectChanged(GH_ObjectEventType.Options);

      if (Kind == GH_ParamKind.output)
        ExpireOwner();

      ExpireSolution(true);
    }

    public static bool GetData(IGH_Component component, IGH_DataAccess DA, string name, out double height, Types.Document document)
    {
      height = default;
      if (!DA.GetData(name, ref height)) return false;
      height += document.Value.GetBasePointLocation(component.Params.Input<Elevation>(name).ElevationBase).Z * Revit.ModelUnits;

      return true;
    }
  }

  public class ElevationInterval : Param_Interval
  {
    public override Guid ComponentGuid => new Guid("4150D40A-7C02-4633-B3B5-CFE4B16855B5");
    public override GH_Exposure Exposure => GH_Exposure.hidden;
    public DBX.ElevationBase ElevationBase { get; set; } = DBX.ElevationBase.InternalOrigin;

    public ElevationInterval()
    {
      Name = "Elevation";
      NickName = "E";
      Description = "Signed distance interval along Z axis";
      Category = "Params";
      SubCategory = "Revit Primitives";
    }

    public override bool Read(GH_IReader reader)
    {
      if (!base.Read(reader))
        return false;

      int elevationBase = (int) DBX.ElevationBase.InternalOrigin;
      reader.TryGetInt32("ElevationBase", ref elevationBase);
      ElevationBase = (DBX.ElevationBase) elevationBase;

      return true;
    }

    public override bool Write(GH_IWriter writer)
    {
      if (!base.Write(writer))
        return false;

      if (ElevationBase != DBX.ElevationBase.InternalOrigin)
        writer.SetInt32("Grouping", (int) ElevationBase);

      return true;
    }

    public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
    {
      Menu_AppendWireDisplay(menu);
      //this.Menu_AppendConnect(menu);
      Menu_AppendDisconnectWires(menu);

      Menu_AppendPrincipalParameter(menu);
      Menu_AppendReverseParameter(menu);
      Menu_AppendFlattenParameter(menu);
      Menu_AppendGraftParameter(menu);
      Menu_AppendSimplifyParameter(menu);
      Menu_AppendElevationBase(menu);

      if (Kind == GH_ParamKind.floating || Kind == GH_ParamKind.input)
      {
        Menu_AppendSeparator(menu);
        if (Menu_CustomSingleValueItem() is ToolStripMenuItem single)
        {
          single.Enabled &= SourceCount == 0;
          menu.Items.Add(single);
        }
        else Menu_AppendPromptOne(menu);

        if (Menu_CustomMultiValueItem() is ToolStripMenuItem more)
        {
          more.Enabled &= SourceCount == 0;
          menu.Items.Add(more);
        }
        else Menu_AppendPromptMore(menu);

        Menu_AppendManageCollection(menu);
        Menu_AppendSeparator(menu);
        Menu_AppendDestroyPersistent(menu);
        Menu_AppendInternaliseData(menu);

        if (Exposure != GH_Exposure.hidden)
          Menu_AppendExtractParameter(menu);
      }
    }

    protected void Menu_AppendElevationBase(ToolStripDropDown menu)
    {
      if (Kind <= GH_ParamKind.floating)
        return;

      var Base = Menu_AppendItem(menu, "Elevation Base") as ToolStripMenuItem;

      Base.Checked = ElevationBase != DBX.ElevationBase.InternalOrigin;
      Menu_AppendItem(Base.DropDown, "Internal Origin", (s, a) => Menu_ElevationBase(DBX.ElevationBase.InternalOrigin), true, ElevationBase == DBX.ElevationBase.InternalOrigin);
      Menu_AppendItem(Base.DropDown, "Project Base Point", (s, a) => Menu_ElevationBase(DBX.ElevationBase.ProjectBasePoint), true, ElevationBase == DBX.ElevationBase.ProjectBasePoint);
      Menu_AppendItem(Base.DropDown, "Survey Point", (s, a) => Menu_ElevationBase(DBX.ElevationBase.SurveyPoint), true, ElevationBase == DBX.ElevationBase.SurveyPoint);
    }

    private void Menu_ElevationBase(DBX.ElevationBase value)
    {
      RecordUndoEvent("Set: Elevation Base");

      ElevationBase = value;

      OnObjectChanged(GH_ObjectEventType.Options);

      if (Kind == GH_ParamKind.output)
        ExpireOwner();

      ExpireSolution(true);
    }
  }
}
