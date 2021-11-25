using ConnectorSNAP.Cache;
using Speckle.SNAP.API;
using Speckle.SNAP.API.s8iSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorSNAP
{
  public class SnapCache : ISNAPCache
  {
    private List<SnapCacheRecord> snapCacheRecords = new List<SnapCacheRecord>();

    public bool GetNatives(out List<object> snapRecords)
    {
      snapRecords = snapCacheRecords.Select(cr => cr.SnapRecord).ToList();
      return true;
    }

    public bool Upsert(List<object> nativeObjects)
    {
      snapCacheRecords.AddRange(nativeObjects.Select(o => new SnapCacheRecord() { SnapRecord = o }));
      return true;
    }

    public bool Upsert(object nativeObject)
    {
      snapCacheRecords.Add(new SnapCacheRecord() { SnapRecord = nativeObject });
      return true;
    }
  }
}
