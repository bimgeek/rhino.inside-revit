using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using RhinoInside.Revit.Convert.Geometry;
using RhinoInside.Revit.External.DB.Extensions;
using RhinoInside.Revit.External.DB.Schemas;
using DB = Autodesk.Revit.DB;
using DBX = RhinoInside.Revit.External.DB;

namespace RhinoInside.Revit.GH
{
  static class ParameterUtils
  {
    internal static IGH_Goo AsGoo(this DB.Parameter parameter)
    {
      if (parameter?.HasValue != true)
        return default;

      switch (parameter.StorageType)
      {
        case DB.StorageType.Integer:
          var integer = parameter.AsInteger();

          if (parameter.Definition is DB.Definition definition)
          {
            var dataType = definition.GetDataType();

            if (dataType == SpecType.Boolean.YesNo)
              return new GH_Boolean(integer != 0);

            if (parameter.Id.TryGetBuiltInParameter(out var builtInInteger))
            {
              switch (builtInInteger)
              {
                case DB.BuiltInParameter.AUTO_JOIN_CONDITION:         return new Types.CurtainGridJoinCondition((DBX.CurtainGridJoinCondition) integer);
                case DB.BuiltInParameter.AUTO_JOIN_CONDITION_WALL:    return new Types.CurtainGridJoinCondition((DBX.CurtainGridJoinCondition) integer);
                case DB.BuiltInParameter.SPACING_LAYOUT_U:            return new Types.CurtainGridLayout((DBX.CurtainGridLayout) integer);
                case DB.BuiltInParameter.SPACING_LAYOUT_1:            return new Types.CurtainGridLayout((DBX.CurtainGridLayout) integer);
                case DB.BuiltInParameter.SPACING_LAYOUT_VERT:         return new Types.CurtainGridLayout((DBX.CurtainGridLayout) integer);
                case DB.BuiltInParameter.SPACING_LAYOUT_V:            return new Types.CurtainGridLayout((DBX.CurtainGridLayout) integer);
                case DB.BuiltInParameter.SPACING_LAYOUT_2:            return new Types.CurtainGridLayout((DBX.CurtainGridLayout) integer);
                case DB.BuiltInParameter.SPACING_LAYOUT_HORIZ:        return new Types.CurtainGridLayout((DBX.CurtainGridLayout) integer);
                case DB.BuiltInParameter.WRAPPING_AT_INSERTS_PARAM:   return new Types.WallWrapping((DBX.WallWrapping) integer);
                case DB.BuiltInParameter.WRAPPING_AT_ENDS_PARAM:      return new Types.WallWrapping((DBX.WallWrapping) integer);
                case DB.BuiltInParameter.WALL_STRUCTURAL_USAGE_PARAM: return new Types.StructuralWallUsage((DB.Structure.StructuralWallUsage) integer);
                case DB.BuiltInParameter.WALL_KEY_REF_PARAM:          return new Types.WallLocationLine((DB.WallLocationLine) integer);
                case DB.BuiltInParameter.FUNCTION_PARAM:              return new Types.WallFunction((DB.WallFunction) integer);
              }

              var builtInIntegerName = builtInInteger.ToString();
              if (builtInIntegerName.Contains("COLOR_") || builtInIntegerName.Contains("_COLOR_") || builtInIntegerName.Contains("_COLOR"))
              {
                int r = integer % 256;
                integer /= 256;
                int g = integer % 256;
                integer /= 256;
                int b = integer % 256;

                return new GH_Colour(System.Drawing.Color.FromArgb(r, g, b));
              }
            }
          }

          return new GH_Integer(integer);

        case DB.StorageType.Double:
          return new GH_Number(parameter.AsDoubleInRhinoUnits());

        case DB.StorageType.String:
          return new GH_String(parameter.AsString());

        case DB.StorageType.ElementId:

          var elementId = parameter.AsElementId();
          if (parameter.Id.TryGetBuiltInParameter(out var builtInElementId))
          {
            if (builtInElementId == DB.BuiltInParameter.ID_PARAM || builtInElementId == DB.BuiltInParameter.SYMBOL_ID_PARAM)
              return new GH_Integer(elementId.IntegerValue);
          }

          return Types.Element.FromElementId(parameter.Element?.Document, parameter.AsElementId());

        default:
          throw new NotImplementedException();
      }
    }

