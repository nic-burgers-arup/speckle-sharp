/*--------------------------------------------------------------------------------------+
|
|     $Source: examples/buildingelement/Column.cs $
|
|  $Copyright: (c) 2017 Bentley Systems, Incorporated. All rights reserved. $
|
+--------------------------------------------------------------------------------------*/
#if (OPENBUILDINGS)
using System;
using System.Collections.Generic;
using Bentley.Interop.ATFCom;
using Bentley.Interop.MicroStationDGN;
using Bentley.Interop.STFCom;
using Bentley.Interop.TFCom;

namespace Objects.Converter.MicroStationOpenRoads
{
  public class BentleyColumn : BuildingElement
  {
    private STFLinearMemberList m_column = null;

    public STFLinearMemberList STFLinear
    {
      get { return m_column; }
    }

    public BentleyColumn()
    {
      m_column = new STFLinearMemberList();
      m_column.Init("");
    }

    public BentleyColumn(Element elm)
    {
      m_column = new STFLinearMemberList();
      m_column.InitFromDescr(elm, true, "");
    }

    public static BentleyColumn CreateColumn(Bentley.GeometryNET.DPoint3d start, Bentley.GeometryNET.DPoint3d end, string crossSection, double uoR)
    {
      BentleyColumn column = new BentleyColumn();
      TFCatalogListClass catalog = (TFCatalogListClass)new TFCatalogList();
      TFCatalogItemList itemList = catalog.GetCatalogItemsByTypeName("Concrete Pile", "");

      //column.DGType = "PILE";
      //column.DGItem = "Concrete";
      //column.PartName = "ARP_Pile_ - _Contiguous_Concrete_1500";
      //column.FamilyName = "ST_Pile_Contiguous_Insitu";

      //TFCatalogItemList itemList = catalog.AsTFCatalog.GetCatalogItemByNames(column.DGType, column.DGItem, "");

      STFLinearMember col = column.STFLinear.AsSTFLinearMember;

      col.InitFromCatalogItem(itemList.AsTFCatalogItem, "");
      col.SetSectionName(crossSection, "");

      STFSection section;
      //col.SetSTFSection(section, "");
      //col.SetSTFSection2(section, "");

      //col.SetPartName
      //col.SetPartFamily

      Point3d p = new Point3d
      {
        X = start.X / uoR,
        Y = start.Y / uoR,
        Z = start.Z / uoR
      };
      Point3d q = new Point3d
      {
        X = end.X / uoR,
        Y = end.Y / uoR,
        Z = end.Z / uoR
      };
      col.SetPQPoints(p, q, "");

      double offset1 = 0;
      double offset2 = 0;
      col.SetLocalOffsets(offset1, offset2, "");

      double rotation = 0;
      col.SetRotation(rotation);

      string name = "Concrete";
      string grade = "C40";
      col.SetMaterial(name, grade, "");

      //string profile = "Test";
      //col.SetProfile(profile, "");

      col.CreateTFFormRecipeList("");

      return column;
    }

    public Element GetElement(ModelReference modelRef)
    {
      Element elm = m_column.AsSTFLinearMember.GetMSElementDescrWritten(modelRef, true);
      return elm;
    }

    public bool AddToModel(ModelReference modelRef)
    {
      Element elm = m_column.AsSTFLinearMember.GetMSElementDescrWritten(modelRef, true);
      modelRef.AddElement(elm);

      return true;
    }

    public bool ReplaceInModel(Element elm)
    {
      ModelReference modelRef = elm.ModelReference;
      Element newElm = m_column.AsSTFLinearMember.GetMSElementDescrWritten(modelRef, true);

      modelRef.ReplaceElement(elm, newElm);

      return true;
    }
  }
}
#endif
