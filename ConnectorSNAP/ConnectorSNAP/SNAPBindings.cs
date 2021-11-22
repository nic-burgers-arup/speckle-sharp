using DesktopUI2;
using DesktopUI2.Models;
using DesktopUI2.Models.Filters;
using DesktopUI2.ViewModels;
using Speckle.Core.Api;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using Speckle.SNAP.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConnectorSNAP
{
  public class SNAPBindings : ConnectorBindings
  {
    public string saveAsFilePath = @"C:\Temp\Received.s8i";

    public List<Exception> ConversionErrors { get; set; } = new List<Exception>();

    /// <summary>
    /// Keeps track of errors in the operations of send/receive.
    /// </summary>
    public List<Exception> OperationErrors { get; set; } = new List<Exception>();

    public SNAPBindings()
    {

    }

    public override string GetActiveViewName() => "SNAPActiveViewName";

    public override List<DesktopUI2.Models.MenuItem> GetCustomStreamMenuItems()
    {
      throw new System.NotImplementedException();
    }

    public override string GetDocumentId()
    {
      throw new System.NotImplementedException();
    }

    public override string GetDocumentLocation()
    {
      throw new System.NotImplementedException();
    }

    public override string GetFileName()
    {
      throw new System.NotImplementedException();
    }

    public override string GetHostAppName() => Applications.SNAP;

    public override List<string> GetObjectsInView()
    {
      throw new System.NotImplementedException();
    }

    public override List<string> GetSelectedObjects()
    {
      throw new System.NotImplementedException();
    }

    public override List<ISelectionFilter> GetSelectionFilters()
    {
      return new List<ISelectionFilter>() { new AllSelectionFilter() };
    }

    public override List<StreamState> GetStreamsInFile()
    {
      throw new System.NotImplementedException();
    }

    public override async Task<StreamState> ReceiveStream(StreamState state, ProgressViewModel progress)
    {
      ConversionErrors.Clear();
      OperationErrors.Clear();

      IProgress<string> loggingProgress = new Progress<string>();

      var kit = KitManager.GetDefaultKit();
      var converter = kit.LoadConverter(Applications.SNAP);
      
      var previouslyReceiveObjects = state.ReceivedObjects;

      var transport = new ServerTransport(state.Client.Account, state.StreamId);

      string referencedObject = state.ReferencedObject;

      if (progress.CancellationTokenSource.Token.IsCancellationRequested)
      {
        return null;
      }

      //if "latest", always make sure we get the latest commit when the user clicks "receive"
      if (state.CommitId == "latest")
      {
        var res = await state.Client.BranchGet(progress.CancellationTokenSource.Token, state.StreamId, state.BranchName, 1);
        referencedObject = res.commits.items.FirstOrDefault().referencedObject;
      }

      var commitObject = await Operations.Receive(
          referencedObject,
          progress.CancellationTokenSource.Token,
          transport,
          onProgressAction: dict => progress.Update(dict),
          onErrorAction: (s, e) =>
          {
            OperationErrors.Add(e);
            //state.Errors.Add(e);
            progress.CancellationTokenSource.Cancel();
          },
          //onTotalChildrenCountKnown: count => Execute.PostToUIThread(() => state.Progress.Maximum = count),
          disposeTransports: true
          );

      if (OperationErrors.Count != 0)
      {
        //Globals.Notify("Failed to get commit.");
        return state;
      }

      if (progress.CancellationTokenSource.Token.IsCancellationRequested)
      {
        return null;
      }

      var topLevelObjects = new List<Base>();
      if (commitObject != null)
      {
        foreach (var prop in commitObject.GetDynamicMembers().Where(m => commitObject[m] is Base))
        {
          topLevelObjects.Add((Base)commitObject[prop]);
        }
      }

      Commands.ConvertToNative(topLevelObjects, converter, loggingProgress);

      if (converter.ConversionErrors != null && converter.ConversionErrors.Count > 0)
      {
        foreach (var ce in converter.ConversionErrors)
        {
          loggingProgress.Report(ce.Message);
          loggingProgress.Report(ce.Message);
        }
      }

      //The cache is filled with natives
      if (Instance.SnapModel.Cache.GetNatives(out var snapRecords) && snapRecords != null && snapRecords.Count > 0)
      {
        Console.WriteLine("Writing " + snapRecords.Count() + " SNAP records in .s8i format to " + saveAsFilePath);

        ((SnapProxy)Instance.SnapModel.Proxy).WriteModel(snapRecords);

        var saved = ((SnapProxy)Instance.SnapModel.Proxy).SaveAs(saveAsFilePath);

        Console.WriteLine("Receiving complete");;
      }
      else
      {
        Console.WriteLine("Conversion resulted in no records to be written to file");
      }

      return state;
    }

    public override void SelectClientObjects(string args)
    {
      throw new System.NotImplementedException();
    }

    public override Task SendStream(StreamState state, ProgressViewModel progress)
    {
      throw new System.NotImplementedException();
    }

    public override void WriteStreamsToFile(List<StreamState> streams)
    {
      throw new System.NotImplementedException();
    }
  }
}
