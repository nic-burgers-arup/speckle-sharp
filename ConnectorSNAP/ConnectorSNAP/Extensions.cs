using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ConnectorSNAP
{
  public static class Extensions
  {
    public static IEnumerable<Type> GetBaseClasses(this Type type)
    {
      return type.BaseType == typeof(object)
          ? type.GetInterfaces()
          : Enumerable
              .Repeat(type.BaseType, 1)
              .Concat(type.BaseType.GetBaseClasses())
              .Distinct();
    }

  }
}
