using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Speckle.Core.Logging;
using Speckle.Core.Kits;

namespace ConverterGSA
{
  #region Primary dimensions
  public static class MassUnits
  {
    public const string Kilogram = "kg";
    public const string Tonne = "t";
    public const string Kilotonne = "kt";
    public const string Gram = "g";
    public const string Pound = "lb";
    public const string Ton = "ton"; //2240lb
    public const string Slug = "sl";
    public const string KilopoundForceSecondSquarePerInch = "kip.s²/in";
    public const string KilopoundForceSecondSquarePerFoot = "kip.s²/ft";
    public const string PoundForceSecondSquarePerInch = "lbf.s²/in";
    public const string PoundForceSecondSquarePerFoot = "lbf.s²/ft"; // 1 slug
    public const string Kilopound = "kip";
    public const string None = "none";

    public static double GetConversionFactor(string from, string to)
    {
      from = GetUnitsFromString(from);
      to = GetUnitsFromString(to);

      return GetConversionFactorToSI(from) / GetConversionFactorToSI(to);
    }

    private static double GetConversionFactorToSI(string from)
    {
      switch (from)
      {
        case Tonne: return 1e3;
        case Kilotonne: return 1e6;
        case Gram: return 1e-3;
        case Pound: return 0.45359237;
        case Ton: return 2240 * GetConversionFactor(Pound, Kilogram);
        case Slug: return 14.59390;
        case KilopoundForceSecondSquarePerInch: return ForceUnits.GetConversionFactor(ForceUnits.KilopoundForce, ForceUnits.Newton) * Math.Pow(1, 2) / Units.GetConversionFactor(Units.Inches, Units.Meters);
        case KilopoundForceSecondSquarePerFoot: return ForceUnits.GetConversionFactor(ForceUnits.KilopoundForce, ForceUnits.Newton) * Math.Pow(1, 2) / Units.GetConversionFactor(Units.Feet, Units.Meters);
        case PoundForceSecondSquarePerInch: return ForceUnits.GetConversionFactor(ForceUnits.PoundForce, ForceUnits.Newton) * Math.Pow(1, 2) / Units.GetConversionFactor(Units.Inches, Units.Meters);
        case PoundForceSecondSquarePerFoot: return ForceUnits.GetConversionFactor(ForceUnits.PoundForce, ForceUnits.Newton) * Math.Pow(1, 2) / Units.GetConversionFactor(Units.Feet, Units.Meters);
        case Kilopound: return 1000 * GetConversionFactor(Pound, Kilogram);
        case None: return 1;
      }
      return 1;
    }

    public static string GetUnitsFromString(string unit)
    {
      if (unit == null) return null;
      switch (unit.ToLower())
      {
        case "kg":
        case "kilogram":
        case "kilograms":
          return Kilogram;
        case "t":
        case "tonne":
        case "tonnes":
          return Tonne;
        case "kt":
        case "kilotonne":
        case "kilotonnes":
          return Kilotonne;
        case "g":
        case "gram":
        case "grams":
          return Gram;
        case "lb":
        case "lbs":
        case "pound":
        case "pounds":
          return Pound;
        case "ton":
          return Ton;
        case "sl":
        case "slug":
        case "slugs":
          return Slug;
        case "kip.s2/in":
        case "kip.s²/in":
        case "kips2/in":
        case "kips²/in":
          return KilopoundForceSecondSquarePerInch;
        case "kip.s2/ft":
        case "kip.s²/ft":
        case "kips2/ft":
        case "kips²/ft":
          return KilopoundForceSecondSquarePerFoot;
        case "lbf.s2/in":
        case "lbf.s²/in":
        case "lbfs2/in":
        case "lbfs²/in":
          return KilopoundForceSecondSquarePerInch;
        case "lbf.s2/ft":
        case "lbf.s²/ft":
        case "lbfs2/ft":
        case "lbfs²/ft":
          return KilopoundForceSecondSquarePerFoot;
        case "kip":
        case "kilopound":
        case "kilopounds":
          return Kilopound;
        case "none":
          return None;
      }

      throw new Exception($"Cannot understand what unit {unit} is.");
    }

