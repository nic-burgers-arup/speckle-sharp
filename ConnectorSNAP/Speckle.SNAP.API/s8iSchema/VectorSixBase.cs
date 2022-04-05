namespace Speckle.SNAP.API.s8iSchema
{
  public abstract class VectorSixBase
  {
    public string Name { get; set; }
    public bool[] Restraints { get; set; } = new bool[6] { false, false, false, false, false, false };
  }
}
