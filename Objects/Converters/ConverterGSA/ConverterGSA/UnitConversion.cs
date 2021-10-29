using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Objects.Structural.Analysis;
using Speckle.Core.Kits;
using Speckle.GSA.API;
using Speckle.GSA.API.GwaSchema;

namespace ConverterGSA
{
  public class UnitConversion
  {
    public double length = 1;
    public double sections = 1;
    public double displacements = 1;
    public double stress = 1;
    public double force = 1;
    public double mass = 1;
    public double time = 1;
    //temperature can't just mutliply by factor
    public double velocity = 1;
    public double acceleration = 1;
    public double energy = 1;
    public double angle = 1;
    public double strain = 1;
    public ModelUnits speckleModelUnits = new ModelUnits();
    public ModelUnits nativeModelUnits = new ModelUnits();

    public UnitConversion()
    {
      SetNativeUnits();
    }

    public UnitConversion(ModelUnits speckleUnits) 
    {
      SetNativeUnits();
      SetSpeckleUnits(speckleUnits);
      CalculateConversionFactors();
    }

    private void SetNativeUnits()
    {
      if (Instance.GsaModel.Cache.GetNatives(out var gsaRecords))
      {
        var gsaUnits = gsaRecords.FindAll(r => r is GsaUnitData).Select(r => (GsaUnitData)r).ToList();
        foreach (var unit in gsaUnits)
        {
          if (!string.IsNullOrEmpty(unit.Name))
          {
            switch (unit.Option)
            {
              case UnitDimension.Length:
                this.nativeModelUnits.length = Units.GetUnitsFromString(unit.Name);
                break;
              case UnitDimension.Sections:
                this.nativeModelUnits.sections = Units.GetUnitsFromString(unit.Name);
                break;
              case UnitDimension.Displacements:
                this.nativeModelUnits.displacements = Units.GetUnitsFromString(unit.Name);
                break;
              case UnitDimension.Stress:
                this.nativeModelUnits.stress = StressUnits.GetUnitsFromString(unit.Name);
                break;
              case UnitDimension.Force:
                this.nativeModelUnits.force = ForceUnits.GetUnitsFromString(unit.Name);
                break;
              case UnitDimension.Mass:
                this.nativeModelUnits.mass = MassUnits.GetUnitsFromString(unit.Name);
                break;
              case UnitDimension.Time:
                this.nativeModelUnits.time = TimeUnits.GetUnitsFromString(unit.Name);
                break;
              case UnitDimension.Temperature:
                this.nativeModelUnits.temperature = TemperatureUnits.GetUnitsFromString(unit.Name);
                break;
              case UnitDimension.Velocity:
                this.nativeModelUnits.velocity = VelocityUnits.GetUnitsFromString(unit.Name);
                break;
              case UnitDimension.Acceleration:
                this.nativeModelUnits.acceleration = AccelerationUnits.GetUnitsFromString(unit.Name);
                break;
              case UnitDimension.Energy:
                this.nativeModelUnits.energy = EnergyUnits.GetUnitsFromString(unit.Name);
                break;
              case UnitDimension.Angle:
                this.nativeModelUnits.angle = AngleUnits.GetUnitsFromString(unit.Name);
                break;
              case UnitDimension.Strain:
                this.nativeModelUnits.strain = StrainUnits.GetUnitsFromString(unit.Name);
                break;
              default:
                //do nothing
                break;
            }
          }
        }
      }
    }

