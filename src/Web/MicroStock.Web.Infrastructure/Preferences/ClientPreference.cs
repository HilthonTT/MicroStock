namespace MicroStock.Web.Infrastructure.Preferences;

public sealed class ClientPreference
{
    public bool IsDarkMode { get; set; } = true;

    public bool IsRTL { get; set; }

    public bool IsDrawerOpen { get; set; }

    public string PrimaryColor { get; set; } = string.Empty;

    public string SecondaryColor { get; set; } = string.Empty;
}
