using System.ComponentModel.DataAnnotations;

namespace CustomerOrderApi.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

    }
}
