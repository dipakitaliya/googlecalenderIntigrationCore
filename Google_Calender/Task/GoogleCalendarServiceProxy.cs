using Google.Apis.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google_Calender.Models;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Calendar = Google_Calender.Models.Calendar;

namespace Google_Calender.Task
{
    public class GoogleCalendarServiceProxy
    {
          private IAuthenticator _authenticator;
        public GoogleCalendarServiceProxy(GoogleAuthenticator googleAuthenticator)
        {
            _authenticator = googleAuthenticator.Authenticator;
        }
        public IEnumerable<Calendar> GetCalendars()
        {
            var calendarService = new CalendarService(_authenticator);
            var calendars = calendarService.CalendarList.List().Fetch().Items.Select(c => new Calendar()
            {
                Id = c.Id,
                Title = c.Summary,
                Location = c.Location,
                Description = c.Description
            });

            return calendars;
        }
        public Calendar GetCalendar(string calendarId)
        {
            var calendarService = new CalendarService(_authenticator);
            var calendar = calendarService.CalendarList.List().Fetch().Items.FirstOrDefault(c => c.Summary.Contains(calendarId));
            if (calendar == null) throw new GoogleCalendarServiceProxyException("There's no calendar with that id");

            return new Calendar()
            {
                Id = calendar.Id,
                Title = calendar.Summary,
                Location = calendar.Location,
                Description = calendar.Description
            };
        }

        public IEnumerable<CalendarEvent> GetEvents(string calendarId, DateTime startDate, DateTime endDate)
        {
            List<CalendarEvent> calendarEvents = null;
            var calendarService = new CalendarService(_authenticator);
            var calendar = GetCalendar(calendarId);

            var request = calendarService.Events.List(calendar.Id);
            request.TimeMin = startDate.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
            request.TimeMax = endDate.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
            var result = request.Fetch().Items;

            if (result != null)
            {
                calendarEvents = result.Select(c => new CalendarEvent()
                {
                    Id = c.Id,
                    CalendarId = calendarId,
                    Title = c.Summary,
                    Location = c.Location,
                    StartDate = DateTime.Parse(c.Start.DateTime),
                    EndDate = DateTime.Parse(c.End.DateTime),
                    Description = c.Description,
                    ColorId = Int32.Parse(c.ColorId),
                    Attendees = c.Attendees != null ? c.Attendees.Select(attendee => attendee.Email) : null
                }).ToList();
            }

            return calendarEvents;
        }

        public CalendarEvent GetEvent(string calendarId, string eventId)
        {
            var calendarService = new CalendarService(_authenticator);
            var calendarEvent = calendarService.Events.Get(calendarId, eventId).Fetch();
            if (calendarEvent == null) throw new GoogleCalendarServiceProxyException("There is no event stored in the calendar with that id");

            return new CalendarEvent()
            {
                Id = calendarEvent.Id,
                CalendarId = calendarId,
                Title = calendarEvent.Summary,
                Location = calendarEvent.Location,
                StartDate = DateTime.Parse(calendarEvent.Start.DateTime),
                EndDate = DateTime.Parse(calendarEvent.End.DateTime),
                Description = calendarEvent.Description,
                ColorId = Int32.Parse(calendarEvent.ColorId),
                Attendees = calendarEvent.Attendees != null ? calendarEvent.Attendees.Select(c => c.Email) : null
            };
        }

        public bool CreateEvent(CalendarEvent calendarEvent)
        {
            var calendarService = new CalendarService(_authenticator);
            var calendar = GetCalendar(calendarEvent.CalendarId);

            Event newEvent = new Event()
            {
                Summary = calendarEvent.Title,
                Location = calendarEvent.Location,
                Description = calendarEvent.Description,
                Start = new EventDateTime() { DateTime = calendarEvent.StartDate.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK") },
                End = new EventDateTime() { DateTime = calendarEvent.EndDate.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK") },
                Attendees = (calendarEvent.Attendees != null) ? calendarEvent.Attendees.Select(email => new EventAttendee { Email = email }).ToList<EventAttendee>() : null,
                ColorId = ((int)calendarEvent.ColorId).ToString()
            };

            var result = calendarService.Events.Insert(newEvent, calendar.Id).Fetch();

            return result != null;
        }

        public bool UpdateEvent(CalendarEvent calendarEvent)
        {
            var calendarService = new CalendarService(_authenticator);
            var calendar = GetCalendar(calendarEvent.CalendarId);
            var toUpdate = calendarService.Events.Get(calendarEvent.CalendarId, calendarEvent.Id).Fetch();
            if (toUpdate == null) throw new GoogleCalendarServiceProxyException("There is no event stored in the calendar with that id");

            toUpdate.Summary = calendarEvent.Title;
            toUpdate.Location = calendarEvent.Location;
            toUpdate.Description = calendarEvent.Description;
            toUpdate.Start = new EventDateTime() { DateTime = calendarEvent.StartDate.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK") };
            toUpdate.End = new EventDateTime() { DateTime = calendarEvent.EndDate.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK") };
            toUpdate.ColorId = ((int)calendarEvent.ColorId).ToString();

            if (calendarEvent.Attendees != null && calendarEvent.Attendees.Count() > 0 && !string.IsNullOrEmpty(calendarEvent.Attendees.First()))
            {
                toUpdate.Attendees = calendarEvent.Attendees.Select(email => new EventAttendee { Email = email }).ToList<EventAttendee>();
            }

            var result = calendarService.Events.Update(toUpdate, calendar.Id, calendarEvent.Id).Fetch();

            return result != null;
        }

        public void DeleteEvent(string calendarId, string eventId)
        {
            var calendarService = new CalendarService(_authenticator);
            calendarService.Events.Delete(calendarId, eventId).Fetch();
        }

      
    }
    public enum GoogleEventColors
    {
        LightBlue = 1, // #a4bdfc
        LightGreen, // #7ae7bf
        LightViolet, // #dbadff
        LightRed, // #ff887c
        Yellow, // #fbd75b
        Orange, // #ffb878
        Turquoise, // #46d6db
        Gray, // #e1e1e1
        Blue, // #5484ed
        Green, // #51b749
        Red // #dc2127
    }

    public class GoogleCalendarServiceProxyException : Exception
    {
        public GoogleCalendarServiceProxyException(string errorMessage) : base(errorMessage) { }
    }
}