using OneClickLauncher.Models;

namespace OneClickLauncher.Services;

public interface IConfigurationService
{
    string ConfigurationFilePath { get; }
    Task<IReadOnlyList<LaunchItem>> LoadAsync();
    Task SaveAsync(IEnumerable<LaunchItem> items);
}
