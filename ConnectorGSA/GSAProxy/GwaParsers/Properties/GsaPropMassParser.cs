using Speckle.GSA.API;
using System.Collections.Generic;
using System.Linq;
using Speckle.GSA.API.GwaSchema;

namespace Speckle.ConnectorGSA.Proxy.GwaParsers
{
  [GsaType(GwaKeyword.PROP_MASS, GwaSetCommandType.Set, true)]
  public class GsaPropMassParser : GwaParser<GsaPropMass>
  {
    public GsaPropMassParser(GsaPropMass gsaPropMass) : base(gsaPropMass) { }

    public GsaPropMassParser() : base(new GsaPropMass()) { }

    public override bool FromGwa(string gwa)
    {
      if (!BasicFromGwa(gwa, out var remainingItems))
      {
        return false;
      }
      var items = remainingItems;
      FromGwaByFuncs(items, out remainingItems, AddName);

      items = remainingItems.Skip(1).ToList();  //Skip colour

      //PROP_MASS.3 | num | name | colour | mass | Ixx | Iyy | Izz | Ixy | Iyz | Izx | mod { | mod_x | mod_y | mod_z }
      //Zero values are valid for origin, but not for vectors below
      if (!FromGwaByFuncs(items, out remainingItems, (v) => double.TryParse(v, out record.Mass),
        (v) => double.TryParse(v, out record.Ixx), (v) => double.TryParse(v, out record.Iyy), (v) => double.TryParse(v, out record.Izz),
        (v) => double.TryParse(v, out record.Ixy), (v) => double.TryParse(v, out record.Iyz), (v) => double.TryParse(v, out record.Izx),
        (v) => v.TryParseStringValue(out record.Mod)))
      {
        return false;
      }

      if (record.Mod == MassModification.Modified)
      {
        if (!FromGwaByFuncs(remainingItems, out remainingItems, (v) => AddModifier(v, out record.ModX), 
          (v) => AddModifier(v, out record.ModY), (v) => AddModifier(v, out record.ModZ)))
        {
          return false;
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

      //PROP_MASS.3 | num | name | colour | mass | Ixx | Iyy | Izz | Ixy | Iyz | Izx | mod { | mod_x | mod_y | mod_z }
      AddItems(ref items, record.Name, "NO_RGB", record.Mass, record.Ixx, record.Iyy, record.Izz, record.Ixy, record.Iyz, record.Izx,
        record.Mod.GetStringValue());
      if (record.Mod == MassModification.Modified)
      {
        AddItems(ref items, ModifierToGwa(record.ModX), ModifierToGwa(record.ModY), ModifierToGwa(record.ModZ));
      }

      gwa = (Join(items, out var gwaLine)) ? new List<string>() { gwaLine } : new List<string>();
      return gwa.Count() > 0;
    }

    private string ModifierToGwa(double? mod)
    {
      if (mod < 0)
      {
        return (-mod * 100).ToString() + "%"; //percentage
      }
      else
      {
        return mod.ToString();
      }
    }

    protected bool AddName(string v)
    {
      record.Name = (string.IsNullOrEmpty(v)) ? null : v;
      return true;
    }

    private bool AddModifier(string v, out double? mod)
    {
      if (v.Contains("%"))
      {
        mod = double.TryParse(v.Replace("%", ""), out var n) ? -(double?)n/100 : null;
      }
      else
      {
        mod = double.TryParse(v, out var n) ? (double?)n : null;
      }
      return true;
    }
  }
}
