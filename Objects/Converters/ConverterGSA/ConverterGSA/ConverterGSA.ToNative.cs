using Objects.Geometry;
using Objects.Structural.Geometry;
using Objects.Structural.GSA.Geometry;
using Objects.Structural.GSA.Materials;
using Objects.Structural.GSA.Properties;
using Objects.Structural.Properties;
using Speckle.Core.Models;
using Speckle.GSA.API;
using Speckle.GSA.API.GwaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Objects.Structural.GSA.Loading;
using Objects.Structural.GSA.Analysis;
using Objects.Structural.GSA.Bridge;
using GwaAxisDirection6 = Speckle.GSA.API.GwaSchema.AxisDirection6;
using AxisDirection6 = Objects.Structural.GSA.Geometry.AxisDirection6;
using Restraint = Objects.Structural.Geometry.Restraint;
using Speckle.Core.Kits;
using Objects.Structural.Loading;
using Objects.Structural;
using Objects.Structural.Materials;
using Objects;
using Objects.Structural.Properties.Profiles;
using Objects.BuiltElements;
using Objects.Structural.Analysis;

namespace ConverterGSA
{
  //Container just for ToNative methods, and their helper methods
  public partial class ConverterGSA
  {
    private Dictionary<Type, Func<Base, List<GsaRecord>>> ToNativeFns;

    private static Point Origin = new Point(0, 0, 0);
    private static Vector UnitX = new Vector(1, 0, 0);
    private static Vector UnitY = new Vector(0, 1, 0);
    private static Vector UnitZ = new Vector(0, 0, 1);
    private static int GeometricDecimalPlaces = 6;

    void SetupToNativeFns()
    {
      ToNativeFns = new Dictionary<Type, Func<Base, List<GsaRecord>>>()
      {
        //{ typeof(Model), ModelToNative },
        { typeof(ModelInfo), ModelInfoToNative },
        //Geometry
        { typeof(Axis), AxisToNative },
        { typeof(Point), PointToNative },
        { typeof(GSANode), GSANodeToNative },
        { typeof(Node), NodeToNative },
        { typeof(GSAElement1D), GSAElement1dToNative },
        { typeof(Element1D), Element1dToNative },
        { typeof(GSAElement2D), GSAElement2dToNative },
        { typeof(Element2D), Element2dToNative },
        { typeof(GSAMember1D), GSAMember1dToNative },
        { typeof(GSAMember2D), GSAMember2dToNative },
        { typeof(GSAAssembly), GSAAssemblyToNative },
        { typeof(GSAGridLine), GSAGridLineToNative },
        { typeof(Storey), StoreyToNative },
        { typeof(GSAGridPlane), GSAGridPlaneToNative },
        { typeof(GSAGridSurface), GSAGridSurfaceToNative },
        { typeof(GSAPolyline), GSAPolylineToNative },
        //Loading
        { typeof(GSALoadCase), GSALoadCaseToNative },
        { typeof(LoadCase), LoadCaseToNative },
        { typeof(GSAAnalysisCase), GSAAnalysisCaseToNative },
        { typeof(GSALoadCombination), GSALoadCombinationToNative },
        { typeof(LoadCombination), LoadCombinationToNative },
        { typeof(GSALoadBeam), GSALoadBeamToNative },
        { typeof(LoadBeam), LoadBeamToNative },
        { typeof(GSALoadFace), GSALoadFaceToNative },
        { typeof(LoadFace), LoadFaceToNative },
        { typeof(GSALoadNode), GSALoadNodeToNative },
        { typeof(LoadNode), LoadNodeToNative },
        { typeof(GSALoadGravity), GSALoadGravityToNative },
        { typeof(LoadGravity), LoadGravityToNative },
        { typeof(GSALoadThermal2d), GSALoadThermal2dToNative },
        { typeof(GSALoadGridPoint), GSALoadGridPointToNative },
        { typeof(GSALoadGridLine), GSALoadGridLineToNative },
        { typeof(GSALoadGridArea), GSALoadGridAreaToNative },
        //Materials
        { typeof(GSASteel), GSASteelToNative },
        { typeof(Steel), SteelToNative },
        { typeof(GSAConcrete), GSAConcreteToNative },
        { typeof(Concrete), ConcreteToNative },
        //Properties
        { typeof(Property1D), Property1dToNative },
        { typeof(GSAProperty1D), GsaProperty1dToNative },
        { typeof(Property2D), Property2dToNative },
        { typeof(GSAProperty2D), GsaProperty2dToNative },
        { typeof(PropertySpring), PropertySpringToNative },
        { typeof(PropertyMass), PropertyMassToNative },
        //Constraints
        { typeof(GSARigidConstraint), GSARigidConstraintToNative },
        { typeof(GSAGeneralisedRestraint), GSAGeneralisedRestraintToNative },
        // Bridge
        { typeof(GSAInfluenceNode), InfNodeToNative},
        { typeof(GSAInfluenceBeam), InfBeamToNative},
        { typeof(GSAAlignment), AlignToNative },
        { typeof(GSAPath), PathToNative },
        { typeof(GSAUserVehicle), GSAUserVehicleToNative },
        // Analysis
        { typeof(GSAStage), AnalStageToNative },
      };
    }

    #region ToNative
    //TO DO: implement conversion code for ToNative

    private List<GsaRecord> ModelToNative(Base speckleObject)
    {
      var model = (Model)speckleObject;
      var retList = new List<GsaRecord>();

      if (model.specs != null && model.specs.settings != null && model.specs.settings.modelUnits != null)
      {
        conversionFactors = new UnitConversion(model.specs.settings.modelUnits);
      }

      
      var speckleObjects = new List<Base>();
      speckleObjects.AddRangeIfNotNull(model.nodes);
      speckleObjects.AddRangeIfNotNull(model.elements);
      speckleObjects.AddRangeIfNotNull(model.loads);
      speckleObjects.AddRangeIfNotNull(model.restraints);
      speckleObjects.AddRangeIfNotNull(model.properties);
      speckleObjects.AddRangeIfNotNull(model.materials);

      var speckleDependencyTree = Instance.GsaModel.SpeckleDependencyTree();
      var objectsByType = speckleObjects.GroupBy(t => t.GetType()).ToDictionary(g => g.Key, g => g.ToList());

      foreach (var gen in speckleDependencyTree)
      {
#if DEBUG
        foreach (var t in gen)
#else
        Parallel.ForEach(gen, t =>
#endif
        {
          if (objectsByType.ContainsKey(t))
          {
#if !DEBUG
            if (parallelisable.ContainsKey(t))
            {
              foreach (Base so in objectsByType[t].Where(o => !string.IsNullOrEmpty(o.applicationId)))
              {
                Instance.GsaModel.Cache.ResolveIndex(parallelisable[t], so.applicationId);
              }

              Parallel.ForEach(objectsByType[t].Cast<Base>(), so =>
              {
                try
                {
                  if (CanConvertToNative(so) && ToNativeFns.ContainsKey(t))
                  {
                    var natives = ToNativeFns[t](so);
                    retList.AddRangeIfNotNull(natives);
                    if (Instance.GsaModel.ConversionProgress != null)
                    {
                      Instance.GsaModel.ConversionProgress.Report(natives != null);
                    }
                  }
                }
                catch
                {
                  ConversionErrors.Add(new Exception("Unable to convert " + t.Name + " " + (so.applicationId ?? so.id) + " - refer to logs for more information"));
                }
              }
              );
            }
            else
#endif
            {
              foreach (Base so in objectsByType[t])
              {
                try
                {
                  if (CanConvertToNative(so) && ToNativeFns.ContainsKey(t))
                  {
                    var natives = ToNativeFns[t](so);
                    retList.AddRangeIfNotNull(natives);
                    if (Instance.GsaModel.ConversionProgress != null)
                    {
                      Instance.GsaModel.ConversionProgress.Report(natives != null);
                    }
                  }
                }
                catch
                {
                  ConversionErrors.Add(new Exception("Unable to convert " + t.Name + " " + (so.applicationId ?? so.id) + " - refer to logs for more information"));
                }
              }
            }
          }
        }
#if !DEBUG
        );
#endif
      }

      return retList;
    }

    private List<GsaRecord> ModelInfoToNative(Base speckleObject)
    {
      var modelInfo = (ModelInfo)speckleObject;
      if (modelInfo.settings != null && modelInfo.settings.modelUnits != null)
      {
        conversionFactors = new UnitConversion(modelInfo.settings.modelUnits);
      }
      return null;
    }

#region Geometry
    private List<GsaRecord> AxisToNative(Base speckleObject)
    {
      var speckleAxis = (Axis)speckleObject;
      var gsaAxis = new GsaAxis()
      {
        ApplicationId = speckleAxis.applicationId,
        Index = speckleAxis.GetIndex<GsaAxis>(),
        Name = speckleAxis.name,
      };
      if (speckleAxis.definition != null)
      {
        if (speckleAxis.definition.origin != null)
        {
          gsaAxis.OriginX = speckleAxis.definition.origin.x;
          gsaAxis.OriginY = speckleAxis.definition.origin.y;
          gsaAxis.OriginZ = speckleAxis.definition.origin.z;
        }
        if (speckleAxis.definition.xdir != null && speckleAxis.definition.xdir.Norm() != 0)
        {
          gsaAxis.XDirX = speckleAxis.definition.xdir.x;
          gsaAxis.XDirY = speckleAxis.definition.xdir.y;
          gsaAxis.XDirZ = speckleAxis.definition.xdir.z;
        }
        if (speckleAxis.definition.ydir != null && speckleAxis.definition.ydir.Norm() != 0)
        {
          gsaAxis.XYDirX = speckleAxis.definition.ydir.x;
          gsaAxis.XYDirY = speckleAxis.definition.ydir.y;
          gsaAxis.XYDirZ = speckleAxis.definition.ydir.z;
        }
      }
      
      return new List<GsaRecord>{ gsaAxis };
    }

    private List<GsaRecord> PointToNative(Base speckleObject)
    {
      var basePoint = (Point)speckleObject;
      var retList = new List<GsaRecord>();

      var speckleNode = new Node(basePoint, null, null, null);

      var gsaNode = new GsaNode()
      {
        ApplicationId = speckleObject.id, // should be identical if xyz coordinates the same
        Index = basePoint.NodeAt(conversionFactors),
        Name = speckleNode.name,
        SpringPropertyIndex = IndexByConversionOrLookup<GsaPropSpr>(speckleNode.springProperty, ref retList),
        MassPropertyIndex = IndexByConversionOrLookup<GsaPropMass>(speckleNode.massProperty, ref retList),
      };

      if (GetRestraint(speckleNode.restraint, out var gsaNodeRestraint, out var gsaRestraint))
      {
        gsaNode.NodeRestraint = gsaNodeRestraint;
        gsaNode.Restraints = gsaRestraint;
      }
      if (GetAxis(speckleNode.constraintAxis, out NodeAxisRefType gsaAxisRefType, out var gsaAxisIndex))
      {
        gsaNode.AxisRefType = gsaAxisRefType;
        gsaNode.AxisIndex = gsaAxisIndex;
      }

      //Unit conversions
      if (basePoint != null)
      {
        var factor = basePoint.GetScaleFactor(conversionFactors);
        gsaNode.X = basePoint.x * factor;
        gsaNode.Y = basePoint.y * factor;
        gsaNode.Z = basePoint.z * factor;
      }

      retList.Add(gsaNode);
      return retList;
    }

    private List<GsaRecord> GSANodeToNative(Base speckleObject)
    {
      var gsaNode = (GsaNode)NodeToNative(speckleObject).First(o => o is GsaNode);
      var speckleNode = (GSANode)speckleObject;
      gsaNode.Colour = speckleNode.colour?.ColourToNative() ?? Colour.NotSet;
      gsaNode.MeshSize = speckleNode.localElementSize.IsPositiveOrNull();
      return new List<GsaRecord>() { gsaNode };
    }

    private List<GsaRecord> NodeToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleNode = (Node)speckleObject;
      var gsaNode = new GsaNode()
      {
        ApplicationId = speckleNode.applicationId,
        Index = speckleNode.NodeAt(conversionFactors),
        Name = speckleNode.name,
        SpringPropertyIndex = IndexByConversionOrLookup<GsaPropSpr>(speckleNode.springProperty, ref retList),
        MassPropertyIndex = IndexByConversionOrLookup<GsaPropMass>(speckleNode.massProperty, ref retList),
      };
      if (GetRestraint(speckleNode.restraint, out var gsaNodeRestraint, out var gsaRestraint))
      {
        gsaNode.NodeRestraint = gsaNodeRestraint;
        gsaNode.Restraints = gsaRestraint;
      }
      if (GetAxis(speckleNode.constraintAxis, out NodeAxisRefType gsaAxisRefType, out var gsaAxisIndex))
      {
        gsaNode.AxisRefType = gsaAxisRefType;
        gsaNode.AxisIndex = gsaAxisIndex;
      }

      //Unit conversions
      if (speckleNode.basePoint != null)
      {
        var factor = speckleNode.GetScaleFactor(conversionFactors);
        gsaNode.X = speckleNode.basePoint.x * factor;
        gsaNode.Y = speckleNode.basePoint.y * factor;
        gsaNode.Z = speckleNode.basePoint.z * factor;
      }

      retList.Add(gsaNode);
      return retList;
    }

    private List<GsaRecord> GSAElement1dToNative(Base speckleObject)
    {
      var gsaRecords = Element1dToNative(speckleObject);
      var gsaElement = (GsaEl)gsaRecords.First(o => o is GsaEl);
      var speckleElement = (GSAElement1D)speckleObject;
      gsaElement.Colour = speckleElement.colour?.ColourToNative() ?? Colour.NotSet;
      gsaElement.Dummy = speckleElement.isDummy;
      gsaElement.Group = speckleElement.group.IsPositiveOrNull();
      return gsaRecords;

      //TODO:
      //SpeckleObject:
      //  string action
    }

    private List<GsaRecord> Element1dToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleElement = (Element1D)speckleObject;
      var gsaElement = new GsaEl()
      {
        ApplicationId = speckleElement.applicationId,
        Index = speckleElement.GetIndex<GsaEl>(),
        Name = speckleElement.name,
        Type = speckleElement.type.ToNative(),
        OrientationNodeIndex = speckleElement.orientationNode.NodeAt(conversionFactors),
        //TaperOffsetPercentageEnd1 - currently not supported
        //TaperOffsetPercentageEnd2 - currently not supported
        PropertyIndex = IndexByConversionOrLookup<GsaSection>(speckleElement.property, ref retList),
        ParentIndex = IndexByConversionOrLookup<GsaMemb>(speckleElement.parent, ref retList)
      };

      if (speckleElement.topology != null && speckleElement.topology.Count > 0)
      {
        gsaElement.NodeIndices = speckleElement.topology.NodeAt(conversionFactors);
      }
      else if (speckleElement.baseLine != null && speckleElement.baseLine.start != null && speckleElement.baseLine.end != null)
      {
        var specklePoints = new List<Point>() { speckleElement.baseLine.start, speckleElement.baseLine.end };
        gsaElement.NodeIndices = specklePoints.NodeAt(conversionFactors);
      }

      if (speckleElement.end1Releases != null && GetReleases(speckleElement.end1Releases, out var gsaRelease1, out var gsaStiffnesses1, out var gsaReleaseInclusion1))
      {
        gsaElement.Releases1 = gsaRelease1;
        gsaElement.Stiffnesses1 = gsaStiffnesses1;
        gsaElement.ReleaseInclusion = gsaReleaseInclusion1;
      }
      if (speckleElement.end2Releases != null && GetReleases(speckleElement.end2Releases, out var gsaRelease2, out var gsaStiffnesses2, out var gsaReleaseInclusion2))
      {
        gsaElement.Releases2 = gsaRelease2;
        gsaElement.Stiffnesses2 = gsaStiffnesses2;
        gsaElement.ReleaseInclusion = gsaReleaseInclusion2;
      }
      if (speckleElement.end1Offset != null && speckleElement.end1Offset.x != 0)
      {
        gsaElement.End1OffsetX = conversionFactors.length * speckleElement.end1Offset.x;
      }
      if (speckleElement.end2Offset != null && speckleElement.end2Offset.x != 0)
      {
        gsaElement.End2OffsetX = conversionFactors.length * speckleElement.end2Offset.x;
      }
      if (speckleElement.end1Offset != null && speckleElement.end2Offset != null)
      {
        if (speckleElement.end1Offset.y == speckleElement.end2Offset.y)
        {
          if (speckleElement.end1Offset.y != 0) gsaElement.OffsetY = conversionFactors.length * speckleElement.end1Offset.y;
        }
        else
        {
          gsaElement.OffsetY = conversionFactors.length * speckleElement.end1Offset.y;
          ConversionErrors.Add(new Exception("Element1dToNative: "
            + "Error converting element1d with application id (" + speckleElement.applicationId + "). "
            + "Different y offsets were assigned at either end."
            + "end 1 y offset of " + gsaElement.OffsetY.ToString() + " has been applied"));
        }
        if (speckleElement.end1Offset.z == speckleElement.end2Offset.z)
        {
          if (speckleElement.end1Offset.z != 0) gsaElement.OffsetZ = conversionFactors.length * speckleElement.end1Offset.z;
        }
        else
        {
          gsaElement.OffsetZ = conversionFactors.length * speckleElement.end1Offset.z;
          ConversionErrors.Add(new Exception("Element1dToNative: "
            + "Error converting element1d with application id (" + speckleElement.applicationId + "). "
            + "Different z offsets were assigned at either end."
            + "end 1 z offset of " + gsaElement.OffsetY.ToString() + " has been applied"));
        }
      }
      if (speckleElement.orientationAngle != 0)   gsaElement.Angle = conversionFactors.ConversionFactorToDegrees() * speckleElement.orientationAngle;