    public static int GetEncodingFromUnit(string unit)
    {
      switch (unit)
      {
        case Kilogram: return 1;
        case Tonne: return 2;
        case Kilotonne: return 3;
        case Gram: return 4;
        case Pound: return 5;
        case Ton: return 6;
        case Slug: return 7;
        case KilopoundForceSecondSquarePerInch: return 8;
        case KilopoundForceSecondSquarePerFoot: return 9;
        case PoundForceSecondSquarePerInch: return 10;
        case PoundForceSecondSquarePerFoot: return 11;
        case Kilopound: return 12;
      }

      return 0;
    }

    public static string GetUnitFromEncoding(double unit)
    {
      switch (unit)
      {
        case 1: return Kilogram;
        case 2: return Tonne;
        case 3: return Kilotonne;
        case 4: return Gram;
        case 5: return Pound;
        case 6: return Ton;
        case 7: return Slug;
        case 8: return KilopoundForceSecondSquarePerInch;
        case 9: return KilopoundForceSecondSquarePerFoot;
        case 10: return PoundForceSecondSquarePerInch;
        case 11: return PoundForceSecondSquarePerFoot;
        case 12: return Kilopound;
      }

      return None;
    }
  }

  public static class TimeUnits
  {
    public const string Second = "s";
    public const string Millisecond = "ms";
    public const string Minute = "min";
    public const string Hour = "h";
    public const string Day = "d";
    public const string None = "none";

    public static double GetConversionFactor(string from, string to)
    {
      from = GetUnitsFromString(from);
      to = GetUnitsFromString(to);

      return GetConversionFactorToSI(from) / GetConversionFactorToSI(to);
    }

    private static double GetConversionFactorToSI(string from)
    {
      switch (from)
      {
        case Millisecond: return 1e-3;
        case Minute: return 60;
        case Hour: return 60 * 60;
        case Day: return 24 * 60 * 60;
        case None: return 1;
      }
      return 1;
    }

    public static string GetUnitsFromString(string unit)
    {
      if (unit == null) return null;
      switch (unit.ToLower())
      {
        case "s":
        case "second":
        case "seconds":
          return Second;
        case "ms":
        case "mil":
        case "millisecond":
        case "milliseconds":
          return Millisecond;
        case "min":
        case "minute":
        case "minutes":
          return Minute;
        case "h":
        case "hr":
        case "hour":
        case "hours":
          return Hour;
        case "d":
        case "day":
        case "days":
          return Day;
        case "none":
          return None;
      }

      throw new Exception($"Cannot understand what unit {unit} is.");
    }

    public static int GetEncodingFromUnit(string unit)
    {
      switch (unit)
      {
        case Second: return 1;
        case Millisecond: return 2;
        case Minute: return 3;
        case Hour: return 4;
        case Day: return 5;
      }

      return 0;
    }

    public static string GetUnitFromEncoding(double unit)
    {
      switch (unit)
      {
        case 1: return Second;
        case 2: return Millisecond;
        case 3: return Minute;
        case 4: return Hour;
        case 5: return Day;
      }

      return None;
    }
  }

  public static class TemperatureUnits
  {
    public const string Celcius = "°C";
    public const string Kelvin = "K";
    public const string Fahrenheit = "°F";
    public const string None = "none";

    public static double Convert(double v, string from, string to)
    {
      from = GetUnitsFromString(from);
      to = GetUnitsFromString(to);

      return ConvertFromSI(ConvertToSI(v, from), to);
    }

    private static double ConvertToSI(double v, string from)
    {
      switch (from)
      {
        case Celcius: return v+273;
        case Fahrenheit: return (5 / 9) * (v - 32) + 273;
        case None: return v;
      }
      return v;
    }

    private static double ConvertFromSI(double v, string to)
    {
      switch (to)
      {
        case Celcius: return v - 273;
        case Fahrenheit: return 9 / 5 * (v - 273) + 32;
        case None: return v;
      }
      return v;
    }

    public static string GetUnitsFromString(string unit)
    {
      if (unit == null) return null;
      switch (unit.ToLower())
      {
        case "c":
        case "oc":
        case "°c":
        case "celcius":
          return Celcius;
        case "k":
        case "kelvin":
          return Kelvin;
        case "f":
        case "of":
        case "°f":
        case "fahrenheit":
          return Fahrenheit;
        case "none":
          return None;
      }

      throw new Exception($"Cannot understand what unit {unit} is.");
    }

