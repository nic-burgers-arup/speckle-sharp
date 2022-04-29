using CsvHelper.Configuration;
using Speckle.SNAP.API;
using Speckle.SNAP.API.s8iSchema;
using System;
using System.Linq;

namespace ConnectorSNAP.Proxy.s81CsvMaps
{
  internal class NodeMap : ClassMap<Node>
  {
    public NodeMap()
    {
      //Reading
      Map(m => m.Name).Index(0).Convert(r => r.Row.GetField(0).Split('/').Last().Trim());
      Map(m => m.X).Index(1);
      Map(m => m.Y).Index(2);
      Map(m => m.Z).Index(3);
      Map(m => m.AdditionalMass).Index(4);
      Map(m => m.Mass).Index(5);
      Map(m => m.Restraint).Index(6);
      Map(m => m.AxisX).Index(7);
      Map(m => m.AxisY).Index(8);
      Map(m => m.AxisZ).Index(9);
      Map(m => m.HistoryOutput).Index(10).TypeConverter<IntBoolConverter>();
      Map(m => m.Output).Index(11).TypeConverter<IntBoolConverter>();

      //Writing
      Map(m => m.Name).Convert(o => Keyword.ND.GetStringValue() + " / " + o.Value.Name);
    }
  }
}
