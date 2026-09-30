using OneClickLauncher.Helpers;
using OneClickLauncher.Models;
using System.IO;

namespace OneClickLauncher.ViewModels;

public sealed class LaunchItemViewModel : ObservableObject
{
    private int _number;

    public LaunchItemViewModel(LaunchItem item)
    {
        Item = item;
        Item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(LaunchItem.ExecutablePath)
                or nameof(LaunchItem.DelaySeconds)
                or nameof(LaunchItem.Enabled)
                or nameof(LaunchItem.SortOrder))
            {
                OnPropertyChanged(args.PropertyName);
                OnPropertyChanged(nameof(FileName));
            }
        };
    }

    public LaunchItem Item { get; }

    public int Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }

    public string ExecutablePath
    {
        get => Item.ExecutablePath;
        set => Item.ExecutablePath = value;
    }

    public int DelaySeconds
    {
        get => Item.DelaySeconds;
        set => Item.DelaySeconds = value;
    }

    public bool Enabled
    {
        get => Item.Enabled;
        set => Item.Enabled = value;
    }

    public int SortOrder
    {
        get => Item.SortOrder;
        set => Item.SortOrder = value;
    }

    public string FileName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ExecutablePath))
            {
                return "未选择程序";
            }

            return Path.GetFileName(ExecutablePath);
        }
    }
}