    internal static bool Set(this DB.Parameter parameter, IGH_Goo value)
    {
      if (parameter is null)
        return default;

      switch (parameter.StorageType)
      {
        case DB.StorageType.Integer:

          if (parameter.Definition is DB.Definition definition)
          {
            if (definition.GetDataType() == SpecType.Boolean.YesNo)
            {
              if (!GH_Convert.ToBoolean(value, out var boolean, GH_Conversion.Both))
                throw new InvalidCastException();

              return parameter.Set(boolean ? 1 : 0);
            }
            else if (parameter.Id.TryGetBuiltInParameter(out var builtInParameter))
            {
              var builtInParameterName = builtInParameter.ToString();
              if (builtInParameterName.Contains("COLOR_") || builtInParameterName.Contains("_COLOR_") || builtInParameterName.Contains("_COLOR"))
              {
                if (!GH_Convert.ToColor(value, out var color, GH_Conversion.Both))
                  throw new InvalidCastException();

                return parameter.Set(((int) color.R) | ((int) color.G << 8) | ((int) color.B << 16));
              }
            }
          }

          if (!GH_Convert.ToInt32(value, out var integer, GH_Conversion.Both))
            throw new InvalidCastException();

          return parameter.Set(integer);

        case DB.StorageType.Double:
          if (!GH_Convert.ToDouble(value, out var real, GH_Conversion.Both))
            throw new InvalidCastException();

          return parameter.SetDoubleInRhinoUnits(real);

        case DB.StorageType.String:
          if (!GH_Convert.ToString(value, out var text, GH_Conversion.Both))
            throw new InvalidCastException();

          return parameter.Set(text);

        case DB.StorageType.ElementId:
          var element = new Types.Element();
          if (!element.CastFrom(value))
            throw new InvalidCastException();

          if (!parameter.Element.Document.IsEquivalent(element.Document))
            throw new ArgumentException("Failed to assign an element from a diferent document.", parameter.Definition.Name);

          return parameter.Set(element.Id);

        default:
          throw new NotImplementedException();
      }
    }

    internal static double AsDoubleInRhinoUnits(this DB.Parameter parameter)
    {
      var value = parameter.AsDouble();

      return SpecType.IsMeasurableSpec(parameter.Definition.GetDataType(), out var spec) ?
        UnitConverter.InRhinoUnits(value, spec) :
        value;
    }

    internal static bool SetDoubleInRhinoUnits(this DB.Parameter parameter, double value)
    {
      return parameter.Set
      (
        SpecType.IsMeasurableSpec(parameter.Definition.GetDataType(), out var spec) ?
        UnitConverter.InHostUnits(value, spec) :
        value
      );
    }

    internal static DB.Parameter GetParameter(IGH_ActiveObject obj, DB.Element element, IGH_Goo goo)
    {
      DB.Parameter parameter = null;
      object key = default;

      if (goo is Types.ParameterValue value)
        goo = new Types.ParameterKey(value.Document, value.Value.Id);

      if (goo is Types.ParameterId id) parameter = element.GetParameter(id.Value);
      else if (goo is Types.ParameterKey parameterKey)
      {
        if (parameterKey.Document.Equals(element.Document)) key = parameterKey.Id;
        else
        {
          switch (parameterKey.Class)
          {
            case DBX.ParameterClass.BuiltIn:
              parameter = parameterKey.Id.TryGetBuiltInParameter(out var builtInParameter) ? element.get_Parameter(builtInParameter) : default; break;
            case DBX.ParameterClass.Project:
              parameter = element.GetParameter(parameterKey.Name, parameterKey.Definition?.GetDataType() ?? SpecType.Empty, DBX.ParameterClass.Project); break;
            case DBX.ParameterClass.Family:
              parameter = element.GetParameter(parameterKey.Name, parameterKey.Definition?.GetDataType() ?? SpecType.Empty, DBX.ParameterClass.Family); break;
            case DBX.ParameterClass.Shared:
              parameter = element.get_Parameter(parameterKey.GuidValue.Value); break;
          }
        }
      }

      if (parameter is null)
      {
        switch (key ?? goo.ScriptVariable())
        {
          case string parameterName:
          {
            parameter = element.GetParameter(parameterName, DBX.ParameterClass.Any);
            if (parameter is null) obj.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Parameter '{parameterName}' is not defined in 'Element'");
            break;
          }
          case int parameterId:
          {
            var elementId = new DB.ElementId(parameterId);
            if (elementId.TryGetBuiltInParameter(out var builtInParameter))
            {
              parameter = element.get_Parameter(builtInParameter);
              if (parameter is null) obj.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Parameter '{DB.LabelUtils.GetLabelFor(builtInParameter)}' is not defined in 'Element'");
            }
            else if (element.Document.GetElement(new DB.ElementId(parameterId)) is DB.ParameterElement parameterElement)
            {
              parameter = element.get_Parameter(parameterElement.GetDefinition());
              if (parameter is null) obj.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Parameter '{parameterElement.Name}' is not defined in 'Element'");
            }
            else obj.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Data conversion failed from {goo.TypeName} to Revit Parameter element");
            break;
          }
          case DB.Parameter param:
          {
            parameter = element.get_Parameter(param.Definition);
            if (parameter is null) obj.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Parameter '{param.Definition.Name}' is not defined in 'Element'");
            break;
          }
          case DB.ElementId elementId:
          {
            if (elementId.TryGetBuiltInParameter(out var builtInParameter))
            {
              parameter = element.get_Parameter(builtInParameter);
              if (parameter is null) obj.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Parameter '{DB.LabelUtils.GetLabelFor(builtInParameter)}' is not  defined in 'Element'");
            }
            else if (element.Document.GetElement(elementId) is DB.ParameterElement parameterElement)
            {
              parameter = element.get_Parameter(parameterElement.GetDefinition());
              if (parameter is null) obj.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Parameter '{parameterElement.Name}' is not defined in 'Element'");
            }
            else obj.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Data conversion failed from {goo.TypeName} to Revit Parameter element");
            break;
          }
          case Guid guid:
          {
            parameter = element.get_Parameter(guid);
            if (parameter is null) obj.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Parameter '{guid}' is not defined in 'Element'");
            break;
          }
          default:
          {
            obj.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Data conversion failed from {goo.TypeName} to Revit Parameter element");
            break;
          }
        }
      }

      return parameter;
    }
  }
}