      retList.Add(gsaElement);
      return retList;
    }

    private List<GsaRecord> GSAElement2dToNative(Base speckleObject)
    {
      var gsaRecords = Element2dToNative(speckleObject);
      var gsaElement = (GsaEl)gsaRecords.First(o => o is GsaEl);
      var speckleElement = (GSAElement2D)speckleObject;
      gsaElement.Colour = speckleElement.colour?.ColourToNative() ?? Colour.NotSet;
      gsaElement.Dummy = speckleElement.isDummy;
      gsaElement.Group = speckleElement.group.IsPositiveOrNull();
      return gsaRecords;
    }

    private List<GsaRecord> Element2dToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleElement = (Element2D)speckleObject;
      var gsaElement = new GsaEl()
      {
        ApplicationId = speckleElement.applicationId,
        Index = speckleElement.GetIndex<GsaEl>(),
        Name = speckleElement.name,
        Type = speckleElement.type.ToNative(),
        PropertyIndex = IndexByConversionOrLookup<GsaProp2d>(speckleElement.property, ref retList),
        ReleaseInclusion = ReleaseInclusion.NotIncluded,
        ParentIndex = IndexByConversionOrLookup<GsaMemb>(speckleElement.parent, ref retList)
      };
      if (speckleElement.topology != null && speckleElement.topology.Count > 0)
      {
        gsaElement.NodeIndices = speckleElement.topology.NodeAt(conversionFactors);
      }

      if (speckleElement.orientationAngle != 0) gsaElement.Angle = conversionFactors.ConversionFactorToDegrees() * speckleElement.orientationAngle;
      if (speckleElement.offset != 0) gsaElement.OffsetZ = conversionFactors.length * speckleElement.offset;

      retList.Add(gsaElement);
      return retList;
    }

    private int? IndexByConversionOrLookup<N>(Base obj, ref List<GsaRecord> extra)
    {
      if (obj == null)
      {
        return null;
      }
      int? index = null;
      if (!string.IsNullOrEmpty(obj.applicationId))
      {
        //index = Instance.GsaModel.Cache.LookupIndex<N>(obj.applicationId);
        index = Instance.GsaModel.Cache.ResolveIndex<N>(obj.applicationId);
        if (index != null) return index;
      }
      if (!ConvertedObjectsList.Contains(obj.id))
      {
        if (!index.IsIndex())
        {
          var nt = typeof(N);
          var st = obj.GetType();
          if (ToNativeFns.ContainsKey(st))
          {
            var gsaRecords = ToNativeFns[st](obj);
            var gsaRecord = gsaRecords.FirstOrDefault(r => r.GetType() == nt);
            if (gsaRecord != null && gsaRecord.Index.IsIndex())
            {
              index = gsaRecord.Index;
              extra.AddRange(gsaRecords);
              ConvertedObjectsList.Add(obj.id);
            }
          }
        }
      } 
      else
      {
        //index = Instance.GsaModel.Cache.LookupIndex<N>(obj.id);
        index = Instance.GsaModel.Cache.ResolveIndex<N>(obj.id);
      }


      return index;
    }

    private List<GsaRecord> GSAMember1dToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleMember = (GSAMember1D)speckleObject;
      var gsaMember = new GsaMemb()
      {
        ApplicationId = speckleMember.applicationId,
        Index = speckleMember.GetIndex<GsaMemb>(),
        Name = speckleMember.name,
        Type = speckleMember.type.ToNativeMember(),
        Colour = speckleMember.colour?.ColourToNative() ?? Colour.NotSet,
        Dummy = speckleMember.isDummy,
        IsIntersector = speckleMember.intersectsWithOthers,
        OrientationNodeIndex = speckleMember.orientationNode.NodeAt(conversionFactors),
        //TaperOffsetPercentageEnd1 - currently not supported
        //TaperOffsetPercentageEnd2 - currently not supported
        PropertyIndex = IndexByConversionOrLookup<GsaSection>(speckleMember.property, ref retList),
      };

      if (speckleMember.topology != null && speckleMember.topology.Count > 0)
      {
        gsaMember.NodeIndices = speckleMember.topology.NodeAt(conversionFactors);
      }
      else if (speckleMember.baseLine != null && speckleMember.baseLine.start != null && speckleMember.baseLine.end != null)
      {
        var specklePoints = new List<Point>() { speckleMember.baseLine.start, speckleMember.baseLine.end };
        gsaMember.NodeIndices = specklePoints.NodeAt(conversionFactors);
      }

      var dynamicMembers = speckleMember.GetMembers();

      //Dynamic properties
      gsaMember.Exposure = speckleMember.GetDynamicEnum<ExposedSurfaces>("Exposure", dynamicMembers);
      var exposure = speckleMember.GetDynamicEnum<ExposedSurfaces>("Exposure", dynamicMembers);
      gsaMember.Exposure = exposure == ExposedSurfaces.NotSet ? ExposedSurfaces.ALL : exposure;
      var analysisType = speckleMember.GetDynamicEnum<AnalysisType>("AnalysisType", dynamicMembers);
      gsaMember.AnalysisType = analysisType == AnalysisType.NotSet ? AnalysisType.BEAM : analysisType;   
      gsaMember.Fire = speckleMember.GetDynamicEnum<FireResistance>("Fire", dynamicMembers);
      gsaMember.RestraintEnd1 = speckleMember.GetDynamicEnum<Speckle.GSA.API.GwaSchema.Restraint>("RestraintEnd1", dynamicMembers);
      gsaMember.RestraintEnd2 = speckleMember.GetDynamicEnum<Speckle.GSA.API.GwaSchema.Restraint>("RestraintEnd2", dynamicMembers);
      gsaMember.EffectiveLengthType = speckleMember.GetDynamicEnum<EffectiveLengthType>("EffectiveLengthType", dynamicMembers);
      gsaMember.LoadHeightReferencePoint = speckleMember.GetDynamicEnum<LoadHeightReferencePoint>("LoadHeightReferencePoint", dynamicMembers);
      gsaMember.CreationFromStartDays = speckleMember.GetDynamicValue<int>("CreationFromStartDays", dynamicMembers);
      gsaMember.StartOfDryingDays = speckleMember.GetDynamicValue<int>("StartOfDryingDays", dynamicMembers);
      gsaMember.AgeAtLoadingDays = speckleMember.GetDynamicValue<int>("AgeAtLoadingDays", dynamicMembers);
      gsaMember.RemovedAtDays = speckleMember.GetDynamicValue<int>("RemovedAtDays", dynamicMembers);
      gsaMember.MemberHasOffsets = speckleMember.GetDynamicValue<bool>("MemberHasOffsets", dynamicMembers);
      gsaMember.End1AutomaticOffset = speckleMember.GetDynamicValue<bool>("End1AutomaticOffset", dynamicMembers);
      gsaMember.End2AutomaticOffset = speckleMember.GetDynamicValue<bool>("End2AutomaticOffset", dynamicMembers);
      gsaMember.LimitingTemperature = conversionFactors.TemperatureToNative(speckleMember.GetDynamicValue<double?>("LimitingTemperature", dynamicMembers));
      gsaMember.LoadHeight = conversionFactors.length * speckleMember.GetDynamicValue<double?>("LoadHeight", dynamicMembers);
      gsaMember.EffectiveLengthYY = conversionFactors.length * speckleMember.GetDynamicValue<double>("EffectiveLengthYY", dynamicMembers).IsPositiveOrNull();
      gsaMember.PercentageYY = speckleMember.GetDynamicValue<double>("PercentageYY", dynamicMembers).IsPositiveOrNull();
      gsaMember.EffectiveLengthZZ = conversionFactors.length * speckleMember.GetDynamicValue<double>("EffectiveLengthZZ", dynamicMembers).IsPositiveOrNull();
      gsaMember.PercentageZZ = speckleMember.GetDynamicValue<double>("PercentageZZ", dynamicMembers).IsPositiveOrNull();
      gsaMember.EffectiveLengthLateralTorsional = conversionFactors.length 
        * speckleMember.GetDynamicValue<double>("EffectiveLengthLateralTorsional", dynamicMembers).IsPositiveOrNull();
      gsaMember.FractionLateralTorsional = speckleMember.GetDynamicValue<double>("FractionLateralTorsional", dynamicMembers).IsPositiveOrNull();

      if (speckleMember.end1Releases != null && GetReleases(speckleMember.end1Releases, out var gsaRelease1, out var gsaStiffnesses1))
      {
        gsaMember.Releases1 = gsaRelease1;
        gsaMember.Stiffnesses1 = gsaStiffnesses1;
      }
      if (speckleMember.end2Releases != null && GetReleases(speckleMember.end2Releases, out var gsaRelease2, out var gsaStiffnesses2))
      {
        gsaMember.Releases2 = gsaRelease2;
        gsaMember.Stiffnesses2 = gsaStiffnesses2;
      }
      if (speckleMember.end1Offset != null && speckleMember.end1Offset.x != 0) gsaMember.End1OffsetX = conversionFactors.length * speckleMember.end1Offset.x;
      if (speckleMember.end2Offset != null && speckleMember.end2Offset.x != 0) gsaMember.End2OffsetX = conversionFactors.length * speckleMember.end2Offset.x;
      if (speckleMember.end1Offset != null && speckleMember.end2Offset != null)
      {
        if (speckleMember.end1Offset.y == speckleMember.end2Offset.y)
        {
          if (speckleMember.end1Offset.y != 0) gsaMember.OffsetY = conversionFactors.length * speckleMember.end1Offset.y;
        }
        else
        {
          gsaMember.OffsetY = conversionFactors.length * speckleMember.end1Offset.y;
          ConversionErrors.Add(new Exception("GSAMember1dToNative: "
            + "Error converting element1d with application id (" + speckleMember.applicationId + "). "
            + "Different y offsets were assigned at either end."
            + "end 1 y offset of " + gsaMember.OffsetY.ToString() + " has been applied"));
        }
        if (speckleMember.end1Offset.z == speckleMember.end2Offset.z)
        {
          if (speckleMember.end1Offset.z != 0) gsaMember.OffsetZ = conversionFactors.length * speckleMember.end1Offset.z;
        }
        else
        {
          gsaMember.OffsetZ = conversionFactors.length * speckleMember.end1Offset.z;
          ConversionErrors.Add(new Exception("GSAMember1dToNative: "
            + "Error converting element1d with application id (" + speckleMember.applicationId + "). "
            + "Different z offsets were assigned at either end."
            + "end 1 z offset of " + gsaMember.OffsetY.ToString() + " has been applied"));
        }
      }
      
      if (speckleMember.orientationAngle != 0) gsaMember.Angle = conversionFactors.ConversionFactorToDegrees() * speckleMember.orientationAngle;
      else gsaMember.Angle = 0;
      if (speckleMember.group > 0) gsaMember.Group = speckleMember.group;
      if (speckleMember.targetMeshSize > 0) gsaMember.MeshSize = conversionFactors.length * speckleMember.targetMeshSize;

      //Dynamic properties
      var members = speckleMember.GetMembers();
      if (members.ContainsKey("Voids") && speckleMember["Voids"] is List<List<Node>>)
      {
        var speckleVoids = speckleObject["Voids"] as List<List<Node>>;
        gsaMember.Voids = speckleVoids.Select(v => v.NodeAt(conversionFactors)).ToList();
      }
      if (members.ContainsKey("Points") && speckleMember["Points"] is List<Node>)
      {
        var specklePoints = speckleObject["Points"] as List<Node>;
        gsaMember.PointNodeIndices = specklePoints.NodeAt(conversionFactors);
      }
      if (members.ContainsKey("Lines") && speckleMember["Lines"] is List<List<Node>>)
      {
        var speckleLines = speckleObject["Lines"] as List<List<Node>>;
        gsaMember.Polylines = speckleLines.Select(v => v.NodeAt(conversionFactors)).ToList();
      }
      if (members.ContainsKey("Areas") && speckleMember["Areas"] is List<List<Node>>)
      {
        var speckleAreas = speckleObject["Areas"] as List<List<Node>>;
        gsaMember.AdditionalAreas = speckleAreas.Select(v => v.NodeAt(conversionFactors)).ToList();
      }
      if (members.ContainsKey("SpanRestraints") && speckleMember["SpanRestraints"] is List<RestraintDefinition>)
      {
        var speckleSpanRestraints = speckleObject["SpanRestraints"] as List<RestraintDefinition>;
        gsaMember.SpanRestraints = speckleSpanRestraints.Select(s => new RestraintDefinition() { All = s.All, Index = s.Index, Restraint = s.Restraint }).ToList();        
      }
      if (members.ContainsKey("PointRestraints") && speckleMember["PointRestraints"] is List<RestraintDefinition>)
      {
        var specklePointRestraints = speckleObject["PointRestraints"] as List<RestraintDefinition>;
        gsaMember.PointRestraints = specklePointRestraints.Select(s => new RestraintDefinition() { All = s.All, Index = s.Index, Restraint = s.Restraint }).ToList();
      }

      retList.Add(gsaMember);
      return retList;
    }

    private List<GsaRecord> GSAMember2dToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleMember = (GSAMember2D)speckleObject;
      //Dynamic properties
      var dynamicMembers = speckleMember.GetMembers();
      var gsaMember = new GsaMemb()
      {
        ApplicationId = speckleMember.applicationId,
        Index = speckleMember.GetIndex<GsaMemb>(),
        Name = speckleMember.name,
        Type = speckleMember.type.ToNativeMember(),
        Colour = speckleMember.colour?.ColourToNative() ?? Colour.NotSet,
        Dummy = speckleMember.isDummy,
        IsIntersector = speckleMember.intersectsWithOthers,

        //Dynamic properties
        Exposure = speckleMember.GetDynamicEnum<ExposedSurfaces>("Exposure", dynamicMembers),
        AnalysisType = speckleMember.GetDynamicEnum<AnalysisType>("AnalysisType", dynamicMembers),
        Fire = speckleMember.GetDynamicEnum<FireResistance>("Fire", dynamicMembers),
        CreationFromStartDays = speckleMember.GetDynamicValue<int>("CreationFromStartDays", dynamicMembers),
        StartOfDryingDays = speckleMember.GetDynamicValue<int>("StartOfDryingDays", dynamicMembers),
        AgeAtLoadingDays = speckleMember.GetDynamicValue<int>("AgeAtLoadingDays", dynamicMembers),
        RemovedAtDays = speckleMember.GetDynamicValue<int>("RemovedAtDays", dynamicMembers),
        OffsetAutomaticInternal = speckleMember.GetDynamicValue<bool>("OffsetAutomaticInternal", dynamicMembers),
        LimitingTemperature = conversionFactors.TemperatureToNative(speckleMember.GetDynamicValue<double?>("LimitingTemperature", dynamicMembers)),
      };

      if (speckleMember.property != null)
      {
        gsaMember.PropertyIndex = IndexByConversionOrLookup<GsaProp2d>(speckleMember.property, ref retList);
      }
      if (speckleMember.topology != null && speckleMember.topology.Count > 0)
      {
        gsaMember.NodeIndices = speckleMember.topology.NodeAt(conversionFactors);
      }

      if (speckleMember.orientationAngle != 0) gsaMember.Angle = conversionFactors.ConversionFactorToDegrees() * speckleMember.orientationAngle;
      if (speckleMember.offset != 0) gsaMember.Offset2dZ = conversionFactors.length * speckleMember.offset;
      if (speckleMember.group > 0) gsaMember.Group = speckleMember.group;
      if (speckleMember.targetMeshSize > 0) gsaMember.MeshSize = conversionFactors.length * speckleMember.targetMeshSize;

      
      if (dynamicMembers.ContainsKey("Voids") && speckleMember["Voids"] is List<List<Node>>)
      {
        var speckleVoids = speckleObject["Voids"] as List<List<Node>>;
        gsaMember.Voids = speckleVoids.Select(v => v.NodeAt(conversionFactors)).ToList();
      }
      if (dynamicMembers.ContainsKey("Points") && speckleMember["Points"] is List<Node>)
      {
        var specklePoints = speckleObject["Points"] as List<Node>;
        gsaMember.PointNodeIndices = specklePoints.NodeAt(conversionFactors);
      }
      if (dynamicMembers.ContainsKey("Lines") && speckleMember["Lines"] is List<List<Node>>)
      {
        var speckleLines = speckleObject["Lines"] as List<List<Node>>;
        gsaMember.Polylines = speckleLines.Select(v => v.NodeAt(conversionFactors)).ToList();
      }
      if (dynamicMembers.ContainsKey("Areas") && speckleMember["Areas"] is List<List<Node>>)
      {
        var speckleAreas = speckleObject["Areas"] as List<List<Node>>;
        gsaMember.AdditionalAreas = speckleAreas.Select(v => v.NodeAt(conversionFactors)).ToList();
      }
      retList.Add(gsaMember);
      return retList;
    }

    private List<GsaRecord> GSAAssemblyToNative(Base speckleObject)
    {
      var speckleAssembly = (GSAAssembly)speckleObject;
      var gsaAssembly = new GsaAssembly()
      {
        ApplicationId = speckleAssembly.applicationId,
        Index = speckleAssembly.GetIndex<GsaAssembly>(),
        Name = speckleAssembly.name,
        SizeY = conversionFactors.length * speckleAssembly.sizeY,
        SizeZ = conversionFactors.length * speckleAssembly.sizeZ,
        CurveType = Enum.TryParse(speckleAssembly.curveType, true, out CurveType ct) ? ct : CurveType.NotSet,
        PointDefn = Enum.TryParse(speckleAssembly.pointDefinition, true, out PointDefinition pd) ? pd : PointDefinition.NotSet,   
        Topo1 = speckleAssembly.end1Node.NodeAt(conversionFactors),
        Topo2 = speckleAssembly.end2Node.NodeAt(conversionFactors),
        OrientNode = speckleAssembly.orientationNode.NodeAt(conversionFactors),
        StoreyIndices = new List<int>(),
        ExplicitPositions = new List<double>(),
      };

      if (speckleAssembly.curveOrder > 0) gsaAssembly.CurveOrder = speckleAssembly.curveOrder;
      if (speckleAssembly.points != null)
      {
        switch (gsaAssembly.PointDefn)
        {
          case PointDefinition.Points: 
            gsaAssembly.NumberOfPoints = (int)speckleAssembly.points[0];
            break;
          case PointDefinition.Spacing:
            gsaAssembly.Spacing = conversionFactors.length * speckleAssembly.points[0];
            break;
          case PointDefinition.Storey:
            gsaAssembly.StoreyIndices = speckleAssembly.points.Select(i => (int)i).ToList();
            break;
          case PointDefinition.Explicit:
            gsaAssembly.ExplicitPositions = speckleAssembly.points.Select(p => conversionFactors.length * p).ToList();
            break;
        }
      }
      if (speckleAssembly.entities != null)
      {
        var speckleNodes = speckleAssembly.entities.FindAll(e => e is Node).Select(e => (Node)e).ToList();
        gsaAssembly.IntTopo = speckleNodes.NodeAt(conversionFactors) ?? new List<int>();
        gsaAssembly.ElementIndices = new List<int>();
        gsaAssembly.ElementIndices.AddRange(speckleAssembly.entities.FindAll(e => e is Element1D).GetIndicies<GsaEl>() ?? new List<int>());
        gsaAssembly.ElementIndices.AddRange(speckleAssembly.entities.FindAll(e => e is Element2D).GetIndicies<GsaEl>() ?? new List<int>());
        gsaAssembly.MemberIndices = new List<int>();
        gsaAssembly.MemberIndices.AddRange(speckleAssembly.entities.FindAll(e => e is GSAMember1D).GetIndicies<GsaMemb>() ?? new List<int>());
        gsaAssembly.MemberIndices.AddRange(speckleAssembly.entities.FindAll(e => e is GSAMember2D).GetIndicies<GsaMemb>() ?? new List<int>());
        if (gsaAssembly.ElementIndices.Count() > 0) gsaAssembly.Type = GSAEntity.ELEMENT;
        else if (gsaAssembly.MemberIndices.Count() > 0) gsaAssembly.Type = GSAEntity.MEMBER;
      }

      return new List<GsaRecord>() { gsaAssembly };
    }

    private List<GsaRecord> GSAGridLineToNative(Base speckleObject)
    {
      var speckleGridLine = (GSAGridLine)speckleObject;

      var gsaGridLine = new GsaGridLine()
      {
        Index = speckleGridLine.GetIndex<GsaGridLine>(),
        Name = speckleGridLine.label,
        ApplicationId = speckleGridLine.applicationId
      };
      if (speckleGridLine.baseLine is Arc)
      {
        gsaGridLine.Type = GridLineType.Arc;
        var speckleArc = (Arc)speckleGridLine.baseLine;
        if (speckleArc.radius != null) gsaGridLine.Length = conversionFactors.length * speckleArc.radius;
        if (speckleArc.plane != null && speckleArc.plane.origin != null)
        {
          var factor = speckleArc.plane.origin.GetScaleFactor(conversionFactors);
          gsaGridLine.XCoordinate = factor * speckleArc.plane.origin.x;
          gsaGridLine.YCoordinate = factor * speckleArc.plane.origin.y;
        }
        
        if (speckleArc.startAngle != null)
        {
          gsaGridLine.Theta1 = speckleArc.startAngle.Value.Degrees();
        }
        if (speckleArc.endAngle != null)
        {
          gsaGridLine.Theta2 = speckleArc.endAngle.Value.Degrees();
        }
      }
      else if (speckleGridLine.baseLine is Line)
      {
        gsaGridLine.Type = GridLineType.Line;
        var speckleLine = (Line)speckleGridLine.baseLine;
        if (speckleLine.start != null && speckleLine.end != null)
        {
          var factor = conversionFactors.length;
          if (speckleLine.start.units == speckleLine.end.units) factor = speckleLine.start.GetScaleFactor(conversionFactors); //ensure both points have the same units, otherwise scale based on model units
          gsaGridLine.XCoordinate = factor * speckleLine.start.x;
          gsaGridLine.YCoordinate = factor * speckleLine.start.y;

          var a = (speckleLine.end.x - speckleLine.start.x);
          var o = (speckleLine.end.y - speckleLine.start.y);
          var h = Hypotenuse(a, o);

          gsaGridLine.Length = factor * h;
          gsaGridLine.Theta1 = Math.Acos(a / h).Degrees();
        }
        
      }

      return new List<GsaRecord>() { gsaGridLine };
    }

    private List<GsaRecord> StoreyToNative(Base speckleObject)
    {
      var speckleStorey = (Storey)speckleObject;

      var gsaGridLine = new GsaGridPlane()
      {
        Index = speckleStorey.GetIndex<GsaGridPlane>(),
        Name = speckleStorey.name,
        ApplicationId = speckleStorey.applicationId,
        Elevation = conversionFactors.length * speckleStorey.elevation,
      };
      return new List<GsaRecord>() { gsaGridLine };
    }

    private List<GsaRecord> GSAGridPlaneToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleGridPlane = (GSAGridPlane)speckleObject;
      var gsaRecords = StoreyToNative(speckleObject);
      if (gsaRecords == null || gsaRecords.Count == 0 || (!(gsaRecords.First() is GsaGridPlane)))
      {
        return null;
      }

      var gsaGridPlane = (GsaGridPlane)(gsaRecords.First());
      if (speckleGridPlane.toleranceBelow.HasValue || speckleGridPlane.toleranceAbove.HasValue)
      {
        gsaGridPlane.Type = GridPlaneType.Storey;
        if (speckleGridPlane.toleranceAbove == null)
        {
          gsaGridPlane.StoreyToleranceAboveAuto = true;
        }
        else
        {
          gsaGridPlane.StoreyToleranceAbove = conversionFactors.length * speckleGridPlane.toleranceAbove;
        }
        if (speckleGridPlane.toleranceBelow == null)
        {
          gsaGridPlane.StoreyToleranceBelowAuto = true;
        }
        else
        {
          gsaGridPlane.StoreyToleranceBelow = conversionFactors.length * speckleGridPlane.toleranceBelow;
        }
      }
      else
      {
        gsaGridPlane.Type = GridPlaneType.General;
      }

      if (speckleGridPlane.axis == null)
      {
        gsaGridPlane.AxisRefType = GridPlaneAxisRefType.NotSet;
      }
      else if (IsGlobalAxis(speckleGridPlane.axis))
      {
        gsaGridPlane.AxisRefType = GridPlaneAxisRefType.Global;
      }
      else if (IsXElevationAxis(speckleGridPlane.axis))
      {
        gsaGridPlane.AxisRefType = GridPlaneAxisRefType.XElevation;
      }
      else if (IsYElevationAxis(speckleGridPlane.axis))
      {
        gsaGridPlane.AxisRefType = GridPlaneAxisRefType.YElevation;
      }
      else
      {
        var axisIndex = IndexByConversionOrLookup<GsaAxis>(speckleGridPlane.axis, ref retList);
        if (axisIndex.IsIndex())
        {
          gsaGridPlane.AxisIndex = axisIndex;
          gsaGridPlane.AxisRefType = GridPlaneAxisRefType.Reference;
        }
      }

      retList.Add(gsaGridPlane);

      return retList;
    }

    private List<GsaRecord> GSAGridSurfaceToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleGridSurface = (GSAGridSurface)speckleObject;

      var gsaGridSurface = new GsaGridSurface()
      {
        Index = speckleGridSurface.GetIndex<GsaGridSurface>(),
        ApplicationId = speckleGridSurface.applicationId,
        Name = speckleGridSurface.name,
        Tolerance = conversionFactors.length * speckleGridSurface.tolerance,
        Angle = conversionFactors.ConversionFactorToDegrees() * speckleGridSurface.spanDirection,
        Expansion = speckleGridSurface.loadExpansion.ToNative(),
        Span = speckleGridSurface.span.ToNative()
      };

      if (speckleGridSurface.elements != null)
      {
        //The ElementIndices collection should be initialised by the GsaGridSurface constructor
        foreach (var e in speckleGridSurface.elements)
        {
          var isMemb = ((e is GSAMember1D) || (e is GSAMember2D));
          bool found = false;
          if (!string.IsNullOrEmpty(e.applicationId))
          {
            if (isMemb)
            {
              var index = Instance.GsaModel.Cache.LookupIndex<GsaMemb>(e.applicationId);
              if (index.HasValue)
              {
                gsaGridSurface.MemberIndices.Add(index.Value);
                found = true;
              }
            }
            else
            {
              var index = Instance.GsaModel.Cache.LookupIndex<GsaEl>(e.applicationId);
              if (index.HasValue)
              {
                gsaGridSurface.ElementIndices.Add(index.Value);
                found = true;
              }
            }
          }
          if (!found)
          {
            int? index = null;
            var nativeObjects = new List<GsaRecord>();
            if (isMemb)
            {
              if (e is GSAMember1D)
              {
                nativeObjects.AddRange(GSAMember1dToNative(e));
              }
              else if (e is GSAMember2D)
              {
                nativeObjects.AddRange(GSAMember2dToNative(e));
              }
              if (nativeObjects.Count > 0)
              {
                var newMemb = nativeObjects.FirstOrDefault(o => o is GsaMemb);
                if (newMemb != null)
                {
                  index = newMemb.Index;
                }
              }
              if (index.HasValue)
              {
                gsaGridSurface.MemberIndices.Add(index.Value);
              }
            }
            else  //element by default otherwise
            {
              if (e is GSAElement1D)
              {
                nativeObjects.AddRange(GSAElement1dToNative(e));
              }
              else if (e is Element1D)
              {
                nativeObjects.AddRange(Element1dToNative(e));
              }
              else if (e is GSAElement2D)
              {
                nativeObjects.AddRange(GSAMember2dToNative(e));
              }
              else if (e is Element2D)
              {
                nativeObjects.AddRange(Element2dToNative(e));
              }
              if (nativeObjects.Count > 0)
              {
                var newEl = nativeObjects.FirstOrDefault(o => o is GsaEl);
                if (newEl != null)
                {
                  index = newEl.Index;
                }
              }
              if (index.HasValue)
              {
                gsaGridSurface.ElementIndices.Add(index.Value);
              }
            }
            retList.AddRange(nativeObjects);
          }
        }

        if (speckleGridSurface.elements.Any(e => ((e is GSAElement1D) || (e is GSAMember1D) || (e is Element1D))))
        {
          gsaGridSurface.Type = GridSurfaceElementsType.OneD;
        }
        else if (speckleGridSurface.elements.Any(e => ((e is GSAElement2D) || (e is GSAMember2D) || (e is Element2D))))
        {
          gsaGridSurface.Type = GridSurfaceElementsType.TwoD;
        }
      }

      if (speckleGridSurface.gridPlane != null)
      {
        if (!string.IsNullOrEmpty(speckleGridSurface.gridPlane.applicationId))
        {
          var planeIndex = Instance.GsaModel.Cache.LookupIndex<GsaGridPlane>(speckleGridSurface.gridPlane.applicationId);
          if (planeIndex.IsIndex())
          {
            gsaGridSurface.PlaneIndex = planeIndex;
            gsaGridSurface.PlaneRefType = GridPlaneAxisRefType.Reference;
          }
          else
          {
            var planeGsaRecords = GSAGridPlaneToNative(speckleGridSurface.gridPlane);
            var gsaPlane = planeGsaRecords.FirstOrDefault(r => r is GsaGridPlane);
            if (gsaPlane != null && gsaPlane.Index.IsIndex())
            {
              gsaGridSurface.PlaneIndex = gsaPlane.Index;
              retList.Add(gsaPlane);
            }
          }
        }
        else if (speckleGridSurface.gridPlane.axis != null)
        {
          if (IsGlobalAxis(speckleGridSurface.gridPlane.axis))
          {
            gsaGridSurface.PlaneRefType = GridPlaneAxisRefType.Global;
          }
          else if (IsXElevationAxis(speckleGridSurface.gridPlane.axis))
          {
            gsaGridSurface.PlaneRefType = GridPlaneAxisRefType.XElevation;
          }
          else if (IsYElevationAxis(speckleGridSurface.gridPlane.axis))
          {
            gsaGridSurface.PlaneRefType = GridPlaneAxisRefType.YElevation;
          }
        }
      }

      retList.Add(gsaGridSurface);
      return retList;
    }

    private double Hypotenuse(double a, double o) => Math.Sqrt((a * a) + (o * o));

    private bool IsGlobalAxis(Axis x) => ((x.axisType == AxisType.Cartesian)
      && x.definition.origin.Equals(Origin, GeometricDecimalPlaces)
      && x.definition.xdir.Equals(UnitX, GeometricDecimalPlaces)
      && x.definition.ydir.Equals(UnitY, GeometricDecimalPlaces)
      && x.definition.normal.Equals(UnitZ, GeometricDecimalPlaces));

    private bool IsXElevationAxis(Axis x) => ((x.axisType == AxisType.Cartesian)
      && x.definition.origin.Equals(Origin, GeometricDecimalPlaces)
      && x.definition.xdir.Equals(UnitY * -1, GeometricDecimalPlaces)
      && x.definition.ydir.Equals(UnitZ, GeometricDecimalPlaces)
      && x.definition.normal.Equals(UnitX * -1, GeometricDecimalPlaces));

    private bool IsYElevationAxis(Axis x) => ((x.axisType == AxisType.Cartesian)
      && x.definition.origin.Equals(Origin, GeometricDecimalPlaces)
      && x.definition.xdir.Equals(UnitX, GeometricDecimalPlaces)
      && x.definition.ydir.Equals(UnitZ, GeometricDecimalPlaces)
      && x.definition.normal.Equals(UnitY * -1, GeometricDecimalPlaces));

    /* not used (yet) - review if needed
    private bool IsVerticalAxis(Axis x) => ((x.axisType == AxisType.Cartesian)
      && x.definition.origin.Equals(Origin, GeometricDecimalPlaces)
      && x.definition.xdir.Equals(UnitZ, GeometricDecimalPlaces)
      && x.definition.ydir.Equals(UnitZ, GeometricDecimalPlaces)
      && x.definition.normal.Equals(UnitY, GeometricDecimalPlaces));
    */

      private List<GsaRecord> GSAPolylineToNative(Base speckleObject)
    {
      var specklePolyline = (GSAPolyline)speckleObject;
      var gsaPolyline = new GsaPolyline()
      {
        ApplicationId = specklePolyline.applicationId,
        Index = specklePolyline.GetIndex<GsaPolyline>(),
        Name = specklePolyline.name,
        GridPlaneIndex = specklePolyline.gridPlane.GetIndex<GsaGridPlane>(),
        NumDim = specklePolyline.Is3d() ? 3 : 2,
        Values = specklePolyline.GetValues().ToList(), //As gsaPolyline has units, no need to scale values
        Units = specklePolyline.units,
        Colour = specklePolyline.colour.ColourToNative(),
      };
      return new List<GsaRecord>() { gsaPolyline };
    }
    
