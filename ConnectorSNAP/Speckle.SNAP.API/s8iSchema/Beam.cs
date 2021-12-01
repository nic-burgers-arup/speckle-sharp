using System.Collections.Generic;

namespace Speckle.SNAP.API.s8iSchema
{
  public class Beam
  {
    //Beams doesn't seem to have names in this format
    public NodeType NodeIEdgeType { get; set; } = NodeType.SecondaryNode;
    public string NodeIEdgeName { get; set; }
    public NodeType NodeJEdgeType { get; set; } = NodeType.SecondaryNode;
    public string NodeJEdgeName { get; set; }
    public StructureType StructureType { get; set; } = StructureType.ReinforcedConcrete;
    public string CrossSection { get; set; }
    public string FinishingCoating { get; set; }
    public bool RigidJointIEdge { get; set; } = false;
    public bool RigidJointJEdge { get; set; } = false;
    public bool Cantilever { get; set; } = false;
    public string Material { get; set; }
    public string SectionProperties { get; set; }
  }
}
