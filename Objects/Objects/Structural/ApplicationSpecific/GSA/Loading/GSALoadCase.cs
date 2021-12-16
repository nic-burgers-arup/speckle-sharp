using Speckle.Newtonsoft.Json;
using Speckle.Core.Kits;
using Objects.Structural.Loading;

namespace Objects.Structural.GSA.Loading
{
  public class GSALoadCase : LoadCase
  {
    public int? nativeId { get; set; }
    public LoadDirection2D direction { get; set; }
    public string include { get; set; }
    public bool bridge { get; set; }
    public GSALoadCase() { }

    [SchemaInfo("GSALoadCase", "Creates a Speckle structural load case for GSA", "GSA", "Loading")]
    public GSALoadCase(string name, LoadType loadType, string source = null, ActionType actionType = ActionType.None, string description = "notset", string include = "undefined", bool bridge = false, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.loadType = loadType;
      this.group = source;
      this.actionType = actionType;
      this.description = description;
      this.include = include;
      this.bridge = bridge;
    }
  }

}
