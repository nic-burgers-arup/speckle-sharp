using Speckle.GSA.API.GwaSchema;
using System.Collections.Generic;
using System.Linq;
using System;
using Speckle.GSA.API;

namespace Speckle.ConnectorGSA.Proxy.GwaParsers
{
  [GsaType(GwaKeyword.ANAL_STAGE_PROP, GwaSetCommandType.SetAt, true, GwaKeyword.ANAL_STAGE, GwaKeyword.PROP_SEC, GwaKeyword.PROP_2D, GwaKeyword.PROP_SPR, GwaKeyword.PROP_MASS)]
  public class GsaAnalStagePropParser : GwaParser<GsaAnalStageProp>
  {
    public GsaAnalStagePropParser(GsaAnalStageProp GsaAnalStageProp) : base(GsaAnalStageProp) { }

    public GsaAnalStagePropParser() : base(new GsaAnalStageProp()) { }

    public override bool FromGwa(string gwa)
    {
      if (!BasicFromGwa(gwa, out var remainingItems))
      {
        return false;
      }

      //ANAL_STAGE_PROP | stage | type | elem_prop | stage_prop
      return FromGwaByFuncs(remainingItems, out var _, (v) => AddNullableIntValue(v, out record.StageIndex), AddType,
        (v) => AddNullableIntValue(v, out record.ElemPropIndex), (v) => AddNullableIntValue(v, out record.StagePropIndex));
    }

    public override bool Gwa(out List<string> gwa, bool includeSet = false)
    {
      if (!InitialiseGwa(includeSet, out var items))
      {
        gwa = new List<string>();
        return false;
      }

      //ANAL_STAGE_PROP | stage | type | elem_prop | stage_prop
      AddItems(ref items, record.StageIndex ?? 0, record.Type.GetStringValue(), record.ElemPropIndex ?? 0, record.StagePropIndex ?? 0);

      gwa = Join(items, out var gwaLine) ? new List<string>() { gwaLine } : new List<string>();
      return (gwa.Count() > 0);
    }

    #region to_gwa_fns
    private bool AddType(string v)
    {
      var parse = Enum.TryParse<ElementPropertyType>(v, true, out record.Type);
      if (!parse)
      {
        var sv = (string.IsNullOrEmpty(v)) ? null : v;
        if (sv == "2D") record.Type = ElementPropertyType.TwoD;
        else if (sv == "3D") record.Type = ElementPropertyType.ThreeD;
      }
      return true;
    }


    #endregion

    #region from_gwa_fns

    #endregion
  }
}
