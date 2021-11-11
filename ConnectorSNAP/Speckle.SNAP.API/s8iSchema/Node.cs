using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.SNAP.API.s8iSchema
{
  public class Node
  {
    public string Name { get; set; }
    public double X { get; set; } = 0;
    public double Y { get; set; } = 0;
    public double Z { get; set; } = 0;
    public double AdditionalMass { get; set; } = 0;
    public double Mass { get; set; } = 0;
    public string Restraint { get; set; }
    public double AxisX { get; set; } = 0;
    public double AxisY { get; set; } = 0;
    public double AxisZ { get; set; } = 0;
    public bool HistoryOutput { get; set; } = false;
    public bool Output { get; set; } = true;
  }
}