#endregion

#region Loading
    private List<GsaRecord> GSALoadCaseToNative(Base speckleObject)
    {
      var gsaLoadCase = (GsaLoadCase)LoadCaseToNative(speckleObject).First(o => o is GsaLoadCase);
      var speckleLoadCase = (GSALoadCase)speckleObject;
      gsaLoadCase.Direction = speckleLoadCase.direction.ToNative();
      gsaLoadCase.Include = speckleLoadCase.include.IncludeOptionToNative();
      if (speckleLoadCase.bridge) gsaLoadCase.Bridge = true;
      return new List<GsaRecord>() { gsaLoadCase };
    }

    private List<GsaRecord> LoadCaseToNative(Base speckleObject)
    {
      var speckleLoadCase = (LoadCase)speckleObject;
      var gsaLoadCase = new GsaLoadCase()
      {
        ApplicationId = speckleLoadCase.applicationId,
        Index = speckleLoadCase.GetIndex<GsaLoadCase>(),
        Title = speckleLoadCase.name,
        CaseType = speckleLoadCase.loadType.ToNative(),
        Category = speckleLoadCase.description.LoadCategoryToNative()
      };
      if (!string.IsNullOrEmpty(speckleLoadCase.group) && int.TryParse(speckleLoadCase.group, out int group))
      {
        gsaLoadCase.Source = group;
      }
      return new List<GsaRecord>() { gsaLoadCase };
    }

    private List<GsaRecord> GSAAnalysisCaseToNative(Base speckleObject)
    {
      var speckleCase = (GSAAnalysisCase)speckleObject;
      var gsaCase = new GsaAnal()
      {
        ApplicationId = speckleCase.applicationId,
        Index = speckleCase.GetIndex<GsaAnal>(),
        Name = speckleCase.name,
        //TaskIndex = speckleCase.task.GetIndex<GsaTask>(), //TODO:
        Desc = GetAnalysisCaseDescription(speckleCase.loadCases, speckleCase.loadFactors),
      };
      return new List<GsaRecord>() { gsaCase };
    }

    private List<GsaRecord> GSALoadCombinationToNative(Base speckleObject)
    {
      var gsaLoadCombination = (GsaCombination)LoadCombinationToNative(speckleObject).First(o => o is GsaCombination);
      var speckleLoadCombination = (GSALoadCombination)speckleObject;
      gsaLoadCombination.Bridge = speckleLoadCombination.GetDynamicValue<bool?>("bridge");
      gsaLoadCombination.Note = speckleLoadCombination.GetDynamicValue<string>("note");
      return new List<GsaRecord>() { gsaLoadCombination };
    }

    private List<GsaRecord> LoadCombinationToNative(Base speckleObject)
    {
      var speckleLoadCombination = (LoadCombination)speckleObject;
      var gsaLoadCombination = new GsaCombination()
      {
        ApplicationId = speckleLoadCombination.applicationId,
        Index = speckleLoadCombination.GetIndex<GsaCombination>(),
        Name = speckleLoadCombination.name,
        Desc = GetLoadCombinationDescription(speckleLoadCombination.combinationType, speckleLoadCombination.loadCases, speckleLoadCombination.loadFactors),
      };
      return new List<GsaRecord>() { gsaLoadCombination };
    }

