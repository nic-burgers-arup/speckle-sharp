﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Speckle.ConnectorGSA;
using Speckle.GSA.API.GwaSchema;
using System.IO;

namespace ConnectorGSATests
{
  public class ProxyTests : SpeckleConnectorFixture
  {
    [Theory]
    [InlineData("SET\tMEMB.8:{speckle_app_id:gh/a}\t5\tTheRest", GwaKeyword.MEMB, 5, "gh/a", "MEMB.8:{speckle_app_id:gh/a}\t5\tTheRest")]
    [InlineData("MEMB.8:{speckle_app_id:gh/a}\t5\tTheRest", GwaKeyword.MEMB, 5, "gh/a", "MEMB.8:{speckle_app_id:gh/a}\t5\tTheRest")]
    [InlineData("SET_AT\t2\tLOAD_2D_THERMAL.2:{speckle_app_id:gh/a}\tTheRest", GwaKeyword.LOAD_2D_THERMAL, 2, "gh/a", "LOAD_2D_THERMAL.2:{speckle_app_id:gh/a}\tTheRest")]
    [InlineData("LOAD_2D_THERMAL.2:{speckle_app_id:gh/a}\tTheRest", GwaKeyword.LOAD_2D_THERMAL, 0, "gh/a", "LOAD_2D_THERMAL.2:{speckle_app_id:gh/a}\tTheRest")]
    public void ParseGwaCommandTests(string gwa, GwaKeyword expKeyword, int expIndex, string expAppId, string expGwaWithoutSet)
    {
      Speckle.ConnectorGSA.Proxy.GsaProxy.ParseGeneralGwa(gwa, out GwaKeyword? keyword, out int? version, out int? foundIndex, 
        out string streamId, out string applicationId, out string gwaWithoutSet, out string keywordAndVersion);

      Assert.Equal(expKeyword, keyword);
      Assert.True(version.HasValue && version.Value > 0);
      Assert.Equal(expIndex, foundIndex ?? 0);
      Assert.Equal(expAppId, applicationId);
      Assert.Equal(expGwaWithoutSet, gwaWithoutSet);
    }

    [Fact]
    public void TestProxyGetDataForCache()
    {
      var proxy = new Speckle.ConnectorGSA.Proxy.GsaProxy();
      proxy.OpenFile(Path.Combine(TestDataDirectory, modelWithoutResultsFile), false);

      var data = proxy.GetGwaData(DesignLayerKeywords, false);

      Assert.Equal(190, data.Count());
      proxy.Close();
    }
  }
}