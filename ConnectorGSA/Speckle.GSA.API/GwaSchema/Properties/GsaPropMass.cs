namespace Speckle.GSA.API.GwaSchema
{
  public class GsaPropMass : GsaRecord
  {
    public string Name { get => name; set { name = value; } }
    public double Mass;
    public double Ixx;
    public double Iyy;
    public double Izz;
    public double Ixy;
    public double Iyz;
    public double Izx;
    public MassModification Mod;
    public double? ModX;
    public double? ModY;
    public double? ModZ;

    public GsaPropMass() : base()
    {
      //Defaults
      Version = 3;
    }
  }
}
