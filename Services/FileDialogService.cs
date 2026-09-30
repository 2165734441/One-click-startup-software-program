using Microsoft.Win32;

namespace OneClickLauncher.Services;

public sealed class FileDialogService : IFileDialogService
{
    public string? PickExecutableFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择要启动的 EXE 程序",
            Filter = "可执行程序 (*.exe)|*.exe",
            CheckFileExists = true,
            Multiselect = false
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