#region LoadBeam
    private List<GsaRecord> GSALoadBeamToNative(Base speckleObject)
    {
      var gsaLoad = (GsaLoadBeam)LoadBeamToNative(speckleObject).First(o => o is GsaLoadBeam);
      var speckleLoad = (GSALoadBeam)speckleObject;
      //Add any app specific conversions here
      return new List<GsaRecord>() { gsaLoad };
    }

    private List<GsaRecord> LoadBeamToNative(Base speckleObject)
    {
      var speckleLoad = (LoadBeam)speckleObject;
      GsaLoadBeam gsaLoad = null;

      var fns = new Dictionary<BeamLoadType, Func<LoadBeam, GsaLoadBeam>>
      { { BeamLoadType.Uniform, LoadBeamUniformToNative },
        { BeamLoadType.Linear, LoadBeamLinearToNative },
        { BeamLoadType.Point, LoadBeamPointToNative },
        { BeamLoadType.Patch, LoadBeamPatchToNative },
        { BeamLoadType.TriLinear, LoadBeamTriLinearToNative },
      };

      //Apply spring type specific properties
      if (fns.ContainsKey(speckleLoad.loadType))
      {
        gsaLoad = fns[speckleLoad.loadType](speckleLoad);
      }
      else
      {
        ConversionErrors.Add(new Exception("LoadBeamToNative: beam load type (" + speckleLoad.loadType.ToString() + ") is not currently supported"));
      }

      return new List<GsaRecord>() { gsaLoad };
    }

    private GsaLoadBeam LoadBeamUniformToNative(LoadBeam speckleLoad)
    {
      var gsaLoad = LoadBeamBaseToNative<GsaLoadBeamUdl>(speckleLoad);
      if (speckleLoad.values != null && speckleLoad.values.Count() >= 1)
      {
        gsaLoad.Load = speckleLoad.values[0];
      }
      return gsaLoad;
    }

    private GsaLoadBeam LoadBeamLinearToNative(LoadBeam speckleLoad)
    {
      var gsaLoad = LoadBeamBaseToNative<GsaLoadBeamLine>(speckleLoad);
      if (speckleLoad.values != null && speckleLoad.values.Count() >= 2)
      {
        gsaLoad.Load1 = speckleLoad.values[0];
        gsaLoad.Load2 = speckleLoad.values[1];
      }
      return gsaLoad;
    }

    private GsaLoadBeam LoadBeamPointToNative(LoadBeam speckleLoad)
    {
      var gsaLoad = LoadBeamBaseToNative<GsaLoadBeamPoint>(speckleLoad);
      if (speckleLoad.values != null && speckleLoad.values.Count() >= 1)
      {
        gsaLoad.Load = speckleLoad.values[0];
      }
      if (speckleLoad.positions != null && speckleLoad.positions.Count() >= 1)
      {
        gsaLoad.Position = speckleLoad.positions[0];
      }
      return gsaLoad;
    }

    private GsaLoadBeam LoadBeamPatchToNative(LoadBeam speckleLoad)
    {
      var gsaLoad = LoadBeamBaseToNative<GsaLoadBeamPatch>(speckleLoad);
      if (speckleLoad.values != null && speckleLoad.values.Count() >= 2)
      {
        gsaLoad.Load1 = speckleLoad.values[0];
        gsaLoad.Load2 = speckleLoad.values[1];
      }
      if (speckleLoad.positions != null && speckleLoad.positions.Count() >= 2)
      {
        gsaLoad.Position1 = speckleLoad.positions[0];
        gsaLoad.Position2Percent = speckleLoad.positions[1];
      }
      return gsaLoad;
    }

    private GsaLoadBeam LoadBeamTriLinearToNative(LoadBeam speckleLoad)
    {
      var gsaLoad = LoadBeamBaseToNative<GsaLoadBeamTrilin>(speckleLoad);
      if (speckleLoad.values != null && speckleLoad.values.Count() >= 2)
      {
        gsaLoad.Load1 = speckleLoad.values[0];
        gsaLoad.Load2 = speckleLoad.values[1];
      }
      if (speckleLoad.positions != null && speckleLoad.positions.Count() >= 2)
      {
        gsaLoad.Position1 = speckleLoad.positions[0];
        gsaLoad.Position2Percent = speckleLoad.positions[1];
      }
      return gsaLoad;
    }

    private T LoadBeamBaseToNative<T>(LoadBeam speckleLoad) where T : GsaLoadBeam
    {
      var gsaLoad = (T)Activator.CreateInstance(typeof(T));
      gsaLoad.ApplicationId = speckleLoad.applicationId;
      gsaLoad.Index = speckleLoad.GetIndex<T>();
      gsaLoad.Name = speckleLoad.name;
      gsaLoad.LoadCaseIndex = speckleLoad.loadCase.GetIndex<GsaLoadCase>();
      gsaLoad.Projected = speckleLoad.isProjected;
      gsaLoad.LoadDirection = speckleLoad.direction.ToNative();
      gsaLoad.ElementIndices = speckleLoad.elements.GetIndicies<GsaEl>() ?? new List<int>();
      gsaLoad.MemberIndices = speckleLoad.elements.GetIndicies<GsaMemb>() ?? new List<int>();
      if (speckleLoad.loadAxis == null)
      {
        gsaLoad.AxisRefType = speckleLoad.loadAxisType.ToNativeBeamAxisRefType();
      }
      else
      {
        if (GetLoadAxis(speckleLoad.loadAxis, out LoadBeamAxisRefType gsaAxisRefType, out var gsaAxisIndex))
        {
          gsaLoad.AxisRefType = gsaAxisRefType;
          gsaLoad.AxisIndex = gsaAxisIndex;
        }
      }
      return gsaLoad;
    }
#endregion

    private List<GsaRecord> GSALoadFaceToNative(Base speckleObject)
    {
      var gsaLoad = (GsaLoad2dFace)LoadFaceToNative(speckleObject).First(o => o is GsaLoad2dFace);
      var speckleLoad = (GSALoadFace)speckleObject;
      //Add any app specific conversions here
      return new List<GsaRecord>() { gsaLoad };
    }

    private List<GsaRecord> LoadFaceToNative(Base speckleObject)
    {
      var speckleLoad = (LoadFace)speckleObject;
      var gsaLoad = new GsaLoad2dFace()
      {
        ApplicationId = speckleLoad.applicationId,
        Index = speckleLoad.GetIndex<GsaLoad2dFace>(),
        Name = speckleLoad.name,
        Type = speckleLoad.loadType.ToNative(),
        LoadCaseIndex = speckleLoad.loadCase.GetIndex<GsaLoadCase>(),
        Values = speckleLoad.values,
        LoadDirection = speckleLoad.direction.ToNative(),
        Projected = speckleLoad.isProjected,
        ElementIndices = speckleLoad.elements.GetIndicies<GsaEl>() ?? new List<int>(),
        MemberIndices = speckleLoad.elements.GetIndicies<GsaMemb>() ?? new List<int>(),
      };
      if (GetLoadAxis(speckleLoad.loadAxis, speckleLoad.loadAxisType, out var gsaAxisRefType, out var gsaAxisIndex))
      {
        gsaLoad.AxisRefType = gsaAxisRefType;
        gsaLoad.AxisIndex = gsaAxisIndex;
      }
      if (speckleLoad.positions != null && speckleLoad.positions.Count() >= 2)
      {
        gsaLoad.R = speckleLoad.positions[0];
        gsaLoad.S = speckleLoad.positions[1];
      }

      return new List<GsaRecord>() { gsaLoad };
    }

    private List<GsaRecord> GSALoadNodeToNative(Base speckleObject)
    {
      var gsaLoad = (GsaLoadNode)LoadNodeToNative(speckleObject).First(o => o is GsaLoadNode);
      var speckleLoad = (GSALoadNode)speckleObject;
      //Add any app specific conversions here
      return new List<GsaRecord>() { gsaLoad };
    }

    private List<GsaRecord> LoadNodeToNative(Base speckleObject)
    {
      var speckleLoad = (LoadNode)speckleObject;
      var gsaLoad = new GsaLoadNode()
      {
        ApplicationId = speckleLoad.applicationId,
        Index = speckleLoad.GetIndex<GsaLoadNode>(),
        Name = speckleLoad.name,
        LoadDirection = speckleLoad.direction.ToNative(),
        LoadCaseIndex = speckleLoad.loadCase.GetIndex<GsaLoadCase>()
      };
      if (speckleLoad.nodes != null && speckleLoad.nodes.Count > 0)
      {
        gsaLoad.NodeIndices = speckleLoad.nodes.Where(n => n!= null && n.basePoint != null)
          .Select(n => Instance.GsaModel.Proxy.NodeAt(n.basePoint.x, n.basePoint.y, n.basePoint.z, 
          Instance.GsaModel.CoincidentNodeAllowance)).ToList();
      }

      if (speckleLoad.value != 0) gsaLoad.Value = speckleLoad.value;
      if (speckleLoad.loadAxis.definition.IsGlobal())
      {
        gsaLoad.GlobalAxis = true;
      }
      else
      {
        gsaLoad.GlobalAxis = false;
        gsaLoad.AxisIndex = speckleLoad.loadAxis.GetIndex<GsaAxis>();
      }
      return new List<GsaRecord>() { gsaLoad };
    }

    private List<GsaRecord> GSALoadGravityToNative(Base speckleObject)
    {
      var gsaLoad = (GsaLoadGravity)LoadGravityToNative(speckleObject).First(o => o is GsaLoadGravity);
      var speckleLoad = (GSALoadGravity)speckleObject;
      //Add any app specific conversions here
      return new List<GsaRecord>() { gsaLoad };
    }

    private List<GsaRecord> LoadGravityToNative(Base speckleObject)
    {
      var speckleLoad = (LoadGravity)speckleObject;
      var gsaLoad = new GsaLoadGravity()
      {
        ApplicationId = speckleLoad.applicationId,
        Index = speckleLoad.GetIndex<GsaLoadGravity>(),
        Name = speckleLoad.name,
        LoadCaseIndex = speckleLoad.loadCase.GetIndex<GsaLoadCase>(),
        ElementIndices = speckleLoad.elements.GetIndicies<GsaEl>() ?? new List<int>(),
        MemberIndices = speckleLoad.elements.GetIndicies<GsaMemb>() ?? new List<int>(),
      };

      if (speckleLoad.nodes != null && speckleLoad.nodes.Count > 0)
      {
        var nodes = speckleLoad.nodes.Select(n => (Node)n).ToList();
        gsaLoad.Nodes = nodes.Select(n => Instance.GsaModel.Proxy.NodeAt(n.basePoint.x, n.basePoint.y, n.basePoint.z, Instance.GsaModel.CoincidentNodeAllowance)).ToList();
      }

      if (speckleLoad.gravityFactors != null)
      { 
        if (speckleLoad.gravityFactors.x != 0) gsaLoad.X = speckleLoad.gravityFactors.x;
        if (speckleLoad.gravityFactors.y != 0) gsaLoad.Y = speckleLoad.gravityFactors.y;
        if (speckleLoad.gravityFactors.z != 0) gsaLoad.Z = speckleLoad.gravityFactors.z;
      }

      return new List<GsaRecord>() { gsaLoad };
    }

    private List<GsaRecord> GSALoadThermal2dToNative(Base speckleObject)
    {
      var speckleLoad = (GSALoadThermal2d)speckleObject;
      var gsaLoad = new GsaLoad2dThermal()
      {
        ApplicationId = speckleLoad.applicationId,
        Index = speckleLoad.GetIndex<GsaLoad2dThermal>(),
        Name = speckleLoad.name,
        LoadCaseIndex = speckleLoad.loadCase.GetIndex<GsaLoadCase>(),
        Type = speckleLoad.type.ToNative(),
        Values = speckleLoad.values,
      };
      if (speckleLoad.elements != null)
      {
        var speckleElements = speckleLoad.elements.Select(o => (Base)o).ToList();
        gsaLoad.ElementIndices = speckleElements.GetIndicies<GsaEl>() ?? new List<int>();
        gsaLoad.MemberIndices = speckleElements.GetIndicies<GsaMemb>() ?? new List<int>();
      }
      return new List<GsaRecord>() { gsaLoad };
    }

    private List<GsaRecord> GSALoadGridPointToNative(Base speckleObject)
    {
      var speckleLoad = (GSALoadGridPoint)speckleObject;
      var gsaLoad = new GsaLoadGridPoint()
      {
        ApplicationId = speckleLoad.applicationId,
        Index = speckleLoad.GetIndex<GsaLoadGridPoint>(),
        Name = speckleLoad.name,
        LoadCaseIndex = speckleLoad.loadCase.GetIndex<GsaLoadCase>(),
        GridSurfaceIndex = speckleLoad.gridSurface.GetIndex<GsaGridSurface>(),
        LoadDirection = speckleLoad.direction.ToNative(),
      };
      if (speckleLoad.value != 0) gsaLoad.Value = speckleLoad.value;
      if (GetLoadAxis(speckleLoad.loadAxis, out AxisRefType gsaAxisRefType, out var gsaAxisIndex))
      {
        gsaLoad.AxisRefType = gsaAxisRefType;
        gsaLoad.AxisIndex = gsaAxisIndex;
      }
      if (speckleLoad.position != null)
      {
        if (speckleLoad.position.x != 0) gsaLoad.X = speckleLoad.position.x;
        if (speckleLoad.position.y != 0) gsaLoad.Y = speckleLoad.position.y;
      }
      return new List<GsaRecord>() { gsaLoad };
    }

    private List<GsaRecord> GSALoadGridLineToNative(Base speckleObject)
    {
      var speckleLoad = (GSALoadGridLine)speckleObject;
      var gsaLoad = new GsaLoadGridLine()
      {
        ApplicationId = speckleLoad.applicationId,
        Index = speckleLoad.GetIndex<GsaLoadGridLine>(),
        Name = speckleLoad.name,
        LoadCaseIndex = speckleLoad.loadCase.GetIndex<GsaLoadCase>(),
        GridSurfaceIndex = speckleLoad.gridSurface.GetIndex<GsaGridSurface>(),
        LoadDirection = speckleLoad.direction.ToNative(),
        Projected = speckleLoad.isProjected,
      };
      if (GetLoadAxis(speckleLoad.loadAxis, out AxisRefType gsaAxisRefType, out var gsaAxisIndex))
      {
        gsaLoad.AxisRefType = gsaAxisRefType;
        gsaLoad.AxisIndex = gsaAxisIndex;
      }
      if (speckleLoad.values != null && speckleLoad.values.Count >= 2)
      {
        if (speckleLoad.values[0] != 0) gsaLoad.Value1 = speckleLoad.values[0];
        if (speckleLoad.values[1] != 0) gsaLoad.Value2 = speckleLoad.values[1];
      }
      if (GetPolyline(speckleLoad.polyline, out LoadLineOption gsaOption, out var gsaPolygon, out var gsaPolygonIndex))
      {
        gsaLoad.Line = gsaOption;
        gsaLoad.Polygon = gsaPolygon;
        gsaLoad.PolygonIndex = gsaPolygonIndex;
      }
      return new List<GsaRecord>() { gsaLoad };
    }

    private List<GsaRecord> GSALoadGridAreaToNative(Base speckleObject)
    {
      var speckleLoad = (GSALoadGridArea)speckleObject;
      var gsaLoad = new GsaLoadGridArea()
      {
        ApplicationId = speckleLoad.applicationId,
        Index = speckleLoad.GetIndex<GsaLoadGridArea>(),
        Name = speckleLoad.name,
        LoadCaseIndex = speckleLoad.loadCase.GetIndex<GsaLoadCase>(),
        GridSurfaceIndex = speckleLoad.gridSurface.GetIndex<GsaGridSurface>(),
        LoadDirection = speckleLoad.direction.ToNative(),
        Projected = speckleLoad.isProjected,
      };
      if (speckleLoad.value != 0) gsaLoad.Value = speckleLoad.value;
      if (GetLoadAxis(speckleLoad.loadAxis, out AxisRefType gsaAxisRefType, out var gsaAxisIndex))
      {
        gsaLoad.AxisRefType = gsaAxisRefType;
        gsaLoad.AxisIndex = gsaAxisIndex;
      }
      if (GetPolyline(speckleLoad.polyline, out LoadAreaOption gsaOption, out var gsaPolygon, out var gsaPolygonIndex))
      {
        gsaLoad.Area = gsaOption;
        gsaLoad.Polygon = gsaPolygon;
        gsaLoad.PolygonIndex = gsaPolygonIndex;
      }
      return new List<GsaRecord>() { gsaLoad };
    }

