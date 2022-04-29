﻿using System.Collections.Generic;
using Speckle.GSA.API;

namespace ConnectorGSA.Models
{
  public class SenderTab : TabBase
  {
    public StreamContentConfig StreamContentConfig { get; set; } = StreamContentConfig.ModelOnly;

    public string LoadCaseList { get; set; }
    public int AdditionalPositionsFor1dElements { get; set; } = 3;
    public bool SendMeaningfulNodes { get; set; } = true;

    public ResultSettings ResultSettings { get; set; } = new ResultSettings();

    public List<StreamState> SenderStreamStates { get => this.StreamStates; }

    private string documentTitle = "";

    public SenderTab() : base(GSALayer.Design)
    {

    }


    internal void SetDocumentName(string name)
    {
      documentTitle = name;
    }
  }
}
