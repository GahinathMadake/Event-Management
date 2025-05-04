using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventManagement.Models;

namespace EventManagement.Models
{
    public class UserEvent
    {
        [Key]
        public int Id { get; set; }

        // Replace this:
        // public string UserEmail { get; set; } = string.Empty;

        // With this:
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Registered";
    }
}
