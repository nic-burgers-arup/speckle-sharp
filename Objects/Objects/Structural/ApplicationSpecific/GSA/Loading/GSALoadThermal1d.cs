using Objects.Structural.Geometry;
using Objects.Structural.Loading;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objects.Structural.ApplicationSpecific.GSA.Loading
{
  public class GSALoadThermal1d : Load
  {
    public int? nativeId { get; set; }
    [DetachProperty]
    [Chunkable(5000)]
    public List<Element1D> elements { get; set; }
    public Thermal1dLoadType type { get; set; }
    public List<double> values { get; set; }

    public GSALoadThermal1d() { }

    [SchemaInfo("GSALoadThermal1d ", "Creates a Speckle 1d thermal load for GSA", "GSA", "Loading")]
    public GSALoadThermal1d(LoadCase loadCase, List<Element1D> elements, Thermal1dLoadType type, List<double> values, string name = null, int? nativeId = null )
    {
      this.nativeId = nativeId;
      this.name = name;
      this.loadCase = loadCase;
      this.elements = elements;
      this.type = type;
      this.values = values;
    }
    
  }

  public enum Thermal1dLoadType
  {
    NotSet = 0,
    Uniform,
    GradientInY,
    GradientInZ
  }
}