#endregion

#region Materials
    private List<GsaRecord> GSASteelToNative(Base speckleObject)
    {
      var gsaSteel = (GsaMatSteel)SteelToNative(speckleObject).First(o => o is GsaMatSteel);
      var speckleSteel = (GSASteel)speckleObject;
      gsaSteel.Mat = GetMat(speckleSteel.GetDynamicValue<Base>("Mat"));
      return new List<GsaRecord>() { gsaSteel };
    }

    private List<GsaRecord> SteelToNative(Base speckleObject)
    {
      //Values based on GSA10.1 with design code AS4100-1998, material grade 200-450 from AS3678
      var speckleSteel = (Steel)speckleObject;
      var eps = GetSteelStrain(speckleSteel.yieldStrength);
      var gsaSteel = new GsaMatSteel()
      {
        ApplicationId = speckleSteel.applicationId,
        Index = speckleSteel.GetIndex<GsaMatSteel>(),
        Mat = new GsaMat()
        {
          E = speckleSteel.elasticModulus.IsPositiveOrNull(),
          F = speckleSteel.yieldStrength.IsPositiveOrNull(),
          Nu = speckleSteel.poissonsRatio.IsPositiveOrNull(),
          G = speckleSteel.shearModulus.IsPositiveOrNull(),
          Rho = speckleSteel.density.IsPositiveOrNull(),
          Alpha = speckleSteel.thermalExpansivity.IsPositiveOrNull(),
          Prop = new GsaMatAnal()
          {
            Type = MatAnalType.MAT_ELAS_ISO,
            NumParams = 6,
            E = speckleSteel.elasticModulus.IsPositiveOrNull(),
            Nu = speckleSteel.poissonsRatio.IsPositiveOrNull(),
            Rho = speckleSteel.density.IsPositiveOrNull(),
            Alpha = speckleSteel.thermalExpansivity.IsPositiveOrNull(),
            G = speckleSteel.shearModulus.IsPositiveOrNull(),
            Damp = speckleSteel.dampingRatio.IsPositiveOrNull(),
          },
          NumUC = 0,
          AbsUC = Dimension.NotSet,
          OrdUC = Dimension.NotSet,
          PtsUC = null,
          NumSC = 0,
          AbsSC = Dimension.NotSet,
          OrdSC = Dimension.NotSet,
          PtsSC = null,
          NumUT = 0,
          AbsUT = Dimension.NotSet,
          OrdUT = Dimension.NotSet,
          PtsUT = null,
          NumST = 0,
          AbsST = Dimension.NotSet,
          OrdST = Dimension.NotSet,
          PtsST = null,
          Eps = speckleSteel.maxStrain.IsPositiveOrNull(),
          Uls = new GsaMatCurveParam()
          {
            Model = new List<MatCurveParamType>() { MatCurveParamType.ELAS_PLAS },
            StrainElasticCompression = eps,
            StrainElasticTension = eps,
            StrainPlasticCompression = eps,
            StrainPlasticTension = eps,
            StrainFailureCompression = speckleSteel.maxStrain.IsPositiveOrNull(),
            StrainFailureTension = speckleSteel.maxStrain.IsPositiveOrNull(),
            GammaF = 1,
            GammaE = 1,
          },
          Sls = new GsaMatCurveParam()
          {
            Model = new List<MatCurveParamType>() { MatCurveParamType.ELAS_PLAS },
            StrainElasticCompression = eps,
            StrainElasticTension = eps,
            StrainPlasticCompression = eps,
            StrainPlasticTension = eps,
            StrainFailureCompression = speckleSteel.maxStrain.IsPositiveOrNull(),
            StrainFailureTension = speckleSteel.maxStrain.IsPositiveOrNull(),
            GammaF = 1,
            GammaE = 1,
          },
          Cost = speckleSteel.cost.IsPositiveOrNull(),
          Type = MatType.STEEL,
        },
        Fy = speckleSteel.yieldStrength.IsPositiveOrNull(),
        Fu = speckleSteel.ultimateStrength.IsPositiveOrNull(),
        EpsP = null,
        Eh = speckleSteel.strainHardeningModulus.IsPositiveOrNull(),
      };

      if (!string.IsNullOrEmpty(speckleSteel.name))
      {
        gsaSteel.Name = speckleSteel.name;
      }

      //TODO:
      //SpeckleObject:
      //  string grade
      //  string designCode
      //  string codeYear
      //  double strength
      //  double materialSafetyFactor
      return new List<GsaRecord>() { gsaSteel };
    }

    private List<GsaRecord> GSAConcreteToNative(Base speckleObject)
    {
      var gsaConcrete = (GsaMatConcrete)ConcreteToNative(speckleObject).First(o => o is GsaMatConcrete);
      var speckleConcrete = (GSAConcrete)speckleObject;
      var dynamicMembers = speckleConcrete.GetMembers();
      gsaConcrete.Mat = GetMat(speckleConcrete.GetDynamicValue<Base>("Mat", dynamicMembers));
      gsaConcrete.Type = speckleConcrete.GetDynamicEnum<MatConcreteType>("Type", dynamicMembers);
      gsaConcrete.Cement = speckleConcrete.GetDynamicEnum<MatConcreteCement>("Cement", dynamicMembers);
      gsaConcrete.Fcd = speckleConcrete.GetDynamicValue<double>("Fcd", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.Fcdc = speckleConcrete.GetDynamicValue<double>("Fcdc", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.Fcfib = speckleConcrete.GetDynamicValue<double>("Fcfib", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.EmEs = speckleConcrete.GetDynamicValue<double>("EmEs", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.N = speckleConcrete.GetDynamicValue<double>("N", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.Emod = speckleConcrete.GetDynamicValue<double>("Emod", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.Eps = speckleConcrete.GetDynamicValue<double>("Eps", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.EpsPeak = speckleConcrete.GetDynamicValue<double>("EpsPeak", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.EpsMax = speckleConcrete.GetDynamicValue<double>("EpsMax", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.EpsAx = speckleConcrete.GetDynamicValue<double>("EpsAx", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.EpsTran = speckleConcrete.GetDynamicValue<double>("EpsTran", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.EpsAxs = speckleConcrete.GetDynamicValue<double>("EpsAxs", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.XdMin = speckleConcrete.GetDynamicValue<double>("XdMin", dynamicMembers);
      gsaConcrete.XdMax = speckleConcrete.GetDynamicValue<double>("XdMax", dynamicMembers);
      gsaConcrete.Beta = speckleConcrete.GetDynamicValue<double>("Beta", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.Shrink = speckleConcrete.GetDynamicValue<double>("Shrink", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.Confine = speckleConcrete.GetDynamicValue<double>("Confine", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.Fcc = speckleConcrete.GetDynamicValue<double>("Fcc", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.EpsPlasC = speckleConcrete.GetDynamicValue<double>("EpsPlasC", dynamicMembers).IsPositiveOrNull();
      gsaConcrete.EpsUC = speckleConcrete.GetDynamicValue<double>("EpsUC", dynamicMembers).IsPositiveOrNull();

      return new List<GsaRecord>() { gsaConcrete };
    }

    private List<GsaRecord> ConcreteToNative(Base speckleObject)
    {
      //Values based on GSA10.1 with design code AS3600-2018
      var speckleConcrete = (Concrete)speckleObject;
      var eps = GetEpsMax(speckleConcrete.compressiveStrength);
      var gsaConcrete = new GsaMatConcrete()
      {
        ApplicationId = speckleConcrete.applicationId,
        Index = speckleConcrete.GetIndex<GsaMatConcrete>(),
        Name = speckleConcrete.name,
        Mat = new GsaMat()
        {
          E = speckleConcrete.elasticModulus.IsPositiveOrNull(),
          F = speckleConcrete.compressiveStrength.IsPositiveOrNull(),
          Nu = speckleConcrete.poissonsRatio.IsPositiveOrNull(),
          G = speckleConcrete.shearModulus.IsPositiveOrNull(),
          Rho = speckleConcrete.density.IsPositiveOrNull(),
          Alpha = speckleConcrete.thermalExpansivity.IsPositiveOrNull(),
          Prop = new GsaMatAnal()
          {
            Type = MatAnalType.MAT_ELAS_ISO,
            NumParams = 6,
            E = speckleConcrete.elasticModulus.IsPositiveOrNull(),
            Nu = speckleConcrete.poissonsRatio.IsPositiveOrNull(),
            Rho = speckleConcrete.density.IsPositiveOrNull(),
            Alpha = speckleConcrete.thermalExpansivity.IsPositiveOrNull(),
            G = speckleConcrete.shearModulus.IsPositiveOrNull(),
            Damp = speckleConcrete.dampingRatio.IsPositiveOrNull(),
          },
          NumUC = 0,
          AbsUC = Dimension.NotSet,
          OrdUC = Dimension.NotSet,
          PtsUC = null,
          NumSC = 0,
          AbsSC = Dimension.NotSet,
          OrdSC = Dimension.NotSet,
          PtsSC = null,
          NumUT = 0,
          AbsUT = Dimension.NotSet,
          OrdUT = Dimension.NotSet,
          PtsUT = null,
          NumST = 0,
          AbsST = Dimension.NotSet,
          OrdST = Dimension.NotSet,
          PtsST = null,
          Eps = null,
          Uls = new GsaMatCurveParam()
          {
            Model = new List<MatCurveParamType>() { MatCurveParamType.RECTANGLE, MatCurveParamType.NO_TENSION },
            StrainElasticCompression = eps,
            StrainElasticTension = null,
            StrainPlasticCompression = eps,
            StrainPlasticTension = null,
            StrainFailureCompression = 0.003,
            StrainFailureTension = 1,
            GammaF = 1,
            GammaE = 1,
          },
          Sls = new GsaMatCurveParam()
          {
            Model = new List<MatCurveParamType>() { MatCurveParamType.LINEAR, MatCurveParamType.INTERPOLATED },
            StrainElasticCompression = 0.003,
            StrainElasticTension = null,
            StrainPlasticCompression = 0.003,
            StrainPlasticTension = null,
            StrainFailureCompression = 0.003,
            StrainFailureTension = speckleConcrete.maxTensileStrain.IsPositiveOrNull(),
            GammaF = 1,
            GammaE = 1,
          },
          Cost = speckleConcrete.cost.IsPositiveOrNull(),
          Type = MatType.CONCRETE,
        },
        Type = MatConcreteType.CYLINDER, //strength type
        Cement = MatConcreteCement.N, //cement class
        Fc = speckleConcrete.compressiveStrength.IsPositiveOrNull(), //concrete strength
        Fcd = (0.85 * speckleConcrete.compressiveStrength).IsPositiveOrNull(), //design strength
        Fcdc = (0.4 * speckleConcrete.compressiveStrength).IsPositiveOrNull(), //cracked strength
        Fcdt = speckleConcrete.tensileStrength.IsPositiveOrNull(), //tensile strength
        Fcfib = (0.6 * speckleConcrete.tensileStrength).IsPositiveOrNull(), //peak strength for FIB/Popovics curves
        EmEs = null, //ratio of initial elastic modulus to secant modulus
        N = 2, //parabolic coefficient (normally 2)
        Emod = 1, //modifier on elastic stiffness typically in range (0.8:1.2)
        EpsPeak = 0.003, //concrete strain at peak SLS stress
        EpsMax = eps, //maximum conrete SLS strain
        EpsU = speckleConcrete.maxCompressiveStrain.IsPositiveOrNull(), //concrete ULS failure strain
        EpsAx = 0.0025, //concrete max compressive ULS strain
        EpsTran = 0.002, //slab transition strain
        EpsAxs = 0.0025, //slab axial strain limit
        Light = speckleConcrete.lightweight, //lightweight flag
        Agg = speckleConcrete.maxAggregateSize.IsPositiveOrNull(), //maximum aggregate size
        XdMin = 0, //minimum x/d in flexure
        XdMax = 1, //maximum x/d in flexure
        Beta = GetBeta(speckleConcrete.compressiveStrength), //depth of rectangular stress block
        Shrink = null, //shrinkage strain
        Confine = null, //confining stress
        Fcc = null, //concrete strength [confined]
        EpsPlasC = null, //plastic strain (ULS) [confined]
        EpsUC = null, //concrete failure strain [confined]
      };
      //TODO:
      //SpeckleObject:
      //  string grade
      //  string designCode
      //  string codeYear
      //  double strength
      //  double materialSafetyFactor
      //  double flexuralStrength
      return new List<GsaRecord>() { gsaConcrete };
    }
#endregion

#region Properties
    private List<GsaRecord> GsaProperty1dToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleProperty = (GSAProperty1D)speckleObject;
      var natives = Property1dToNative(speckleObject);
      retList.AddRange(natives);
      var gsaSection = (GsaSection)natives.FirstOrDefault(n => n is GsaSection);
      if (gsaSection != null)
      {
        gsaSection.Colour = (Enum.TryParse(speckleProperty.colour, true, out Colour gsaColour) ? gsaColour : Colour.NO_RGB);
        gsaSection.Mass = (speckleProperty.additionalMass == 0) ? null : (double?)speckleProperty.additionalMass;
        gsaSection.Cost = (speckleProperty.cost == 0) ? null : (double?)speckleProperty.cost;
        if (speckleProperty.designMaterial != null && gsaSection.Components != null && gsaSection.Components.Count > 0)
        {
          var sectionComp = (SectionComp)gsaSection.Components.First();
          if (speckleProperty.designMaterial.materialType == MaterialType.Steel && speckleProperty.designMaterial != null)
          {
            sectionComp.MaterialType = Section1dMaterialType.STEEL;
            sectionComp.MaterialIndex = IndexByConversionOrLookup<GsaMatSteel>(speckleProperty.designMaterial, ref retList);

            var steelMaterial = (Steel)speckleProperty.designMaterial;
            var gsaSectionSteel = new SectionSteel()
            {
              //GradeIndex = 0,
              //Defaults
              PlasElas = 1,
              NetGross = 1,
              Exposed = 1,
              Beta = 0.4,
              Type = SectionSteelSectionType.Undefined,
              Plate = SectionSteelPlateType.Undefined,
              Locked = false
            };
            gsaSection.Components.Add(gsaSectionSteel);
          }
          else if (speckleProperty.designMaterial.materialType == MaterialType.Concrete && speckleProperty.designMaterial != null)
          {
            sectionComp.MaterialType = Section1dMaterialType.CONCRETE;
            //sectionComp.MaterialIndex = Instance.GsaModel.Cache.LookupIndex<GsaMatConcrete>(speckleProperty.designMaterial.applicationId);
            sectionComp.MaterialIndex = Instance.GsaModel.Cache.ResolveIndex<GsaMatConcrete>(speckleProperty.designMaterial.applicationId);

            var gsaSectionConc = new SectionConc();
            var gsaSectionLink = new SectionLink();
            var gsaSectionCover = new SectionCover();
            var gsaSectionTmpl = new SectionTmpl();
            gsaSection.Components.Add(gsaSectionConc);
            gsaSection.Components.Add(gsaSectionLink);
            gsaSection.Components.Add(gsaSectionCover);
            gsaSection.Components.Add(gsaSectionTmpl);
          }
          else
          {
            //Not supported yet
          }
        }
      }
      return retList;
    }
    
    //Note: there should be no ToNative for SectionProfile because it's not a type that will create a first-class citizen in the GSA model
    //      so there is basically a ToNative of that class here in this method too
    private List<GsaRecord> Property1dToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleProperty = (Property1D)speckleObject;
      
      var gsaSection = new GsaSection()
      {
        Index = speckleProperty.GetIndex<GsaSection>(),
        Name = speckleProperty.name,
        ApplicationId = speckleProperty.applicationId,
        Type = speckleProperty.memberType.ToNative(),
        //PoolIndex = 0,
        ReferencePoint = speckleProperty.referencePoint.ToNative(),
        RefY = (speckleProperty.offsetY == 0) ? null : (double?)speckleProperty.offsetY,
        RefZ = (speckleProperty.offsetZ == 0) ? null : (double?)speckleProperty.offsetZ,
        Fraction = 1,
        //Left = 0,
        //Right = 0,
        //Slab = 0,
        Components = new List<GsaSectionComponentBase>()
      };
      

      var sectionComp = new SectionComp()
      {
        Name = (speckleProperty.profile == null || string.IsNullOrEmpty(speckleProperty.profile.name)) ? null : speckleProperty.profile.name
      };
      if(speckleProperty.material != null)
      {
        if (speckleProperty.material.materialType == MaterialType.Steel && speckleProperty.material != null)
        {
          sectionComp.MaterialType = Section1dMaterialType.STEEL;
          sectionComp.MaterialIndex = IndexByConversionOrLookup<GsaMatSteel>(speckleProperty.material, ref retList);
          var steelMaterial = (Steel)speckleProperty.material;
          var gsaSectionSteel = new SectionSteel()
          {
            //GradeIndex = 0,
            //Defaults
            PlasElas = 1,
            NetGross = 1,
            Exposed = 1,
            Beta = 0.4,
            Type = SectionSteelSectionType.Undefined,
            Plate = SectionSteelPlateType.Undefined,
            Locked = false
          };
          ;
          gsaSection.Components.Add(sectionComp);
          gsaSection.Components.Add(gsaSectionSteel);
        }
        else if (speckleProperty.material.materialType == MaterialType.Concrete && speckleProperty.material != null)
        {
          sectionComp.MaterialType = Section1dMaterialType.CONCRETE;
          sectionComp.MaterialIndex = IndexByConversionOrLookup<GsaMatConcrete>(speckleProperty.material, ref retList);
          gsaSection.Components.Add(sectionComp);

          var gsaSectionConc = new SectionConc();
          var gsaSectionLink = new SectionLink();
          var gsaSectionCover = new SectionCover();
          var gsaSectionTmpl = new SectionTmpl();
          gsaSection.Components.Add(gsaSectionConc);
          gsaSection.Components.Add(gsaSectionLink);
          gsaSection.Components.Add(gsaSectionCover);
          gsaSection.Components.Add(gsaSectionTmpl);
        }
        else
        {
          //Not supported yet
        }
      } else
      {
        gsaSection.Components.Add(sectionComp);
      }

      if (speckleProperty.profile != null)
      {
        Property1dProfileToNative(speckleProperty.profile, out sectionComp.ProfileDetails, out sectionComp.ProfileGroup);
      }

      retList.Add(gsaSection);
      return retList;
    }
    
    private bool CurveToGsaOutline(ICurve outline, ref List<double?> Y, ref List<double?> Z, ref List<string> actions)
    {
      if (!(outline is Curve))
      {
        return false;
      }
      var pointCoords = ((Curve)outline).points.GroupBy(3).Select(g => g.ToList()).ToList();
      foreach (var coords in pointCoords)
      {
        Y.Add(coords[1]);
        Z.Add(coords[2]);
      }
      actions.Add("M");
      actions.AddRange(Enumerable.Repeat("L", (pointCoords.Count() - 1)));
      return true;
    }

    private bool Property1dProfileToNative(SectionProfile sectionProfile, out ProfileDetails gsaProfileDetails, out Section1dProfileGroup gsaProfileGroup)
    {
      if (sectionProfile.shapeType == ShapeType.Catalogue)
      {
        var p = (Catalogue)sectionProfile;
        gsaProfileDetails = new ProfileDetailsCatalogue()
        {
          Profile = p.description
        };
        gsaProfileGroup = Section1dProfileGroup.Catalogue;
      }
      else if (sectionProfile.shapeType == ShapeType.Explicit)
      {
        var p = (Explicit)sectionProfile;
        gsaProfileDetails = new ProfileDetailsExplicit() { Area = p.area, Iyy = p.Iyy, Izz = p.Izz, J = p.J, Ky = p.Ky, Kz = p.Kz };
        gsaProfileGroup = Section1dProfileGroup.Explicit;
      }
      else if (sectionProfile.shapeType == ShapeType.Perimeter)
      {
        var p = (Perimeter)sectionProfile;
        var hollow = (p.voids != null && p.voids.Count > 0);
        gsaProfileDetails = new ProfileDetailsPerimeter()
        {
          Type = "P"
        };
        if (p.outline is Curve && (p.voids == null || (p.voids.All(v => v is Curve))))
        {
          ((ProfileDetailsPerimeter)gsaProfileDetails).Actions = new List<string>();
          ((ProfileDetailsPerimeter)gsaProfileDetails).Y = new List<double?>();
          ((ProfileDetailsPerimeter)gsaProfileDetails).Z = new List<double?>();

          CurveToGsaOutline(p.outline, ref ((ProfileDetailsPerimeter)gsaProfileDetails).Y, 
            ref ((ProfileDetailsPerimeter)gsaProfileDetails).Z, ref ((ProfileDetailsPerimeter)gsaProfileDetails).Actions);

          if (hollow)
          {
            foreach (var v in p.voids)
            {
              CurveToGsaOutline(v, ref ((ProfileDetailsPerimeter)gsaProfileDetails).Y, 
                ref ((ProfileDetailsPerimeter)gsaProfileDetails).Z, ref ((ProfileDetailsPerimeter)gsaProfileDetails).Actions);
            }
          }
        }
        gsaProfileGroup = Section1dProfileGroup.Perimeter;
      }
      else
      {
        gsaProfileGroup = Section1dProfileGroup.Standard;
        if (sectionProfile.shapeType == ShapeType.Rectangular)
        {
          var p = (Rectangular)sectionProfile;
          var hollow = (p.flangeThickness > 0 || p.webThickness > 0);
          if (hollow)
          {
            gsaProfileDetails = new ProfileDetailsTwoThickness() { ProfileType = Section1dStandardProfileType.RectangularHollow };
            ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.depth, p.width, p.webThickness, p.flangeThickness);
          }
          else
          {
            gsaProfileDetails = new ProfileDetailsRectangular() { ProfileType = Section1dStandardProfileType.Rectangular };
            ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.depth, p.width);
          }
        }
        else if (sectionProfile.shapeType == ShapeType.Circular)
        {
          var p = (Circular)sectionProfile;
          var hollow = (p.wallThickness > 0);
          if (hollow)
          {
            gsaProfileDetails = new ProfileDetailsCircularHollow() { ProfileType = Section1dStandardProfileType.CircularHollow };
            ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.radius * 2, p.wallThickness);
          }
          else
          {
            gsaProfileDetails = new ProfileDetailsCircular() { ProfileType = Section1dStandardProfileType.Circular };
            ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.radius * 2);
          }
        }
        else if (sectionProfile.shapeType == ShapeType.Angle)
        {
          var p = (Angle)sectionProfile;
          gsaProfileDetails = new ProfileDetailsTwoThickness() { ProfileType = Section1dStandardProfileType.Angle };
          ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.depth, p.width, p.webThickness, p.flangeThickness);
        }
        else if (sectionProfile.shapeType == ShapeType.Channel)
        {
          var p = (Channel)sectionProfile;
          gsaProfileDetails = new ProfileDetailsTwoThickness() { ProfileType = Section1dStandardProfileType.Channel };
          ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.depth, p.width, p.webThickness, p.flangeThickness);
        }
        else if (sectionProfile.shapeType == ShapeType.I)
        {
          var p = (ISection)sectionProfile;
          gsaProfileDetails = new ProfileDetailsTwoThickness() { ProfileType = Section1dStandardProfileType.ISection };
          ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.depth, p.width, p.webThickness, p.flangeThickness);
        }
        else if (sectionProfile.shapeType == ShapeType.Tee)
        {
          var p = (Tee)sectionProfile;
          gsaProfileDetails = new ProfileDetailsTwoThickness() { ProfileType = Section1dStandardProfileType.Tee };
          ((ProfileDetailsStandard)gsaProfileDetails).SetValues(p.depth, p.width, p.webThickness, p.flangeThickness);
        }
        else
        {
          gsaProfileDetails = null;
        }
      }
      return true;
    }

    private List<GsaRecord> GsaProperty2dToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleProperty = (GSAProperty2D)speckleObject;
      var natives = Property2dToNative(speckleObject);
      retList.AddRange(natives);
      var gsaProp2d = (GsaProp2d)natives.FirstOrDefault(n => n is GsaProp2d);
      if (gsaProp2d != null)
      {
        gsaProp2d.Colour = (Enum.TryParse(speckleProperty.colour, true, out Colour gsaColour) ? gsaColour : Colour.NO_RGB);
        gsaProp2d.Mass = speckleProperty.additionalMass;
        gsaProp2d.Profile = speckleProperty.concreteSlabProp;
        if (speckleProperty.designMaterial != null)
        {
          int? materialIndex = null;
          if (speckleProperty.designMaterial.materialType == MaterialType.Steel && speckleProperty.designMaterial is GSASteel)
          {
            //var mat = (GSASteel)speckleProperty.designMaterial;
            materialIndex = IndexByConversionOrLookup<GsaMatSteel>(speckleProperty.designMaterial, ref retList);
            //materialIndex = Instance.GsaModel.Cache.LookupIndex<GsaMatSteel>(speckleProperty.designMaterial.applicationId);
            gsaProp2d.MatType = Property2dMaterialType.Steel;
          }
          else if (speckleProperty.designMaterial.materialType == MaterialType.Concrete && speckleProperty.designMaterial is GSAConcrete)
          {
            //materialIndex = Instance.GsaModel.Cache.LookupIndex<GsaMatConcrete>(speckleProperty.designMaterial.applicationId);
            materialIndex = IndexByConversionOrLookup<GsaMatConcrete>(speckleProperty.designMaterial, ref retList);
            gsaProp2d.MatType = Property2dMaterialType.Concrete;
          }
          else
          {
            //Not supported yet
            gsaProp2d.MatType = Property2dMaterialType.Generic;
          }

          if (materialIndex.HasValue)
          {
            gsaProp2d.GradeIndex = materialIndex;
          }
          else
          {
            //TO DO: ToNative() of the material
          }
        }
      }
      return retList;
    }

    //Note: there should be no ToNative for SectionProfile because it's not a type that will create a first-class citizen in the GSA model
    //      so there is basically a ToNative of that class here in this method too
    private List<GsaRecord> Property2dToNative(Base speckleObject)
    {
      var retList = new List<GsaRecord>();
      var speckleProperty = (Property2D)speckleObject;

      var gsaProp2d = new GsaProp2d()
      {
        Index = speckleProperty.GetIndex<GsaProp2d>(),
        Name = speckleProperty.name,
        ApplicationId = speckleProperty.applicationId,
        Thickness = (speckleProperty.thickness == 0) ? null : (double?)speckleProperty.thickness,
        RefZ = speckleProperty.zOffset,
        RefPt = speckleProperty.refSurface.ToNative(),
        Type = speckleProperty.type.ToNative(),
        InPlaneStiffnessPercentage = speckleProperty.modifierInPlane == 0 ? null : (double?)speckleProperty.modifierInPlane,
        BendingStiffnessPercentage = speckleProperty.modifierBending == 0 ? null : (double?)speckleProperty.modifierBending,
        ShearStiffnessPercentage = speckleProperty.modifierShear == 0 ? null : (double?)speckleProperty.modifierShear,
        VolumePercentage = speckleProperty.modifierVolume == 0 ? null : (double?)speckleProperty.modifierVolume
      };

      if (speckleProperty.orientationAxis == null)
      {
        gsaProp2d.AxisRefType = AxisRefType.Local;
      }
      else
      {
        if (speckleProperty.orientationAxis.definition != null && speckleProperty.orientationAxis.definition.IsGlobal())
        {
          gsaProp2d.AxisRefType = AxisRefType.Global;
        }
        else
        {
          var axisIndex = IndexByConversionOrLookup<GsaAxis>(speckleProperty.orientationAxis, ref retList);
          if (axisIndex.IsIndex())
          {
            gsaProp2d.AxisIndex = axisIndex;
            gsaProp2d.AxisRefType = AxisRefType.Reference;
          }
          else
          {
            gsaProp2d.AxisRefType = AxisRefType.Local;
          }
        }
      }
      retList.Add(gsaProp2d);
      return retList;
    }

    private List<GsaRecord> PropertyMassToNative(Base speckleObject)
    {
      var specklePropertyMass = (PropertyMass)speckleObject;
      var gsaPropMass = new GsaPropMass()
      {
        Index = Instance.GsaModel.Cache.ResolveIndex<GsaPropMass>(specklePropertyMass.applicationId),
        Name = specklePropertyMass.name,
        ApplicationId = specklePropertyMass.applicationId,
        Mass = specklePropertyMass.mass,
        Ixx = specklePropertyMass.inertiaXX,
        Iyy = specklePropertyMass.inertiaYY,
        Izz = specklePropertyMass.inertiaZZ,
        Ixy = specklePropertyMass.inertiaXY,
        Iyz = specklePropertyMass.inertiaYZ,
        Izx = specklePropertyMass.inertiaZX
      };
      gsaPropMass.Mod = (specklePropertyMass.massModified) ? MassModification.Modified : MassModification.Defined;
      gsaPropMass.ModXPercentage = specklePropertyMass.massModifierX;
      gsaPropMass.ModYPercentage = specklePropertyMass.massModifierY;
      gsaPropMass.ModZPercentage = specklePropertyMass.massModifierZ;

      return new List<GsaRecord>() { gsaPropMass };
    }

    private List<GsaRecord> PropertySpringToNative(Base speckleObject)
    {
      var fns = new Dictionary<PropertyTypeSpring, Func<PropertySpring, GsaPropSpr, bool>>
      { { PropertyTypeSpring.Axial, SetPropertySpringAxial },
        { PropertyTypeSpring.Torsional, SetPropertySpringTorsional },
        { PropertyTypeSpring.CompressionOnly, SetPropertySpringCompression },
        { PropertyTypeSpring.TensionOnly, SetPropertySpringTension },
        { PropertyTypeSpring.LockUp, SetPropertySpringLockup },
        { PropertyTypeSpring.Gap, SetPropertySpringGap },
        { PropertyTypeSpring.Friction, SetPropertySpringFriction },
        { PropertyTypeSpring.General, SetPropertySpringGeneral }
        //CONNECT not yet supported
        //MATRIX not yet supported
      };

      var specklePropertySpring = (PropertySpring)speckleObject;
      var gsaPropSpr = new GsaPropSpr()
      {
        Index = Instance.GsaModel.Cache.ResolveIndex<GsaPropSpr>(specklePropertySpring.applicationId),
        Name = specklePropertySpring.name,
        ApplicationId = specklePropertySpring.applicationId,
        DampingRatio = specklePropertySpring.dampingRatio
      };
      if (fns.ContainsKey(specklePropertySpring.springType))
      {
        gsaPropSpr.Stiffnesses = new Dictionary<GwaAxisDirection6, double>();
        fns[specklePropertySpring.springType](specklePropertySpring, gsaPropSpr);
      }
      else
      {
        ConversionErrors.Add(new Exception("PropertySpring: spring type (" + specklePropertySpring.springType.ToString() + ") is not currently supported"));
      }

      return new List<GsaRecord>() { gsaPropSpr };
    }

    private bool SetPropertySpringAxial(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.Axial;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.X, specklePropertySpring.stiffnessX);
      return true;
    }

    private bool SetPropertySpringTorsional(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.Torsional;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.XX, specklePropertySpring.stiffnessXX);
      return true;
    }

    private bool SetPropertySpringCompression(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.Compression;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.X, specklePropertySpring.stiffnessX);
      return true;
    }

    private bool SetPropertySpringTension(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.Tension;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.X, specklePropertySpring.stiffnessX);
      return true;
    }

    private bool SetPropertySpringLockup(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      //Also for LOCKUP, there are positive and negative parameters, but these aren't supported yet
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.Lockup;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.X, specklePropertySpring.stiffnessX);
      return true;
    }

    private bool SetPropertySpringGap(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.Gap;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.X, specklePropertySpring.stiffnessX);
      return true;
    }

    private bool SetPropertySpringFriction(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.Friction;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.X, specklePropertySpring.stiffnessX);
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.Y, specklePropertySpring.stiffnessY);
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.Z, specklePropertySpring.stiffnessZ);
      gsaPropSpr.FrictionCoeff = specklePropertySpring.frictionCoefficient;
      return true;
    }

    private bool SetPropertySpringGeneral(PropertySpring specklePropertySpring, GsaPropSpr gsaPropSpr)
    {
      gsaPropSpr.PropertyType = StructuralSpringPropertyType.General;
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.X, specklePropertySpring.stiffnessX);
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.Y, specklePropertySpring.stiffnessY);
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.Z, specklePropertySpring.stiffnessZ);
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.XX, specklePropertySpring.stiffnessXX);
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.YY, specklePropertySpring.stiffnessYY);
      gsaPropSpr.Stiffnesses.Add(GwaAxisDirection6.ZZ, specklePropertySpring.stiffnessZZ);
      return true;
    }

