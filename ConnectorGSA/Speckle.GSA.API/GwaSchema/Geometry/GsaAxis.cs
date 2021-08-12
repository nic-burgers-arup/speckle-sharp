﻿using System;
using System.Collections.Generic;
using System.Linq;


namespace Speckle.GSA.API.GwaSchema
{
  public class GsaAxis : GsaRecord_
  {
    //Only supporting cartesian at this stage
    public string Name { get => name; set { name = value; } }
    public double OriginX;
    public double OriginY;
    public double OriginZ;
    public double? XDirX;
    public double? XDirY;
    public double? XDirZ;
    public double? XYDirX;
    public double? XYDirY;
    public double? XYDirZ;

    public GsaAxis() : base()
    {
      //Defaults
      Version = 1;
    }
  }
}