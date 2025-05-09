using System.ComponentModel.DataAnnotations;

namespace EventManagement.Models
{
    public class Login
    {
        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
