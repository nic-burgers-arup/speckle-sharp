using Speckle.Core.Kits;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objects.Structural.Property.SectionMapping
{
  /// <summary>
  /// Facilitates section mapping by storing software and catalogue associated with native software
  /// </summary>
  public class SectionMapping : Base
  {
    public string NativeSoftware { get; set; }
    public string NativeCatalogue { get; set; }

    public SectionMapping()
    {

    }
  }
}
