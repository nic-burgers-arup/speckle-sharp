using Grasshopper.Kernel.Types;
using Objects.Geometry;
using Objects.Primitive;
using Rhino.Geometry;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry.Collections;
using Speckle.Core.Models;
using Speckle.Core.Kits;
using System;
using System.Collections.Generic;
using System.Linq;
using Node = Objects.Structural.Geometry.Node;
using Element1D = Objects.Structural.Geometry.Element1D;
using Element2D = Objects.Structural.Geometry.Element2D;
using Element3D = Objects.Structural.Geometry.Element3D;
using Column = Objects.BuiltElements.Column;
using Beam = Objects.BuiltElements.Beam;
using Wall = Objects.BuiltElements.Wall;
using Floor = Objects.BuiltElements.Floor;
using Ceiling = Objects.BuiltElements.Ceiling;
using Roof = Objects.BuiltElements.Roof;
using Opening = Objects.BuiltElements.Opening;
using Point = Objects.Geometry.Point;
using Mesh = Objects.Geometry.Mesh;
using View3D = Objects.BuiltElements.View3D;
using RH = Rhino.Geometry;
using RV = Objects.BuiltElements.Revit;

namespace Objects.Converter.RhinoGh
{
  public partial class ConverterRhinoGh
  {
    public Node PointToSpeckleNode(RH.Point point)
    {
      return new Node((Point)ConvertToSpeckle(point), null, null, null) { units = ModelUnits };
    }

    public Element1D CurveToSpeckleElement1D(RH.Curve curve)
    {
      return new Element1D((ICurve)ConvertToSpeckle(curve)) { units = ModelUnits };
    }

    public Element2D MeshToSpeckleElement2D(RH.Mesh mesh)
    {
      var outlines = mesh.GetOutlines(RH.Plane.WorldXY);

      var nodes = new List<Node>();
      var edges = mesh.GetNakedEdges().ToList();
      var outerEdgeArea = edges.Max(x => AreaMassProperties.Compute(x.ToPolylineCurve()).Area);
      var outerEdge = edges.First(x => AreaMassProperties.Compute(x.ToPolylineCurve()).Area == outerEdgeArea);
      foreach (var point in outerEdge)
      {
        var pt = new RH.Point(point);
        nodes.Add(PointToSpeckleNode(pt));
      }

      var voids = new List<List<Node>>();
      edges.Remove(outerEdge);
      foreach(var edge in edges)
      {
        var voidLoop = new List<Node>();
        foreach (var point in edge)
        {
          var pt = new RH.Point(point);
          voidLoop.Add(PointToSpeckleNode(pt));
        }
        voids.Add(voidLoop);
      }
      return new Element2D(nodes, voids) { units = ModelUnits };
    }

    public Element3D MeshToSpeckleElement3D(RH.Mesh mesh)
    {
      return new Element3D((Mesh)ConvertToSpeckle(mesh)) { units = ModelUnits };
    }
  }
}