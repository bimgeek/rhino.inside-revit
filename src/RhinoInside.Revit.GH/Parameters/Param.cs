using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace RhinoInside.Revit.GH.Parameters
{
  public abstract class Param<T> : GH_Param<T>
    where T : class, IGH_Goo
  {
    protected override Bitmap Icon => ((Bitmap) Properties.Resources.ResourceManager.GetObject(GetType().Name)) ??
                                      ImageBuilder.BuildIcon(IconTag, Properties.Resources.UnknownIcon);

    protected virtual string IconTag => typeof(T).Name.Substring(0, 1);

    public virtual void SetInitCode(string code) => NickName = code;

    protected Param(string name, string nickname, string description, string category, string subcategory) :
      base(name, nickname, description, category, subcategory, GH_ParamAccess.item)
    {
      Debug.Assert(GetType().IsPublic, $"{GetType()} is not public, Grasshopper will fail deserializing this type.");

      ComponentVersion = CurrentVersion;

      if (Obsolete)
      {
        foreach (var obsolete in GetType().GetCustomAttributes(typeof(ObsoleteAttribute), false).Cast<ObsoleteAttribute>())
        {
          if (!string.IsNullOrEmpty(obsolete.Message))
            Description = obsolete.Message + Environment.NewLine + Description;
        }
      }
    }

    #region IO
    protected virtual Version CurrentVersion => GetType().Assembly.GetName().Version;
    protected Version ComponentVersion { get; private set; }

    public override bool Read(GH_IReader reader)
    {
      if (!base.Read(reader))
        return false;

      string version = "0.0.0.0";
      reader.TryGetString("ComponentVersion", ref version);
      ComponentVersion = Version.TryParse(version, out var componentVersion) ?
        componentVersion : new Version(0, 0, 0, 0);

      if (ComponentVersion > CurrentVersion)
      {
        var assemblyName = Grasshopper.Instances.ComponentServer.FindAssemblyByObject(this)?.Name ?? GetType().Assembly.GetName().Name;
        reader.AddMessage($"Component '{Name}' was saved with a newer version.{Environment.NewLine}Please update '{assemblyName}' to version {ComponentVersion} or above.", GH_Message_Type.error);
      }

      return true;
    }

    public override bool Write(GH_IWriter writer)
    {
      if (!base.Write(writer))
        return false;

      if (ComponentVersion > CurrentVersion)
        writer.SetString("ComponentVersion", ComponentVersion.ToString());
      else
        writer.SetString("ComponentVersion", CurrentVersion.ToString());

      return true;
    }
    #endregion

    public override void PostProcessData()
    {
      if (ComponentVersion > CurrentVersion)
      {
        var assemblyName = Grasshopper.Instances.ComponentServer.FindAssemblyByObject(this)?.Name ?? GetType().Assembly.GetName().Name;
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"This component was saved with a newer version.{Environment.NewLine}Please update '{assemblyName}' to version {ComponentVersion} or above.");
        VolatileData.Clear();
        return;
      }

      base.PostProcessData();
    }
  }

  public abstract class ParamWithPreview<T> : Param<T>, IGH_PreviewObject
    where T : class, IGH_Goo
  {
    protected ParamWithPreview(string name, string nickname, string description, string category, string subcategory) :
    base(name, nickname, description, category, subcategory)
    { }

    #region IGH_PreviewObject
    bool IGH_PreviewObject.Hidden { get; set; }
    bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;
    BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();
    void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);
    void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);
    #endregion
  }
}
