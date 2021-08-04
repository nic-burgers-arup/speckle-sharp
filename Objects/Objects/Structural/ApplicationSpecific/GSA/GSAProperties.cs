﻿using Speckle.Newtonsoft.Json;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using Objects.Geometry;
using Objects.Structural.Properties;
using Objects.Structural.Materials;

namespace Objects.Structural.GSA.Properties
{
    public class GSAProperty1D : Property1D
    {
        public int nativeId { get; set; }
        public Material designMaterial { get; set; } 
        public double cost { get; set; }
        public double additionalMass { get; set; } 
        public string poolRef { get; set; }
        public GSAProperty1D() { }

        [SchemaInfo("GSAProperty1D", "Creates a Speckle structural 1D element property for GSA", "GSA", "Properties")]
        public GSAProperty1D(int nativeId, string name, Material material, string grade, SectionProfile profile, double cost = 0, double additionalMass = 0, string poolRef = null)
        {
            this.nativeId = nativeId;
            this.name = name;
            this.material = material;
            this.grade = grade;
            this.profile = profile;
            this.nativeId = nativeId;
            this.cost = cost;
            this.additionalMass = additionalMass;
            this.poolRef = poolRef;
        }
    }

    public class GSAProperty2D : Property2D
    {
        public int nativeId { get; set; }
        public Material designMaterial { get; set; }
        public double cost { get; set; }
        public double additionalMass { get; set; }
        public string concreteSlabProp { get; set; }
        public GSAProperty2D() { }

        [SchemaInfo("GSAProperty2D", "Creates a Speckle structural 2D element property for GSA", "GSA", "Properties")]
        public GSAProperty2D(int nativeId, string name, Material material, double thickness)
        {
            this.nativeId = nativeId;
            this.name = name;
            this.material = material;
            this.thickness = thickness;
            this.nativeId = nativeId;
        }
    }
}