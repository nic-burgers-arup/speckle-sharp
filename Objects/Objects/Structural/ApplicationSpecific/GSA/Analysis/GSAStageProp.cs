using Speckle.Newtonsoft.Json;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using Objects.Structural.Properties;

namespace Objects.Structural.GSA.Analysis
{
  public class GSAStageProp : Base
  {
    public int? nativeId { get; set; }

    [DetachProperty]
    public GSAStage stage { get; set; }
    public PropertyType type { get; set; }

    [DetachProperty]
    public Property elementProperty { get; set; }

    [DetachProperty]
    public Property stageProperty { get; set; }

    public GSAStageProp() { }

    [SchemaInfo("GSAStageProp", "Creates a Speckle structural analysis stage property for GSA", "GSA", "Analysis")]
    public GSAStageProp(GSAStage stage, Property elementProperty, Property stageProperty, PropertyType type = PropertyType.Beam, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.stage = stage;
      this.elementProperty = elementProperty;
      this.stageProperty = stageProperty;
      this.type = type;
    }
  }

  public enum PropertyType
  {
    Beam = 0,
    Spring,
    Mass,
    TwoD,
    //Link,
    //Cable,
    //ThreeD,
    //Damper
  }
}
