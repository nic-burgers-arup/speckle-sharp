using Objects.Geometry;
using Objects.Structural.Geometry;
using Objects.Structural.Loading;
using Objects.Structural.GSA.Geometry;
using Speckle.Core.Kits;

namespace Objects.Structural.GSA.Loading
{
  public class GSALoadGridArea : GSALoadGrid
  {
    public Polyline polyline { get; set; }
    public bool isProjected { get; set; }
    public double value { get; set; }
    public GSALoadGridArea() { }

    [SchemaInfo("GSALoadGridArea", "Creates a Speckle structural grid area load for GSA", "GSA", "Loading")]
    public GSALoadGridArea(int nativeId, GSAGridSurface gridSurface, Axis loadAxis, LoadDirection2D direction, Polyline polyline, bool isProjected, double value)
    {
      this.name = name;
      this.loadCase = loadCase;
      this.nativeId = nativeId;
      this.gridSurface = gridSurface;
      this.loadAxis = loadAxis;
      this.direction = direction;
      this.polyline = polyline;
      this.isProjected = isProjected;
      this.value = value;
    }
  }
}
