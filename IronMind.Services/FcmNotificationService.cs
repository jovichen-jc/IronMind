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

        // Production (ECS): inject FIREBASE_SERVICE_ACCOUNT_JSON from AWS Secrets Manager
        // Local dev: set Firebase:ServiceAccountJson in appsettings.Development.json
        var json = config["Firebase:ServiceAccountJson"]
            ?? throw new InvalidOperationException(
                "Firebase:ServiceAccountJson is not configured. " +
                "Set it via the FIREBASE__SERVICE_ACCOUNT_JSON environment variable or appsettings.Development.json.");

        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromJson(json)
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
