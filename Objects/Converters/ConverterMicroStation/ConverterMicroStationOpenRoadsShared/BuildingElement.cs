/*--------------------------------------------------------------------------------------+
|
|     $Source: examples/buildingelement/BuildingElement.cs $
|
|  $Copyright: (c) 2017 Bentley Systems, Incorporated. All rights reserved. $
|
+--------------------------------------------------------------------------------------*/
#if (OPENBUILDINGS)
//using System;
//using System.Collections.Generic;
//using System.Text;
//using Bentley.DgnPlatformNET;
//using Bentley.DgnPlatformNET.Elements;
//using Bentley.GeometryNET;
//using Bentley.Building.Api;

using Bentley.Interop.MicroStationDGN;
using Bentley.Interop.STFCom;
using Bentley.Interop.ATFCom;
using Bentley.Interop.TFCom;

namespace Objects.Converter.MicroStationOpenRoads
{
    /*=================================================================================**//**
    * @bsiclass                                                               Bentley Systems
    +===============+===============+===============+===============+===============+======*/
    public class BuildingElement
    {
      private TFCatalogList m_datagroup;
      private Bentley.Building.Api.TFFormRecipeList m_form;

      private string m_typeName;
      private string m_itemName;

      private string m_familyName;
      private string m_partName;

      public Bentley.Building.Api.TFFormRecipeList Form
      {
        get { return m_form; }
        set { m_form = value; }
      }

      public TFCatalogList DataGroup
      {
        get { return m_datagroup; }
      }
      public string DGItem
      {
        get { return m_itemName; }
        set { m_itemName = value; }
      }
      public string DGType
      {
        get { return m_typeName; }
        set { m_typeName = value; }
      }

      public string PartName
      {
        get { return m_partName; }
        set { m_partName = value; }
      }

      public string FamilyName
      {
        get { return m_familyName; }
        set { m_familyName = value; }
      }

      public BuildingElement()
      {
        m_datagroup = new TFCatalogList();
        m_datagroup.Init("");
        m_form = null;
      }

      public BuildingElement(Bentley.DgnPlatformNET.Elements.Element elm)
      {
        m_datagroup = new TFCatalogList();
        m_form = new Bentley.Building.Api.TFFormRecipeList();
        m_form.InitFromElement2(elm, "");

        Bentley.Building.Api.ITFFormRecipe f = m_form as Bentley.Building.Api.ITFFormRecipe;
        f.GetPartName(0, out m_partName);
        f.GetPartFamilyName(0, out m_familyName);

        Bentley.Building.Api.TFCatalogItemList itemList = new Bentley.Building.Api.TFCatalogItemList();
        itemList.InitFromElementDescr(elm, 0);

        Bentley.Building.Api.ITFCatalogItem item = itemList as Bentley.Building.Api.ITFCatalogItem;
        GetDataGroupNames(item);
      }

      public void InitForm(Bentley.DgnPlatformNET.DgnModelRef modelRef)
      {
        m_form = new Bentley.Building.Api.TFFormRecipeList();
        m_form.Init("");
        m_form.SetModelRef(modelRef, 0);
      }

      public virtual bool AddToModel()
      {
        m_form.Synchronize(0);
        SetupPartFamily();

        Bentley.DgnPlatformNET.Elements.Element elm;
        m_form.GetElement(0, out elm);

        elm.AddToModel();
        AttachDataGroup(elm);

        return true;
      }

      public virtual bool ReplaceInModel(Bentley.DgnPlatformNET.Elements.Element elm)
      {
        m_form.Synchronize(0);
        SetupPartFamily();

        Bentley.DgnPlatformNET.Elements.Element newElm;
        m_form.GetElementForPersisting(0, out newElm);
        newElm.ReplaceInModel(elm);
        AttachDataGroup(newElm);

        return true;
      }

      public bool GetDataGroupNames(Bentley.Building.Api.ITFCatalogItem item)
      {
        item.GetName(0, out m_itemName);
        Bentley.Building.Api.ITFCatalogTypeList typeList;
        item.GetOwningType(0, out typeList);
        Bentley.Building.Api.ITFCatalogType type = typeList as Bentley.Building.Api.ITFCatalogType;
        type.GetName(0, out m_typeName);

        return true;
      }

      private bool SetupPartFamily()
      {
        Bentley.Building.Api.ITFFormRecipe f = m_form as Bentley.Building.Api.ITFFormRecipe;
        f.SetPartName(m_partName, 0);
        f.SetPartFamilyName(m_familyName, 0);

        return true;
      }

      private bool AttachDataGroup(Bentley.DgnPlatformNET.Elements.Element elm)
      {
        Bentley.Building.Api.ITFCatalog catalog = m_datagroup as Bentley.Building.Api.ITFCatalog;

        Bentley.Building.Api.ITFCatalogItemList itemList;
        catalog.GetCatalogItemByNames(m_typeName, m_itemName, 0, out itemList);

        Bentley.Building.Api.ITFCatalogItem item;
        item = itemList as Bentley.Building.Api.ITFCatalogItem;
        item.SetFamilyAndPartValueChar(m_familyName, m_partName, 0);
        item.AttachLinkagesOnElement(ref elm, 0);

        return true;
      }
    }
  }
#endif

