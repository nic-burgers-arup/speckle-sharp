using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.SNAP.API
{
  public enum MessageLevel
  {
    Debug,
    Information,
    Error,
    Fatal
  }

  public enum MessageIntent
  {
    Display,
    TechnicalLog,
    Telemetry
  }

  public enum ResultGroup
  {
    Unknown = 0,
  }

  public enum ResultType
  {
    Unknown = 0,
  }
}
