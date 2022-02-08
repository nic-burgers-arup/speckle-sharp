using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;

namespace Objects.Structural.GSA.Analysis
{
  public class GSAAnalysisTask : Base
  {
    public int? nativeId { get; set; } //equiv to num
    public string name { get; set; }

    [DetachProperty]
    public GSAStage stage { get; set; }
    public string solver { get; set; }
    public SolutionType solutionType { get; set; }
    public int? modeParameter1 { get; set; } = 1; //start mode
    public int? modeParameter2 { get; set; } = 0; //number of modes
    public int? numIterations { get; set; } = 128;
    public string PDeltaOption { get; set; } = "SELF";
    public string PDeltaCase { get; set; }
    public string PrestressCase { get; set; }
    public string resultSyntax { get; set; } = "DRCMEFNSBUT";
    public PruningOption prune { get; set; }
    public GeometryChecksOption geometry { get; set; }
    public double? lower { get; set; }
    public double? upper { get; set; }
    public RaftPrecisionOption raft { get; set; }
    public ResidualSaveOption residual { get; set; }
    public double? shift { get; set; }
    public double? stiff { get; set; }
    public double? massFilter { get; set; }
    public int? maxCycle { get; set; }

    [DetachProperty]
    public List<GSAAnalysisCase> analysisCases { get; set; }
    public GSAAnalysisTask() { }

    //[SchemaInfo("GSAAnalysisTask (task only)", "Creates a Speckle structural analysis task for GSA", "GSA", "Analysis")]
    //public GSATask(SolutionType solutionType, string name = null, int? nativeId = null)
    //{
    //  this.nativeId = nativeId;
    //  this.name = name;
    //  this.solutionType = solutionType;
    //}

    //[SchemaInfo("GSAAnalysisTask", "Creates a Speckle structural analysis task for GSA", "GSA", "Analysis")]
    //public GSAAnalysisTask(SolutionType solutionType, List<LoadCombination> analysisCases, string name = null, int? nativeId = null)
    //{
    //  this.nativeId = nativeId;
    //  this.name = name;
    //  this.solutionType = solutionType;
    //  this.analysisCases = analysisCases;
    //}

    [SchemaInfo("GSAAnalysisTask", "Creates a Speckle structural analysis task for GSA", "GSA", "Analysis")]
    public GSAAnalysisTask(SolutionType solutionType, List<GSAAnalysisCase> analysisCases, GSAStage stage = null, string name = null, int? nativeId = null)
    {
      this.nativeId = nativeId;
      this.name = name;
      this.solutionType = solutionType;
      this.analysisCases = analysisCases;
      this.stage = stage;
    }
  }

  public enum SolutionType
  {
    Static = 0,
    NonlinearStatic,
    Modal,
    //Ritz,
    //Buckling,
    //StaticPDelta,
    //ModalPDelta,
    //RitzPDelta,
    //Mass,
    //Stability,
    //BucklingNonLinear
  }

  public enum PruningOption
  {
    None,
    Influence
  }

  public enum GeometryChecksOption
  {
    Error,
    Severe
  }

  public enum RaftPrecisionOption
  {
    Low,
    High
  }

  public enum ResidualSaveOption
  {
    No,
    NoIfNotConverged,
    Yes
  }
}
