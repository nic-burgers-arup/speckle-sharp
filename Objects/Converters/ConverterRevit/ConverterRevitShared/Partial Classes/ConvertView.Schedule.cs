using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Objects.Organization;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using DB = Autodesk.Revit.DB;

namespace Objects.Converter.Revit
{
  public partial class ConverterRevit
  {
    #region ToNative
    private ApplicationObject DataTableToNative(DataTable speckleTable)
    {
      var docObj = GetExistingElementByApplicationId(speckleTable.applicationId);
      var appObj = new ApplicationObject(speckleTable.id, speckleTable.speckle_type) 
      { 
        applicationId = speckleTable.applicationId 
      };

      if (docObj == null)
      {
        throw new NotSupportedException("Creating brand new schedules is currently not supported");
      }

      if (docObj is not ViewSchedule revitSchedule)
      {
        throw new Exception($"Existing element with UniqueId = {docObj.UniqueId} is of the type {docObj.GetType()}, not of the expected type, DB.ViewSchedule");
      }

      var speckleIndexToRevitScheduleDataMap = new Dictionary<int, RevitScheduleData>();
      foreach (var columnInfo in RevitScheduleUtils.ScheduleColumnIteration(revitSchedule))
      {
        AddToIndexToScheduleMap(columnInfo, speckleTable, speckleIndexToRevitScheduleDataMap);
      }

      var originalTableIds = new FilteredElementCollector(Doc, revitSchedule.Id)
        .ToElementIds();
      foreach (var rowInfo in RevitScheduleUtils.ScheduleRowIteration(revitSchedule))
      {
        UpdateDataInRow(rowInfo, originalTableIds, revitSchedule, speckleTable, speckleIndexToRevitScheduleDataMap);
      }

      appObj.Update(convertedItem: docObj, createdId: docObj.UniqueId, status: ApplicationObject.State.Updated);
      return appObj;
    }

    private static void AddToIndexToScheduleMap(ScheduleColumnIterationInfo info, DataTable speckleTable, Dictionary<int, RevitScheduleData> speckleIndexToRevitScheduleDataMap)
    {
      var fieldInt = info.field.ParameterId.IntegerValue;
      var incomingColumnIndex = speckleTable.columnMetadata
        .FindIndex(b => b["BuiltInParameterInteger"] is long paramInt && paramInt == fieldInt);

      if (incomingColumnIndex == -1)
      {
        return;
      }

      var scheduleData = new RevitScheduleData
      {
        ColumnIndex = info.columnIndex - info.numHiddenFields,
        Parameter = (BuiltInParameter)fieldInt
      };
      speckleIndexToRevitScheduleDataMap.Add(incomingColumnIndex, scheduleData);
    }

    private void UpdateDataInRow(ScheduleRowIterationInfo info, ICollection<ElementId> originalTableIds, ViewSchedule revitSchedule, DataTable speckleTable, Dictionary<int, RevitScheduleData> speckleIndexToRevitScheduleDataMap)
    {
      var elementIds = ElementApplicationIdsInRow(info.rowIndex, info.section, originalTableIds, revitSchedule, info.tableSection);

      foreach (var id in elementIds)
      {
        var speckleObjectRowIndex = speckleTable.rowMetadata
          .FindIndex(b => b["RevitApplicationIds"] is IList list && list.Contains(id));

        if (speckleObjectRowIndex == -1)
        {
          continue;
        }

        foreach (var kvp in speckleIndexToRevitScheduleDataMap)
        {
          var speckleObjectColumnIndex = kvp.Key;
          var revitScheduleData = kvp.Value;
          var existingValue = revitSchedule.GetCellText(info.tableSection, info.rowIndex, revitScheduleData.ColumnIndex);
          var newValue = speckleTable.data[speckleObjectRowIndex][speckleObjectColumnIndex];
          if (existingValue == newValue.ToString())
          {
            continue;
          }

          var element = Doc.GetElement(id);
          if (element == null)
            continue;

          TrySetParam(element, revitScheduleData.Parameter, newValue, "none");
        }
      }
    }

    #endregion

    #region ToSpeckle
    private DataTable ScheduleToSpeckle(DB.ViewSchedule revitSchedule)
    {
      var speckleTable = new DataTable
      {
        applicationId = revitSchedule.UniqueId
      };

      var originalTableIds = new FilteredElementCollector(Doc, revitSchedule.Id)
          .ToElementIds();

      var skippedIndicies = new Dictionary<SectionType, List<int>>();
      var columnHeaders = new List<string>();

      DefineColumnMetadata(revitSchedule, speckleTable, originalTableIds, columnHeaders);
      PopulateDataTableRows(revitSchedule, speckleTable, originalTableIds, skippedIndicies);

      speckleTable.headerRowIndex = Math.Max(0, GetTableHeaderIndex(revitSchedule, skippedIndicies, columnHeaders.FirstOrDefault()));

      if (!revitSchedule.Definition.ShowHeaders)
      {
        AddHeaderRow(speckleTable, columnHeaders);
      }

       return speckleTable;
    }

