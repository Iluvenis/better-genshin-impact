﻿using BetterGenshinImpact.Service.Notification.Model.Enum;
using System.Text.Json.Serialization;

namespace BetterGenshinImpact.Service.Notification.Model;

public record LifecycleNotificationData : INotificationData
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NotificationEvent Event { get; set; }

    public static LifecycleNotificationData Test()
    {
        return new LifecycleNotificationData() { Event = NotificationEvent.Test };
    }
}
