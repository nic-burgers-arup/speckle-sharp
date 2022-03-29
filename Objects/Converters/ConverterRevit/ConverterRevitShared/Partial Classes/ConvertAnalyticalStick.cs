using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Objects.BuiltElements;
using Objects.BuiltElements.Revit;
using Objects.Structural.Geometry;
using Objects.Structural.Properties;
using Objects.Structural.Properties.Profiles;
using Objects.Structural.Materials;
using Speckle.Core.Models;
using DB = Autodesk.Revit.DB;

namespace Objects.Converter.Revit
{
  public partial class ConverterRevit
  {
    public List<ApplicationPlaceholderObject> AnalyticalStickToNative(Element1D speckleStick)
    {
      List<ApplicationPlaceholderObject> placeholderObjects = new List<ApplicationPlaceholderObject> { };
      XYZ offset1 = VectorToNative(speckleStick.end1Offset);
      XYZ offset2 = VectorToNative(speckleStick.end2Offset);
      List<ApplicationPlaceholderObject> placeholders = new List<ApplicationPlaceholderObject> { };

      switch (speckleStick.memberType)
      {
        case MemberType.Beam:
          RevitBeam revitBeam = new RevitBeam();
          revitBeam.applicationId = speckleStick.applicationId;
          //This only works for CSIC sections now for sure. Need to test on other sections
          revitBeam.type = ParseFamilyTypeFromProperty(speckleStick.property.name);
          revitBeam.baseLine = speckleStick.baseLine;
          //Beam beam = new Beam(speckleStick.baseLine);
          revitBeam.family = ParseFamilyNameFromProperty(speckleStick.property.name);
          placeholders = BeamToNative(revitBeam);
          DB.FamilyInstance nativeRevitBeam = (DB.FamilyInstance)placeholders[0].NativeObject;
          AnalyticalModelStick analyticalModel = (AnalyticalModelStick)nativeRevitBeam.GetAnalyticalModel();
          analyticalModel.SetReleases(true, Convert.ToBoolean(speckleStick.end1Releases.stiffnessX), Convert.ToBoolean(speckleStick.end1Releases.stiffnessY), Convert.ToBoolean(speckleStick.end1Releases.stiffnessZ), Convert.ToBoolean(speckleStick.end1Releases.stiffnessXX), Convert.ToBoolean(speckleStick.end1Releases.stiffnessYY), Convert.ToBoolean(speckleStick.end1Releases.stiffnessZZ));
          analyticalModel.SetReleases(false, Convert.ToBoolean(speckleStick.end2Releases.stiffnessX), Convert.ToBoolean(speckleStick.end2Releases.stiffnessY), Convert.ToBoolean(speckleStick.end2Releases.stiffnessZ), Convert.ToBoolean(speckleStick.end2Releases.stiffnessXX), Convert.ToBoolean(speckleStick.end2Releases.stiffnessYY), Convert.ToBoolean(speckleStick.end2Releases.stiffnessZZ));
          analyticalModel.SetOffset(AnalyticalElementSelector.StartOrBase, offset1);
          analyticalModel.SetOffset(AnalyticalElementSelector.EndOrTop, offset2);
          return placeholders;
        //case ElementType1D.Brace:
        //  RevitBrace revitBrace = new RevitBrace();
        //  revitBrace.type = speckleStick.property.name.Replace('X', 'x');
        //  revitBrace.baseLine = speckleStick.baseLine;
        //  //Brace brace = new Brace(speckleStick.baseLine);
        //  placeholders = BraceToNative(revitBrace);
        //  DB.FamilyInstance nativeRevitBrace = (DB.FamilyInstance)placeholders[0].NativeObject;
        //  analyticalModel = (AnalyticalModelStick)nativeRevitBrace.GetAnalyticalModel();
        //  analyticalModel.SetReleases(true, Convert.ToBoolean(speckleStick.end1Releases.stiffnessX), Convert.ToBoolean(speckleStick.end1Releases.stiffnessY), Convert.ToBoolean(speckleStick.end1Releases.stiffnessZ), Convert.ToBoolean(speckleStick.end1Releases.stiffnessXX), Convert.ToBoolean(speckleStick.end1Releases.stiffnessYY), Convert.ToBoolean(speckleStick.end1Releases.stiffnessZZ));
        //  analyticalModel.SetReleases(false, Convert.ToBoolean(speckleStick.end2Releases.stiffnessX), Convert.ToBoolean(speckleStick.end2Releases.stiffnessY), Convert.ToBoolean(speckleStick.end2Releases.stiffnessZ), Convert.ToBoolean(speckleStick.end2Releases.stiffnessXX), Convert.ToBoolean(speckleStick.end2Releases.stiffnessYY), Convert.ToBoolean(speckleStick.end2Releases.stiffnessZZ));
        //  analyticalModel.SetOffset(AnalyticalElementSelector.StartOrBase, offset1);
        //  analyticalModel.SetOffset(AnalyticalElementSelector.EndOrTop, offset2);
        //  return placeholders;
        case MemberType.Column:
          RevitColumn revitColumn = new RevitColumn();
          revitColumn.applicationId = speckleStick.applicationId;
          revitColumn.type = ParseFamilyTypeFromProperty(speckleStick.property.name);
          revitColumn.baseLine = speckleStick.baseLine;
          revitColumn.units = speckleStick.end1Offset.units; // column units are used for setting offset
          revitColumn.family = ParseFamilyNameFromProperty(speckleStick.property.name);
          placeholders = ColumnToNative(revitColumn, StructuralType.Column);
          DB.FamilyInstance nativeRevitColumn = (DB.FamilyInstance)placeholders[0].NativeObject;
          AnalyticalModelColumn analyticalModelCol = (AnalyticalModelColumn)nativeRevitColumn.GetAnalyticalModel();
          analyticalModelCol.SetReleases(true, Convert.ToBoolean(speckleStick.end1Releases.stiffnessX), Convert.ToBoolean(speckleStick.end1Releases.stiffnessY), Convert.ToBoolean(speckleStick.end1Releases.stiffnessZ), Convert.ToBoolean(speckleStick.end1Releases.stiffnessXX), Convert.ToBoolean(speckleStick.end1Releases.stiffnessYY), Convert.ToBoolean(speckleStick.end1Releases.stiffnessZZ));
          analyticalModelCol.SetReleases(false, Convert.ToBoolean(speckleStick.end2Releases.stiffnessX), Convert.ToBoolean(speckleStick.end2Releases.stiffnessY), Convert.ToBoolean(speckleStick.end2Releases.stiffnessZ), Convert.ToBoolean(speckleStick.end2Releases.stiffnessXX), Convert.ToBoolean(speckleStick.end2Releases.stiffnessYY), Convert.ToBoolean(speckleStick.end2Releases.stiffnessZZ));
          analyticalModelCol.SetOffset(AnalyticalElementSelector.StartOrBase, offset1);
          analyticalModelCol.SetOffset(AnalyticalElementSelector.EndOrTop, offset2);
          return placeholders;
      }
      return placeholderObjects;
    }

