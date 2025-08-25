using Microsoft.JSInterop;

namespace DriftMindWeb.Services;

public interface ITimezoneService
{
    Task InitializeAsync();
    DateTime ConvertToUserTime(DateTime utcDateTime);
    string FormatUserTime(DateTime utcDateTime, string format = "HH:mm");
    string GetUserTimeZoneId();
    bool IsInitialized { get; }
}

public class TimezoneService : ITimezoneService
{
    private readonly IJSRuntime _jsRuntime;
    private TimeZoneInfo? _userTimeZone;
    private string _userTimeZoneId = "UTC";
    private bool _isInitialized = false;

    public TimezoneService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public bool IsInitialized => _isInitialized;

    public async Task InitializeAsync()
    {
        try
        {
            // Get user's timezone from browser
            _userTimeZoneId = await _jsRuntime.InvokeAsync<string>("timezoneHelper.getUserTimeZone");
            
            // Convert to .NET TimeZoneInfo
            _userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_userTimeZoneId);
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            // Fallback to UTC on any error
            Console.WriteLine($"Timezone initialization failed: {ex.Message}");
            _userTimeZoneId = "UTC";
            _userTimeZone = TimeZoneInfo.Utc;
            _isInitialized = true;
        }
    }

    public DateTime ConvertToUserTime(DateTime utcDateTime)
    {
        if (!_isInitialized || _userTimeZone == null)
        {
            return utcDateTime; // Return UTC if not initialized
        }

        try
        {
            // Ensure we're working with UTC time
            var utcTime = utcDateTime.Kind == DateTimeKind.Utc 
                ? utcDateTime 
                : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, _userTimeZone);
        }
        catch (Exception)
        {
            return utcDateTime; // Return original on conversion error
        }
    }

    public string FormatUserTime(DateTime utcDateTime, string format = "HH:mm")
    {
        var localTime = ConvertToUserTime(utcDateTime);
        return localTime.ToString(format);
    }

    public string GetUserTimeZoneId()
    {
        return _userTimeZoneId;
    }
}
