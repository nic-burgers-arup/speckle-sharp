using Interop.Gsa_10_1;
using Speckle.GSA.API.GwaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Speckle.ConnectorGSA.Proxy.GwaParsers
{
  [GsaType(GwaKeyword.POLYLINE, GwaSetCommandType.Set, true, true, true, GwaKeyword.GRID_PLANE)]

  public class GsaPolylineParser : GwaParser<GsaPolyline>
  {
    public GsaPolylineParser(GsaPolyline gsaPolyline) : base(gsaPolyline) { }
    public GsaPolylineParser() : base(new GsaPolyline()) { }

    public override bool FromGwa(string gwa)
    {
      if (!BasicFromGwa(gwa, out var remainingItems))
      {
        return false;
      }
      //Constructed from using 10.1 rather than consulting docs at https://www.oasys-software.com/help/gsa/10.1/GSA_Text.html
      //which, at the time of writing, only reports up to version 3
      //POLYLINE | num | name | colour | grid_plane | num_dim | desc
      return FromGwaByFuncs(remainingItems, out var _, AddName, (v) => AddColour(v, out record.Colour), 
        (v) => AddNullableIntValue(v, out record.GridPlaneIndex), (v) => int.TryParse(v, out record.NumDim), ProcDesc);
    }

    public override bool Gwa(out List<string> gwa, bool includeSet = false)
    {
      if (!InitialiseGwa(includeSet, out var items))
      {
        gwa = new List<string>();
        return false;
      }

      //POLYLINE | num | name | colour | grid_plane | num_dim | desc
      AddItems(ref items, record.Name ?? $"Polyline {record.Index}", Colour.NO_RGB.ToString(), (record.GridPlaneIndex ?? 0), 
        record.NumDim, AddDesc());

      gwa = Join(items, out var gwaLine) ? new List<string>() { gwaLine } : new List<string>();
      return (gwa.Count() > 0);
    }

    #region to_gwa_fns
    private string AddDesc()
    {
      var desc = new List<string>();
      var v = record.Values;
      for (int i = 0; i < v.Count; i += record.NumDim)
      {
        var point = "(" + v[i] + "," + v[i + 1];
        if (record.NumDim == 3) point += "," + v[i + 2];
        point += ")";
        desc.Add(point); 
      }
      if (!string.IsNullOrEmpty(record.Units))
      {
        desc.Add("(" + record.Units + ")");
      }
      return string.Join(" ", desc.ToArray());
    }
    #endregion

    #region from_gwa_fns
    protected bool AddName(string v)
    {
      record.Name = (string.IsNullOrEmpty(v)) ? null : v;
      return true;
    }
    protected bool ProcDesc(string v)
    {
      //coordinates
      var coords = new List<double>();
      v = RemoveWhitespace(v);
      Regex regex = new Regex(@"(?<=\().+?(?=\))"); //finds the contents in between ( and ) but removes the brackets
      foreach (Match match in regex.Matches(v))
      {
        if (match.Value.Contains(','))
        {
          var point = match.Value.Split(',').Select(c => c.ToDouble()).ToList();
          coords.AddRange(point);
        }
        else
        {
          record.Units = match.Value;
        }
      }
      record.Values = coords;
      return true;
    }
    #endregion

    private string RemoveWhitespace(string input)
    {
      var len = input.Length;
      var src = input.ToCharArray();
      int dstIdx = 0;
      for (int i = 0; i < len; i++)
      {
        var ch = src[i];
        switch (ch)
        {
          case '\u0020':
          case '\u00A0':
          case '\u1680':
          case '\u2000':
          case '\u2001':
          case '\u2002':
          case '\u2003':
          case '\u2004':
          case '\u2005':
          case '\u2006':
          case '\u2007':
          case '\u2008':
          case '\u2009':
          case '\u200A':
          case '\u202F':
          case '\u205F':
          case '\u3000':
          case '\u2028':
          case '\u2029':
          case '\u0009':
          case '\u000A':
          case '\u000B':
          case '\u000C':
          case '\u000D':
          case '\u0085':
            continue;
          default:
            src[dstIdx++] = ch;
            break;
        }
      }
      return new string(src, 0, dstIdx);
    }
  }
}
