using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.SNAP.API
{
  public class StringValue : Attribute
  {
    public string Value { get; protected set; }

    public StringValue(string v)
    {
      Value = v;
    }
  }


}
