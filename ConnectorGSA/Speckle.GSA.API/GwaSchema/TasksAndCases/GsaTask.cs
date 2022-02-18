using System.Collections.Generic;

namespace Speckle.GSA.API.GwaSchema
{
  public class GsaTask : GsaRecord
  {
    public string Name { get => name; set { name = value; } }
    public int? StageIndex; //0 corresponds to whole model (default)
    public string Solver; //GSS
    public StructuralSolutionType Solution; //STATIC
    public int? Mode1; //1
    public int? Mode2; //0
    public int? NumIter; //128
    public string PDelta; //SELF
    public string PDeltaCase; //none
    public string Prestress; //none
    public string Result; //DRCMEFNSBUT
    public StructuralPruningOption Prune; //NONE
    public StructuralGeometryChecksOption Geometry; //FATAL
    public double? Lower; //NONE or value
    public double? Upper; //NONE or value
    public StructuralRaftPrecisionOption Raft; //RAFT_LO
    public StructuralResidualSaveOption Residual; //RESID_NO
    public double? Shift; //0
    public double? Stiff; //1
    public double? MassFilter; //0
    public int? MaxCycle; //1000000

    public GsaTask()
    {
      Version = 2;
    }
  }
}


//TASK.2 | task | name | stage | GSS | solution | mode_1 | mode_2 | num_iter | p_delta | p_delta_case | prestress | result | prune | geometry | lower | upper | raft | residual | shift | stiff | mass_filter | max_cycle