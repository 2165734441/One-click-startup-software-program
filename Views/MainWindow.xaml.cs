using System.Windows;
using OneClickLauncher.Services;
using OneClickLauncher.ViewModels;

namespace OneClickLauncher.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel(
            new JsonConfigurationService(),
            new FileDialogService(),
            new ProgramLauncherService(),
            new MessageService());

        DataContext = _viewModel;
        Loaded += MainWindowLoaded;
    }

    private async void MainWindowLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= MainWindowLoaded;
        await _viewModel.InitializeAsync();
    }
}
