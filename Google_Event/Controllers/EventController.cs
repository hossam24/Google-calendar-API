using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google_Event.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly GoogleCalendarService _calendarService;

    public EventsController(IConfiguration configuration, GoogleCalendarService calendarService)
    {
        _configuration = configuration;
        _calendarService = calendarService;
    }

    [HttpGet]
    /////view the events
    public async Task<IActionResult> ViewEvents([FromQuery] EventFilterModel filters)
    {
        try
        {
           
            CalendarService calendarService = await _calendarService.InitializeCalendarServiceAsync();

           
            var events = await GetGoogleCalendarEvents(calendarService, filters);

            return Ok(events);
        }
        catch (Exception ex)
        {
            
            return StatusCode(500, "An error occurred: " + ex.Message);
        }
    }

    private async Task<IList<Event>> GetGoogleCalendarEvents(CalendarService calendarService, EventFilterModel filters)
    {
      
        
        var request = calendarService.Events.List("primary");
        request.TimeMin = filters.StartDateTime; 
        request.TimeMax = filters.EndDateTime;   

        
        if (!string.IsNullOrWhiteSpace(filters.SearchQuery))
        {
            request.Q = filters.SearchQuery;
        }

       
        var events = new List<Event>();
        string pageToken = null;
        do
        {
            request.PageToken = pageToken;
            var response = await request.ExecuteAsync();

            if (response.Items != null)
            {
                events.AddRange(response.Items);
            }

            pageToken = response.NextPageToken;
        }
        while (pageToken != null);

        return events;
    }

    [HttpPost]
    ///add event
    public async Task<IActionResult> CreateEvent([FromBody] EventModel eventModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            
            CalendarService calendarService = await _calendarService.InitializeCalendarServiceAsync();

           
            if (eventModel.StartDateTime <= DateTime.Now || IsFridayOrSaturday(eventModel.StartDateTime))
            {
                return BadRequest("Invalid event date.");
            }

           
            Event createdEvent = await CreateGoogleCalendarEvent(calendarService, eventModel);

            if (createdEvent != null)
            {
                return Created("uri_to_created_event", createdEvent);
            }

            return BadRequest("Event creation failed.");
        }
        catch (Exception ex)
        {
           
            return StatusCode(500, "An error occurred: " + ex.Message);
        }

    }
    private async Task<Event> CreateGoogleCalendarEvent(CalendarService calendarService, EventModel eventModel)
    {
        Event newEvent = new Event
        {
            Summary = eventModel.Summary,
            Description = eventModel.Description,
            Start = new EventDateTime
            {
                DateTime = eventModel.StartDateTime,
                TimeZone = _configuration["TimeZone"]
            },
            End = new EventDateTime
            {
                DateTime = eventModel.EndDateTime,
                TimeZone = _configuration["TimeZone"]
            }
        };

        try
        {
            return await calendarService.Events.Insert(newEvent, "primary").ExecuteAsync();
        }
        catch (Exception ex)
        {
            // Handle exceptions and return an appropriate response
            return null;
        }
    }
    private bool IsFridayOrSaturday(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday;
    }


    ///delete event.
    [HttpDelete("{eventId}")]
    public async Task<IActionResult> DeleteEvent(string eventId)
    {
        try
        {
           
            CalendarService calendarService = await _calendarService.InitializeCalendarServiceAsync();

           
            if (string.IsNullOrEmpty(eventId))
            {
                return BadRequest("Invalid eventId.");
            }

           
            var deleteRequest = calendarService.Events.Delete("primary", eventId);
            await deleteRequest.ExecuteAsync();

            
            return NoContent();
        }
        catch (Google.GoogleApiException ex)
        {
           
            if (ex.Error.Code == 404)
            {
                return NotFound("Event not found.");
            }

          
            return StatusCode(500, "An error occurred: " + ex.Error.Message);
        }
        catch (Exception ex)
        {
      
            return StatusCode(500, "An error occurred: " + ex.Message);
        }
    }


}