    private void AddHeaderRow(DataTable speckleTable, List<string> headers)
    {
      speckleTable.AddRow(metadata: new Base(), index: speckleTable.headerRowIndex, headers.ToArray());
    }

    private void DefineColumnMetadata(ViewSchedule revitSchedule, DataTable speckleTable, ICollection<ElementId> originalTableIds, List<string> columnHeaders)
    {
      Element firstElement = null;
      Element firstType = null;
      if (originalTableIds.Count > 0)
      {
        firstElement = Doc.GetElement(originalTableIds.First());
        firstType = Doc.GetElement(firstElement.GetTypeId());
      }

      foreach (var columnInfo in RevitScheduleUtils.ScheduleColumnIteration(revitSchedule))
      {
        AddColumnMetadataToDataTable(columnInfo, revitSchedule, speckleTable, columnHeaders, firstType, firstElement);
      }
    }

    private static void AddColumnMetadataToDataTable(ScheduleColumnIterationInfo info, ViewSchedule revitSchedule, DataTable speckleTable, List<string> columnHeaders, Element firstType, Element firstElement)
    {
      // add column header to list for potential future use
      columnHeaders.Add(info.field.ColumnHeading);

      var builtInParameter = (BuiltInParameter)info.field.ParameterId.IntegerValue;

      var columnMetadata = new Base();
      columnMetadata["BuiltInParameterInteger"] = info.field.ParameterId.IntegerValue;
      columnMetadata["FieldType"] = info.field.FieldType.ToString();

      Parameter param;
      if (info.field.FieldType == ScheduleFieldType.ElementType)
      {
        if (firstType != null)
        {
          param = firstType.get_Parameter(builtInParameter);
          columnMetadata["IsReadOnly"] = param?.IsReadOnly;
        }
      }
      else if (info.field.FieldType == ScheduleFieldType.Instance)
      {
        if (firstElement != null)
        {
          param = firstElement.get_Parameter(builtInParameter);
          columnMetadata["IsReadOnly"] = param?.IsReadOnly;
        }
      }
      else
      {
        var scheduleCategory = (BuiltInCategory)revitSchedule.Definition.CategoryId.IntegerValue;
        SpeckleLog.Logger.Warning("Schedule of category, {scheduleCategory}, contains field of type {builtInParameter} which has an unsupported field type, {fieldType}",
          scheduleCategory,
          builtInParameter,
          info.field.FieldType.ToString());
      }
      speckleTable.DefineColumn(columnMetadata);
    }

    private void PopulateDataTableRows(ViewSchedule revitSchedule, DataTable speckleTable, ICollection<ElementId> originalTableIds, Dictionary<SectionType, List<int>> skippedIndicies)
    {
      foreach (var rowInfo in RevitScheduleUtils.ScheduleRowIteration(revitSchedule))
      {
        try
        {
          var rowAdded = AddRowToSpeckleTable(
            revitSchedule,
            speckleTable,
            originalTableIds,
            rowInfo.tableSection,
            rowInfo.section,
            rowInfo.columnCount,
            rowInfo.rowIndex
          );
          if (!rowAdded)
          {
            if (!skippedIndicies.ContainsKey(rowInfo.tableSection))
            {
              skippedIndicies.Add(rowInfo.tableSection, new List<int>());
            }
            skippedIndicies[rowInfo.tableSection].Add(rowInfo.rowIndex);
          }
        }
        catch (Autodesk.Revit.Exceptions.ArgumentOutOfRangeException ex)
        {
        }
      }
    }

    private int GetTableHeaderIndex(ViewSchedule revitSchedule, Dictionary<SectionType, List<int>> skippedIndicies, string firstColumnHeader)
    {
      if (!revitSchedule.Definition.ShowHeaders)
      {
        return RevitScheduleUtils.ExecuteInTemporaryTransaction(() =>
        {
          revitSchedule.Definition.ShowHeaders = true;
          return GetHeaderIndexFromScheduleWithHeaders(revitSchedule, skippedIndicies, firstColumnHeader);
        }, Doc);
      }

      return GetHeaderIndexFromScheduleWithHeaders(revitSchedule, skippedIndicies, firstColumnHeader);
    }

    private static int GetHeaderIndexFromScheduleWithHeaders(ViewSchedule revitSchedule, Dictionary<SectionType, List<int>> skippedIndicies, string firstColumnHeader)
    {
      foreach (var rowInfo in RevitScheduleUtils.ScheduleRowIteration(revitSchedule, skippedIndicies))
      {
        var cellValue = revitSchedule.GetCellText(rowInfo.tableSection, rowInfo.rowIndex, 0);
        if (cellValue != firstColumnHeader)
        {
          continue;
        }
        return rowInfo.masterRowIndex;
      }
      return -1;
    }