#endregion

#region Constraints
    private List<GsaRecord> GSAGeneralisedRestraintToNative(Base speckleObject)
    {
      var speckleGenRest = (GSAGeneralisedRestraint)speckleObject;
      var gsaGenRest = new GsaGenRest()
      {
        ApplicationId = speckleGenRest.applicationId,
        Index = speckleGenRest.GetIndex<GsaGenRest>(),
        Name = speckleGenRest.name,
        NodeIndices = speckleGenRest.nodes.NodeAt(conversionFactors),
        StageIndices = speckleGenRest.stages.Select(s=>(Base)s).ToList().GetIndicies<GsaAnalStage>(),
      };
      if (speckleGenRest.restraint != null && speckleGenRest.restraint.code.Length >= 6)
      {
        gsaGenRest.X = speckleGenRest.restraint.code[0] == 'F' ? RestraintCondition.Constrained : RestraintCondition.Free;
        gsaGenRest.Y = speckleGenRest.restraint.code[1] == 'F' ? RestraintCondition.Constrained : RestraintCondition.Free;
        gsaGenRest.Z = speckleGenRest.restraint.code[2] == 'F' ? RestraintCondition.Constrained : RestraintCondition.Free;
        gsaGenRest.XX = speckleGenRest.restraint.code[3] == 'F' ? RestraintCondition.Constrained : RestraintCondition.Free;
        gsaGenRest.YY = speckleGenRest.restraint.code[4] == 'F' ? RestraintCondition.Constrained : RestraintCondition.Free;
        gsaGenRest.ZZ = speckleGenRest.restraint.code[5] == 'F' ? RestraintCondition.Constrained : RestraintCondition.Free;
      }
      return new List<GsaRecord>() { gsaGenRest };
    }

    private List<GsaRecord> GSARigidConstraintToNative(Base speckleObject)
    {
      var speckleRigid = (GSARigidConstraint)speckleObject;
      var gsaRigid = new GsaRigid()
      {
        ApplicationId = speckleRigid.applicationId,
        Index = speckleRigid.GetIndex<GsaRigid>(),
        Name = speckleRigid.name,
        Type = speckleRigid.type.ToNative(),
        Link = GetRigidConstraint(speckleRigid.constraintCondition),
        PrimaryNode = speckleRigid.primaryNode.NodeAt(conversionFactors),
        ConstrainedNodes = speckleRigid.constrainedNodes.NodeAt(conversionFactors),
        Stage = speckleRigid.stages.Select(s => (Base)s).ToList().GetIndicies<GsaAnalStage>(),
        ParentMember = speckleRigid.parentMember.GetIndex<GsaMemb>(),
      };
      return new List<GsaRecord>() { gsaRigid };
    }
