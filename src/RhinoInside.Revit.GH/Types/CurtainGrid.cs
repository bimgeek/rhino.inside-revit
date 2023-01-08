using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using ARDB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Types
{
  using Convert.Geometry;
  using Eto.Forms;
  using External.DB;
  using External.DB.Extensions;

  [Kernel.Attributes.Name("Curtain Grid")]
  public class CurtainGrid : DocumentObject,
    IGH_GeometricGoo,
    IGH_PreviewData,
    IHostObjectAccess
  {
    public new ARDB.CurtainGrid Value => base.Value as ARDB.CurtainGrid;
    public HostObject Host { get; private set; }
    int GridIndex = -1;

    public CurtainGrid() : base() { }
    public CurtainGrid(HostObject host, ARDB.CurtainGrid value, int gridIndex) : base(host.Document, value)
    {
      Host = host;
      GridIndex = gridIndex;
    }

    #region DocumentObject
    public override string DisplayName
    {
      get
      {
        if (Value is ARDB.CurtainGrid grid)
          return $"Curtain Grid [{grid.NumVLines + 1} x {grid.NumULines + 1}]";

        return "Curtain Grid";
      }
    }

    protected override void ResetValue()
    {
      clippingBox = default;
      curves = default;

      base.ResetValue();
    }
    #endregion

    #region IGH_Goo
    public override bool CastTo<Q>(out Q target)
    {
      target = default;

      if (typeof(Q).IsAssignableFrom(typeof(GH_Plane)))
      {
        target = (Q) (object) new GH_Plane(Location);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Mesh)))
      {
        var mesh = Mesh;
        if (mesh is null) return false;

        target = (Q) (object) new GH_Mesh(mesh);
        return target is object;
      }

      //if (typeof(Q).IsAssignableFrom(typeof(GH_Surface)))
      //{
      //  var trimmedSurface = TrimmedSurface;
      //  if (trimmedSurface is null) return false;

      //  target = (Q) (object) new GH_Surface(trimmedSurface);
      //  return target is object;
      //}

      if (typeof(Q).IsAssignableFrom(typeof(GH_Brep)))
      {
        var polySurface = PolySurface;
        if (polySurface is null) return false;

        target = (Q) (object) new GH_Brep(polySurface);
        return target is object;
      }

      return base.CastTo(out target);
    }
    #endregion

    #region IGH_GeometricGoo
    BoundingBox IGH_GeometricGoo.Boundingbox => GetBoundingBox(Transform.Identity);
    Guid IGH_GeometricGoo.ReferenceID
    {
      get => Guid.Empty;
      set { if (value != Guid.Empty) throw new InvalidOperationException(); }
    }

    bool IGH_GeometricGoo.IsReferencedGeometry => false;
    bool IGH_GeometricGoo.IsGeometryLoaded => Value is object;
    IGH_GeometricGoo IGH_GeometricGoo.DuplicateGeometry() => default;
    public BoundingBox GetBoundingBox(Transform xform)
    {
      var bbox = BoundingBox.Empty;
      foreach (var curve in Curves)
        bbox.Union(curve.GetBoundingBox(xform));

      return bbox;
    }

    IGH_GeometricGoo IGH_GeometricGoo.Transform(Transform xform) => default;
    IGH_GeometricGoo IGH_GeometricGoo.Morph(SpaceMorph xmorph) => default;
    bool IGH_GeometricGoo.LoadGeometry() => false;
    bool IGH_GeometricGoo.LoadGeometry(RhinoDoc doc) => false;
    void IGH_GeometricGoo.ClearCaches() => ResetValue();
    #endregion

    #region IGH_PreviewData
    void IGH_PreviewData.DrawViewportWires(GH_PreviewWireArgs args)
    {
      foreach (var curve in Curves)
        args.Pipeline.DrawCurve(curve, args.Color, args.Thickness);
    }

    void IGH_PreviewData.DrawViewportMeshes(GH_PreviewMeshArgs args) { }

    private BoundingBox? clippingBox;
    BoundingBox IGH_PreviewData.ClippingBox
    {
      get
      {
        if (!clippingBox.HasValue)
        {
          clippingBox = BoundingBox.Empty;
          foreach (var curve in Curves)
            clippingBox.Value.Union(curve.GetBoundingBox(false));
        }

        return clippingBox.Value;
      }
    }
    #endregion

    #region Implementation
    static IEnumerable<ARDB.CurtainGrid> HostCurtainGrids(ARDB.HostObject host)
    {
      var grids = default(IEnumerable<ARDB.CurtainGrid>);
      switch (host)
      {
        case ARDB.CurtainSystem curtainSystem: grids = curtainSystem.CurtainGrids?.Cast<ARDB.CurtainGrid>(); break;
        case ARDB.ExtrusionRoof extrusionRoof: grids = extrusionRoof.CurtainGrids?.Cast<ARDB.CurtainGrid>(); break;
        case ARDB.FootPrintRoof footPrintRoof: grids = footPrintRoof.CurtainGrids?.Cast<ARDB.CurtainGrid>(); break;
        case ARDB.Wall wall: grids = wall.CurtainGrid is null ? null : Enumerable.Repeat(wall.CurtainGrid, 1); break;
      }

      return grids;
    }

    static IList<ARDB.Reference> GetFaceReferences(ARDB.HostObject host)
    {
      var references = new List<ARDB.Reference>();

      try { references.AddRange(ARDB.HostObjectUtils.GetBottomFaces(host)); }
      catch (Autodesk.Revit.Exceptions.ApplicationException) { }
      try { references.AddRange(ARDB.HostObjectUtils.GetTopFaces(host)); }
      catch (Autodesk.Revit.Exceptions.ApplicationException) { }
      try { references.AddRange(ARDB.HostObjectUtils.GetSideFaces(host, ARDB.ShellLayerType.Interior)); }
      catch (Autodesk.Revit.Exceptions.ApplicationException) { }
      try { references.AddRange(ARDB.HostObjectUtils.GetSideFaces(host, ARDB.ShellLayerType.Exterior)); }
      catch (Autodesk.Revit.Exceptions.ApplicationException) { }

      return references;
    }

    static bool IsCurtainGridOnFace(ICollection<ARDB.CurtainCell> cells, ARDB.Face face)
    {
      var result = cells.Count > 0;

      var tol = GeometryTolerance.Internal;
      foreach (var cell in cells)
      {
        foreach (var loop in cell.CurveLoops.Cast<ARDB.CurveArray>())
        {
          foreach (var curve in loop.Cast<ARDB.Curve>())
          {
            var center = curve.Evaluate(0.5, true);
            var distance = face.Project(center).Distance;
            if (distance > tol.VertexTolerance)
              return false;
          }
        }
      }

      return result;
    }

    static ARDB.Reference FindReference(ARDB.HostObject host, ARDB.CurtainGrid value)
    {
      if (host is ARDB.Wall wall)
        return new ARDB.Reference(wall);

      var cells = value.GetCurtainCells();
      foreach (var reference in GetFaceReferences(host))
      {
        if (host.GetGeometryObjectFromReference(reference) is ARDB.Face face && IsCurtainGridOnFace(cells, face))
          return reference;
      }

      return default;
    }

    ARDB.CurtainGrid FindCurtainGrid(ARDB.HostObject host, ARDB.Reference reference)
    {
      if (host is ARDB.Wall wall)
      {
        return wall.CurtainGrid;
      }
      else
      {
        if
        (
          reference.ElementReferenceType == ARDB.ElementReferenceType.REFERENCE_TYPE_SURFACE &&
          host.GetGeometryObjectFromReference(reference) is ARDB.Face face &&
          HostCurtainGrids(host) is IEnumerable<ARDB.CurtainGrid> grids
        )
        {
          foreach (var grid in grids)
          {
            if (IsCurtainGridOnFace(grid.GetCurtainCells(), face))
              return grid;
          }
        }
      }

      return default;
    }
    #endregion

    #region Geometry
    /// <summary>
    /// <see cref="Rhino.Geometry.Plane"/> where this element is located.
    /// </summary>
    public Plane Location
    {
      get
      {
        if (TrimmedSurface is Brep trimmedSurface && trimmedSurface.Faces.Count == 1)
        {
          var surface = trimmedSurface.Faces[0];
          if (surface.FrameAt(surface.Domain(0).Mid, surface.Domain(1).Mid, out var location))
            return location;
        }

        return NaN.Plane;
      }
    }

    static readonly PolyCurve[] EmptyCurves = new PolyCurve[0];

    PolyCurve[] planarCurves;
    public PolyCurve[] PlanarCurves
    {
      get
      {
        if (planarCurves is null)
        {
          planarCurves = Value is ARDB.CurtainGrid grid ?
            planarCurves = grid.GetCurtainCells().SelectMany
            (
              x =>
              {
                try { return x.PlanarizedCurveLoops.ToArray(GeometryDecoder.ToPolyCurve); }
                catch { return EmptyCurves; }
              }
            ).
            ToArray() :
            EmptyCurves;
        }

        return planarCurves;
      }
    }

    PolyCurve[] curves;
    public PolyCurve[] Curves
    {
      get
      {
        if (curves is null)
        {
          curves = Value is ARDB.CurtainGrid grid ?
            grid.GetCurtainCells().SelectMany
            (
              x =>
              {
                try { return x.CurveLoops.ToArray(GeometryDecoder.ToPolyCurve); }
                catch { return EmptyCurves; }
              }
            ).
            ToArray():
            EmptyCurves;
        }

        return curves;
      }
    }

    public Brep TrimmedSurface
    {
      get
      {
        if (Value is ARDB.CurtainGrid curtainGrid)
        {
          var shells = new List<Brep>();
          using (Document.RollBackScope())
          {
            // Looks like an additional scope is necessary here to avoid a memory corruption!?!?
            using (Document.RollBackScope())
            {
              // We apply a type that has spacing layout set to None to generate just one Cell
              {
                var type = Host.Type.Value;
                type = type.Duplicate(Guid.NewGuid().ToString()) as ARDB.HostObjAttributes;
                if (type is ARDB.WallType)
                  type.get_Parameter(ARDB.BuiltInParameter.AUTO_PANEL_WALL).Update(ElementIdExtension.InvalidElementId);
                else
                  type.get_Parameter(ARDB.BuiltInParameter.AUTO_PANEL).Update(ElementIdExtension.InvalidElementId);
                type.get_Parameter(ARDB.BuiltInParameter.SPACING_LAYOUT_U).Update(0);
                type.get_Parameter(ARDB.BuiltInParameter.SPACING_LAYOUT_V).Update(0);
                type.get_Parameter(ARDB.BuiltInParameter.AUTO_MULLION_BORDER1_VERT)?.Update(ElementIdExtension.InvalidElementId);
                type.get_Parameter(ARDB.BuiltInParameter.AUTO_MULLION_BORDER2_VERT)?.Update(ElementIdExtension.InvalidElementId);
                type.get_Parameter(ARDB.BuiltInParameter.AUTO_MULLION_BORDER1_HORIZ)?.Update(ElementIdExtension.InvalidElementId);
                type.get_Parameter(ARDB.BuiltInParameter.AUTO_MULLION_BORDER2_HORIZ)?.Update(ElementIdExtension.InvalidElementId);
                Host.Type = ElementType.FromValue(type) as ElementType;
              }

              var linesToDelete = new List<ARDB.ElementId>(curtainGrid.NumULines * curtainGrid.NumVLines);
              foreach (var uId in curtainGrid.GetUGridLineIds())
              {
                var u = Document.GetElement(uId) as ARDB.CurtainGridLine;
                u.get_Parameter(ARDB.BuiltInParameter.GRIDLINE_SPEC_STATUS)?.Update(0);
                u.Lock = false;
                linesToDelete.Add(uId);
              }

              foreach (var vId in curtainGrid.GetVGridLineIds())
              {
                var v = Document.GetElement(vId) as ARDB.CurtainGridLine;
                v.get_Parameter(ARDB.BuiltInParameter.GRIDLINE_SPEC_STATUS)?.Update(0);
                v.Lock = false;
                linesToDelete.Add(vId);
              }

              Document.Delete(linesToDelete);
              Document.Regenerate();

              var identityGrid = (Host as ICurtainGridsAccess).CurtainGrids[GridIndex];
              foreach (var cell in identityGrid.CurtainCells)
              {
                if (cell.PolySurface is Brep brep)
                  shells.Add(brep);
              }
            }
          }

          return shells.Count == 1 ? shells[0] : Brep.MergeBreps(shells, RhinoMath.UnsetValue);
        }

        return default;
      }
    }

    public Brep PolySurface
    {
      get
      {
        if (Value is ARDB.CurtainGrid curtainGrid)
        {
          var cells = CurtainCells.Select(x => x.PolySurface).OfType<Brep>().ToArray();
          var merged = Brep.MergeBreps(cells, RhinoMath.UnsetValue);

          if (merged?.IsValid is false)
            merged.Repair(GeometryDecoder.Tolerance.VertexTolerance);

          return merged;
        }

        return default;
      }
    }

    public Mesh Mesh
    {
      get
      {
        if (CurtainCells is IEnumerable<CurtainCell> curtainCells)
        {
          var mesh = new Mesh();
          mesh.Append(curtainCells.Select(x => x.Mesh).OfType<Mesh>());
          return mesh;
        }

        return default;
      }
    }
    #endregion

    #region Properties
    public IEnumerable<CurtainGridLine> UGridLines => Value is ARDB.CurtainGrid grid ?
      grid.GetUGridLineIds().Select(x => CurtainGridLine.FromElementId(Document, x) as Types.CurtainGridLine) :
      default;

    public IEnumerable<CurtainGridLine> VGridLines => Value is ARDB.CurtainGrid grid ?
      grid.GetVGridLineIds().Select(x => CurtainGridLine.FromElementId(Document, x) as Types.CurtainGridLine) :
      default;

    public IEnumerable<CurtainCell> CurtainCells => Value is ARDB.CurtainGrid grid ?
      grid.GetCurtainCells().Zip(grid.GetPanelIds(), (Cell, PanelId) => (Cell, PanelId)).
      Select(x => new CurtainCell(InstanceElement.FromElementId(Document, x.PanelId) as InstanceElement, x.Cell)) :
      default;
    #endregion
  }

  [Kernel.Attributes.Name("Curtain Cell")]
  public class CurtainCell : DocumentObject
  {
    public new ARDB.CurtainCell Value => base.Value as ARDB.CurtainCell;
    public InstanceElement Panel { get; private set; }

    public CurtainCell() : base() { }
    public CurtainCell(Types.InstanceElement panel, ARDB.CurtainCell value) : base(panel.Document, value)
    {
      Panel = panel;
    }

    #region DocumentObject
    public override string DisplayName =>  "Curtain Cell";
    #endregion

    #region IGH_Goo
    public override bool CastTo<Q>(out Q target)
    {
      target = default;

      if (typeof(Q).IsAssignableFrom(typeof(GH_Plane)))
      {
        target = (Q) (object) new GH_Plane(Location);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Mesh)))
      {
        var mesh = Mesh;
        if (mesh is null) return false;

        target = (Q) (object) new GH_Mesh(mesh);
        return target is object;
      }

      //if (typeof(Q).IsAssignableFrom(typeof(GH_Surface)))
      //{
      //  var trimmedSurface = TrimmedSurface;
      //  if (trimmedSurface is null) return false;

      //  target = (Q) (object) new GH_Surface(trimmedSurface);
      //  return target is object;
      //}

      if (typeof(Q).IsAssignableFrom(typeof(GH_Brep)))
      {
        var polySurface = PolySurface;
        if (polySurface is null) return false;

        target = (Q) (object) new GH_Brep(polySurface);
        return target is object;
      }

      return base.CastTo(out target);
    }
    #endregion

    #region Geometry
    /// <summary>
    /// <see cref="Rhino.Geometry.Plane"/> where this element is located.
    /// </summary>
    public Plane Location
    {
      get
      {
        if (Panel is InstanceElement panel)
        {
          var location = panel.Location;
          location = new Plane(location.Origin, location.XAxis, Vector3d.CrossProduct(location.XAxis, location.YAxis));
          return location;
        }

        return NaN.Plane;
      }
    }

    static readonly PolyCurve[] EmptyCurves = new PolyCurve[0];

    PolyCurve[] planarCurves;
    public PolyCurve[] PlanarCurves
    {
      get
      {
        if (planarCurves is null && Value is ARDB.CurtainCell cell)
        {
          try { planarCurves = cell.PlanarizedCurveLoops.ToArray(GeometryDecoder.ToPolyCurve); }
          catch { planarCurves = EmptyCurves; }
        }

        return planarCurves;
      }
    }

    public Surface Surface
    {
      get
      {
        if (PlanarCurves is PolyCurve[] planarCurves)
        {
          var plane = Location;
          var bbox = BoundingBox.Empty;
          var curves = new List<Curve>();
          foreach (var loop in planarCurves)
          {
            var curve = Curve.ProjectToPlane(loop, plane);
            bbox.Union(curve.GetBoundingBox(plane));
            curves.Add(curve);
          }

          if (bbox.IsValid)
            return new PlaneSurface(plane, new Interval(bbox.Min.X, bbox.Max.X), new Interval(bbox.Min.Y, bbox.Max.Y));
        }

        return default;
      }
    }

    public Brep TrimmedSurface => Brep.CreateFromSurface(Surface);

    public Brep PolySurface
    {
      get
      {
        if (PlanarCurves is PolyCurve[] planarCurves && Surface is Surface surface)
          return surface.CreateTrimmedSurface(planarCurves, GeometryDecoder.Tolerance.VertexTolerance);

        return default;
      }
    }

    public Mesh Mesh
    {
      get
      {
        if (PlanarCurves is Curve[] planarCurves && planarCurves.Length > 0)
        {
          using (var mp = new MeshingParameters(0.0, GeometryTolerance.Model.ShortCurveTolerance)
          {
            Tolerance = GeometryTolerance.Model.VertexTolerance,
            SimplePlanes = true,
            JaggedSeams = true,
            RefineGrid = false,
            GridMinCount = 1,
            GridMaxCount = 1,
            GridAspectRatio = 0
          })
          {
            var mesh = new Mesh();
            var breps = Brep.CreatePlanarBreps(planarCurves, GeometryTolerance.Model.VertexTolerance);
            mesh.Append(breps.SelectMany
            (
              x =>
              {
                var faces = Mesh.CreateFromBrep(x, mp);
                x.Dispose();

                foreach(var face in faces)
                  face.MergeAllCoplanarFaces(GeometryTolerance.Model.VertexTolerance, GeometryTolerance.Model.AngleTolerance);

                return faces;
              })
            );

            return mesh;
          }
        }

        return default;
      }
    }
    #endregion
  }
}
