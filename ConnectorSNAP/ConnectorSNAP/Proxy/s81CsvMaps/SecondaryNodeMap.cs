using CsvHelper.Configuration;
using Speckle.SNAP.API;
using Speckle.SNAP.API.s8iSchema;
using System;
using System.Linq;

namespace ConnectorSNAP.Proxy.s81CsvMaps
{
  internal sealed class SecondaryNodeMap : ClassMap<SecondaryNode>
  {
    public SecondaryNodeMap()
    {
      //Reading
      Map(m => m.Name).Index(0).Convert(r => r.Row.GetField(0).Split('/').Last().Trim());
      Map(m => m.X).Index(1);
      Map(m => m.Y).Index(2);
      Map(m => m.Z).Index(3);

      //Writing
      Map(m => m.Name).Convert(o => Keyword.SN.GetStringValue() + " / " + o.Value.Name);
    }
  }
}
