using Speckle.SNAP.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorSNAP.Proxy.s81CsvMaps
{
  public enum Keyword
  {
    [StringValue("REM")]  //Remark
    REM,
    [StringValue("BM")] //Beam
    BM,
    [StringValue("SN")] //Secondary node
    SN,
    [StringValue("ND")] //Secondary node
    ND
  }
}