    private void SetSpeckleUnits(ModelUnits speckleUnits)
    {
      if (speckleUnits != null)
      {
        if (!string.IsNullOrEmpty(speckleUnits.length))        this.speckleModelUnits.length = Units.GetUnitsFromString(speckleUnits.length);
        if (!string.IsNullOrEmpty(speckleUnits.sections))      this.speckleModelUnits.sections = Units.GetUnitsFromString(speckleUnits.sections);
        if (!string.IsNullOrEmpty(speckleUnits.displacements)) this.speckleModelUnits.displacements = Units.GetUnitsFromString(speckleUnits.displacements);
        if (!string.IsNullOrEmpty(speckleUnits.stress))        this.speckleModelUnits.stress = StressUnits.GetUnitsFromString(speckleUnits.stress);
        if (!string.IsNullOrEmpty(speckleUnits.force))         this.speckleModelUnits.force = ForceUnits.GetUnitsFromString(speckleUnits.force);
        if (!string.IsNullOrEmpty(speckleUnits.mass))          this.speckleModelUnits.mass = MassUnits.GetUnitsFromString(speckleUnits.mass);
        if (!string.IsNullOrEmpty(speckleUnits.time))          this.speckleModelUnits.time = TimeUnits.GetUnitsFromString(speckleUnits.time);
        if (!string.IsNullOrEmpty(speckleUnits.velocity))      this.speckleModelUnits.velocity = VelocityUnits.GetUnitsFromString(speckleUnits.velocity);
        if (!string.IsNullOrEmpty(speckleUnits.acceleration))  this.speckleModelUnits.acceleration = AccelerationUnits.GetUnitsFromString(speckleUnits.acceleration);
        if (!string.IsNullOrEmpty(speckleUnits.angle))         this.speckleModelUnits.angle = AngleUnits.GetUnitsFromString(speckleUnits.angle);
        if (!string.IsNullOrEmpty(speckleUnits.energy))        this.speckleModelUnits.energy = EnergyUnits.GetUnitsFromString(speckleUnits.energy);
        if (!string.IsNullOrEmpty(speckleUnits.strain))        this.speckleModelUnits.strain = StrainUnits.GetUnitsFromString(speckleUnits.strain);
      }
    }

    private void CalculateConversionFactors()
    {
      if (!string.IsNullOrEmpty(this.speckleModelUnits.length) && !string.IsNullOrEmpty(this.nativeModelUnits.length))
      {
        this.length = Units.GetConversionFactor(this.speckleModelUnits.length, this.nativeModelUnits.length);
      }
      if (!string.IsNullOrEmpty(this.speckleModelUnits.sections) && !string.IsNullOrEmpty(this.nativeModelUnits.sections))
      {
        this.sections = Units.GetConversionFactor(this.speckleModelUnits.sections, this.nativeModelUnits.sections);
      }
      if (!string.IsNullOrEmpty(this.speckleModelUnits.displacements) && !string.IsNullOrEmpty(this.nativeModelUnits.displacements))
      {
        this.displacements = Units.GetConversionFactor(this.speckleModelUnits.displacements, this.nativeModelUnits.displacements);
      }
      if (!string.IsNullOrEmpty(this.speckleModelUnits.stress) && !string.IsNullOrEmpty(this.nativeModelUnits.stress))
      {
        this.stress = StressUnits.GetConversionFactor(this.speckleModelUnits.stress, this.nativeModelUnits.stress);
      }
      if (!string.IsNullOrEmpty(this.speckleModelUnits.force) && !string.IsNullOrEmpty(this.nativeModelUnits.force))
      {
        this.force = ForceUnits.GetConversionFactor(this.speckleModelUnits.force, this.nativeModelUnits.force);
      }
      if (!string.IsNullOrEmpty(this.speckleModelUnits.mass) && !string.IsNullOrEmpty(this.nativeModelUnits.mass))
      {
        this.mass = MassUnits.GetConversionFactor(this.speckleModelUnits.mass, this.nativeModelUnits.mass);
      }
      if (!string.IsNullOrEmpty(this.speckleModelUnits.time) && !string.IsNullOrEmpty(this.nativeModelUnits.time))
      {
        this.time = TimeUnits.GetConversionFactor(this.speckleModelUnits.time, this.nativeModelUnits.time);
      }
      if (!string.IsNullOrEmpty(this.speckleModelUnits.velocity) && !string.IsNullOrEmpty(this.nativeModelUnits.velocity))
      {
        this.velocity = VelocityUnits.GetConversionFactor(this.speckleModelUnits.velocity, this.nativeModelUnits.velocity);
      }
      if (!string.IsNullOrEmpty(this.speckleModelUnits.acceleration) && !string.IsNullOrEmpty(this.nativeModelUnits.acceleration))
      {
        this.acceleration = AccelerationUnits.GetConversionFactor(this.speckleModelUnits.acceleration, this.nativeModelUnits.acceleration);
      }
      if (!string.IsNullOrEmpty(this.speckleModelUnits.angle) && !string.IsNullOrEmpty(this.nativeModelUnits.angle))
      {
        this.angle = AngleUnits.GetConversionFactor(this.speckleModelUnits.angle, this.nativeModelUnits.angle);
      }
      if (!string.IsNullOrEmpty(this.speckleModelUnits.energy) && !string.IsNullOrEmpty(this.nativeModelUnits.energy))
      {
        this.energy = EnergyUnits.GetConversionFactor(this.speckleModelUnits.energy, this.nativeModelUnits.energy);
      }
      if (!string.IsNullOrEmpty(this.speckleModelUnits.strain) && !string.IsNullOrEmpty(this.nativeModelUnits.strain))
      {
        this.strain = StrainUnits.GetConversionFactor(this.speckleModelUnits.strain, this.nativeModelUnits.strain);
      }
    }

