using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.SNAP.API;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
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
        try
        {
          var headless = new Headless();
          Headless.AttachConsole(-1);

          headless.RunCLI(e.Args);

          Console.WriteLine("Finished requested action");

          FreeConsole();
        }
        catch
        {
        }
        finally
        {
          ConsoleNewLine();
        }

        Current.Shutdown();
      }
    }

    [DllImport("kernel32")]
    private static extern bool FreeConsole();

    private void ConsoleNewLine()
    {
      try
      {
        // When using a winforms app with AttachConsole the app complets but there is no newline after the process stops. This gives the newline and looks normal from the console:
        System.Windows.Forms.SendKeys.SendWait("{ENTER}");
      }
      catch (Exception e)
      {
        Debug.Fail(e.ToString());
      }
    }
  }
}
