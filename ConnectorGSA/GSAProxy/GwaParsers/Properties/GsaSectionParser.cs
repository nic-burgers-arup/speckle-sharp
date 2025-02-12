﻿using Speckle.GSA.API;
using Speckle.GSA.API.GwaSchema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Speckle.ConnectorGSA.Proxy.GwaParsers
{
  [GsaType(GwaKeyword.SECTION, GwaSetCommandType.Set, true, true, true, GwaKeyword.MAT_CONCRETE ,GwaKeyword.MAT_STEEL)]
  public class GsaSectionParser : GwaParser<GsaSection>
  {
    public GsaSectionParser(GsaSection gsaSection) : base(gsaSection) { }

    public GsaSectionParser() : base(new GsaSection()) { }

    private static readonly List<Type> sectionParsers = Helper.GetTypesImplementingInterface<ISectionComponentGwaParser>().ToList();
    private static readonly Dictionary<GwaKeyword, Type> sectionCompParserByKeyword = sectionParsers.ToDictionary(p => Helper.GetGwaKeyword(p), p => p);
    private static readonly Dictionary<Type, Type> sectionParsersByType = sectionParsers.ToDictionary(p => p.BaseType.GetGenericArguments().First(), p => p);

    //Notes about the documentation:
    //- it leaves out the last 3 parameters
    //- mistakenly leaves out the pipe between 'slab' and 'num'
    //- the 'num' doesn't seem to represent the number of components at all (e.g. it is 1 even with 3 components), just whether there's at least one
    //- there is no mention of the last 3 arguments 0 | 0 | NO_ENVIRON/ENVIRON

    //SECTION.7 | ref | colour | name | memb | pool | point | refY | refZ | mass | fraction | cost | left | right | slab | num { < comp > } | 0 | 0 | NO_ENVIRON
    // where <comp> could be one or more of:
    //SECTION_COMP | ref | name | matAnal | matType | matRef | desc | offset_y | offset_z | rotn | reflect | pool
    //SECTION_CONC | ref | grade | agg
    //SECTION_STEEL | ref | grade | plasElas | netGross | exposed | beta | type | plate | lock
    //SECTION_LINK (not in documentation)
    //SECTION_COVER | ref | type:UNIFORM | cover | outer 
    //    or SECTION_COVER | ref | type:VARIABLE | top | bot | left | right | outer
    //    or SECTION_COVER | ref | type:FACE | num | face[] | outer
    //SECTION_TMPL (not in documentation except for a mention that is deprecated, despite being included in GWA generated by GSA 10.1

    public override bool FromGwa(string gwa)
    {
      //Because this GWA is actually comprised of a SECTION proper plus embedded SECTION_COMP and other components
      if (!ProcessComponents(gwa, out var gwaSectionProper))
      {
        return false;
      }

      //Assume the first partition is the one for SECTION proper
      if (!BasicFromGwa(gwaSectionProper, out var items))
      {
        return false;
      }

      var numComponents = 0;
      //SECTION.7 | ref | colour | name | memb | pool | point | refY | refZ | mass | fraction | cost | left | right | slab | num { < comp > } | 0 | 0 | NO_ENVIRON
      if (!(FromGwaByFuncs(items, out var remainingItems, (v) => AddColour(v, out record.Colour), AddName, (v) => v.TryParseStringValue(out record.Type), 
          (v) => AddNullableIndex(v, out record.PoolIndex), (v) => v.TryParseStringValue(out record.ReferencePoint), (v) => AddNullableDoubleValue(v, out record.RefY), 
          (v) => AddNullableDoubleValue(v, out record.RefZ), (v) => AddNullableDoubleValue(v, out record.Mass), (v) => AddNullableDoubleValue(v, out record.Fraction), 
          (v) => AddNullableDoubleValue(v, out record.Cost), (v) => AddNullableDoubleValue(v, out record.Left), (v) => AddNullableDoubleValue(v, out record.Right), 
          (v) => AddNullableDoubleValue(v, out record.Slab), (v) => int.TryParse(v, out numComponents))))
      {
        return false;
      }

      //The final partition should have, tacked onto its end, the final three items of the entire original GWA (0 | 0 | NO_ENVIRON).
      //For now, leave these where they are as they should be ignored by the FromGwa of whatever is the final partition (section component) anyway

      //This check reflects the fact that the num parameter
      return ((numComponents == 0 && (record.Components == null || record.Components.Count() == 0)) || (numComponents == 1 && record.Components.Count() > 0));
    }

    //Doesn't take version into account yet
    public override bool Gwa(out List<string> gwa, bool includeSet = false)
    {
      if (!InitialiseGwa(includeSet, out var items))
      {
        gwa = new List<string>();
        return false;
      }

      //SECTION.7 | ref | colour | name | memb | pool | point | refY | refZ | mass | fraction | cost | left | right | slab | num { < comp > } | 0 | 0 | NO_ENVIRON
      AddItems(ref items, record.Colour.ToString(), record.Name, record.Type.GetStringValue(), record.PoolIndex ?? 0, record.ReferencePoint.GetStringValue(),
        record.RefY ?? 0, record.RefZ ?? 0, record.Mass ?? 0, record.Fraction ?? 0, record.Cost ?? 0, record.Left ?? 0, record.Right ?? 0, record.Slab ?? 0,
        record.Components == null || record.Components.Count() == 0 ? 0 : 1);

      ProcessComponents(ref items);

      AddItems(ref items, 0, 0, record.Environ ? "ENVIRON" : "NO_ENVIRON");

      gwa = Join(items, out var gwaLine) ? new List<string>() { gwaLine } : new List<string>();

      return (gwa.Count() > 0);
    }

    #region to_gwa_fns
    private bool ProcessComponents(ref List<string> items)
    {
      foreach (var comp in record.Components)
      {
        var sectionCompParser = (ISectionComponentGwaParser)Activator.CreateInstance(sectionParsersByType[comp.GetType()], comp);
        if (sectionCompParser.GwaItems(out var compItems, false, false))
        {
          items.AddRange(compItems);
        }
      }
      return true;
    }
    #endregion

    #region from_gwa_fns
    protected bool AddName(string v)
    {
      record.Name = (string.IsNullOrEmpty(v)) ? null : v;
      return true;
    }

    private bool ProcessComponents(string gwa, out string gwaSectionProper)
    {
      //This will only catch the section component keywords that have been implemented.  This will mean the GWA of any other as-yet-not-implemented
      //section components will be the trailing end of either the SECTION proper or one of the implemented section components.
      //This will be picked up later - for now just return the partitions based on the implemented section types' keywords
      //var sectionCompTypesByKeywords = SectionCompTypes.ToDictionary(t => (GwaKeyword)t.GetAttribute<GsaType>("Keyword"), t => t);

      //First break up the GWA into the SECTION proper and the components
      var sectionCompStartIndicesTypes = new Dictionary<int, GwaKeyword>();
      foreach (var sckw in sectionCompParserByKeyword.Keys)
      {
        var index = gwa.IndexOf(sckw.GetStringValue());
        if (index > 0)
        {
          sectionCompStartIndicesTypes.Add(index, sckw);
        }
      }
      var orderedComponentStartIndices = sectionCompStartIndicesTypes.Keys.OrderBy(i => i).ToList();

      var gwaPieces = new List<string>();
      var startIndex = 0;
      foreach (var i in orderedComponentStartIndices)
      {
        var gwaPartition = gwa.Substring(startIndex, i - startIndex);
        gwaPieces.Add(gwaPartition.TrimEnd('\t'));
        startIndex = i;
      }
      gwaPieces.Add(gwa.Substring(startIndex));

      gwaSectionProper = gwaPieces.First();

      var sectionComps = new List<GsaSectionComponentBase>();
      var partitionIndex = 1;
      foreach (var i in orderedComponentStartIndices)
      {
        var sectionCompParser = (IGwaParser)Activator.CreateInstance(sectionCompParserByKeyword[sectionCompStartIndicesTypes[i]]);
        sectionCompParser.FromGwa(gwaPieces[partitionIndex++]);
        startIndex = i;

        if (record.Components == null)
        {
          record.Components = new List<GsaSectionComponentBase>();
        }
        record.Components.Add((GsaSectionComponentBase)sectionCompParser.Record);
      }

      
      return true;
    }
    #endregion
  }
}