#endregion

#region Bridge

    private List<GsaRecord> AlignToNative(Base speckleObject)
    {
      var speckleAlign = (GSAAlignment)speckleObject;
      var gsaAlign = new GsaAlign()
      {
        ApplicationId = speckleAlign.applicationId,
        Index = speckleAlign.GetIndex<GsaAlign>(),
        Chain = speckleAlign.chainage,
        Curv = speckleAlign.curvature,
        Name = speckleAlign.name,
        Sid = speckleAlign.id,
        GridSurfaceIndex = speckleAlign.gridSurface.GetIndex<GsaGridSurface>(),
        NumAlignmentPoints = speckleAlign.GetNumAlignmentPoints(),
      };
      return new List<GsaRecord>() { gsaAlign };
    }

    private List<GsaRecord> InfBeamToNative(Base speckleObject)
    {
      var speckleInfBeam = (GSAInfluenceBeam)speckleObject;
      //var elementIndex = ((GsaEl)Element1dToNative(speckleInfBeam.element).First()).Index;
      var gsaInfBeam = new GsaInfBeam
      {
        ApplicationId = speckleInfBeam.applicationId,
        Index = speckleInfBeam.GetIndex<GsaInfBeam>(),
        Name = speckleInfBeam.name,
        Direction = speckleInfBeam.direction.ToNative(),
        Element = speckleInfBeam.element.GetIndex<GsaEl>(),
        Factor = speckleInfBeam.factor,
        Position = (speckleInfBeam.position > 0 && speckleInfBeam.position < 1) ? (double?)speckleInfBeam.position : null,
        Sid = speckleObject.id,
        Type = speckleInfBeam.type.ToNative(),
      };
      return new List<GsaRecord>() { gsaInfBeam };
    }
    
    private List<GsaRecord> InfNodeToNative(Base speckleObject)
    {
      var speckleInfNode = (GSAInfluenceNode)speckleObject;
      
      //var nodeIndex = ((GsaNode)(NodeToNative(speckleInfNode.node).First())).Index;
      var gsaInfNode = new GsaInfNode()
      {
        ApplicationId = speckleObject.applicationId,
        Index = speckleInfNode.GetIndex<GsaInfNode>(),
        Name = speckleInfNode.name,
        Direction = speckleInfNode.direction.ToNative(),
        Factor = speckleInfNode.factor,
        Sid = speckleObject.id,
        Type = speckleInfNode.type.ToNative(),
        Node = speckleInfNode.node.GetIndex<GsaNode>(),
      };
      if (GetAxis(speckleInfNode.axis, out AxisRefType gsaRefType, out var axisIndex))
      {
        gsaInfNode.AxisRefType = gsaRefType;
        gsaInfNode.AxisIndex = axisIndex;
      }
      return new List<GsaRecord>() { gsaInfNode };
    }
    
    private List<GsaRecord> PathToNative(Base speckleObject)
    {
      var specklePath = (GSAPath)speckleObject;
      //var lookupIndex = Instance.GsaModel.Cache.LookupIndex<GsaAlign>(specklePath.alignment.applicationId);
      //GsaAlign gsaAlign = null;
      
      //if (lookupIndex != null)
      //{
      //  gsaAlign = (GsaAlign)(AlignToNative(specklePath.alignment)).First();
      //  lookupIndex = gsaAlign.Index;
      //}

      var gsaPath = new GsaPath()
      {
        ApplicationId = specklePath.applicationId,
        Index = specklePath.GetIndex<GsaPath>(),
        Name = specklePath.name,
        Sid = speckleObject.id,
        Factor = specklePath.factor,
        Alignment = specklePath.alignment.GetIndex<GsaAlign>(),
        Group = specklePath.group.IsPositiveOrNull(),
        Left = specklePath.left,
        Right = specklePath.right,
        NumMarkedLanes = specklePath.numMarkedLanes.IsPositiveOrNull(),
        Type = specklePath.type.ToNative(),
      };
      //if(gsaAlign != null)
      //  return new List<GsaRecord>() { gsaAlign, gsaPath };
      //else
      return new List<GsaRecord>() { gsaPath };
    }

    private List<GsaRecord> GSAUserVehicleToNative(Base speckleObject)
    {
      var speckleVehicle = (GSAUserVehicle)speckleObject;
      var gsaVehicle = new GsaUserVehicle()
      {
        ApplicationId = speckleVehicle.applicationId,
        Index = speckleVehicle.GetIndex<GsaUserVehicle>(),
        Name = speckleVehicle.name,
        Width = speckleVehicle.width.IsPositiveOrNull(),
        NumAxle = speckleVehicle.axlePositions.Count(),
        AxlePosition = speckleVehicle.axlePositions,
        AxleOffset = speckleVehicle.axleOffsets,
        AxleLeft = speckleVehicle.axleLeft,
        AxleRight = speckleVehicle.axleRight,
      };
      return new List<GsaRecord>() { gsaVehicle };
    }
    
#endregion

#region Analysis Stage
    
    public List<GsaRecord> AnalStageToNative(Base speckleObject)
    {
      var analStage = (GSAStage)speckleObject;
      var gsaAnalStage = new GsaAnalStage()
      {
        ApplicationId = analStage.applicationId,
        Index = analStage.GetIndex<GsaAnalStage>(),
        Name = analStage.name,
        Days = analStage.stageTime.IsPositiveOrNull(),
        Phi = analStage.creepFactor.IsPositiveOrNull(),
      };
      if (analStage.colour != null)
      {
        gsaAnalStage.Colour = analStage.colour.ColourToNative();
      }
      if (analStage.elements != null)
      {
        gsaAnalStage.ElementIndices = new List<int>();
        gsaAnalStage.ElementIndices.AddRange(analStage.elements.FindAll(e => e is Element1D).GetIndicies<GsaEl>() ?? new List<int>());
        gsaAnalStage.ElementIndices.AddRange(analStage.elements.FindAll(e => e is Element2D).GetIndicies<GsaEl>() ?? new List<int>());
        gsaAnalStage.MemberIndices = new List<int>();
        gsaAnalStage.MemberIndices.AddRange(analStage.elements.FindAll(e => e is GSAMember1D).GetIndicies<GsaMemb>() ?? new List<int>());
        gsaAnalStage.MemberIndices.AddRange(analStage.elements.FindAll(e => e is GSAMember2D).GetIndicies<GsaMemb>() ?? new List<int>());
      }
      if (analStage.lockedElements != null)
      {
        gsaAnalStage.LockElementIndices = new List<int>();
        gsaAnalStage.LockElementIndices.AddRange(analStage.lockedElements.FindAll(e => e is Element1D).GetIndicies<GsaEl>() ?? new List<int>());
        gsaAnalStage.LockElementIndices.AddRange(analStage.lockedElements.FindAll(e => e is Element2D).GetIndicies<GsaEl>() ?? new List<int>());
        gsaAnalStage.LockMemberIndices = new List<int>();
        gsaAnalStage.LockMemberIndices.AddRange(analStage.lockedElements.FindAll(e => e is GSAMember1D).GetIndicies<GsaMemb>() ?? new List<int>());
        gsaAnalStage.LockMemberIndices.AddRange(analStage.lockedElements.FindAll(e => e is GSAMember2D).GetIndicies<GsaMemb>() ?? new List<int>());
      }
      return new List<GsaRecord>() { gsaAnalStage };
    }

#endregion

#endregion

#region Helper
#region ToNative
#region Geometry
    /*
    private int GetElementIndex(object obj)
    {
      if (obj is GSAElement1D element1D)
        return element1D.nativeId;
      else if (obj is GSAElement2D element2D)
        return element2D.nativeId;
      else
        return -1;
    }
    */

#region Axis
    private bool GetAxis(Axis speckleAxis, out NodeAxisRefType gsaAxisRefType, out int? gsaAxisIndex)
    {
      gsaAxisRefType = NodeAxisRefType.NotSet;
      gsaAxisIndex = null;
      if (speckleAxis == null)
      {
        return false;
      }
      else if (speckleAxis.definition != null)
      {
        if (speckleAxis.definition.IsGlobal())
        {
          gsaAxisRefType = NodeAxisRefType.Global;
        }
        else if (speckleAxis.definition.IsXElevation())
        {
          gsaAxisRefType = NodeAxisRefType.XElevation;
        }
        else if (speckleAxis.definition.IsYElevation())
        {
          gsaAxisRefType = NodeAxisRefType.YElevation;
        }
        else if (speckleAxis.definition.IsVertical())
        {
          gsaAxisRefType = NodeAxisRefType.Vertical;
        }
      }
      else if (speckleAxis.applicationId != null)
      {
        gsaAxisRefType = NodeAxisRefType.Reference;
        gsaAxisIndex = speckleAxis.GetIndex<GsaAxis>();
      }
      else
      {
        return false;
      }
      
      return true;
    }

    private bool GetAxis(Axis speckleAxis, out AxisRefType gsaAxisRefType, out int? gsaAxisIndex)
    {
      gsaAxisRefType = AxisRefType.NotSet;
      gsaAxisIndex = null;
      if (speckleAxis == null)
      {
        gsaAxisRefType = AxisRefType.Local;
      }
      else if (speckleAxis.definition != null && speckleAxis.definition.IsGlobal())
      {
        gsaAxisRefType = AxisRefType.Global;
      }
      else if (speckleAxis.applicationId != null)
      {
        gsaAxisRefType = AxisRefType.Reference;
        gsaAxisIndex = speckleAxis.GetIndex<GsaAxis>();
      }
      else
      {
        return false;
      }

      return true;
    }
#endregion

#region Node
    private bool GetRestraint(Restraint speckleRestraint, out NodeRestraint gsaNodeRestraint, out List<GwaAxisDirection6> gsaRestraint)
    {
      gsaRestraint = null; //default

      switch(speckleRestraint.code)
      {
        case "RRRRRR":
          gsaNodeRestraint = NodeRestraint.Free;
          break;
        case "FFFRRR":
          gsaNodeRestraint = NodeRestraint.Pin;
          break;
        case "FFFFFF":
          gsaNodeRestraint = NodeRestraint.Fix;
          break;
        default:
          gsaNodeRestraint = NodeRestraint.Custom;
          gsaRestraint = new List<GwaAxisDirection6>();
          int i = 0;
          foreach(char c in speckleRestraint.code)
          {
            if (c == 'F')
            {
              if (i == 0) gsaRestraint.Add(GwaAxisDirection6.X);
              else if (i == 1) gsaRestraint.Add(GwaAxisDirection6.Y);
              else if (i == 2) gsaRestraint.Add(GwaAxisDirection6.Z);
              else if (i == 3) gsaRestraint.Add(GwaAxisDirection6.XX);
              else if (i == 4) gsaRestraint.Add(GwaAxisDirection6.YY);
              else if (i == 5) gsaRestraint.Add(GwaAxisDirection6.ZZ);
            }
            i++;
          }
          break;
      }
      return true;
    }
#endregion

#region Element
    private bool GetReleases(Restraint speckleRelease, out Dictionary<GwaAxisDirection6,ReleaseCode> gsaRelease, out List<double> gsaStiffnesses, out ReleaseInclusion gsaReleaseInclusion)
    {
      if (speckleRelease.code == "FFFFFF")
      {
        gsaReleaseInclusion = ReleaseInclusion.NotIncluded;
        gsaRelease = null;
        gsaStiffnesses = null;
      }
      else if (speckleRelease.code.ToUpperInvariant().IndexOf('K') > 0)
      {
        gsaReleaseInclusion = ReleaseInclusion.Stiff;
        gsaRelease = speckleRelease.code.ReleasesToNative();
        gsaStiffnesses = new List<double>();
        if (speckleRelease.stiffnessX > 0) gsaStiffnesses.Add(conversionFactors.force / conversionFactors.length * speckleRelease.stiffnessX);
        if (speckleRelease.stiffnessY > 0) gsaStiffnesses.Add(conversionFactors.force / conversionFactors.length * speckleRelease.stiffnessY);
        if (speckleRelease.stiffnessZ > 0) gsaStiffnesses.Add(conversionFactors.force / conversionFactors.length * speckleRelease.stiffnessZ);
        if (speckleRelease.stiffnessXX > 0) gsaStiffnesses.Add(conversionFactors.force * conversionFactors.length * speckleRelease.stiffnessXX);
        if (speckleRelease.stiffnessYY > 0) gsaStiffnesses.Add(conversionFactors.force * conversionFactors.length * speckleRelease.stiffnessYY);
        if (speckleRelease.stiffnessZZ > 0) gsaStiffnesses.Add(conversionFactors.force * conversionFactors.length * speckleRelease.stiffnessZZ);
      }
      else
      {
        gsaReleaseInclusion = ReleaseInclusion.Included;
        gsaRelease = speckleRelease.code.ReleasesToNative();
        gsaStiffnesses = null;
      }
      return true;
    }

    private bool GetReleases(Restraint speckleRelease, out Dictionary<GwaAxisDirection6, ReleaseCode> gsaRelease, out List<double> gsaStiffnesses)
    {
      if (speckleRelease.code.ToUpperInvariant().IndexOf('K') > 0)
      {
        gsaRelease = speckleRelease.code.ReleasesToNative();
        gsaStiffnesses = new List<double>();
        if (speckleRelease.stiffnessX > 0) gsaStiffnesses.Add(conversionFactors.force / conversionFactors.length * speckleRelease.stiffnessX);
        if (speckleRelease.stiffnessY > 0) gsaStiffnesses.Add(conversionFactors.force / conversionFactors.length * speckleRelease.stiffnessY);
        if (speckleRelease.stiffnessZ > 0) gsaStiffnesses.Add(conversionFactors.force / conversionFactors.length * speckleRelease.stiffnessZ);
        if (speckleRelease.stiffnessXX > 0) gsaStiffnesses.Add(conversionFactors.force * conversionFactors.length * speckleRelease.stiffnessXX);
        if (speckleRelease.stiffnessYY > 0) gsaStiffnesses.Add(conversionFactors.force * conversionFactors.length * speckleRelease.stiffnessYY);
        if (speckleRelease.stiffnessZZ > 0) gsaStiffnesses.Add(conversionFactors.force * conversionFactors.length * speckleRelease.stiffnessZZ);
      }
      else
      {
        gsaRelease = speckleRelease.code.ReleasesToNative();
        gsaStiffnesses = null;
      }
      return true;
    }
#endregion
#endregion

