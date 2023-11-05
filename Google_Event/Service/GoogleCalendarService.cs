using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google_Event.Models;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

public class GoogleCalendarService
{
    private readonly IConfiguration _configuration;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string[] _scopes = { CalendarService.Scope.Calendar };
    private readonly string _refreshToken;

    public GoogleCalendarService(IConfiguration configuration)
    {
        _configuration = configuration;
        _clientId = _configuration["GoogleApi:ClientId"];
        _clientSecret = _configuration["GoogleApi:ClientSecret"];
    }

    public async Task<CalendarService> InitializeCalendarServiceAsync(CancellationToken cancellationToken = default)
    {
        UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret
            },
            _scopes,
            "user",
            cancellationToken
        );

        return new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "ManageEvent.com"
        });
    }

    public async Task<Event> CreateGoogleCalendarEvent(CalendarService calendarService, EventModel eventModel)
    {
        Event newEvent = new Event
        {
            Summary = eventModel.Summary,
            Description = eventModel.Description,
            Start = new EventDateTime
            {
                DateTime = eventModel.StartDateTime,
                TimeZone = "UTC" 
            },
            End = new EventDateTime
            {
                DateTime = eventModel.EndDateTime,
                TimeZone = "UTC" 
            }
        };

        try
        {
            return await calendarService.Events.Insert(newEvent, "primary").ExecuteAsync();
        }
        catch (Exception ex)
        {
            
            return null;
        }
    }
    
    //private async Task<string> GetValidAccessTokenAsync()
    //{
    //    var tokenResponse = await _tokenProvider.GetAccessTokenAsync();
    //    if (tokenResponse.IsExpired)
    //    {
           
    //        tokenResponse = await _tokenProvider.RefreshAccessTokenAsync();
    //    }

    //    return tokenResponse.AccessToken;
    //}


}