    public static int GetEncodingFromUnit(string unit)
    {
      switch (unit)
      {
        case Celcius: return 1;
        case Kelvin: return 2;
        case Fahrenheit: return 3;
      }

      return 0;
    }

    public static string GetUnitFromEncoding(double unit)
    {
      switch (unit)
      {
        case 1: return Celcius;
        case 2: return Kelvin;
        case 3: return Fahrenheit;
      }

      return None;
    }
  }

  //Electric Current
  //Luminous Intensity
  //Amount of Matter
  #endregion

  #region Other Dimensions
  public static class ForceUnits
  {
    public const string Newton = "N";
    public const string Kilonewton = "kN";
    public const string Meganewtown = "MN";
    public const string PoundForce = "lbf";
    public const string KilopoundForce = "kip";
    public const string TonneForce = "tf";
    public const string None = "none";

    public static double GetConversionFactor(string from, string to)
    {
      from = GetUnitsFromString(from);
      to = GetUnitsFromString(to);

      return GetConversionFactorToSI(from) / GetConversionFactorToSI(to);
    }

    private static double GetConversionFactorToSI(string from)
    {
      switch (from)
      {
        case Kilonewton: return 1e3;
        case Meganewtown: return 1e6;
        case PoundForce: return 4.4482216;
        case KilopoundForce: return 4448.2216;
        case TonneForce: return 9806.65;
        case None: return 1;
      }
      return 1;
    }

    public static string GetUnitsFromString(string unit)
    {
      if (unit == null) return null;
      switch (unit.ToLower())
      {
        case "n":
        case "newton":
        case "newtowns":
          return Newton;
        case "kn":
        case "kilonewton":
        case "kilonewtons":
          return Kilonewton;
        case "mn":
        case "meganewton":
        case "meganewtons":
          return Meganewtown;
        case "lbf":
        case "pound-force":
        case "poundforce":
          return PoundForce;
        case "kip":
        case "kilopound-force":
        case "kilopoundforce":
          return KilopoundForce;
        case "tf":
        case "ton-force":
        case "tonne-force":
        case "tonforce":
        case "tonneforce":
          return TonneForce;
        case "none":
          return None;
      }

      throw new Exception($"Cannot understand what unit {unit} is.");
    }

    public static int GetEncodingFromUnit(string unit)
    {
      switch (unit)
      {
        case Newton: return 1;
        case Kilonewton: return 2;
        case Meganewtown: return 3;
        case PoundForce: return 4;
        case KilopoundForce: return 5;
        case TonneForce: return 6;
      }

      return 0;
    }

    public static string GetUnitFromEncoding(double unit)
    {
      switch (unit)
      {
        case 1: return Newton;
        case 2: return Kilonewton;
        case 3: return Meganewtown;
        case 4: return PoundForce;
        case 5: return KilopoundForce;
        case 6: return TonneForce;
      }

      return None;
    }
  }

  public static class StressUnits
  {
    public const string Pascal = "Pa";
    public const string Kilopascal = "kPa";
    public const string Megapascal = "MPa";
    public const string Gigapascal = "GPa";
    public const string PoundPerSquareInch = "psi";
    public const string PoundPerSquareFoot = "psf";
    public const string KilopoundPerSquareInch = "ksi";
    public const string None = "none";

    public static double GetConversionFactor(string from, string to)
    {
      from = GetUnitsFromString(from);
      to = GetUnitsFromString(to);

      return GetConversionFactorToSI(from) / GetConversionFactorToSI(to);
    }

    private static double GetConversionFactorToSI(string from)
    {
      switch (from)
      {
        case Kilopascal: return 1e3;
        case Megapascal: return 1e6;
        case Gigapascal: return 1e9;
        case PoundPerSquareInch: return ForceUnits.GetConversionFactor(ForceUnits.PoundForce, ForceUnits.Newton) / Math.Pow(Units.GetConversionFactor(Units.Inches, Units.Meters), 2);
        case PoundPerSquareFoot: return ForceUnits.GetConversionFactor(ForceUnits.PoundForce, ForceUnits.Newton) / Math.Pow(Units.GetConversionFactor(Units.Feet, Units.Meters), 2);
        case KilopoundPerSquareInch: return ForceUnits.GetConversionFactor(ForceUnits.KilopoundForce, ForceUnits.Newton) / Math.Pow(Units.GetConversionFactor(Units.Inches, Units.Meters), 2);
        case None: return 1;
      }
      return 1;
    }

