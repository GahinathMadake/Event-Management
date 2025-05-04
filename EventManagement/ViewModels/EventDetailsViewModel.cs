using EventManagement.Models;

namespace EventManagement.ViewModels
{
    public class EventDetailsViewModel
    {
        public Event Event { get; set; } = new Event(); // default instance
        public List<User> RegisteredUsers { get; set; } = new List<User>(); // empty list
    }
}
