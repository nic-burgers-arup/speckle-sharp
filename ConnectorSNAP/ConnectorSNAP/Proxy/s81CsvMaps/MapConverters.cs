using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Speckle.SNAP.API;
using System;

namespace ConnectorSNAP.Proxy.s81CsvMaps
{
  internal class EnumStringConverter<T> : DefaultTypeConverter where T : struct, IConvertible
  {
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) => (Enum.TryParse(text, true, out T result)) ? result : default(T);

    public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData) => ((T)value).GetStringValue(true);
  }

  internal class EnumIntConverter<T> : DefaultTypeConverter where T : struct, IConvertible
  {
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) => (Enum.TryParse(text, true, out T result)) ? result : default(T);

    public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData) => ((int)value).ToString();
  }

  internal class IntBoolConverter : DefaultTypeConverter
  {
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) => ToBool(text);
    public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData) => ToString((bool)value);
    public static bool ToBool(string v) => (v != "0");
    public static string ToString(bool v) => v ? "1" : "0";
  }
}