#region Loading
    private bool GetLoadAxis(Axis speckleAxis, out LoadBeamAxisRefType gsaAxisRefType, out int? gsaAxisIndex)
    {
      gsaAxisIndex = null;
      if (speckleAxis == null)
      {
        gsaAxisRefType = LoadBeamAxisRefType.NotSet;
        return false;
      }
      else if (speckleAxis.definition.IsGlobal())
      {
        gsaAxisRefType = LoadBeamAxisRefType.Global;
      }
      else
      {
        gsaAxisIndex = speckleAxis.GetIndex<GsaAxis>();
        if (gsaAxisIndex == null)
        {
          //TODO: handle local, and natural cases
          gsaAxisRefType = LoadBeamAxisRefType.NotSet;
          return false;
        }
        else
        {
          gsaAxisRefType = LoadBeamAxisRefType.Reference;
        }
      }

      return true;
    }

    private bool GetLoadAxis(Axis speckleAxis, out AxisRefType gsaAxisRefType, out int? gsaAxisIndex)
    {
      gsaAxisIndex = null;
      if (speckleAxis == null)
      {
        gsaAxisRefType = AxisRefType.NotSet;
        return false;
      }
      else if (speckleAxis.definition.IsGlobal())
      {
        gsaAxisRefType = AxisRefType.Global;
      }
      else
      {
        gsaAxisIndex = speckleAxis.GetIndex<GsaAxis>();
        if (gsaAxisIndex == null)
        {
          //TODO: handle local, and natural cases
          gsaAxisRefType = AxisRefType.NotSet;
          return false;
        }
        else
        {
          gsaAxisRefType = AxisRefType.Reference;
        }
      }

      return true;
    }

    private bool GetLoadAxis(Axis speckleAxis, LoadAxisType speckleAxisType, out AxisRefType gsaAxisRefType, out int? gsaAxisIndex)
    {
      gsaAxisIndex = null;
      if (speckleAxis == null)
      {
        gsaAxisRefType = speckleAxisType.ToNative();
      }
      else if (speckleAxis.definition.IsGlobal())
      {
        gsaAxisRefType = AxisRefType.Global;
      }
      else
      {
        gsaAxisIndex = speckleAxis.GetIndex<GsaAxis>();
        if (gsaAxisIndex == null)
        {
          //TODO: handle local, and natural cases
          gsaAxisRefType = AxisRefType.NotSet;
          return false;
        }
        else
        {
          gsaAxisRefType = AxisRefType.Reference;
        }
      }

      return true;
    }

    private string GetAnalysisCaseDescription(List<LoadCase> speckleLoadCases, List<double> speckleLoadFactors)
    {
      var gsaDescription = "";
      for (var i = 0; i < speckleLoadCases.Count(); i++)
      {
        if (i > 0 && speckleLoadFactors[i] > 0) gsaDescription += "+";
        if (speckleLoadFactors[i] == 1)
        {
          //Do nothing
        }
        else if (speckleLoadFactors[i] == -1)
        {
          gsaDescription += "-";
        }
        else
        {
          gsaDescription += speckleLoadFactors[i].ToString();
        }
        gsaDescription += "L" + speckleLoadCases[i].GetIndex<GsaLoadCase>();
      }
      return gsaDescription;
    }

    private string GetLoadCombinationDescription(CombinationType type, List<Base> loadCases, List<double> loadFactors)
    {
      if (type != CombinationType.LinearAdd) return null; //TODO - handle other cases
      var desc = "";

      for (var i = 0; i < loadCases.Count(); i++)
      {
        if (i > 0 && loadFactors[i] > 0) desc += "+";
        if (loadFactors[i] == 1)
        {
          //Do nothing
        }
        else if (loadFactors[i] == -1)
        {
          desc += "-";
        }
        else
        {
          desc += loadFactors[i].ToString();
        }
        if (loadCases[i].GetType() == typeof(GSALoadCombination))
        {
          desc += "C" + loadCases[i].GetIndex<GsaCombination>();
        }
        else if (loadCases[i].GetType() == typeof(GSAAnalysisCase))
        {
          desc += "A" + loadCases[i].GetIndex<GsaAnal>();
        }
        else
        {
          return null;
        }

      }
      return desc;
    }

    private bool GetPolyline(Polyline specklePolyline, out LoadLineOption gsaOption, out string gsaPolygon, out int? gsaPolgonIndex)
    {
      //Defaults outputs
      gsaOption = LoadLineOption.NotSet;
      gsaPolygon = "";
      gsaPolgonIndex = null;

      //Try and find index, else create string
      if (specklePolyline == null) return false;
      //if (specklePolyline.applicationId != null) gsaPolgonIndex = Instance.GsaModel.Cache.LookupIndex<GsaPolyline>(specklePolyline.applicationId);
      if (specklePolyline.applicationId != null) gsaPolgonIndex = Instance.GsaModel.Cache.ResolveIndex<GsaPolyline>(specklePolyline.applicationId);
      if (gsaPolgonIndex == null) gsaPolygon = specklePolyline.ToGwaString();
      if (gsaPolgonIndex == null && gsaPolygon == "") return false;
      else if (gsaPolgonIndex != null) gsaOption = LoadLineOption.PolyRef;
      else gsaOption = LoadLineOption.Polygon;
      return true;
    }

    private bool GetPolyline(Polyline specklePolyline, out LoadAreaOption gsaOption, out string gsaPolygon, out int? gsaPolgonIndex)
    {
      //Defaults outputs
      gsaOption = LoadAreaOption.Plane;
      gsaPolygon = "";
      gsaPolgonIndex = null;

      //Try and find index, else create string
      if (specklePolyline == null) return true;
      //if (specklePolyline.applicationId != null) gsaPolgonIndex = Instance.GsaModel.Cache.LookupIndex<GsaPolyline>(specklePolyline.applicationId);
      if (specklePolyline.applicationId != null) gsaPolgonIndex = Instance.GsaModel.Cache.ResolveIndex<GsaPolyline>(specklePolyline.applicationId);
      if (gsaPolgonIndex == null) gsaPolygon = specklePolyline.ToString();
      if (gsaPolgonIndex == null && gsaPolygon == "")
      {
        gsaOption = LoadAreaOption.NotSet;
        return false;
      }
      else if (gsaPolgonIndex != null) gsaOption = LoadAreaOption.PolyRef;
      else gsaOption = LoadAreaOption.Polygon;
      return true;
    }
#endregion

#region Materials
    private GsaMatSteel GsaMatSteelExample(Steel speckleSteel)
    {
      return new GsaMatSteel()
      {
        Index = speckleSteel.GetIndex<GsaMatSteel>(),
        ApplicationId = speckleSteel.applicationId,
        Name = "",
        Mat = new GsaMat()
        {
          Name = "",
          E = 2e11,
          F = 360000000,
          Nu = 0.3,
          G = 7.692307692e+10,
          Rho = 7850,
          Alpha = 1.2e-5,
          Prop = new GsaMatAnal()
          {
            Type = MatAnalType.MAT_ELAS_ISO,
            NumParams = 6,
            E = 2e11,
            Nu = 0.3,
            Rho = 7850,
            Alpha = 1.2e-5,
            G = 7.692307692e+10,
            Damp = 0
          },
          NumUC = 0,
          AbsUC = Dimension.NotSet,
          OrdUC = Dimension.NotSet,
          PtsUC = new double[0],
          NumSC = 0,
          AbsSC = Dimension.NotSet,
          OrdSC = Dimension.NotSet,
          PtsSC = new double[0],
          NumUT = 0,
          AbsUT = Dimension.NotSet,
          OrdUT = Dimension.NotSet,
          PtsUT = new double[0],
          NumST = 0,
          AbsST = Dimension.NotSet,
          OrdST = Dimension.NotSet,
          PtsST = new double[0],
          Eps = 0.05,
          Uls = new GsaMatCurveParam()
          {
            Model = new List<MatCurveParamType>() { MatCurveParamType.UNDEF },
            StrainElasticCompression = 0.0018,
            StrainElasticTension = 0.0018,
            StrainPlasticCompression = 0.0018,
            StrainPlasticTension = 0.0018,
            StrainFailureCompression = 0.05,
            StrainFailureTension = 0.05,
            GammaF = 1,
            GammaE = 1
          },
          Sls = new GsaMatCurveParam()
          {
            Model = new List<MatCurveParamType>() { MatCurveParamType.ELAS_PLAS },
            StrainElasticCompression = 0.0018,
            StrainElasticTension = 0.0018,
            StrainPlasticCompression = 0.0018,
            StrainPlasticTension = 0.0018,
            StrainFailureCompression = 0.05,
            StrainFailureTension = 0.05,
            GammaF = 1,
            GammaE = 1
          },
          Cost = 0,
          Type = MatType.STEEL
        },
        Fy = 360000000,
        Fu = 450000000,
        EpsP = 0,
        Eh = 0,
      };
    }

    private GsaMat GetMat(Base speckleObject)
    {
      if (speckleObject == null) return null;

      var dynamicMembers = speckleObject.GetMembers();
      var gsaMat = new GsaMat();
      gsaMat.Name = speckleObject.GetDynamicValue<string>("Name", dynamicMembers);
      gsaMat.E = speckleObject.GetDynamicValue<double>("E", dynamicMembers).IsPositiveOrNull();
      gsaMat.F = speckleObject.GetDynamicValue<double>("F", dynamicMembers).IsPositiveOrNull();
      gsaMat.Nu = speckleObject.GetDynamicValue<double>("Nu", dynamicMembers).IsPositiveOrNull();
      gsaMat.G = speckleObject.GetDynamicValue<double>("G", dynamicMembers).IsPositiveOrNull();
      gsaMat.Rho = speckleObject.GetDynamicValue<double>("Rho", dynamicMembers).IsPositiveOrNull();
      gsaMat.Alpha = speckleObject.GetDynamicValue<double>("Alpha", dynamicMembers).IsPositiveOrNull();
      gsaMat.Prop = GetMatAnal(speckleObject.GetDynamicValue<Base>("Prop", dynamicMembers));
      gsaMat.Uls = GetMatCurveParam(speckleObject.GetDynamicValue<Base>("Uls", dynamicMembers));
      gsaMat.Sls = GetMatCurveParam(speckleObject.GetDynamicValue<Base>("Sls", dynamicMembers));
      gsaMat.Eps = speckleObject.GetDynamicValue<double>("Eps", dynamicMembers).IsPositiveOrNull();
      gsaMat.Cost = speckleObject.GetDynamicValue<double>("Cost", dynamicMembers).IsPositiveOrNull();
      gsaMat.Type = speckleObject.GetDynamicEnum<MatType>("Type", dynamicMembers);
      gsaMat.PtsUC = speckleObject.GetDynamicValue<double[]>("PtsUC", dynamicMembers);
      gsaMat.PtsUC = speckleObject.GetDynamicValue<double[]>("PtsSC", dynamicMembers);
      gsaMat.PtsUC = speckleObject.GetDynamicValue<double[]>("PtsUT", dynamicMembers);
      gsaMat.PtsUC = speckleObject.GetDynamicValue<double[]>("PtsST", dynamicMembers);
      if (gsaMat.PtsUC != null)
      {
        gsaMat.NumUC = gsaMat.PtsUC.Length;
        gsaMat.AbsUC = speckleObject.GetDynamicEnum<Dimension>("AbsUC", dynamicMembers);
        gsaMat.OrdUC = speckleObject.GetDynamicEnum<Dimension>("OrdUC", dynamicMembers);
      }
      if (gsaMat.PtsSC != null)
      {
        gsaMat.NumSC = gsaMat.PtsSC.Length;
        gsaMat.AbsSC = speckleObject.GetDynamicEnum<Dimension>("AbsSC", dynamicMembers);
        gsaMat.OrdSC = speckleObject.GetDynamicEnum<Dimension>("OrdSC", dynamicMembers);
      }
      if (gsaMat.PtsUT != null)
      {
        gsaMat.NumUT = gsaMat.PtsUT.Length;
        gsaMat.AbsUT = speckleObject.GetDynamicEnum<Dimension>("AbsUT", dynamicMembers);
        gsaMat.OrdUT = speckleObject.GetDynamicEnum<Dimension>("OrdUT", dynamicMembers);
      }
      if (gsaMat.PtsST != null)
      {
        gsaMat.NumST = gsaMat.PtsST.Length;
        gsaMat.AbsST = speckleObject.GetDynamicEnum<Dimension>("AbsST", dynamicMembers);
        gsaMat.OrdST = speckleObject.GetDynamicEnum<Dimension>("OrdST", dynamicMembers);
      }
      return gsaMat;
    }

    private GsaMatAnal GetMatAnal(Base speckleObject)
    {
      if (speckleObject == null) return null;

      var dynamicMembers = speckleObject.GetMembers();
      var gsaMatAnal = new GsaMatAnal();
      gsaMatAnal.Name = speckleObject.GetDynamicValue<string>("Name", dynamicMembers);
      var index = speckleObject.GetDynamicValue<long?>("Index", dynamicMembers);
      if(index == null) index = speckleObject.GetDynamicValue<int?>("Index", dynamicMembers);
      gsaMatAnal.Index = (int?)index;

      gsaMatAnal.Colour = speckleObject.GetDynamicEnum<Colour>("Colour", dynamicMembers);
      gsaMatAnal.Type = speckleObject.GetDynamicEnum<MatAnalType>("Type", dynamicMembers);
      gsaMatAnal.NumParams = speckleObject.GetDynamicValue<int>("NumParams", dynamicMembers);
      gsaMatAnal.E = speckleObject.GetDynamicValue<double>("E", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Nu = speckleObject.GetDynamicValue<double>("Nu", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Rho = speckleObject.GetDynamicValue<double>("Rho", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Alpha = speckleObject.GetDynamicValue<double>("Alpha", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.G = speckleObject.GetDynamicValue<double>("G", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Damp = speckleObject.GetDynamicValue<double>("Damp", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Yield = speckleObject.GetDynamicValue<double>("Yield", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Ultimate = speckleObject.GetDynamicValue<double>("Ultimate", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Eh = speckleObject.GetDynamicValue<double>("Eh", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Beta = speckleObject.GetDynamicValue<double>("Beta", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Cohesion = speckleObject.GetDynamicValue<double>("Cohesion", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Phi = speckleObject.GetDynamicValue<double>("Phi", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Psi = speckleObject.GetDynamicValue<double>("Psi", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Scribe = speckleObject.GetDynamicValue<double>("Scribe", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Ex = speckleObject.GetDynamicValue<double>("Ex", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Ey = speckleObject.GetDynamicValue<double>("Ey", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Ez = speckleObject.GetDynamicValue<double>("Ez", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Nuxy = speckleObject.GetDynamicValue<double>("Nuxy", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Nuyz = speckleObject.GetDynamicValue<double>("Nuyz", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Nuzx = speckleObject.GetDynamicValue<double>("Nuzx", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Alphax = speckleObject.GetDynamicValue<double>("Alphax", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Alphay = speckleObject.GetDynamicValue<double>("Alphay", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Alphaz = speckleObject.GetDynamicValue<double>("Alphaz", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Gxy = speckleObject.GetDynamicValue<double>("Gxy", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Gyz = speckleObject.GetDynamicValue<double>("Gyz", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Gzx = speckleObject.GetDynamicValue<double>("Gzx", dynamicMembers).IsPositiveOrNull();
      gsaMatAnal.Comp = speckleObject.GetDynamicValue<double>("Comp", dynamicMembers).IsPositiveOrNull();
      return gsaMatAnal;
    }

    private GsaMatCurveParam GetMatCurveParam(Base speckleObject)
    {
      if (speckleObject == null) return null;

      var dynamicMembers = speckleObject.GetMembers();
      var gsaMatCurveParam = new GsaMatCurveParam();
      gsaMatCurveParam.Name = speckleObject.GetDynamicValue<string>("Name", dynamicMembers);
      var model = speckleObject.GetDynamicValue<List<string>>("Model", dynamicMembers);
      if (model == null)
      {
        gsaMatCurveParam.Model = new List<MatCurveParamType>() { MatCurveParamType.UNDEF };
      }
      else
      {
        gsaMatCurveParam.Model = model.Select(s => Enum.TryParse(s, true, out MatCurveParamType v) ? v : MatCurveParamType.UNDEF).ToList();
      }
      gsaMatCurveParam.StrainElasticCompression = speckleObject.GetDynamicValue<double>("StrainElasticCompression", dynamicMembers).IsPositiveOrNull();
      gsaMatCurveParam.StrainElasticTension = speckleObject.GetDynamicValue<double>("StrainElasticTension", dynamicMembers).IsPositiveOrNull();
      gsaMatCurveParam.StrainPlasticCompression = speckleObject.GetDynamicValue<double>("StrainPlasticCompression", dynamicMembers).IsPositiveOrNull();
      gsaMatCurveParam.StrainPlasticTension = speckleObject.GetDynamicValue<double>("StrainPlasticTension", dynamicMembers).IsPositiveOrNull();
      gsaMatCurveParam.StrainFailureCompression = speckleObject.GetDynamicValue<double>("StrainFailureCompression", dynamicMembers).IsPositiveOrNull();
      gsaMatCurveParam.StrainFailureTension = speckleObject.GetDynamicValue<double>("StrainFailureTension", dynamicMembers).IsPositiveOrNull();
      gsaMatCurveParam.GammaF = speckleObject.GetDynamicValue<double>("GammaF", dynamicMembers).IsPositiveOrNull();
      gsaMatCurveParam.GammaE = speckleObject.GetDynamicValue<double>("GammaE", dynamicMembers).IsPositiveOrNull();
      return gsaMatCurveParam;
    }

    private double GetBeta(double fc) => LinearInterp(20e6, 100e6, 0.92, 0.72, Math.Abs(fc)); //TODO: - units

    private double GetEpsMax(double fc) => LinearInterp(20e6, 100e6, 0.00024, 0.00084, Math.Abs(fc)); //TODO: - units

    private double GetSteelStrain(double fy) => LinearInterp(200e6, 450e6, 0.001, 0.00225, Math.Abs(fy)); //TODO - units
#endregion

#region Properties
    private GsaSection GsaSectionExample(GSAProperty1D speckleProperty)
    {
      return new GsaSection()
      {
        Index = speckleProperty.GetIndex<GsaSection>(),
        Name = speckleProperty.name,
        ApplicationId = speckleProperty.applicationId,
        Colour = Colour.NO_RGB,
        Type = Section1dType.Generic,
        //PoolIndex = 0,
        ReferencePoint = ReferencePoint.Centroid,
        RefY = 0,
        RefZ = 0,
        Mass = 0,
        Fraction = 1,
        Cost = 0,
        Left = 0,
        Right = 0,
        Slab = 0,
        Components = new List<GsaSectionComponentBase>()
        {
          new SectionComp()
          {
            Name = "",
            //MatAnalIndex = 0,
            MaterialType = Section1dMaterialType.STEEL,
            MaterialIndex = 1,
            OffsetY = 0,
            OffsetZ = 0,
            Rotation = 0,
            Reflect = ComponentReflection.NONE,
            //Pool = 0,
            TaperType = Section1dTaperType.NONE,
            //TaperPos = 0
            ProfileGroup = Section1dProfileGroup.Catalogue,
            ProfileDetails = new ProfileDetailsCatalogue()
            {
              Group = Section1dProfileGroup.Catalogue,
              Profile = "CAT A-UB 610UB125 19981201"
            }
          },
          new SectionSteel()
          {
            //GradeIndex = 0,
            PlasElas = 1,
            NetGross = 1,
            Exposed = 1,
            Beta = 0.4,
            Type = SectionSteelSectionType.HotRolled,
            Plate = SectionSteelPlateType.Undefined,
            Locked = false
          }
        },
        Environ = false
      };
    }
#endregion

#region Constraint
    private Dictionary<GwaAxisDirection6, List<GwaAxisDirection6>> GetRigidConstraint(Dictionary<AxisDirection6, List<AxisDirection6>> speckleConstraint)
    {
      if (speckleConstraint == null) return null;

      var gsaConstraint = new Dictionary<GwaAxisDirection6, List<GwaAxisDirection6>>();
      foreach (var key in speckleConstraint.Keys)
      {
        var speckleKey = key.ToNative();
        gsaConstraint[speckleKey] = new List<GwaAxisDirection6>();
        foreach (var val in speckleConstraint[key])
        {
          gsaConstraint[speckleKey].Add(val.ToNative());
        }
      }
      return gsaConstraint;
    }
#endregion

#region Other
    private double LinearInterp(double x1, double x2, double y1, double y2, double x) => (y2 - y1) / (x2 - x1) * (x - x1) + y1;
#endregion

#endregion
#endregion
  }
}
