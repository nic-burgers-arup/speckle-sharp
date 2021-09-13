﻿using System.Linq;
using Xunit;
using System.IO;
using Speckle.ConnectorGSA.Proxy.GwaParsers;
using Speckle.GSA.API;
using Speckle.ConnectorGSA.Proxy;

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
      Instance.GsaModel.Layer = GSALayer.DesignOnly;
      var proxy = new Speckle.ConnectorGSA.Proxy.GsaProxy();
      proxy.OpenFile(Path.Combine(TestDataDirectory, modelWithoutResultsFile), false);

      Assert.True(proxy.GetGwaData(false, out var records));
      proxy.Close();

      Assert.Equal(188, records.Count());
    }

    [Fact]
    public void TestDependencyTreeSimple()
    {
      var chars = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g' };
      var t = new TypeTreeCollection<char>(chars);
      t.Integrate('a', 'c', 'd');
      t.Integrate('c', 'e', 'f');
      var generations = t.Generations();
    }

    [Fact]
    public void TestDependencyTreeComplex()
    {
      var chars = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g' };
      var t = new TypeTreeCollection<char>(chars);
      t.Integrate('a', 'c', 'd', 'f');
      t.Integrate('c', 'f', 'g');
      t.Integrate('d', 'g');
      t.Integrate('b', 'd', 'e');
      var generations = t.Generations().Select(g => g.OrderBy(v => v).ToList()).ToList();
      Assert.Equal(new[] { 'a', 'b' }, generations[2]);
      Assert.Equal(new[] { 'c', 'd', 'e' }, generations[1]);
      Assert.Equal(new[] { 'f', 'g' }, generations[0]);
    }
  }
}