    private Element1D AnalyticalStickToSpeckle(AnalyticalModelStick revitStick)
    {
      if (!revitStick.IsEnabled())
        return null;

      var speckleElement1D = new Element1D();
      switch (revitStick.Category.Name)
      {
        case "Analytical Columns":
          speckleElement1D.memberType = MemberType.Column;
          speckleElement1D.type = ElementType1D.Column;
          break;
        case "Analytical Beams":
          speckleElement1D.memberType = MemberType.Beam;
          speckleElement1D.type = ElementType1D.Beam;
          break;
        case "Analytical Braces":
          speckleElement1D.memberType = MemberType.Beam;
          speckleElement1D.type = ElementType1D.Brace;
          break;
        default:
          speckleElement1D.memberType = MemberType.Generic1D;
          speckleElement1D.type = ElementType1D.Beam;
          break;
      }

      var curves = revitStick.GetCurves(AnalyticalCurveType.RigidLinkHead).ToList();
      curves.AddRange(revitStick.GetCurves(AnalyticalCurveType.ActiveCurves));
      curves.AddRange(revitStick.GetCurves(AnalyticalCurveType.RigidLinkTail));

      if (curves.Count > 1)
      {
        var curveList = CurveListToSpeckle(curves);
        var firstSegment = (Geometry.Line)curveList.segments[0];
        var lastSegment = (Geometry.Line)curveList.segments[-1];
        var baseLine = new Geometry.Line(firstSegment.start, lastSegment.end);
        speckleElement1D.baseLine = baseLine;
      }
      else
        speckleElement1D.baseLine = LineToSpeckle((Line)curves[0]);

      var coordinateSystem = revitStick.GetLocalCoordinateSystem();
      if (coordinateSystem != null)
        speckleElement1D.localAxis = new Geometry.Plane(PointToSpeckle(coordinateSystem.Origin), VectorToSpeckle(coordinateSystem.BasisZ), VectorToSpeckle(coordinateSystem.BasisX), VectorToSpeckle(coordinateSystem.BasisY));

      var startOffset = revitStick.GetOffset(AnalyticalElementSelector.StartOrBase);
      var endOffset = revitStick.GetOffset(AnalyticalElementSelector.EndOrTop);
      speckleElement1D.end1Offset = VectorToSpeckle(startOffset);
      speckleElement1D.end2Offset = VectorToSpeckle(endOffset);

      var startRelease = GetParamValue<int>(revitStick, BuiltInParameter.STRUCTURAL_START_RELEASE_TYPE);
      var endRelease = GetParamValue<int>(revitStick, BuiltInParameter.STRUCTURAL_END_RELEASE_TYPE);
      if (startRelease == 0)
        speckleElement1D.end1Releases = new Restraint(RestraintType.Fixed);
      else
      {
        var botReleaseX = GetParamValue<int>(revitStick, BuiltInParameter.STRUCTURAL_BOTTOM_RELEASE_FX) == 1 ? "R" : "F";
        var botReleaseY = GetParamValue<int>(revitStick, BuiltInParameter.STRUCTURAL_BOTTOM_RELEASE_FY) == 1 ? "R" : "F";
        var botReleaseZ = GetParamValue<int>(revitStick, BuiltInParameter.STRUCTURAL_BOTTOM_RELEASE_FZ) == 1 ? "R" : "F";
        var botReleaseXX = GetParamValue<int>(revitStick, BuiltInParameter.STRUCTURAL_BOTTOM_RELEASE_MX) == 1 ? "R" : "F";
        var botReleaseYY = GetParamValue<int>(revitStick, BuiltInParameter.STRUCTURAL_BOTTOM_RELEASE_MY) == 1 ? "R" : "F";
        var botReleaseZZ = GetParamValue<int>(revitStick, BuiltInParameter.STRUCTURAL_BOTTOM_RELEASE_MZ) == 1 ? "R" : "F";

        string botReleaseCode = botReleaseX + botReleaseY + botReleaseZ + botReleaseXX + botReleaseYY + botReleaseZZ;
        speckleElement1D.end1Releases = new Restraint(botReleaseCode);
      }

      if (endRelease == 0)
        speckleElement1D.end2Releases = new Restraint(RestraintType.Fixed);
      else
      {
        var topReleaseX = GetParamValue<int>(revitStick, BuiltInParameter.STRUCTURAL_TOP_RELEASE_FX) == 1 ? "R" : "F";
        var topReleaseY = GetParamValue<int>(revitStick, BuiltInParameter.STRUCTURAL_TOP_RELEASE_FY) == 1 ? "R" : "F";
        var topReleaseZ = GetParamValue<int>(revitStick, BuiltInParameter.STRUCTURAL_TOP_RELEASE_FZ) == 1 ? "R" : "F";
        var topReleaseXX = GetParamValue<int>(revitStick, BuiltInParameter.STRUCTURAL_TOP_RELEASE_MX) == 1 ? "R" : "F";
        var topReleaseYY = GetParamValue<int>(revitStick, BuiltInParameter.STRUCTURAL_TOP_RELEASE_MY) == 1 ? "R" : "F";
        var topReleaseZZ = GetParamValue<int>(revitStick, BuiltInParameter.STRUCTURAL_TOP_RELEASE_MZ) == 1 ? "R" : "F";

        string topReleaseCode = topReleaseX + topReleaseY + topReleaseZ + topReleaseXX + topReleaseYY + topReleaseZZ;
        speckleElement1D.end2Releases = new Restraint(topReleaseCode);
      }

      var prop = new Property1D();

      var stickFamily = (Autodesk.Revit.DB.FamilyInstance)Doc.GetElement(revitStick.GetElementId());
      var section = stickFamily.Symbol.GetStructuralSection();
      var speckleSection = new SectionProfile();
      speckleSection.name = section.StructuralSectionShapeName;

      // If section general shape enum is not defined, us section shape enum to derive profile
      if (section.StructuralSectionGeneralShape != DB.Structure.StructuralSections.StructuralSectionGeneralShape.NotDefined)
      {
        switch (section.StructuralSectionGeneralShape)
        {
          case DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralI: // Double T structural sections
            speckleSection = ISectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralT: // Tees structural sections
            speckleSection = TeeSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralH: // Rectangular Pipe structural sections
            speckleSection = RectangularHollowSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralR: // Pipe structural sections
            speckleSection = CircularHollowSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralF: // Flat Bar structural sections
            speckleSection = RectangularSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralS: // Round Bar structural sections
            speckleSection = CircularSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralW: // Angle structural sections
            speckleSection = AngleSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralU: // Channel  structural sections
            speckleSection = ChannelSectionToSpeckle(section);
            break;
          default:
            speckleSection.name = section.StructuralSectionShapeName;
            break;
        }
      }
      else
      {
        switch (section.StructuralSectionShape)
        {
          case DB.Structure.StructuralSections.StructuralSectionShape.IWideFlange:
            speckleSection = ISectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionShape.IParallelFlange:
            speckleSection = ISectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionShape.StructuralTees:
            speckleSection = TeeSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionShape.ISplitParallelFlange:
            speckleSection = TeeSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionShape.RectangleHSS:
            speckleSection = RectangularHollowSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionShape.RoundHSS:
            speckleSection = CircularHollowSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionShape.PipeStandard:
            speckleSection = CircularHollowSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionShape.RectangularBar:
            speckleSection = RectangularSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionShape.RoundBar:
            speckleSection = CircularSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionShape.LAngle:
            speckleSection = AngleSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionShape.LProfile:
            speckleSection = AngleSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionShape.CProfile:
            speckleSection = ChannelSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionShape.ConcreteRectangle:
            speckleSection = RectangularSectionToSpeckle(section);
            break;
          case DB.Structure.StructuralSections.StructuralSectionShape.ConcreteRound:
            speckleSection = CircularSectionToSpeckle(section);
            break;
          // Not all structural section types are currently implemented
          default:
            speckleSection.name = section.StructuralSectionShapeName;
            break;
        }
      }

      var materialType = stickFamily.StructuralMaterialType;
      var structMat = (DB.Material)Doc.GetElement(stickFamily.StructuralMaterialId);
      if (structMat == null)
        structMat = (DB.Material)Doc.GetElement(stickFamily.Symbol.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsElementId());

      var structAsset = (PropertySetElement)Doc.GetElement(structMat.StructuralAssetId);

      // If material has no physical properties in revit, assign null
      var materialAsset = structAsset != null ? structAsset.GetStructuralAsset() : null;

      //materialAsset = ((PropertySetElement)Doc.GetElement(structMat.StructuralAssetId)).GetStructuralAsset();

      Structural.Materials.Material speckleMaterial = null;

      switch (materialType)
      {
        case StructuralMaterialType.Concrete:
          var concreteMaterial = new Concrete
          {
            name = structMat.Name,
            materialType = Structural.MaterialType.Concrete,
            grade = null,
            designCode = null,
            codeYear = null,
            elasticModulus = materialAsset != null ? materialAsset.YoungModulus.X : 0,
            compressiveStrength = materialAsset != null ? materialAsset.ConcreteCompression : 0,
            tensileStrength = 0,
            flexuralStrength = 0,
            maxCompressiveStrain = 0,
            maxTensileStrain = 0,
            maxAggregateSize = 0,
            lightweight = materialAsset != null ? materialAsset.Lightweight : false,
            poissonsRatio = materialAsset != null ? materialAsset.PoissonRatio.X : 0,
            shearModulus = materialAsset != null ? materialAsset.ShearModulus.X : 0,
            density = materialAsset != null ? materialAsset.Density : 0,
            thermalExpansivity = materialAsset != null ? materialAsset.ThermalExpansionCoefficient.X : 0,
            dampingRatio = 0
          };
          speckleMaterial = concreteMaterial;
          break;
        case StructuralMaterialType.Steel:
          var steelMaterial = new Steel
          {
            name = structMat.Name,
            materialType = Structural.MaterialType.Steel,
            grade = materialAsset != null ? materialAsset.Name : null,
            designCode = null,
            codeYear = null,
            elasticModulus = materialAsset != null ? materialAsset.YoungModulus.X : 0, // Newtons per foot meter 
            yieldStrength = materialAsset != null ? materialAsset.MinimumYieldStress : 0, // Newtons per foot meter
            ultimateStrength = materialAsset != null ? materialAsset.MinimumTensileStrength : 0, // Newtons per foot meter
            maxStrain = 0,
            poissonsRatio = materialAsset != null ? materialAsset.PoissonRatio.X : 0,
            shearModulus = materialAsset != null ? materialAsset.ShearModulus.X : 0, // Newtons per foot meter
            density = materialAsset != null ? materialAsset.Density : 0, // kilograms per cubed feet 
            thermalExpansivity = materialAsset != null ? materialAsset.ThermalExpansionCoefficient.X : 0, // inverse Kelvin
            dampingRatio = 0
          };
          speckleMaterial = steelMaterial;
          break;
        case StructuralMaterialType.Wood:
          var timberMaterial = new Timber
          {
            name = structMat.Name,
            materialType = Structural.MaterialType.Timber,
            grade = materialAsset != null ? materialAsset.WoodGrade : null,
            designCode = null,
            codeYear = null,
            elasticModulus = materialAsset != null ? materialAsset.YoungModulus.X : 0, // Newtons per foot meter 
            poissonsRatio = materialAsset != null ? materialAsset.PoissonRatio.X : 0,
            shearModulus = materialAsset != null ? materialAsset.ShearModulus.X : 0, // Newtons per foot meter
            density = materialAsset != null ? materialAsset.Density : 0, // kilograms per cubed feet 
            thermalExpansivity = materialAsset != null ? materialAsset.ThermalExpansionCoefficient.X : 0, // inverse Kelvin
            species = materialAsset != null ? materialAsset.WoodSpecies : null,
            dampingRatio = 0
          };
          timberMaterial["bendingStrength"] = materialAsset != null ? materialAsset.WoodBendingStrength : 0;
          timberMaterial["parallelCompressionStrength"] = materialAsset != null ? materialAsset.WoodParallelCompressionStrength : 0;
          timberMaterial["parallelShearStrength"] = materialAsset != null ? materialAsset.WoodParallelShearStrength : 0;
          timberMaterial["perpendicularCompressionStrength"] = materialAsset != null ? materialAsset.WoodPerpendicularCompressionStrength : 0;
          timberMaterial["perpendicularShearStrength"] = materialAsset != null ? materialAsset.WoodPerpendicularShearStrength : 0;
          speckleMaterial = timberMaterial;
          break;
        default:
          var defaultMaterial = new Objects.Structural.Materials.Material
          {
            name = structMat.Name
          };
          speckleMaterial = defaultMaterial;
          break;
      }
      speckleMaterial.applicationId = $"{materialType}:{structMat.UniqueId}";

      prop.profile = speckleSection;
      prop.material = speckleMaterial;
      prop.name = $"{stickFamily.Symbol.FamilyName}:{stickFamily.Name}";
      prop.applicationId = stickFamily.Symbol.UniqueId;

      var structuralElement = Doc.GetElement(revitStick.GetElementId());
      var mark = GetParamValue<string>(structuralElement, BuiltInParameter.ALL_MODEL_MARK);

      if (revitStick is AnalyticalModelColumn)
      {
        speckleElement1D.memberType = MemberType.Column;
        speckleElement1D.type = ElementType1D.Column;
        var locationMark = GetParamValue<string>(structuralElement, BuiltInParameter.COLUMN_LOCATION_MARK);
        if (locationMark == null)
          speckleElement1D.name = mark;
        else
          speckleElement1D.name = locationMark;
      }
      else
      {
        speckleElement1D.name = mark;
      }

      speckleElement1D.property = prop;

    GetAllRevitParamsAndIds(speckleElement1D, revitStick);
    speckleElement1D.displayValue = GetElementDisplayMesh(Doc.GetElement(revitStick.GetElementId()));
    return speckleElement1D;
  }

