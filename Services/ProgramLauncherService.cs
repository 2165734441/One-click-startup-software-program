using System.Diagnostics;
using System.IO;
using OneClickLauncher.Models;

namespace OneClickLauncher.Services;

public sealed class ProgramLauncherService : IProgramLauncherService
{
    public bool Exists(string executablePath)
    {
        return !string.IsNullOrWhiteSpace(executablePath) && File.Exists(executablePath);
    }

    public void Launch(LaunchItem item)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = item.ExecutablePath,
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(item.ExecutablePath) ?? string.Empty
        };

        Process.Start(startInfo);
    }

    public void OpenFileLocation(string executablePath)
    {
        if (Exists(executablePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{executablePath}\"",
                UseShellExecute = true
            });
            return;
        }

        var directory = Path.GetDirectoryName(executablePath);
        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = directory,
                UseShellExecute = true
            });
        }
    }
}