    public bool UpdateConversionFactors(ModelUnits speckleUnits)
    {
      if (!string.IsNullOrEmpty(speckleUnits.length))
      {
        this.speckleModelUnits.length = Units.GetUnitsFromString(speckleUnits.length);
        if (!string.IsNullOrEmpty(nativeModelUnits.length))
        {
          this.length = Units.GetConversionFactor(this.speckleModelUnits.length, this.nativeModelUnits.length);
        }
      }
      if (!string.IsNullOrEmpty(speckleUnits.sections))
      {
        this.speckleModelUnits.sections = Units.GetUnitsFromString(speckleUnits.sections);
        if (!string.IsNullOrEmpty(nativeModelUnits.sections))
        {
          this.sections = Units.GetConversionFactor(this.speckleModelUnits.sections, this.nativeModelUnits.sections);
        }
      }
      if (!string.IsNullOrEmpty(speckleUnits.displacements))
      {
        this.speckleModelUnits.displacements = Units.GetUnitsFromString(speckleUnits.displacements);
        if (!string.IsNullOrEmpty(nativeModelUnits.displacements))
        {
          this.displacements = Units.GetConversionFactor(this.speckleModelUnits.displacements, this.nativeModelUnits.displacements);
        }
      }
      if (!string.IsNullOrEmpty(speckleUnits.stress))
      {
        this.speckleModelUnits.stress = StressUnits.GetUnitsFromString(speckleUnits.stress);
        if (!string.IsNullOrEmpty(nativeModelUnits.stress))
        {
          this.stress = StressUnits.GetConversionFactor(this.speckleModelUnits.stress, this.nativeModelUnits.stress);
        }
      }
      if (!string.IsNullOrEmpty(speckleUnits.force))
      {
        this.speckleModelUnits.force = ForceUnits.GetUnitsFromString(speckleUnits.force);
        if (!string.IsNullOrEmpty(nativeModelUnits.force))
        {
          this.force = ForceUnits.GetConversionFactor(this.speckleModelUnits.force, this.nativeModelUnits.force);
        }
      }
      if (!string.IsNullOrEmpty(speckleUnits.mass))
      {
        this.speckleModelUnits.mass = MassUnits.GetUnitsFromString(speckleUnits.mass);
        if (!string.IsNullOrEmpty(nativeModelUnits.mass))
        {
          this.mass = MassUnits.GetConversionFactor(this.speckleModelUnits.mass, this.nativeModelUnits.mass);
        }
      }
      if (!string.IsNullOrEmpty(speckleUnits.time))
      {
        this.speckleModelUnits.time = TimeUnits.GetUnitsFromString(speckleUnits.time);
        if (!string.IsNullOrEmpty(nativeModelUnits.length))
        {
          this.time = TimeUnits.GetConversionFactor(this.speckleModelUnits.time, this.nativeModelUnits.time);
        }
      }
      if (!string.IsNullOrEmpty(speckleUnits.velocity))
      {
        this.speckleModelUnits.velocity = VelocityUnits.GetUnitsFromString(speckleUnits.velocity);
        if (!string.IsNullOrEmpty(nativeModelUnits.velocity))
        {
          this.velocity = VelocityUnits.GetConversionFactor(this.speckleModelUnits.velocity, this.nativeModelUnits.velocity);
        }
      }
      if (!string.IsNullOrEmpty(speckleUnits.acceleration))
      {
        this.speckleModelUnits.acceleration = AccelerationUnits.GetUnitsFromString(speckleUnits.acceleration);
        if (!string.IsNullOrEmpty(nativeModelUnits.acceleration))
        {
          this.acceleration = AccelerationUnits.GetConversionFactor(this.speckleModelUnits.acceleration, this.nativeModelUnits.acceleration);
        }
      }
      if (!string.IsNullOrEmpty(speckleUnits.angle))
      {
        this.speckleModelUnits.angle = AngleUnits.GetUnitsFromString(speckleUnits.angle);
        if (!string.IsNullOrEmpty(nativeModelUnits.angle))
        {
          this.angle = AngleUnits.GetConversionFactor(this.speckleModelUnits.angle, this.nativeModelUnits.angle);
        }
      }
      if (!string.IsNullOrEmpty(speckleUnits.energy))
      {
        this.speckleModelUnits.energy = EnergyUnits.GetUnitsFromString(speckleUnits.energy);
        if (!string.IsNullOrEmpty(nativeModelUnits.energy))
        {
          this.energy = EnergyUnits.GetConversionFactor(this.speckleModelUnits.energy, this.nativeModelUnits.energy);
        }
      }
      if (!string.IsNullOrEmpty(speckleUnits.strain))
      {
        this.speckleModelUnits.strain = StrainUnits.GetUnitsFromString(speckleUnits.strain);
        if (!string.IsNullOrEmpty(nativeModelUnits.strain))
        {
          this.strain = StrainUnits.GetConversionFactor(this.speckleModelUnits.strain, this.nativeModelUnits.strain);
        }
      }
      return true;
    } 

