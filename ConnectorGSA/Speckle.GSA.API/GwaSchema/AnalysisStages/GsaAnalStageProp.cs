using System.Collections.Generic;

namespace Speckle.GSA.API.GwaSchema
{
  public class GsaAnalStageProp : GsaRecord
  {
    public int? StageIndex;
    public ElementPropertyType Type;
    public int? ElemPropIndex;
    public int? StagePropIndex;

    public GsaAnalStageProp()
    {
      Version = 1;
    }
  }
}