    public static string GetUnitsFromString(string unit)
    {
      if (unit == null) return null;
      switch (unit.ToLower())
      {
        case "pa":
        case "kg/m/s2":
        case "kg/m/s²":
        case "n/m2":
        case "n/m²":
        case "pascal":
        case "pascals":
          return Pascal;
        case "kpa":
        case "kn/m2":
        case "kilopascal":
        case "kilopascals":
          return Kilopascal;
        case "mpa":
        case "mn/m2":
        case "mn/m²":
        case "n/mm2":
        case "n/mm²":
        case "megapascal":
        case "megapascals":
          return Megapascal;
        case "gpa":
        case "gn/m2":
        case "gn/m²":
        case "kn/mm2":
        case "kn/mm²":
        case "gigapascal":
        case "gigapascals":
          return Gigapascal;
        case "psi":
        case "lb/in2":
        case "lb/in²":
        case "poundpersquareinch":
        case "poundspersquareinch":
          return PoundPerSquareInch;
        case "psf":
        case "lb/ft2":
        case "lb/ft²":
        case "poundpersquarefoot":
        case "poundspersquarefoot":
          return PoundPerSquareFoot;
        case "ksi":
        case "kip/in2":
        case "kip/in²":
        case "kilopoundpersquareinch":
        case "kilopoundspersquareinch":
          return KilopoundPerSquareInch;
        case "none":
          return None;
      }

      throw new Exception($"Cannot understand what unit {unit} is.");
    }

    public static int GetEncodingFromUnit(string unit)
    {
      switch (unit)
      {
        case Pascal: return 1;
        case Kilopascal: return 2;
        case Megapascal: return 3;
        case Gigapascal: return 4;
        case PoundPerSquareInch: return 5;
        case PoundPerSquareFoot: return 6;
        case KilopoundPerSquareInch: return 7;
      }

      return 0;
    }

    public static string GetUnitFromEncoding(double unit)
    {
      switch (unit)
      {
        case 1: return Pascal;
        case 2: return Kilopascal;
        case 3: return Megapascal;
        case 4: return Gigapascal;
        case 5: return PoundPerSquareInch;
        case 6: return PoundPerSquareFoot;
        case 7: return KilopoundPerSquareInch;
      }

      return None;
    }
  }

  public static class StrainUnits
  {
    public const string Strain = "ε";
    public const string PercentStrain = "%ε";
    public const string Millistrain = "mε";
    public const string Microstrain = "με";
    public const string None = "none";

    public static double GetConversionFactor(string from, string to)
    {
      from = GetUnitsFromString(from);
      to = GetUnitsFromString(to);

      return GetConversionFactorToSI(from) / GetConversionFactorToSI(to);
    }

    private static double GetConversionFactorToSI(string from)
    {
      switch (from)
      {
        case PercentStrain: return 1e2;
        case Millistrain: return 1e3;
        case Microstrain: return 1e6;
        case None: return 1;
      }
      return 1;
    }

    public static string GetUnitsFromString(string unit)
    {
      if (unit == null) return null;
      switch (unit.ToLower())
      {
        case "ε":
        case "strain":
        case "-":
          return Strain;
        case "%ε":
        case "percent-strain":
        case "percentstrain":
          return PercentStrain;
        case "mε":
        case "milli-strain":
        case "millistrain":
          return Millistrain;
        case "με":
        case "micro-strain":
        case "microstrain":
          return Microstrain;
        case "none":
          return None;
      }

      throw new Exception($"Cannot understand what unit {unit} is.");
    }

    public static int GetEncodingFromUnit(string unit)
    {
      switch (unit)
      {
        case Strain: return 1;
        case PercentStrain: return 2;
        case Millistrain: return 3;
        case Microstrain: return 4;
      }

      return 0;
    }

    public static string GetUnitFromEncoding(double unit)
    {
      switch (unit)
      {
        case 1: return Strain;
        case 2: return PercentStrain;
        case 3: return Millistrain;
        case 4: return Microstrain;
      }

      return None;
    }
  }