    public double ConversionFactorToNative(UnitDimension dimension, string speckleUnit)
    {
      if (string.IsNullOrEmpty(speckleUnit)) return 1;

      switch (dimension)
      {
        case UnitDimension.Length:        
          return string.IsNullOrEmpty(this.nativeModelUnits.length) ? 1 : Units.GetConversionFactor(speckleUnit, this.nativeModelUnits.length);
        case UnitDimension.Sections:      
          return string.IsNullOrEmpty(this.nativeModelUnits.sections) ? 1 : Units.GetConversionFactor(speckleUnit, this.nativeModelUnits.sections);
        case UnitDimension.Displacements: 
          return string.IsNullOrEmpty(this.nativeModelUnits.displacements) ? 1 : Units.GetConversionFactor(speckleUnit, this.nativeModelUnits.displacements);
        case UnitDimension.Stress:        
          return string.IsNullOrEmpty(this.nativeModelUnits.stress) ? 1 : StressUnits.GetConversionFactor(speckleUnit, this.nativeModelUnits.stress);
        case UnitDimension.Force:         
          return string.IsNullOrEmpty(this.nativeModelUnits.force) ? 1 : ForceUnits.GetConversionFactor(speckleUnit, this.nativeModelUnits.force);
        case UnitDimension.Mass:          
          return string.IsNullOrEmpty(this.nativeModelUnits.mass) ? 1 : MassUnits.GetConversionFactor(speckleUnit, this.nativeModelUnits.mass);
        case UnitDimension.Time:          
          return string.IsNullOrEmpty(this.nativeModelUnits.time) ? 1 : TimeUnits.GetConversionFactor(speckleUnit, this.nativeModelUnits.time);
        case UnitDimension.Velocity:      
          return string.IsNullOrEmpty(this.nativeModelUnits.velocity) ? 1 : VelocityUnits.GetConversionFactor(speckleUnit, this.nativeModelUnits.velocity);
        case UnitDimension.Acceleration:  
          return string.IsNullOrEmpty(this.nativeModelUnits.acceleration) ? 1 : AccelerationUnits.GetConversionFactor(speckleUnit, this.nativeModelUnits.acceleration);
        case UnitDimension.Energy:        
          return string.IsNullOrEmpty(this.nativeModelUnits.energy) ? 1 : EnergyUnits.GetConversionFactor(speckleUnit, this.nativeModelUnits.energy);
        case UnitDimension.Angle:         
          return string.IsNullOrEmpty(this.nativeModelUnits.angle) ? 1 : AngleUnits.GetConversionFactor(speckleUnit, this.nativeModelUnits.angle);
        case UnitDimension.Strain:        
          return string.IsNullOrEmpty(this.nativeModelUnits.strain) ? 1 : StrainUnits.GetConversionFactor(speckleUnit, this.nativeModelUnits.strain);
        default:                          
          return 1;
      }
    }

    public double ConversionFactorToDegrees() => string.IsNullOrEmpty(this.speckleModelUnits.angle) ? 1 : AngleUnits.GetConversionFactor(this.speckleModelUnits.angle, AngleUnits.Degree);

    public double? TemperatureToNative(double? speckleValue) => TemperatureToNative(speckleValue, this.speckleModelUnits.temperature);

    public double? TemperatureToNative(double? speckleValue, string speckleUnit)
    {
      if (speckleValue == null || string.IsNullOrEmpty(this.nativeModelUnits.temperature) || string.IsNullOrEmpty(speckleUnit))
      {
        return speckleValue;
      }
      else
      {
        return TemperatureUnits.Convert(speckleValue.Value, speckleUnit, this.nativeModelUnits.temperature);
      }
    } 
  }
}
