using CsvHelper.Configuration;
using Speckle.SNAP.API;
using Speckle.SNAP.API.s8iSchema;
using System;
using System.Linq;


namespace ConnectorSNAP.Proxy.s81CsvMaps
{
  internal sealed class MaterialSteelMap : ClassMap<MaterialSteel>
  {
    public MaterialSteelMap()
    {
      //Reading
      Map(m => m.Name).Index(0).Convert(r => r.Row.GetField(0).Split('/').Last().Trim());

      //Writing
      Map(m => m.Name).Convert(o => Keyword.MS.GetStringValue() + " / " + o.Value.Name);
      Map().Constant("0,,0,");
    }
  }
}