  public static class VelocityUnits
  {
    public const string MetersPerSecond = "m/s";
    public const string CentimetersPerSecond = "cm/s";
    public const string MillimetersPerSecond = "mm/s";
    public const string FeetPerSecond = "ft/s";
    public const string InchPerSecond = "in/s";
    public const string KilometersPerHour = "kph";
    public const string MilesPerHour = "mph";
    public const string None = "none";

    public static double GetConversionFactor(string from, string to)
    {
      from = GetUnitsFromString(from);
      to = GetUnitsFromString(to);

      return GetConversionFactorToSI(from) / GetConversionFactorToSI(to);
    }

    private static double GetConversionFactorToSI(string from)
    {
      switch (from)
      {
        case CentimetersPerSecond: return Units.GetConversionFactor(Units.Centimeters, Units.Meters) / 1;
        case MillimetersPerSecond: return Units.GetConversionFactor(Units.Millimeters, Units.Meters) / 1;
        case FeetPerSecond: return Units.GetConversionFactor(Units.Feet, Units.Meters) / 1;
        case InchPerSecond: return Units.GetConversionFactor(Units.Inches, Units.Meters) / 1;
        case KilometersPerHour: return Units.GetConversionFactor(Units.Kilometers, Units.Meters) / TimeUnits.GetConversionFactor(TimeUnits.Hour, TimeUnits.Second);
        case MilesPerHour: return Units.GetConversionFactor(Units.Miles, Units.Meters) / TimeUnits.GetConversionFactor(TimeUnits.Hour, TimeUnits.Second);
        case None: return 1;
      }
      return 1;
    }

    public static string GetUnitsFromString(string unit)
    {
      if (unit == null) return null;
      switch (unit.ToLower())
      {
        case "m/s":
          return MetersPerSecond;
        case "cm/s":
          return CentimetersPerSecond;
        case "mm/s":
          return MillimetersPerSecond;
        case "ft/s":
          return FeetPerSecond;
        case "in/s":
          return InchPerSecond;
        case "km/h":
        case "km/hr":
        case "kph":
          return KilometersPerHour;
        case "mi/h":
        case "mi/hr":
        case "mph":
          return MilesPerHour;
        case "none":
          return None;
      }

      throw new Exception($"Cannot understand what unit {unit} is.");
    }

    public static int GetEncodingFromUnit(string unit)
    {
      switch (unit)
      {
        case MetersPerSecond: return 1;
        case CentimetersPerSecond: return 2;
        case MillimetersPerSecond: return 3;
        case FeetPerSecond: return 4;
        case InchPerSecond: return 5;
        case KilometersPerHour: return 6;
        case MilesPerHour: return 7;
      }

      return 0;
    }

    public static string GetUnitFromEncoding(double unit)
    {
      switch (unit)
      {
        case 1: return MetersPerSecond;
        case 2: return CentimetersPerSecond;
        case 3: return MillimetersPerSecond;
        case 4: return FeetPerSecond;
        case 5: return InchPerSecond;
        case 6: return KilometersPerHour;
        case 7: return MilesPerHour;
      }

      return None;
    }
  }

  public static class AccelerationUnits
  {
    public const string MetersPerSquareSecond = "m/s²";
    public const string CentimetersPerSquareSecond = "cm/s²"; //a.k.a Gal
    public const string MillimetersPerSquareSecond = "mm/s²";
    public const string FeetPerSquareSecond = "ft/s²";
    public const string InchPerSquareSecond = "in/s²";
    public const string Gravity = "g";
    public const string PercentGravity = "%g";
    public const string Milligravity = "mg";
    public const string None = "none";

    public static double GetConversionFactor(string from, string to)
    {
      from = GetUnitsFromString(from);
      to = GetUnitsFromString(to);

      return GetConversionFactorToSI(from) / GetConversionFactorToSI(to);
    }

    private static double GetConversionFactorToSI(string from)
    {
      switch (from)
      {
        case CentimetersPerSquareSecond: return Units.GetConversionFactor(Units.Centimeters, Units.Meters);
        case MillimetersPerSquareSecond: return Units.GetConversionFactor(Units.Millimeters, Units.Meters);
        case FeetPerSquareSecond: return Units.GetConversionFactor(Units.Feet, Units.Meters);
        case InchPerSquareSecond: return Units.GetConversionFactor(Units.Inches, Units.Meters);
        case Gravity: return 9.80665;
        case PercentGravity: return GetConversionFactor(Gravity, MetersPerSquareSecond) * 1e-2;
        case Milligravity: return GetConversionFactor(Gravity, MetersPerSquareSecond) * 1e-3;
        case None: return 1;
      }
      return 1;
    }

