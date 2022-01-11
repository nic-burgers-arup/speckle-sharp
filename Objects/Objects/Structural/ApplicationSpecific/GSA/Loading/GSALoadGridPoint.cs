using Objects.Geometry;
using Objects.Structural.Geometry;
using Objects.Structural.Loading;
using Objects.Structural.GSA.Geometry;
using Speckle.Core.Kits;

namespace Objects.Structural.GSA.Loading
{
  public class GSALoadGridPoint : GSALoadGrid
  {
    public Point position { get; set; }
    public double value { get; set; }
    public GSALoadGridPoint() { }

    [SchemaInfo("GSALoadGridPoint", "Creates a Speckle structural grid point load for GSA", "GSA", "Loading")]
    public GSALoadGridPoint(LoadCase loadCase, GSAGridSurface gridSurface, Axis loadAxis, LoadDirection2D direction, Point position, double value, string name = null, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.loadCase = loadCase;
      this.gridSurface = gridSurface;
      this.loadAxis = loadAxis;
      this.direction = direction;
      this.position = position;
      this.value = value;
    }
  }
}
