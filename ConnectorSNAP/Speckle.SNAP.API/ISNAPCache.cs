using Speckle.SNAP.API.s8iSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.SNAP.API
{
  public interface ISNAPCache
  {
    bool GetNatives(out List<object> snapRecords);
    bool Upsert(object record);
  }
}