    public static string GetUnitsFromString(string unit)
    {
      if (unit == null) return null;
      switch (unit.ToLower())
      {
        case "m/s2":
        case "m/s²":
          return MetersPerSquareSecond;
        case "cm/s2":
        case "cm/s²":
        case "gal":
        case "galileo":
          return CentimetersPerSquareSecond;
        case "mm/s2":
        case "mm/s²":
          return MillimetersPerSquareSecond;
        case "ft/s2":
        case "ft/s²":
          return FeetPerSquareSecond;
        case "in/s2":
        case "in/s²":
          return InchPerSquareSecond;
        case "g":
        case "gravity":
          return Gravity;
        case "%g":
        case "percent-gravity":
        case "percent-g":
          return PercentGravity;
        case "mg":
        case "milli-g":
        case "milli-gravity":
          return Milligravity;
        case "none":
          return None;
      }

      throw new Exception($"Cannot understand what unit {unit} is.");
    }

    public static int GetEncodingFromUnit(string unit)
    {
      switch (unit)
      {
        case MetersPerSquareSecond: return 1;
        case CentimetersPerSquareSecond: return 2;
        case MillimetersPerSquareSecond: return 3;
        case FeetPerSquareSecond: return 4;
        case InchPerSquareSecond: return 5;
        case Gravity: return 6;
        case PercentGravity: return 7;
        case Milligravity: return 8;
      }

      return 0;
    }

    public static string GetUnitFromEncoding(double unit)
    {
      switch (unit)
      {
        case 1: return MetersPerSquareSecond;
        case 2: return CentimetersPerSquareSecond;
        case 3: return MillimetersPerSquareSecond;
        case 4: return FeetPerSquareSecond;
        case 5: return InchPerSquareSecond;
        case 6: return Gravity;
        case 7: return PercentGravity;
        case 8: return Milligravity;
      }

      return None;
    }
  }

  public static class EnergyUnits
  {
    public const string Joule = "J";
    public const string Kilojoule = "kJ";
    public const string Megajoule = "MJ";
    public const string Gigajoule = "GJ";
    public const string KilowattHour = "kwh";
    public const string InchPoundForce = "in.lbf";
    public const string FootPoundForce = "ft.lbf";
    public const string Calorie = "cal";
    public const string BritishThermalUnit = "btu";
    public const string None = "none";

    public static double GetConversionFactor(string from, string to)
    {
      from = GetUnitsFromString(from);
      to = GetUnitsFromString(to);

      return GetConversionFactorToSI(from) / GetConversionFactorToSI(to);
    }

    private static double GetConversionFactorToSI(string from)
    {
      switch (from)
      {
        case Kilojoule: return 1e3;
        case Megajoule: return 1e6;
        case Gigajoule: return 1e9;
        case KilowattHour: return 1e3 * TimeUnits.GetConversionFactor(TimeUnits.Hour, TimeUnits.Second);
        case InchPoundForce: return Units.GetConversionFactor(Units.Inches, Units.Meters) * ForceUnits.GetConversionFactor(ForceUnits.PoundForce, ForceUnits.Newton);
        case FootPoundForce: return Units.GetConversionFactor(Units.Feet, Units.Meters) * ForceUnits.GetConversionFactor(ForceUnits.PoundForce, ForceUnits.Newton);
        case Calorie: return 4.184;
        case BritishThermalUnit: return 1055.0559;
        case None: return 1;
      }
      return 1;
    }

