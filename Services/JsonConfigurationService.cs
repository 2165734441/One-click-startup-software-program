using System.IO;
using System.Text.Json;
using OneClickLauncher.Models;

namespace OneClickLauncher.Services;

public sealed class JsonConfigurationService : IConfigurationService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public JsonConfigurationService()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var directory = Path.Combine(localAppData, "OneClickLauncher");
        Directory.CreateDirectory(directory);
        ConfigurationFilePath = Path.Combine(directory, "config.json");
    }

    public string ConfigurationFilePath { get; }

    public async Task<IReadOnlyList<LaunchItem>> LoadAsync()
    {
        if (!File.Exists(ConfigurationFilePath))
        {
            return [];
        }

        try
        {
            await using var stream = File.OpenRead(ConfigurationFilePath);
            var configuration = await JsonSerializer.DeserializeAsync<AppConfiguration>(stream, SerializerOptions);
            return configuration?.Items
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id)
                .ToList() ?? [];
        }
        catch
        {
            var backupPath = ConfigurationFilePath + ".broken";
            try
            {
                File.Copy(ConfigurationFilePath, backupPath, overwrite: true);
            }
            catch
            {
                // If backup fails, start with an empty configuration.
            }

            return [];
        }
    }

    public async Task SaveAsync(IEnumerable<LaunchItem> items)
    {
        var configuration = new AppConfiguration
        {
            Items = items
                .OrderBy(item => item.SortOrder)
                .Select(item => new LaunchItem
                {
                    Id = item.Id,
                    ExecutablePath = item.ExecutablePath,
                    DelaySeconds = item.DelaySeconds,
                    Enabled = item.Enabled,
                    SortOrder = item.SortOrder
                })
                .ToList()
        };

        await using var stream = File.Create(ConfigurationFilePath);
        await JsonSerializer.SerializeAsync(stream, configuration, SerializerOptions);
    }
}
