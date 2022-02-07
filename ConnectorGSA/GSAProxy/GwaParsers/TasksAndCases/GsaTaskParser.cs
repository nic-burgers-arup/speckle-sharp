using Speckle.GSA.API.GwaSchema;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Speckle.ConnectorGSA.Proxy.GwaParsers
{
  //This needs review
  [GsaType(GwaKeyword.TASK, GwaSetCommandType.Set, true, GwaKeyword.ANAL_STAGE)]
  public class GsaTaskParser : GwaParser<GsaTask>
  {
    public GsaTaskParser(GsaTask gsaTask) : base(gsaTask) { }

    public GsaTaskParser() : base(new GsaTask()) { }

    public override bool FromGwa(string gwa)
    {
      if (!BasicFromGwa(gwa, out var remainingItems))
      {
        return false;
      }

      //ANAL | case | name | task | desc
      //TASK.2 | task | name | stage | GSS | solution | mode_1 | mode_2 | num_iter | p_delta | p_delta_case | prestress | result | prune | geometry | lower | upper | raft | residual | shift | stiff | mass_filter | max_cycle
      return FromGwaByFuncs(remainingItems, out var _, AddName, (v) => AddNullableIntValue(v, out record.StageIndex), (v) => AddStringValue(v, out record.Solver),
        (v) => Enum.TryParse<StructuralSolutionType>(v, true, out record.Solution), (v) => AddNullableIntValue(v, out record.Mode1), (v) => AddNullableIntValue(v, out record.Mode2),
        (v) => AddNullableIntValue(v, out record.NumIter), (v) => AddStringValue(v, out record.PDelta), (v) => AddStringValue(v, out record.PDelta),
        (v) => AddStringValue(v, out record.Prestress), (v) => AddStringValue(v, out record.Result), (v) => Enum.TryParse<StructuralPruningOption>(v, true, out record.Prune),
        (v) => Enum.TryParse<StructuralGeometryChecksOption>(v, true, out record.Geometry), (v) => AddNullableDoubleValue(v, out record.Lower), (v) => AddNullableDoubleValue(v, out record.Upper),
        (v) => Enum.TryParse<StructuralRaftPrecisionOption>(v, true, out record.Raft), (v) => Enum.TryParse<StructuralResidualSaveOption>(v, true, out record.Residual),
        (v) => AddNullableDoubleValue(v, out record.Shift), (v) => AddNullableDoubleValue(v, out record.Stiff), (v) => AddNullableDoubleValue(v, out record.MassFilter), 
        (v) => AddNullableIntValue(v, out record.MaxCycle));
    }

    public override bool Gwa(out List<string> gwa, bool includeSet = false)
    {
      if (!InitialiseGwa(includeSet, out var items))
      {
        gwa = new List<string>();
        return false;
      }

      //TASK.2 | task | name | stage | GSS | solution | mode_1 | mode_2 | num_iter | p_delta | p_delta_case | prestress | result | prune | geometry | lower | upper | raft | residual | shift | stiff | mass_filter | max_cycle
      AddItems(ref items, record.Name ?? $"Task {record.Index}", record.StageIndex ?? 0, record.Solver, record.Solution.ToString(), record.Mode1 ?? 1, record.Mode2 ?? 0, record.NumIter ?? 128, record.PDelta, 
        record.PDeltaCase ?? "none", record.Prestress ?? "none", record.Result, record.Prune, record.Geometry, AddCutoff(record.Lower), AddCutoff(record.Upper), record.Raft, record.Residual,
        record.Shift ?? 0, record.Stiff ?? 1, record.MassFilter ?? 0, record.MaxCycle ?? 1000000);

      gwa = Join(items, out var gwaLine) ? new List<string>() { gwaLine } : new List<string>();
      return (gwa.Count() > 0);
    }

    #region from_gwa_fns
    protected bool AddName(string v)
    {
      record.Name = (string.IsNullOrEmpty(v)) ? null : v;
      return true;
    }

    #endregion

    #region to_gwa_fns
    protected object AddCutoff(double? v)
    {
      if (v is null) return "NONE";
      else return v;
    }

    #endregion
  }
}
