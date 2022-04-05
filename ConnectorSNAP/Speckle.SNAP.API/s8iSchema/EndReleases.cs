using System.Collections.Generic;

namespace Speckle.SNAP.API.s8iSchema
{
  public class EndReleases : VectorSixBase, ISnapRecordNamed
  {
    public object[] defaultRemaining { get; set; } = new object[] { 0, 0, 0, 0, 0, 0, null, 0 };
  }
}
