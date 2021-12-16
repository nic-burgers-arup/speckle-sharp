using Speckle.Core.Models;
using System.Collections.Generic;
using Objects.Structural.Geometry;
using Objects.Structural.Loading;
using Speckle.Core.Kits;

namespace Objects.Structural.GSA.Loading
{
  public class GSALoadThermal2d : Load
  {
    public int? nativeId { get; set; }

    [DetachProperty]
    [Chunkable(5000)]
    public List<Element2D> elements { get; set; }
    public Thermal2dLoadType type { get; set; }
    public List<double> values { get; set; }
    public GSALoadThermal2d() { }

    [SchemaInfo("GSALoadThermal2d ", "Creates a Speckle 2d thermal load for GSA", "GSA", "Loading")]
    public GSALoadThermal2d(LoadCase loadCase, List<Element2D> elements, Thermal2dLoadType type, List<double> values, string name = null, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.loadCase = loadCase;
      this.elements = elements;
      this.type = type;
      this.values = values;
    }
  }

  public enum Thermal2dLoadType
  {
    NotSet = 0,
    Uniform,
    Gradient,
    General
  }
}
