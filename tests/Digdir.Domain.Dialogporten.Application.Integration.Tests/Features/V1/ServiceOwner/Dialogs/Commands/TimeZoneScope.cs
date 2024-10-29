using System.Reflection;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Features.V1.ServiceOwner.Dialogs.Commands;

public class FakeLocalTimeZone : IDisposable
{
    private readonly TimeZoneInfo _actualLocalTimeZoneInfo;

    private static void SetLocalTimeZone(TimeZoneInfo timeZoneInfo)
    {
        var info = typeof(TimeZoneInfo).GetField("s_cachedData", BindingFlags.NonPublic | BindingFlags.Static);
        var cachedData = info!.GetValue(null);

        var field = cachedData!.GetType().GetField("_localTimeZone", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Instance);
        field!.SetValue(cachedData, timeZoneInfo);
    }

    public FakeLocalTimeZone(TimeZoneInfo timeZoneInfo)
    {
        _actualLocalTimeZoneInfo = TimeZoneInfo.Local;
        SetLocalTimeZone(timeZoneInfo);
    }

    void IDisposable.Dispose()
    {
        SetLocalTimeZone(_actualLocalTimeZoneInfo);
        GC.SuppressFinalize(this);
    }
}
// public class TimeZoneScope : IDisposable
// {
//     private readonly TimeZoneInfo _originalTimeZone;
//
//     public TimeZoneScope(TimeZoneInfo newTimeZone)
//     {
//         _originalTimeZone = TimeZoneInfo.Local;
//         SetTimeZone(newTimeZone);
//     }
//
//     private void SetTimeZone(TimeZoneInfo timeZone)
//     {
//         if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//         {
//             // Windows-specific implementation to set the time zone
//             TimeZoneInfo.ClearCachedData();
//             TimeZoneInfo.Local = timeZone;
//         }
//         else
//         {
//             // Linux/Unix-specific implementation to set the time zone
//             Environment.SetEnvironmentVariable("TZ", timeZone.Id);
//             TimeZoneInfo.ClearCachedData();
//         }
//     }
//
//     public void Dispose()
//     {
//         SetTimeZone(_originalTimeZone);
//     }
// }