    private bool AddRowToSpeckleTable(ViewSchedule revitSchedule, DataTable speckleTable, ICollection<ElementId> originalTableIds, SectionType tableSection, TableSectionData section, int columnCount, int rowIndex)
    {
      var rowData = new List<string>();
      for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
      {
        rowData.Add(revitSchedule.GetCellText(tableSection, rowIndex, columnIndex));
      }

      if (!rowData.Where(s => !string.IsNullOrEmpty(s)).Any())
      {
        return false;
      }
      var metadata = new Base();
      metadata["RevitApplicationIds"] = ElementApplicationIdsInRow(rowIndex, section, originalTableIds, revitSchedule, tableSection);

      try
      {
        speckleTable.AddRow(metadata: metadata, objects: rowData.ToArray());
      }
      catch (ArgumentException)
      {
        // trying to add an invalid row. Just don't add it and continue to the next
        return false;
      }

      return true;
    }
    #endregion

    private List<string> ElementApplicationIdsInRow(int rowNumber, TableSectionData section, ICollection<ElementId> orginialTableIds, DB.ViewSchedule revitSchedule, SectionType tableSection)
    {
      var elementApplicationIdsInRow = new List<string>();
      var remainingIdsInRow = RevitScheduleUtils.ExecuteInTemporaryTransaction(() =>
      {
        section.RemoveRow(rowNumber);
        return new FilteredElementCollector(Doc, revitSchedule.Id)
          .ToElementIds()
          .ToList();
      }, Doc);

      // the section must be recomputed here because of our hacky row deleting trick
      var table = revitSchedule.GetTableData();
      section = table.GetSectionData(tableSection);

      if (remainingIdsInRow == null || remainingIdsInRow.Count == orginialTableIds.Count)
        return elementApplicationIdsInRow;

      foreach (var id in orginialTableIds)
      {
        if (remainingIdsInRow.Contains(id)) continue;
        elementApplicationIdsInRow.Add(Doc.GetElement(id).UniqueId);
      }

      return elementApplicationIdsInRow;
    }
  }

  public struct RevitScheduleData
  {
    public int ColumnIndex;
    public BuiltInParameter Parameter;
  }
  public struct ScheduleRowIterationInfo
  {
    public SectionType tableSection;
    public TableSectionData section;
    public int rowIndex;
    public int columnCount;
    public int masterRowIndex;
  }
  public struct ScheduleColumnIterationInfo
  {
    public ScheduleField field;
    public int columnIndex;
    public int columnCount;
    public int numHiddenFields;
  }
  public static class RevitScheduleUtils
  {
    public static IEnumerable<ScheduleRowIterationInfo> ScheduleRowIteration(ViewSchedule revitSchedule, Dictionary<SectionType, List<int>> skippedIndicies = null)
    {
      var masterRowIndex = 0;

      foreach (SectionType tableSection in Enum.GetValues(typeof(SectionType)))
      {
        // the table must be recomputed here because of our hacky row deleting trick
        var table = revitSchedule.GetTableData();
        var section = table.GetSectionData(tableSection);

        if (section == null)
        {
          continue;
        }
        var rowCount = section.NumberOfRows;
        var columnCount = section.NumberOfColumns;

        for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
          yield return new ScheduleRowIterationInfo 
          {
            tableSection = tableSection,
            section = section,
            rowIndex = rowIndex, 
            columnCount = columnCount 
          };

          if (skippedIndicies == null || !skippedIndicies.TryGetValue(tableSection, out var indicies) || !indicies.Contains(rowIndex))
          {
            // this "skippedIndicies" dict contains the indicies that contain only empty values
            // these values were skipped when adding them to the DataTable, so the indicies of the revitSchedule
            // and the Speckle DataTable will differ at these indicies (and all subsequent indicies)

            // therefore we only want to increment the masterRowIndex if this row was added to the Speckle DataTable
            masterRowIndex++;
          }
        }
      }
    }
    public static IEnumerable<ScheduleColumnIterationInfo> ScheduleColumnIteration(ViewSchedule revitSchedule)
    {
      var scheduleFieldOrder = revitSchedule.Definition.GetFieldOrder();
      var numHiddenFields = 0;

      for (var columnIndex = 0; columnIndex < scheduleFieldOrder.Count; columnIndex++)
      {
        var field = revitSchedule.Definition.GetField(scheduleFieldOrder[columnIndex]);

        // we cannot get the values for hidden fields, so we need to subtract one from the index that is passed to
        // tableView.GetCellText.
        if (field.IsHidden)
        {
          numHiddenFields++;
          continue;
        }

        yield return new ScheduleColumnIterationInfo
        {
          field = field,
          columnIndex = columnIndex,
          columnCount = scheduleFieldOrder.Count,
          numHiddenFields = numHiddenFields
        };
      }
    }

    public static TResult ExecuteInTemporaryTransaction<TResult>(Func<TResult> function, Document doc)
    {
      TResult result = default;
      if (!doc.IsModifiable)
      {
        using var t = new Transaction(doc, "This Transaction Will Never Get Committed");
        try
        {
          t.Start();
          result = function();
        }
        catch
        {
          // ignore because we're just going to rollback
        }
        finally
        {
          t.RollBack();
        }
      }
      else
      {
        using var t = new SubTransaction(doc);
        try
        {
          t.Start();
          result = function();
        }
        catch
        {
          // ignore because we're just going to rollback
        }
        finally
        {
          t.RollBack();
        }
      }

      return result;
    }
  }
}
