﻿using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Objects.BuiltElements.Revit;
using Speckle.Core.Models;
using Column = Objects.BuiltElements.Column;
using DB = Autodesk.Revit.DB;
using Line = Objects.Geometry.Line;
using Point = Objects.Geometry.Point;

namespace Objects.Converter.Revit
{
  public partial class ConverterRevit
  {
    public List<ApplicationPlaceholderObject> ColumnToNative(Column speckleColumn, StructuralType structuralType = StructuralType.NonStructural)
    {
      if (speckleColumn.baseLine == null)
      {
        throw new Speckle.Core.Logging.SpeckleException("Only line based Beams are currently supported.");
      }

      DB.FamilySymbol familySymbol = GetElementType<FamilySymbol>(speckleColumn);

      var baseLine = CurveToNative(speckleColumn.baseLine).get_Item(0);
      var startPoint = baseLine.GetEndPoint(0);
      var endPoint = baseLine.GetEndPoint(1);

      // If the start point elevation is higher than the end point elevation, reverse the line.
      if (startPoint.Z > endPoint.Z)
      {
        baseLine = DB.Line.CreateBound(endPoint, startPoint);
      }

      DB.Level level = null;
      DB.Level topLevel = null;
      DB.FamilyInstance revitColumn = null;
      
      var isLineBased = true;

      var speckleRevitColumn = speckleColumn as RevitColumn;

      // If family name or type not present in Revit model, add speckle section info as instance parameters
      if (familySymbol.FamilyName != speckleRevitColumn.family || familySymbol.Name != speckleRevitColumn.type)
      {
        var paramNames = new List<string> { "Section Family", "Section Type" };
        var paramValues = new List<object> { speckleRevitColumn.family, speckleRevitColumn.type };
        speckleRevitColumn.parameters = AddSpeckleParameters(speckleRevitColumn.parameters, paramNames, paramValues);
      }

      if (speckleRevitColumn != null)
      {
        level = LevelToNative(speckleRevitColumn.level);
        topLevel = LevelToNative(speckleRevitColumn.topLevel);
        //non slanted columns are point based
        isLineBased = speckleRevitColumn.isSlanted;
      }

      if (level == null)
      {
        level = LevelToNative(LevelFromCurve(baseLine));
        topLevel = LevelToNative(LevelFromPoint(baseLine.GetEndPoint(1)));
      }

      //try update existing 
      var docObj = GetExistingElementByApplicationId(speckleColumn.applicationId);
      bool isUpdate = false;
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
            revitColumn = (DB.FamilyInstance)revitElement;
            switch (revitColumn.Location)
            {
              case LocationCurve crv:
                crv.Curve = baseLine;
                break;
              case LocationPoint pt:
                pt.Point = startPoint;
                break;
            }

            // check for a type change
            if (!string.IsNullOrEmpty(familySymbol.Name) && familySymbol.Name != revitElement.Name)
            {
              revitColumn.ChangeTypeId(familySymbol.Id);
            }
          }
          isUpdate = true;
        }
        catch { }
      }

      if (revitColumn == null && isLineBased)
      {
        revitColumn = Doc.Create.NewFamilyInstance(baseLine, familySymbol, level, structuralType);
        if (revitColumn.Symbol.Family.FamilyPlacementType == FamilyPlacementType.CurveDrivenStructural)
        {
          StructuralFramingUtils.DisallowJoinAtEnd(revitColumn, 0);
          StructuralFramingUtils.DisallowJoinAtEnd(revitColumn, 1);
        }
      }

      //try with a point based column
      if (speckleRevitColumn != null && revitColumn == null && !isLineBased)
      {
        var basePoint = startPoint.Z < endPoint.Z ? startPoint : endPoint; // pick the lowest
        revitColumn = Doc.Create.NewFamilyInstance(basePoint, familySymbol, level, structuralType);
        //
        //rotate, we know it must be a RevitColumn
        var axis = DB.Line.CreateBound(new XYZ(basePoint.X, basePoint.Y, 0), new XYZ(basePoint.X, basePoint.Y, 1000));
        var rotationAngle = speckleRevitColumn.rotation - (revitColumn.Location as LocationPoint).Rotation;

        // This call is time-consuming so only call if section actually requires rotation
        if(rotationAngle != 0)
        {
          (revitColumn.Location as LocationPoint).Rotate(axis, rotationAngle);
        }
      }

      if (revitColumn == null)
      {
        throw (new Exception($"Failed to create column for {speckleColumn.applicationId}."));
      }

      TrySetParam(revitColumn, BuiltInParameter.FAMILY_BASE_LEVEL_PARAM, level);
      TrySetParam(revitColumn, BuiltInParameter.FAMILY_TOP_LEVEL_PARAM, topLevel);

      if (speckleRevitColumn != null)
      {
        if (speckleRevitColumn.handFlipped != revitColumn.HandFlipped)
        {
          revitColumn.flipHand();
        }

        if (speckleRevitColumn.facingFlipped != revitColumn.FacingFlipped)
        {
          revitColumn.flipFacing();
        }

        //do change offset for slanted columns, it's automatic
        if (!isLineBased)
          SetOffsets(revitColumn, speckleRevitColumn);

        SetInstanceParameters(revitColumn, speckleRevitColumn);
      }

      var placeholders = new List<ApplicationPlaceholderObject>() { new ApplicationPlaceholderObject { applicationId = speckleColumn.applicationId, ApplicationGeneratedId = revitColumn.UniqueId, NativeObject = revitColumn } };

      // TODO: nested elements.
      Report.Log($"{(isUpdate ? "Updated" : "Created")} Column {revitColumn.Id}");
      return placeholders;
    }

    /// <summary>
    /// Some families eg columns, need offsets to be set in a specific way. This tries to cover that.
    /// </summary>
    /// <param name="speckleElement"></param>
    /// <param name="familyInstance"></param>
    private void SetOffsets(DB.FamilyInstance familyInstance, RevitColumn speckleRevitColumn)
    {
      var topOffsetParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);
      var baseOffsetParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);
      var baseLevelParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
      var topLevelParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);

      if (topLevelParam == null || baseLevelParam == null || baseOffsetParam == null || topOffsetParam == null)
      {
        return;
      }

      var baseOffset = ScaleToNative(speckleRevitColumn.baseOffset, speckleRevitColumn.units);
      var topOffset = ScaleToNative(speckleRevitColumn.topOffset, speckleRevitColumn.units);

      //these have been set previously
      //DB.Level level = Doc.GetElement(baseLevelParam.AsElementId()) as DB.Level;
      //DB.Level topLevel = Doc.GetElement(topLevelParam.AsElementId()) as DB.Level;

      //checking if BASE offset needs to be set before or after TOP offset
      //      if ((topLevel != null && level.Elevation + baseOffset == topLevel.Elevation) ||
      //       (topLevel!=null && topLevel.Elevation == level.Elevation && baseOffset > 0)) //edge case
      //    {
      baseOffsetParam.Set(baseOffset);
      topOffsetParam.Set(topOffset);
      //    }
      //    else
      //    {
      //       topOffsetParam.Set(topOffset);
      //      baseOffsetParam.Set(baseOffset);
      //    }

    }

    public Base ColumnToSpeckle(DB.FamilyInstance revitColumn)
    {
      var symbol = Doc.GetElement(revitColumn.GetTypeId()) as FamilySymbol;

      var speckleColumn = new RevitColumn();
      speckleColumn.family = symbol.FamilyName;
      speckleColumn.type = Doc.GetElement(revitColumn.GetTypeId()).Name;
      speckleColumn.level = ConvertAndCacheLevel(revitColumn, BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
      speckleColumn.topLevel = ConvertAndCacheLevel(revitColumn, BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
      speckleColumn.baseOffset = GetParamValue<double>(revitColumn, BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);
      speckleColumn.topOffset = GetParamValue<double>(revitColumn, BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);
      speckleColumn.facingFlipped = revitColumn.FacingFlipped;
      speckleColumn.handFlipped = revitColumn.HandFlipped;
      speckleColumn.isSlanted = revitColumn.IsSlantedColumn;

      //geometry
      var baseGeometry = LocationToSpeckle(revitColumn);
      var baseLine = baseGeometry as ICurve;

      //make line from point and height
      if (baseLine == null && baseGeometry is Point basePoint)
      {
        var elevation = ConvertAndCacheLevel(revitColumn, BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).elevation;
        baseLine = new Line(basePoint, new Point(basePoint.x, basePoint.y, elevation + speckleColumn.topOffset, ModelUnits), ModelUnits);
      }

      if (baseLine == null)
      {
        return RevitElementToSpeckle(revitColumn);
      }

      speckleColumn.baseLine = baseLine; //all speckle columns should be line based

      GetAllRevitParamsAndIds(speckleColumn, revitColumn,
        new List<string> { "FAMILY_BASE_LEVEL_PARAM", "FAMILY_TOP_LEVEL_PARAM", "FAMILY_BASE_LEVEL_OFFSET_PARAM", "FAMILY_TOP_LEVEL_OFFSET_PARAM", "SCHEDULE_BASE_LEVEL_OFFSET_PARAM", "SCHEDULE_TOP_LEVEL_OFFSET_PARAM" });

      if (revitColumn.Location is LocationPoint)
      {
        speckleColumn.rotation = ((LocationPoint)revitColumn.Location).Rotation;
      }

      speckleColumn.displayValue = GetElementMesh(revitColumn);

      return speckleColumn;
    }

  }
}