    public static string GetUnitsFromString(string unit)
    {
      if (unit == null) return null;
      switch (unit.ToLower())
      {
        case "j":
        case "joule":
        case "joules":
          return Joule;
        case "kj":
        case "kilojoule":
        case "kilojoules":
          return Kilojoule;
        case "mj":
        case "megajoule":
        case "megajoules":
          return Megajoule;
        case "gj":
        case "gigajoule":
        case "gigajoules":
          return Gigajoule;
        case "kwh":
        case "kw.h":
        case "kilowatthour":
        case "kilowatthours":
          return KilowattHour;
        case "inlbf":
        case "in.lbf":
          return InchPoundForce;
        case "ftlbf":
        case "ft.lbf":
          return FootPoundForce;
        case "cal":
        case "calorie":
        case "calories":
          return Calorie;
        case "btu":
        case " britishthermalunit":
          return BritishThermalUnit;
        case "none":
          return None;
      }

      throw new Exception($"Cannot understand what unit {unit} is.");
    }

    public static int GetEncodingFromUnit(string unit)
    {
      switch (unit)
      {
        case Joule: return 1;
        case Kilojoule: return 2;
        case Megajoule: return 3;
        case Gigajoule: return 4;
        case KilowattHour: return 5;
        case InchPoundForce: return 6;
        case FootPoundForce: return 7;
        case Calorie: return 8;
        case BritishThermalUnit: return 9;
      }

      return 0;
    }

    public static string GetUnitFromEncoding(double unit)
    {
      switch (unit)
      {
        case 1: return Joule;
        case 2: return Kilojoule;
        case 3: return Megajoule;
        case 4: return Gigajoule;
        case 5: return KilowattHour;
        case 6: return InchPoundForce;
        case 7: return FootPoundForce;
        case 8: return Calorie;
        case 9: return BritishThermalUnit;
      }

      return None;
    }
  }

  public static class AngleUnits
  {
    public const string Radian = "rad";
    public const string Degree = "deg";
    public const string Gradian = "grad";
    public const string None = "none";

    public static double GetConversionFactor(string from, string to)
    {
      from = GetUnitsFromString(from);
      to = GetUnitsFromString(to);

      return GetConversionFactorToSI(from) / GetConversionFactorToSI(to);
    }

    private static double GetConversionFactorToSI(string from)
    {
      switch (from)
      {
        case Degree: return Math.PI / 180;
        case Gradian: return Math.PI / 200;
        case None: return 1;
      }
      return 1;
    }

    public static string GetUnitsFromString(string unit)
    {
      if (unit == null) return null;
      switch (unit.ToLower())
      {
        case "r":
        case "rad":
        case "radian":
        case "radians":
          return Radian;
        case "°":
        case "o":
        case "deg":
        case "degree":
        case "degrees":
          return Degree;
        case "g":
        case "ᵍ":
        case "grad":
        case "gradian":
        case "gradians":
        case "grade":
        case "gon":
          return Gradian;
        case "none":
          return None;
      }

      throw new Exception($"Cannot understand what unit {unit} is.");
    }

    public static int GetEncodingFromUnit(string unit)
    {
      switch (unit)
      {
        case Radian: return 1;
        case Degree: return 2;
        case Gradian: return 3;
      }

      return 0;
    }

    public static string GetUnitFromEncoding(double unit)
    {
      switch (unit)
      {
        case 1: return Radian;
        case 2: return Degree;
        case 3: return Gradian;
      }

      return None;
    }
  }

