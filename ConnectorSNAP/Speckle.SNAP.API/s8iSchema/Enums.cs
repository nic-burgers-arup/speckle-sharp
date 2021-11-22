using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.SNAP.API.s8iSchema
{
  public enum NodeType
  {
    Node = 1,
    SecondaryNode = 0
  }

  public enum StructureType
  {
    [StringValue("RC/SRC")]
    ReinforcedConcrete = 0,
    [StringValue("S")]
    Steel = 1,
    [StringValue("Wood")]
    Wood = 2,
    [StringValue("Other")]
    Other = 3
  }
}
