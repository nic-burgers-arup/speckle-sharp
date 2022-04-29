using Speckle.Core.Kits;
using Speckle.Core.Models;
using Speckle.SNAP.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConnectorSNAP
{
  public class SnapModel : ISNAPModel
  {
    internal static SnapProxy proxy = new SnapProxy();
    internal static SnapCache cache = new SnapCache();

    public static ISNAPModel Instance = new SnapModel();

    public ISNAPProxy Proxy { get => proxy; set => proxy = (SnapProxy)value; }
    public ISNAPCache Cache { get => cache; }

    public bool SendResults { get; set; }
    public int LoggingMinimumLevel { get; set; }
    public string Units { get; set; }
    public double CoincidentNodeAllowance { get; set; }
    public bool SendOnlyMeaningfulNodes { get; set; } = true;
  }
}