  /* Not needed
  public static class DensityUnits
  {
    public const string KilogramsPerCubicMeter = "kg/m3";
    public const string GramsPerCubicCentimeter = "g/cm3";
    public const string SlugsPerCubicFoot = "sl/ft3";
    public const string PoundPerCubicFoot = "lb/ft3";
    public const string None = "none";

    public static double GetConversionFactor(string from, string to)
    {
      from = GetUnitsFromString(from);
      to = GetUnitsFromString(to);

      return GetConversionFactorToSI(from) * GetConversionFactorFromSI(to);
    }

    private static double GetConversionFactorToSI(string from)
    {
      switch (from)
      {
        case DensityUnits.GramsPerCubicCentimeter: return 1e3;
        case DensityUnits.SlugsPerCubicFoot: return MassUnits.GetConversionFactor(MassUnits.Slug, MassUnits.Kilogram) / Math.Pow(Units.GetConversionFactor(Units.Feet, Units.Meters), 3);
        case DensityUnits.PoundPerCubicFoot: return MassUnits.GetConversionFactor(MassUnits.Pound, MassUnits.Kilogram) / Math.Pow(Units.GetConversionFactor(Units.Feet, Units.Meters), 3);
        case DensityUnits.None: return 1;
      }
      return 1;
    }

    private static double GetConversionFactorFromSI(string to)
    {
      switch (to)
      {
        case DensityUnits.GramsPerCubicCentimeter: return 1e-3;
        case DensityUnits.SlugsPerCubicFoot: return MassUnits.GetConversionFactor(MassUnits.Kilogram, MassUnits.Slug) / Math.Pow(Units.GetConversionFactor(Units.Meters, Units.Feet), 3);
        case DensityUnits.PoundPerCubicFoot: return MassUnits.GetConversionFactor(MassUnits.Kilogram, MassUnits.Pound) / Math.Pow(Units.GetConversionFactor(Units.Meters, Units.Feet), 3);
        case DensityUnits.None: return 1;
      }
      return 1;
    }

    public static string GetUnitsFromString(string unit)
    {
      if (unit == null) return null;
      switch (unit.ToLower())
      {
        case "kg/m3":
          return DensityUnits.KilogramsPerCubicMeter;
        case "g/cm3":
          return DensityUnits.GramsPerCubicCentimeter;
        case "sl/ft3":
          return DensityUnits.SlugsPerCubicFoot;
        case "none":
          return DensityUnits.None;
      }

      throw new Exception($"Cannot understand what unit {unit} is.");
    }

    public static int GetEncodingFromUnit(string unit)
    {
      switch (unit)
      {
        case KilogramsPerCubicMeter: return 1;
        case GramsPerCubicCentimeter: return 2;
        case SlugsPerCubicFoot: return 3;
        case PoundPerCubicFoot: return 4;
      }

      return 0;
    }

    public static string GetUnitFromEncoding(double unit)
    {
      switch (unit)
      {
        case 1: return KilogramsPerCubicMeter;
        case 2: return GramsPerCubicCentimeter;
        case 3: return SlugsPerCubicFoot;
        case 4: return PoundPerCubicFoot;
      }

      return None;
    }
  }

  public static class MomentUnits
  {
    public const string NewtownMeter = "Nm";
    public const string KilonewtownMeter = "kNm";
    public const string MeganewtownMeter = "MNm";
    public const string NewtownMillimeter = "Nmm";
    public const string None = "none";

    public static double GetConversionFactor(string from, string to)
    {
      from = GetUnitsFromString(from);
      to = GetUnitsFromString(to);

      return GetConversionFactorToSI(from) * GetConversionFactorFromSI(to);
    }

    private static double GetConversionFactorToSI(string from)
    {
      switch (from)
      {
        case MomentUnits.KilonewtownMeter: return 1e3;
        case MomentUnits.MeganewtownMeter: return 1e6;
        case MomentUnits.NewtownMillimeter: return 1e-3;
        case MomentUnits.None: return 1;
      }
      return 1;
    }

    private static double GetConversionFactorFromSI(string to)
    {
      switch (to)
      {
        case MomentUnits.KilonewtownMeter: return 1e-3;
        case MomentUnits.MeganewtownMeter: return 1e-6;
        case MomentUnits.NewtownMillimeter: return 1e3;
        case MomentUnits.None: return 1;
      }
      return 1;
    }

    public static string GetUnitsFromString(string unit)
    {
      if (unit == null) return null;
      switch (unit.ToLower())
      {
        case "nm":
          return MomentUnits.NewtownMeter;
        case "knm":
          return MomentUnits.KilonewtownMeter;
        case "mnm":
          return MomentUnits.MeganewtownMeter;
        case "nmm":
          return MomentUnits.NewtownMillimeter;
        case "none":
          return MomentUnits.None;
      }

      throw new Exception($"Cannot understand what unit {unit} is.");
    }

    public static int GetEncodingFromUnit(string unit)
    {
      switch (unit)
      {
        case NewtownMeter: return 1;
        case KilonewtownMeter: return 2;
        case MeganewtownMeter: return 3;
        case NewtownMillimeter: return 4;
      }

      return 0;
    }

    public static string GetUnitFromEncoding(double unit)
    {
      switch (unit)
      {
        case 1: return NewtownMeter;
        case 2: return KilonewtownMeter;
        case 3: return MeganewtownMeter;
        case 4: return NewtownMillimeter;
      }

      return None;
    }
  }*/

  #endregion
}
