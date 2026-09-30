using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using OneClickLauncher.Helpers;
using OneClickLauncher.Models;
using OneClickLauncher.Services;

namespace OneClickLauncher.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly IConfigurationService _configurationService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IProgramLauncherService _programLauncherService;
    private readonly IMessageService _messageService;
    private bool _isLaunching;
    private string _currentStatus = "准备就绪";
    private int _nextId = 1;

    public MainViewModel(
        IConfigurationService configurationService,
        IFileDialogService fileDialogService,
        IProgramLauncherService programLauncherService,
        IMessageService messageService)
    {
        _configurationService = configurationService;
        _fileDialogService = fileDialogService;
        _programLauncherService = programLauncherService;
        _messageService = messageService;

        Items.CollectionChanged += ItemsChanged;

        AddItemCommand = new RelayCommand(_ => AddItem());
        SelectExecutableCommand = new RelayCommand(parameter => SelectExecutable(parameter as LaunchItemViewModel));
        DeleteItemCommand = new RelayCommand(parameter => DeleteItem(parameter as LaunchItemViewModel));
        MoveUpCommand = new RelayCommand(parameter => MoveUp(parameter as LaunchItemViewModel), parameter => CanMoveUp(parameter as LaunchItemViewModel));
        MoveDownCommand = new RelayCommand(parameter => MoveDown(parameter as LaunchItemViewModel), parameter => CanMoveDown(parameter as LaunchItemViewModel));
        OpenLocationCommand = new RelayCommand(parameter => OpenLocation(parameter as LaunchItemViewModel));
        LaunchAllCommand = new AsyncRelayCommand(_ => LaunchAllAsync(), _ => Items.Any(item => item.Enabled) && !IsLaunching);
    }

    public ObservableCollection<LaunchItemViewModel> Items { get; } = [];
    public ObservableCollection<string> StatusLines { get; } = [];

    public ICommand AddItemCommand { get; }
    public ICommand SelectExecutableCommand { get; }
    public ICommand DeleteItemCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand OpenLocationCommand { get; }
    public ICommand LaunchAllCommand { get; }

    public bool HasItems => Items.Count > 0;

    public bool IsLaunching
    {
        get => _isLaunching;
        private set
        {
            if (SetProperty(ref _isLaunching, value))
            {
                OnPropertyChanged(nameof(CanEdit));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool CanEdit => !IsLaunching;

    public string CurrentStatus
    {
        get => _currentStatus;
        private set => SetProperty(ref _currentStatus, value);
    }

    public string ConfigurationFilePath => _configurationService.ConfigurationFilePath;

    public async Task InitializeAsync()
    {
        var loadedItems = await _configurationService.LoadAsync();

        foreach (var item in loadedItems)
        {
            AddExistingItem(item);
        }

        _nextId = Items.Select(item => item.Item.Id).DefaultIfEmpty(0).Max() + 1;
        Reindex();
        CurrentStatus = HasItems ? "准备就绪" : "还没有启动程序";
    }

    private void AddItem()
    {
        var path = _fileDialogService.PickExecutableFile();
        if (path is null)
        {
            return;
        }

        var item = new LaunchItem
        {
            Id = _nextId++,
            ExecutablePath = path,
            DelaySeconds = 1,
            Enabled = true,
            SortOrder = Items.Count + 1
        };

        AddExistingItem(item);
        Reindex();
        SaveSoon();
        CurrentStatus = $"已添加启动项 {Items.Count}";
    }

    private void SelectExecutable(LaunchItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        var path = _fileDialogService.PickExecutableFile();
        if (path is null)
        {
            return;
        }

        item.ExecutablePath = path;
        SaveSoon();
        CurrentStatus = $"已更新启动项 {item.Number}";
    }

    private void DeleteItem(LaunchItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(item.ExecutablePath))
        {
            var confirmed = _messageService.Confirm($"确定删除启动项 {item.Number} 吗？", "确认删除");
            if (!confirmed)
            {
                return;
            }
        }

        item.Item.PropertyChanged -= ItemPropertyChanged;
        Items.Remove(item);
        Reindex();
        SaveSoon();
        CurrentStatus = HasItems ? "已删除启动项" : "还没有启动程序";
    }

    private void MoveUp(LaunchItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        var index = Items.IndexOf(item);
        if (index <= 0)
        {
            return;
        }

        Items.Move(index, index - 1);
        Reindex();
        SaveSoon();
    }

    private void MoveDown(LaunchItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        var index = Items.IndexOf(item);
        if (index < 0 || index >= Items.Count - 1)
        {
            return;
        }

        Items.Move(index, index + 1);
        Reindex();
        SaveSoon();
    }

    private bool CanMoveUp(LaunchItemViewModel? item)
    {
        return CanEdit && item is not null && Items.IndexOf(item) > 0;
    }

    private bool CanMoveDown(LaunchItemViewModel? item)
    {
        return CanEdit && item is not null && Items.IndexOf(item) < Items.Count - 1;
    }

    private void OpenLocation(LaunchItemViewModel? item)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.ExecutablePath))
        {
            CurrentStatus = "请先选择 EXE";
            return;
        }

        _programLauncherService.OpenFileLocation(item.ExecutablePath);
    }

    private async Task LaunchAllAsync()
    {
        IsLaunching = true;
        StatusLines.Clear();
        CurrentStatus = "开始启动";

        try
        {
            foreach (var item in Items.OrderBy(item => item.SortOrder).Where(item => item.Enabled).ToList())
            {
                if (item.DelaySeconds > 0)
                {
                    CurrentStatus = $"等待 {item.DelaySeconds} 秒后启动 {item.Number}...";
                    await Task.Delay(TimeSpan.FromSeconds(item.DelaySeconds));
                }

                CurrentStatus = $"正在启动 {item.Number}...";
                StatusLines.Add(CurrentStatus);

                if (!_programLauncherService.Exists(item.ExecutablePath))
                {
                    var message = $"启动项{item.Number}的程序不存在，请重新选择EXE。";
                    StatusLines.Add(message);
                    CurrentStatus = message;
                    continue;
                }

                try
                {
                    _programLauncherService.Launch(item.Item);
                    StatusLines.Add($"✓ {item.Number} 已启动");
                }
                catch (Exception ex)
                {
                    StatusLines.Add($"启动项 {item.Number} 启动失败：{ex.Message}");
                }
            }

            StatusLines.Add("启动完成");
            CurrentStatus = "启动完成";
        }
        finally
        {
            IsLaunching = false;
        }
    }

    private void AddExistingItem(LaunchItem item)
    {
        item.PropertyChanged += ItemPropertyChanged;
        Items.Add(new LaunchItemViewModel(item));
    }

    private void Reindex()
    {
        for (var index = 0; index < Items.Count; index++)
        {
            Items[index].Number = index + 1;
            Items[index].SortOrder = index + 1;
        }

        OnPropertyChanged(nameof(HasItems));
        CommandManager.InvalidateRequerySuggested();
    }

    private void ItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasItems));
        CommandManager.InvalidateRequerySuggested();
    }

    private void ItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(LaunchItem.ExecutablePath)
            or nameof(LaunchItem.DelaySeconds)
            or nameof(LaunchItem.Enabled)
            or nameof(LaunchItem.SortOrder))
        {
            SaveSoon();
        }
    }

    private void SaveSoon()
    {
        _ = SaveAsync();
    }

    private async Task SaveAsync()
    {
        try
        {
            await _configurationService.SaveAsync(Items.Select(item => item.Item));
        }
        catch (Exception ex)
        {
            CurrentStatus = $"配置保存失败：{ex.Message}";
        }
    }
}
