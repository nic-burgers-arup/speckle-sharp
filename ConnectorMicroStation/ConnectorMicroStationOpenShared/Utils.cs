﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Speckle.Core.Kits;

using Bentley.DgnPlatformNET;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;

namespace Speckle.ConnectorMicroStationOpen
{
  public static class Utils
  {
#if MICROSTATION
    public static string BentleyAppName = Applications.MicroStation;
    public static string AppName = "MicroStation";
#elif OPENROADS
    public static string BentleyAppName = Applications.OpenRoads;
    public static string AppName = "OpenRoads";
#elif OPENRAIL
    public static string BentleyAppName = Applications.OpenRail;
    public static string AppName = "OpenRail";
#elif OPENBUILDINGS
    public static string BentleyAppName = Applications.OpenBuildings;
    public static string AppName = "OpenBuildings";
#endif

    /// <summary>
    /// Gets the ids of all visible model objects that can be converted to Speckle
    /// </summary>
    /// <param name="model"></param>
    /// <param name="converter"></param>
    /// <returns></returns>
    public static List<string> ConvertibleObjects(this DgnModel model, ISpeckleConverter converter)
    {
      var objs = new List<string>();

      if (model == null)
      {
        return new List<string>();
      }

      var graphicElements = model.GetGraphicElements();
      var elementEnumerator = (ModelElementsEnumerator)graphicElements.GetEnumerator();
      var elements = graphicElements.Where(el => !el.IsInvisible).Select(el => el).ToList();

      foreach (var element in elements)
      {
        if (converter.CanConvertToSpeckle(element) && !element.IsInvisible)
          objs.Add(element.ElementId.ToString());
      }

      objs = graphicElements.Where(el => !el.IsInvisible).Select(el => el.ElementId.ToString()).ToList();
      return objs;
    }
  }
}
