using Speckle.GSA.API;
using Speckle.GSA.API.GwaSchema;
using Speckle.GSA.API.GwaSchema.Loading.Beam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.ConnectorGSA.Proxy.GwaParsers.Loading.Beam
{
  [GsaType(GwaKeyword.LOAD_1D_THERMAL, GwaSetCommandType.SetAt, true, GwaKeyword.LOAD_TITLE, GwaKeyword.EL, GwaKeyword.MEMB)]
  public class GsaLoad1dThermalParser : GwaParser<GsaLoad1dThermal>
  {
    public GsaLoad1dThermalParser(GsaLoad1dThermal gsaLoad1dThermal) : base(gsaLoad1dThermal) { }
    public GsaLoad1dThermalParser() : base(new GsaLoad1dThermal()) { }

    public override bool FromGwa(string gwa)
    {
      if (!BasicFromGwa(gwa, out var remainingItems))
      {
        return false;
      }
      var items = remainingItems;

      //LOAD_1D_THERMAL.2 | name | list | case | type | values(n)
      //LOAD_1D_THERMAL.2 | name | list | case | type | value
      //LOAD_1D_THERMAL.2 | name | item | case | type | pos_1 | value_1 | pos_2 | value_2
      if (!FromGwaByFuncs(items, out remainingItems, AddName, (v) => AddEntities(v, out record.MemberIndices, out record.ElementIndices),
        (v) => AddNullableIndex(v, out record.LoadCaseIndex), AddType))
      {
        return false;
      }
      items = remainingItems;

      if (items.Count() > 0)
      {
        record.Values = new List<double>();
        foreach (var item in items)
        {
          if (double.TryParse(item, out double v))
          {
            record.Values.Add(v);
          }
          else
          {
            return false;
          }
        }
      }
      return true;
    }

    public override bool Gwa(out List<string> gwa, bool includeSet = false)
    {
      if (!InitialiseGwa(includeSet, out var items))
      {
        gwa = new List<string>();
        return false;
      }

      //LOAD_2D_THERMAL.2 | name | list | case | type | values(n)
      AddItems(ref items, record.Name, AddEntities(record.MemberIndices, record.ElementIndices), record.LoadCaseIndex ?? 0, record.Type.GetStringValue(), AddValues());

      gwa = (Join(items, out var gwaLine)) ? new List<string>() { gwaLine } : new List<string>();
      return gwa.Count() > 0;
    }


    #region to_gwa_fns

    private string AddValues()
    {
      if (record.Values != null && record.Values.Count() > 0)
      {
        return string.Join("\t", record.Values);
      }
      else
      {
        return "";
      }
    }

    private string AddType()
    {
      switch (record.Type)
      {
        case Load1dThermalType.Uniform:
          return "CONS";
        case Load1dThermalType.GradientInY:
          return "DY";
        case Load1dThermalType.GradientInZ:
          return "DZ";
        default: return "";
      }
    }

    #endregion

    #region from_gwa_functions

    protected bool AddName(string v)
    {
      record.Name = (string.IsNullOrEmpty(v)) ? null : v;
      return true;
    }

    private bool AddType(string v)
    {
      switch (v)
      {
        case "CONS":
          record.Type = Load1dThermalType.Uniform;
          return true;
        case "DY":
          record.Type = Load1dThermalType.GradientInY;
          return true;
        case "DZ":
          record.Type = Load1dThermalType.GradientInZ;
          return true;
        default:
          return false;
      }
    }


    #endregion
  }
}
