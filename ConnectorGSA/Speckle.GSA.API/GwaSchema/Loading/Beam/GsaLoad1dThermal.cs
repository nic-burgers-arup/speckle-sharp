using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.GSA.API.GwaSchema.Loading.Beam
{
  public class GsaLoad1dThermal : GsaRecord
  {
    public string Name { get => name; set { name = value; } }
    public List<int> ElementIndices;
    public List<int> MemberIndices;
    public int? LoadCaseIndex;
    public Load1dThermalType Type;
    public List<double> Values;


    public GsaLoad1dThermal() : base()
    {
      Version = 2;
    }

  }
}
