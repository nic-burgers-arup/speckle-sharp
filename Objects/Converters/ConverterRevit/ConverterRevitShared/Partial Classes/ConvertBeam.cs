﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Objects.BuiltElements;
using Objects.BuiltElements.Revit;
using Speckle.Core.Models;
using System.Collections.Generic;
using DB = Autodesk.Revit.DB;

namespace Objects.Converter.Revit
{
  public partial class ConverterRevit
  {
    public List<ApplicationPlaceholderObject> BeamToNative(Beam speckleBeam, StructuralType structuralType = StructuralType.Beam)
    {
      
      if (speckleBeam.baseLine == null)
      {
        throw new Speckle.Core.Logging.SpeckleException("Only line based Beams are currently supported.");
      }

      DB.FamilySymbol familySymbol = GetElementType<FamilySymbol>(speckleBeam);
      var baseLine = CurveToNative(speckleBeam.baseLine).get_Item(0);
      DB.Level level = null;
      DB.FamilyInstance revitBeam = null;

      //comes from revit or schema builder, has these props
      var speckleRevitBeam = speckleBeam as RevitBeam;

      // If family name or type not present in Revit model, add speckle section info as instance parameters
      if (familySymbol.FamilyName != speckleRevitBeam.family || familySymbol.Name != speckleRevitBeam.type)
      {
        var paramNames = new List<string> { "Section Family", "Section Type" };
        var paramValues = new List<object> { speckleRevitBeam.family, speckleRevitBeam.type };
        speckleRevitBeam.parameters = AddSpeckleParameters(speckleRevitBeam.parameters, paramNames, paramValues);
        Report.Log($"Instance parameters containing family name and family type added to Beam");
      }
      
      if (speckleRevitBeam != null)
      {
        if (level != null)
        {
          level = GetLevelByName(speckleRevitBeam.level.name);
        }
      }

      level ??= ConvertLevelToRevit(speckleRevitBeam?.level ?? LevelFromCurve(baseLine));

      var isUpdate = false;

      //try update existing 
      var docObj = GetExistingElementByApplicationId(speckleBeam.applicationId);

      if (docObj != null)
      {
        try
        {
          var analyticalStick = docObj as AnalyticalModelStick;
          var revitType = Doc.GetElement(docObj.GetTypeId()) as ElementType;

          // Gets physical element associated with analytical element
          var revitElement = Doc.GetElement(analyticalStick.GetElementId()) as DB.FamilyInstance;

          // if family changed, tough luck. delete and let us create a new one.
          if (familySymbol.FamilyName != revitElement.Symbol.FamilyName)
          {
            Doc.Delete(docObj.Id);
          }
          else
          {
            revitBeam = (DB.FamilyInstance)revitElement;
            (revitBeam.Location as LocationCurve).Curve = baseLine;

            // check for a type change
            if (!string.IsNullOrEmpty(familySymbol.FamilyName) && familySymbol.FamilyName != revitElement.Name)
            {
              revitBeam.ChangeTypeId(familySymbol.Id);
            }
          }
          isUpdate = true;
        }
        catch
        {
          //something went wrong, re-create it
        }
      }

      //create family instance
      if (revitBeam == null)
      {
        revitBeam = Doc.Create.NewFamilyInstance(baseLine, familySymbol, level, structuralType);

        if (structuralType == StructuralType.Beam)
        {
          StructuralFramingUtils.DisallowJoinAtEnd(revitBeam, 0);
          StructuralFramingUtils.DisallowJoinAtEnd(revitBeam, 1);
        }
      }

      //reference level, only for beams
      TrySetParam(revitBeam, BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM, level);

      if (speckleRevitBeam != null)
      {
        SetInstanceParameters(revitBeam, speckleRevitBeam);
      }

      // TODO: get sub families, it's a family! 
      var placeholders = new List<ApplicationPlaceholderObject>() { new ApplicationPlaceholderObject { applicationId = speckleBeam.applicationId, ApplicationGeneratedId = revitBeam.UniqueId, NativeObject = revitBeam } };

      // TODO: nested elements.

      Report.Log($"{(isUpdate ? "Updated" : "Created")} Beam {revitBeam.Id}");

      return placeholders;
    }

    private Base BeamToSpeckle(DB.FamilyInstance revitBeam)
    {
      var baseGeometry = LocationToSpeckle(revitBeam);
      var baseLine = baseGeometry as ICurve;
      if (baseLine == null)
      {
        Report.Log($"Beam has no valid baseline, converting as generic element {revitBeam.Id}");
        return RevitElementToSpeckle(revitBeam);
      }
      var symbol = Doc.GetElement(revitBeam.GetTypeId()) as FamilySymbol;

      var speckleBeam = new RevitBeam();
      speckleBeam.family = symbol.FamilyName;
      speckleBeam.type = revitBeam.Document.GetElement(revitBeam.GetTypeId()).Name;
      speckleBeam.baseLine = baseLine;
      speckleBeam.level = ConvertAndCacheLevel(revitBeam, BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM);
      speckleBeam.displayValue = GetElementMesh(revitBeam);

      GetAllRevitParamsAndIds(speckleBeam, revitBeam);

      Report.Log($"Converted Beam {revitBeam.Id}");
      return speckleBeam;
    }
  }
}