namespace RhinoInside.Revit.GH.Components
{
  using External.DB.Extensions;
  using Grasshopper.Kernel.Parameters;

  public class ElementParameterGet : Component
  {
    public override Guid ComponentGuid => new Guid("D86050F2-C774-49B1-9973-FB3AB188DC94");
    public override GH_Exposure Exposure => GH_Exposure.quarternary;

    public ElementParameterGet() : base
    (
      name: "Get Element Parameter",
      nickname: "GetParam",
      description: "Gets the parameter value of a specified Revit Element",
      category: "Revit",
      subCategory:"Element"
    )
    { }

    protected override void RegisterInputParams(GH_InputParamManager manager)
    {
      manager.AddParameter(new Parameters.Element(), "Element", "E", "Element to query", GH_ParamAccess.item);
      manager.AddGenericParameter("ParameterKey", "K", "Element parameter to query", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager manager)
    {
      manager.AddParameter(new Parameters.ParameterValue(), "ParameterValue", "V", "Element parameter value", GH_ParamAccess.item);
    }

    protected override void TrySolveInstance(IGH_DataAccess DA)
    {
      if (!Params.GetData(DA, "Element", out Types.Element element, x => x.IsValid)) return;
      if (!Params.GetData(DA, "ParameterKey", out IGH_Goo key)) return;

      var parameter = ParameterUtils.GetParameter(this, element.Value, key);
      DA.SetData("ParameterValue", parameter);
    }
  }

  public class ElementParameterSet : TransactionalChainComponent
  {
    public override Guid ComponentGuid => new Guid("8F1EE110-7FDA-49E0-BED4-E8E0227BC021");
    public override GH_Exposure Exposure => GH_Exposure.quarternary;

    public ElementParameterSet() : base
    (
      name: "Set Element Parameter",
      nickname: "SetParam",
      description: "Sets the parameter value of a specified Revit Element",
      category: "Revit",
      subCategory: "Element"
    )
    { }

    protected override ParamDefinition[] Inputs => inputs;
    static readonly ParamDefinition[] inputs =
    {
      new ParamDefinition
      (
        new Parameters.Element()
        {
          Name = "Element",
          NickName = "E",
          Description = "Element to update",
        }
      ),
      new ParamDefinition
      (
        new Param_GenericObject()
        {
          Name = "ParameterKey",
          NickName = "K",
          Description = "Element parameter to modify",
        }
      ),
      new ParamDefinition
      (
        new Param_GenericObject()
        {
          Name = "ParameterValue",
          NickName = "V",
          Description = "Element parameter value",
        }
      ),
    };

    protected override ParamDefinition[] Outputs => outputs;
    static readonly ParamDefinition[] outputs =
    {
      new ParamDefinition
      (
        new Parameters.Element()
        {
          Name = "Element",
          NickName = "E",
          Description = "Updated Element",
        }
      ),
    };

    protected override void TrySolveInstance(IGH_DataAccess DA)
    {
      if (!Params.GetData(DA, "Element", out Types.Element element, x => x.IsValid)) return;
      if (!Params.GetData(DA, "ParameterKey", out IGH_Goo key)) return;
      if (!Params.GetData(DA, "ParameterValue", out IGH_Goo value)) return;

      var parameter = ParameterUtils.GetParameter(this, element.Value, key);
      if (parameter is null)
        return;

      StartTransaction(element.Document);

      if (parameter.Set(value))
        DA.SetData("Element", element);
      else
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unable to set parameter '{parameter.Definition.Name}' : '{value}' is not a valid value.");
    }
  }

