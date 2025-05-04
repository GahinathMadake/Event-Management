using System.ComponentModel.DataAnnotations;

namespace EventManagement.Models
{
    public class Review
    {
        public int Id { get; set; } // Review ID

        [Required]
        public string Author { get; set; } = string.Empty; // Name of the reviewer

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Content { get; set; } = string.Empty; // Content of the review

        [Required]
        [Range(1, 5)] // Rating between 1 and 5
        public int Rating { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow; // Date of the review

        // Foreign Key reference to User
        public int UserId { get; set; }  // The ID of the user who wrote the review
        public User? User { get; set; }  // The User object associated with the review
        
    }
}
