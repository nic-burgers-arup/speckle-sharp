using Objects.Structural.Analysis;
using Objects.Structural.Geometry;
using Objects.Structural.Materials;
using Speckle.Core.Models;
using Speckle.SNAP.API;
using Speckle.SNAP.API.s8iSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConverterSNAP
{
  public partial class ConverterSNAP
  {
    private void SetupToNativeFns()
    {
      ToNativeFns = new Dictionary<Type, Func<Base, List<object>>>()
      {
        { typeof(Model), ModelToNative },
        { typeof(Element1D), Element1DToNative },
        { typeof(Objects.Structural.Geometry.Node), NodeToNative }
      };

      var structuralAssembly = Assembly.GetAssembly(ToNativeFns.Keys.First());
      var toNativeFnsToAdd = new Dictionary<Type, Func<Base, List<object>>>();
      var toNativeFnTypes = ToNativeFns.Keys.ToList();
      foreach (var t in toNativeFnTypes)
      {
        var subclasses = structuralAssembly.GetTypes().Where(st => st.BaseType == t);
        if (subclasses != null && subclasses.Count() > 0)
        {
          foreach (var sc in subclasses)
          {
            ToNativeFns.Add(sc, ToNativeFns[t]);
          }
        }
      }
    }

    //This is the only method with a LIST of objects as an input - and intentionally not part of NativeFns list
    private List<object> SpeckleObjectsToNative(List<Base> speckleObjects)
    {
      var retList = new List<object>();

      var speckleDependencyTree = Helper.SpeckleDependencyTree();

      var objectsByType = speckleObjects.GroupBy(o => o.GetType()).ToDictionary(g => g.Key, g => g.ToList());

      foreach (var gen in speckleDependencyTree)
      {
        foreach (var t in gen)
        {
          if (objectsByType.ContainsKey(t))
          {
            foreach (Base so in objectsByType[t])
            {
              try
              {
                if (CanConvertToNative(so))
                {
                  var natives = ToNativeFns[t](so);
                  retList.AddRange(natives);
                }
              }
              catch
              {
                ConversionErrors.Add(new Exception("Unable to convert " + t.Name + " " + (so.applicationId ?? so.id)
                  + " - refer to logs for more information"));
              }
            }
          }
        }
      }
      return retList;
    }

    private List<object> ModelToNative(Base speckleObject)
    {
      var model = (Model)speckleObject;

      var speckleObjects = new List<Base>();
      speckleObjects.AddRangeIfNotNull(model.nodes);
      speckleObjects.AddRangeIfNotNull(model.elements);
      speckleObjects.AddRangeIfNotNull(model.loads);
      speckleObjects.AddRangeIfNotNull(model.restraints);
      speckleObjects.AddRangeIfNotNull(model.properties);
      speckleObjects.AddRangeIfNotNull(model.materials);

      return SpeckleObjectsToNative(speckleObjects);
    }

    private List<object> NodeToNative(Base speckleObject)
    {
      var retList = new List<object>();
      var speckleNode = (Objects.Structural.Geometry.Node)speckleObject;

      if (MapsToSecondaryNode(speckleNode))
      {
        var snapSecondaryNode = new SecondaryNode()
        {
          Name = speckleNode.applicationId,
          X = speckleNode.basePoint.x,
          Y = speckleNode.basePoint.y,
          Z = speckleNode.basePoint.z
        };

        retList.Add(snapSecondaryNode);
      }
      else
      {
        var snapNode = new Speckle.SNAP.API.s8iSchema.Node()
        {
          Name = speckleNode.applicationId,
          X = speckleNode.basePoint.x,
          Y = speckleNode.basePoint.y,
          Z = speckleNode.basePoint.z,
        };

        if (speckleNode.massProperty != null)
        {
          snapNode.AdditionalMass = speckleNode.massProperty.mass;
        }

        retList.Add(snapNode);
      }
      return retList;
    }

    private List<object> Element1DToNative(Base speckleObject)
    {
      var retList = new List<object>();
      var speckleElement = (Element1D)speckleObject;


      if (speckleElement.type == ElementType1D.Beam)
      {
        var snapBeam = new Beam();

        if (speckleElement.end1Node != null)
        {
          snapBeam.NodeIEdgeName = speckleElement.end1Node.applicationId;
          snapBeam.NodeIEdgeType = MapsToSecondaryNode(speckleElement.end1Node) ? NodeType.SecondaryNode : NodeType.Node;
        }
        if (speckleElement.end2Node != null)
        {
          snapBeam.NodeJEdgeName = speckleElement.end2Node.applicationId;
          snapBeam.NodeJEdgeType = MapsToSecondaryNode(speckleElement.end2Node) ? NodeType.SecondaryNode : NodeType.Node;
        }
        if (speckleElement.property != null)
        {
          snapBeam.CrossSection = speckleElement.property.applicationId;
          if (speckleElement.property.material != null)
          {
            snapBeam.Material = speckleElement.property.material.applicationId;
            var materialType = speckleElement.property.material.GetType();
            snapBeam.StructureType = materialType.InheritsOrImplements(typeof(Steel))
              ? StructureType.Steel
              : materialType.InheritsOrImplements(typeof(Concrete))
                ? StructureType.ReinforcedConcrete
                : StructureType.Other;
          }
        }


        retList.Add(snapBeam);
      }
      
      return retList;
    }

    private bool MapsToSecondaryNode(Objects.Structural.Geometry.Node n)
    {
      return (n.restraint == null && n.massProperty == null);
    }
  }
}
