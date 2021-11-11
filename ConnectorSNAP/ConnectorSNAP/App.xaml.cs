using Speckle.SNAP.API;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ConnectorSNAP
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private void Application_Startup(object sender, StartupEventArgs e)
    {
      CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
      CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

      Instance.SnapModel = new SnapModel();

      if (e.Args.Length == 0)
      {
        MainWindow wnd = new MainWindow();
        wnd.Show();
      }
      else
      {
        var headless = new Headless();
        Headless.AttachConsole(-1);

        headless.RunCLI(e.Args);
        Current.Shutdown();
      }
    }
  }
}
