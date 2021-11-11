using System;
using System.Collections.Generic;

namespace Speckle.SNAP.API
{
  public interface ISNAPModel
  {
    ISNAPCache Cache { get; }
    ISNAPProxy Proxy { get; set; }

    //Settings - results
    bool SendResults { get; }
    int LoggingMinimumLevel { get; set; }
    string Units { get; set; }
    double CoincidentNodeAllowance { get; set; }
    bool SendOnlyMeaningfulNodes { get; set; }
  }
}
