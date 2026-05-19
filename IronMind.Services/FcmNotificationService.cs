using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using IronMind.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace IronMind.Services;

public class FcmNotificationService : INotificationService
{
    public FcmNotificationService(IConfiguration config)
    {
        if (FirebaseApp.DefaultInstance is not null) return;

        var path = config["Firebase:ServiceAccountPath"]
            ?? throw new InvalidOperationException("Firebase:ServiceAccountPath not set in config.");

        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(path)
        });
    }

    public async Task SendAsync(string deviceToken, string title, string body)
    {
        var message = new Message
        {
            Token = deviceToken,
            Notification = new Notification { Title = title, Body = body }
        };
        await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }
}
