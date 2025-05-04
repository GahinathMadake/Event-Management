using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Models
{
    public class Event
    {
        [Key]  // Primary Key
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        public int Capacity { get; set; }  // Number of people who can attend

         // 🔹 Add these 2 lines
         public int UserId { get; set; } // Foreign Key to User
         public User? User { get; set; }  // Navigation Property

         // Add this to complete the many-to-many relationship
         public ICollection<UserEvent> UserEvents { get; set; } = new List<UserEvent>();
    }
}
