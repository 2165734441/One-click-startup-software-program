using OneClickLauncher.Helpers;

namespace OneClickLauncher.Models;

public sealed class LaunchItem : ObservableObject
{
    private int _id;
    private string _executablePath = string.Empty;
    private int _delaySeconds = 1;
    private bool _enabled = true;
    private int _sortOrder;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string ExecutablePath
    {
        get => _executablePath;
        set => SetProperty(ref _executablePath, value);
    }

    public int DelaySeconds
    {
        get => _delaySeconds;
        set => SetProperty(ref _delaySeconds, Math.Max(0, value));
    }

    public bool Enabled
    {
        get => _enabled;
        set => SetProperty(ref _enabled, value);
    }

    public int SortOrder
    {
        get => _sortOrder;
        set => SetProperty(ref _sortOrder, value);
    }
}
