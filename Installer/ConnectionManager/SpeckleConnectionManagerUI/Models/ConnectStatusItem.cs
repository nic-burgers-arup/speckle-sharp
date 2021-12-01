using ReactiveUI;

namespace SpeckleConnectionManagerUI.Models
{
  public class ConnectStatusItem : ReactiveObject
  {
    public int Identifier { get; set; }
    public string? ServerName { get; set; }
    public string? ServerUrl { get; set; }
    public string? ConnectText { get; set; } = "CONNECT";

    private bool _disconnected = true;
    public bool Disconnected
    {
      get => _disconnected;
      set => this.RaiseAndSetIfChanged(ref _disconnected, value);
    }

    private bool _default = false;
    public bool Default
    {
      get => _default;
      set => this.RaiseAndSetIfChanged(ref _default, value);
    }

    public string? _defaultServerLabel;
    public string? DefaultServerLabel
    {
      get => _defaultServerLabel;
      set => this.RaiseAndSetIfChanged(ref _defaultServerLabel, value);
    }

    private string _colour = "Red";
    public string Colour
    {
      get => _colour;
      set => this.RaiseAndSetIfChanged(ref _colour, value);
    }
  }
}