using OneClickLauncher.Models;

namespace OneClickLauncher.Services;

public interface IProgramLauncherService
{
    bool Exists(string executablePath);
    void Launch(LaunchItem item);
    void OpenFileLocation(string executablePath);
}