  public class ElementParameterReset : TransactionalChainComponent
  {
    public override Guid ComponentGuid => new Guid("2C374E6D-A547-45AC-B77D-04DD61317622");
    public override GH_Exposure Exposure => GH_Exposure.quarternary | GH_Exposure.obscure;
    protected override string IconTag => "R";

    public ElementParameterReset() : base
    (
      name: "Reset Element Parameter",
      nickname: "ResetParam",
      description: "Resets the parameter value of a specified Revit Element",
      category: "Revit",
      subCategory: "Element"
    )
    { }

    protected override ParamDefinition[] Inputs => inputs;
    static readonly ParamDefinition[] inputs =
    {
      new ParamDefinition
      (
        new Parameters.Element()
        {
          Name = "Element",
          NickName = "E",
          Description = "Element to update",
        }
      ),
      new ParamDefinition
      (
        new Param_GenericObject()
        {
          Name = "ParameterKey",
          NickName = "K",
          Description = "Element parameter to modify",
        }
      ),
    };

    protected override ParamDefinition[] Outputs => outputs;
    static readonly ParamDefinition[] outputs =
    {
      new ParamDefinition
      (
        new Parameters.Element()
        {
          Name = "Element",
          NickName = "E",
          Description = "Updated Element",
        }
      ),
    };

    protected override void TrySolveInstance(IGH_DataAccess DA)
    {
      if (!Params.GetData(DA, "Element", out Types.Element element, x => x.IsValid)) return;
      if (!Params.GetData(DA, "ParameterKey", out IGH_Goo key)) return;

      var parameter = ParameterUtils.GetParameter(this, element.Value, key);
      if (parameter is null)
        return;

      StartTransaction(element.Document);

      if (!parameter.ResetValue())
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Unable to reset parameter '{parameter.Definition.Name}'.");

      DA.SetData("Element", element);
    }
  }

  public class QueryElementParameters : Component
  {
    public override Guid ComponentGuid => new Guid("44515A6B-84EE-4DBD-8241-17EDBE07C5B6");
    public override GH_Exposure Exposure => GH_Exposure.quarternary;

    public QueryElementParameters()
    : base("Query Element Parameters", "Parameters", "Get the parameters of the specified Element", "Revit", "Element")
    { }

    protected override void RegisterInputParams(GH_InputParamManager manager)
    {
      manager.AddParameter(new Parameters.Element(), "Element", "E", "Element to query", GH_ParamAccess.item);
      manager[manager.AddTextParameter("Name", "N", "Filter params by Name", GH_ParamAccess.item)].Optional = true;
      manager[manager.AddParameter(new Parameters.Param_Enum<Types.ParameterGroup>(), "Group", "G", "Filter params by the group they belong", GH_ParamAccess.item)].Optional = true;
      manager[manager.AddBooleanParameter("ReadOnly", "R", "Filter params by its ReadOnly property", GH_ParamAccess.item)].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager manager)
    {
      manager.AddParameter(new Parameters.ParameterKey(), "Parameters", "P", "Element parameters", GH_ParamAccess.list);
    }

    protected override void TrySolveInstance(IGH_DataAccess DA)
    {
      if (!Params.GetData(DA, "Element", out Types.Element element, x => x.IsValid)) return;

      var filterName = Params.GetData(DA, "Name", out string parameterName);
      var filterGroup = Params.GetData(DA, "Group", out ParameterGroup parameterGroup);
      var filterReadOnly = Params.GetData(DA, "ReadOnly", out bool? readOnly);

      var parameters = new List<DB.Parameter>(element.Value.Parameters.Size);
      foreach (var group in element.Value.GetParameters(DBX.ParameterClass.Any).GroupBy(x => x.Definition?.GetGroupType() ?? ParameterGroup.Empty).OrderBy(x => x.Key))
      {
        foreach (var param in group.OrderBy(x => x.Id.IntegerValue))
        {
          if (string.IsNullOrEmpty(param.Definition.Name))
            continue;

          if (filterName && !param.Definition.Name.IsSymbolNameLike(parameterName))
            continue;

          if (filterGroup && parameterGroup != (param.Definition?.GetGroupType() ?? ParameterGroup.Empty))
            continue;

          if (filterReadOnly && readOnly != param.IsReadOnly)
            continue;

          parameters.Add(param);
        }
      }

      DA.SetDataList("Parameters", parameters);
    }
  }
}
