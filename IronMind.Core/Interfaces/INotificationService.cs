namespace IronMind.Core.Interfaces;

public interface INotificationService
{
    Task SendAsync(string deviceToken, string title, string body);
}
