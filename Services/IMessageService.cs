namespace OneClickLauncher.Services;

public interface IMessageService
{
    bool Confirm(string message, string title);
    void ShowInfo(string message, string title);
}
