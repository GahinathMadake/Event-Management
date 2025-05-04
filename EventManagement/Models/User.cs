using System.ComponentModel.DataAnnotations;

namespace EventManagement.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public required string FullName { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required]
        public string Role { get; set; } = "User"; // Default role

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            // Add this navigation property to complete the relationship
        public ICollection<Event> Events { get; set; } = new List<Event>();
        public ICollection<UserEvent> UserEvents { get; set; } = new List<UserEvent>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    }
}