    private ISection ISectionToSpeckle(DB.Structure.StructuralSections.StructuralSection section)
    {
      return new ISection()
      {
        name = section.StructuralSectionShapeName,
        shapeType = Structural.ShapeType.I,
        depth = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralI).GetProperty("Height").GetValue(section)),
        width = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralI).GetProperty("Width").GetValue(section)),
        webThickness = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralI).GetProperty("WebThickness").GetValue(section)),
        flangeThickness = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralI).GetProperty("FlangeThickness").GetValue(section)),
        area = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralI).GetProperty("SectionArea").GetValue(section)),
        weight = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralI).GetProperty("NominalWeight").GetValue(section)),
        Iyy = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralI).GetProperty("MomentOfInertiaStrongAxis").GetValue(section)),
        Izz = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralI).GetProperty("MomentOfInertiaWeakAxis").GetValue(section)),
        J = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralI).GetProperty("TorsionalMomentOfInertia").GetValue(section))
      };
    }

    private Tee TeeSectionToSpeckle(DB.Structure.StructuralSections.StructuralSection section)
    {
      return new Tee()
      {
        name = section.StructuralSectionShapeName,
        shapeType = Structural.ShapeType.Tee,
        depth = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralT).GetProperty("Height").GetValue(section)),
        width = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralT).GetProperty("Width").GetValue(section)),
        webThickness = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralT).GetProperty("WebThickness").GetValue(section)),
        flangeThickness = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralT).GetProperty("FlangeThickness").GetValue(section)),
        area = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralT).GetProperty("SectionArea").GetValue(section)),
        weight = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralT).GetProperty("NominalWeight").GetValue(section)),
        Iyy = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralT).GetProperty("MomentOfInertiaStrongAxis").GetValue(section)),
        Izz = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralT).GetProperty("MomentOfInertiaWeakAxis").GetValue(section)),
        J = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralT).GetProperty("TorsionalMomentOfInertia").GetValue(section))
      };
    }

    private Rectangular RectangularHollowSectionToSpeckle(DB.Structure.StructuralSections.StructuralSection section)
    {
      var wallThickness = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralH).GetProperty("WallNominalThickness").GetValue(section));

      return new Rectangular()
      {
        name = section.StructuralSectionShapeName,
        shapeType = Structural.ShapeType.Rectangular,
        depth = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralH).GetProperty("Height").GetValue(section)),
        width = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralH).GetProperty("Width").GetValue(section)),
        webThickness = wallThickness,
        flangeThickness = wallThickness,
        area = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralH).GetProperty("SectionArea").GetValue(section)),
        weight = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralH).GetProperty("NominalWeight").GetValue(section)),
        Iyy = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralH).GetProperty("MomentOfInertiaStrongAxis").GetValue(section)),
        Izz = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralH).GetProperty("MomentOfInertiaWeakAxis").GetValue(section)),
        J = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralH).GetProperty("TorsionalMomentOfInertia").GetValue(section)),
      };
    }

    private Rectangular RectangularSectionToSpeckle(DB.Structure.StructuralSections.StructuralSection section)
    {
      return new Rectangular()
      {
        name = section.StructuralSectionShapeName,
        shapeType = Structural.ShapeType.Rectangular,
        depth = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralF).GetProperty("Height").GetValue(section)),
        width = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralF).GetProperty("Width").GetValue(section)),
        area = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralF).GetProperty("SectionArea").GetValue(section)),
        weight = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralF).GetProperty("NominalWeight").GetValue(section)),
        Iyy = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralF).GetProperty("MomentOfInertiaStrongAxis").GetValue(section)),
        Izz = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralF).GetProperty("MomentOfInertiaWeakAxis").GetValue(section)),
        J = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralF).GetProperty("TorsionalMomentOfInertia").GetValue(section)),
      };
    }

    private Circular CircularHollowSectionToSpeckle(DB.Structure.StructuralSections.StructuralSection section)
    {
      return new Circular()
      {
        name = section.StructuralSectionShapeName,
        shapeType = Structural.ShapeType.Circular,
        radius = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralR).GetProperty("Diameter").GetValue(section) / 2),
        wallThickness = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralR).GetProperty("WallNominalThickness").GetValue(section)),
        area = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralR).GetProperty("SectionArea").GetValue(section)),
        weight = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralR).GetProperty("NominalWeight").GetValue(section)),
        Iyy = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralR).GetProperty("MomentOfInertiaStrongAxis").GetValue(section)),
        Izz = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralR).GetProperty("MomentOfInertiaWeakAxis").GetValue(section)),
        J = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralR).GetProperty("TorsionalMomentOfInertia").GetValue(section))
      };
    }

    private Circular CircularSectionToSpeckle(DB.Structure.StructuralSections.StructuralSection section)
    {
      return new Circular()
      {
        name = section.StructuralSectionShapeName,
        shapeType = Structural.ShapeType.Circular,
        radius = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralS).GetProperty("Diameter").GetValue(section) / 2),
        area = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralS).GetProperty("SectionArea").GetValue(section)),
        weight = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralS).GetProperty("NominalWeight").GetValue(section)),
        Iyy = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralS).GetProperty("MomentOfInertiaStrongAxis").GetValue(section)),
        Izz = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralS).GetProperty("MomentOfInertiaWeakAxis").GetValue(section)),
        J = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralS).GetProperty("TorsionalMomentOfInertia").GetValue(section))
      };
    }

    private Angle AngleSectionToSpeckle(DB.Structure.StructuralSections.StructuralSection section)
    {
      return new Angle()
      {
        name = section.StructuralSectionShapeName,
        shapeType = Structural.ShapeType.Angle,
        depth = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralW).GetProperty("Height").GetValue(section)),
        width = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralW).GetProperty("Width").GetValue(section)),
        webThickness = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralW).GetProperty("WebThickness").GetValue(section)),
        flangeThickness = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralW).GetProperty("FlangeThickness").GetValue(section)),
        area = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralW).GetProperty("SectionArea").GetValue(section)),
        weight = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralW).GetProperty("NominalWeight").GetValue(section)),
        Iyy = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralW).GetProperty("MomentOfInertiaStrongAxis").GetValue(section)),
        Izz = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralW).GetProperty("MomentOfInertiaWeakAxis").GetValue(section)),
        J = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralW).GetProperty("TorsionalMomentOfInertia").GetValue(section))
      };
    }

    private Channel ChannelSectionToSpeckle(DB.Structure.StructuralSections.StructuralSection section)
    {
      return new Channel()
      {
        name = section.StructuralSectionShapeName,
        shapeType = Structural.ShapeType.Channel,
        depth = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralU).GetProperty("Height").GetValue(section)),
        width = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralU).GetProperty("Width").GetValue(section)),
        webThickness = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralU).GetProperty("WebThickness").GetValue(section)),
        flangeThickness = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralU).GetProperty("FlangeThickness").GetValue(section)),
        area = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralU).GetProperty("SectionArea").GetValue(section)),
        weight = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralU).GetProperty("NominalWeight").GetValue(section)),
        Iyy = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralU).GetProperty("MomentOfInertiaStrongAxis").GetValue(section)),
        Izz = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralU).GetProperty("MomentOfInertiaWeakAxis").GetValue(section)),
        J = ScaleToSpeckle((double)typeof(DB.Structure.StructuralSections.StructuralSectionGeneralU).GetProperty("TorsionalMomentOfInertia").GetValue(section))
      };
    }
  }
}