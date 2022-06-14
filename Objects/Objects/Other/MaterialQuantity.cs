﻿
using Autodesk.Revit.DB.Structure;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using DB = Autodesk.Revit.DB;

namespace Objects.Other
{
    public class MaterialQuantity : Base
    {
        public Objects.Other.Material material { get; set; }
        public Objects.BuiltElements.Revit.Parameter volume { get; set; }

        public Objects.BuiltElements.Revit.Parameter area { get; set; }
        [Speckle.Core.Kits.SchemaInfo("MaterialQuantity", "Creates the quantity of a material")]
        public MaterialQuantity(Objects.Other.Material m, Objects.BuiltElements.Revit.Parameter volume, Objects.BuiltElements.Revit.Parameter area)
        {
            this.material = m;
            this.volume = volume;
            this.area = area;
        }
    }
}
