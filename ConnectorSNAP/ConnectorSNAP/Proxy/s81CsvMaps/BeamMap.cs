using CsvHelper.Configuration;
using Speckle.SNAP.API;
using Speckle.SNAP.API.s8iSchema;
using System;
using System.Linq;

namespace ConnectorSNAP.Proxy.s81CsvMaps
{
  internal sealed class BeamMap : ClassMap<Beam>
  {
    public BeamMap()
    {
      //Reading
      Map(m => m.NodeIEdgeType).Index(0).Convert(r => (r.Row.GetField(0).Split('/').Last().Trim() == "0") ? NodeType.SecondaryNode : NodeType.Node);
      Map(m => m.NodeIEdgeName).Index(1);
      Map(m => m.NodeJEdgeType).Index(2).Convert(r => (r.Row.GetField(2) == "0") ? NodeType.SecondaryNode : NodeType.Node);
      Map(m => m.NodeJEdgeName).Index(3);
      Map(m => m.StructureType).Index(4).TypeConverter<EnumIntConverter<StructureType>>();
      Map(m => m.CrossSection).Index(5);
      Map(m => m.FinishingCoating).Index(6);
      Map(m => m.RigidJointIEdge).Index(7).TypeConverter<IntBoolConverter>();
      Map(m => m.RigidJointJEdge).Index(8).TypeConverter<IntBoolConverter>();
      Map(m => m.Cantilever).Index(9).TypeConverter<IntBoolConverter>();
      Map(m => m.Material).Index(10);
      Map(m => m.SectionProperties).Index(11);

      //Writing
      Map(m => m.NodeIEdgeType).Convert(o => Keyword.BM.GetStringValue() + " / " + (int)o.Value.NodeIEdgeType);
      Map(m => m.NodeJEdgeType).Convert(o => ((int)o.Value.NodeJEdgeType).ToString());
    }
  }
}
