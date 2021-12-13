using ConnectorSNAP;
using Speckle.Core.Credentials;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace ConnectorSNAPTests
{
  public class ConnectorSNAPTests
  {
    private string rxStreamId = "9937956209";
    private string rxServerUrl = "https://speckle.xyz";

    private string saveAsAlternativeFilepath(string fn)
    {
      return Path.Combine(TestDataDirectory, fn.Split('.').First() + "_test.gwb");
    }

    protected string TestDataDirectory { get => AppDomain.CurrentDomain.BaseDirectory.TrimEnd(new[] { '\\' }) + @"\..\..\..\TestModels\"; }

    protected string modelWithoutResultsFile = "Structural Demo.gwb";
    protected string modelWithResultsFile = "Structural Demo Results.gwb";

    protected string v2ServerUrl = "https://v2.speckle.arup.com";

    [Fact]
    public void HeadlessReceiveBothModels()
    {
      var headless = new Headless();
      var account = AccountManager.GetDefaultAccount();
      var cliResult = headless.RunCLI("receiver",
        "--file", Path.Combine(TestDataDirectory, "Received.s8i"),
        "--streamIDs", rxStreamId,
        "--nodeAllowance", "0.1");

      Assert.True(cliResult);
    }

    [Fact]
    public void CsvReadAndWrite()
    {
      var inputFilePath = @"C:\Nicolaas\Repo\speckle-sharp-nic-burgers-arup\ConnectorSNAP\TestModels\test.s8i";
      var outputFilePath = @"C:\Nicolaas\Repo\speckle-sharp-nic-burgers-arup\ConnectorSNAP\TestModels\test_output.s8i";

      var proxy = new SnapProxy();
      proxy.OpenFile(inputFilePath);
      proxy.GetRecords(out List<object> records);
      proxy.WriteModel(records);
      Assert.True(proxy.Errors.Count() == 0);

      Assert.True(proxy.SaveAs(outputFilePath));
    }
  }
}
