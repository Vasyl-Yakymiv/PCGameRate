using System.ComponentModel.DataAnnotations;

namespace PCGameRate.Models
{
    public class User
    {
        [Key]
        public string UserId { get; set; }
        public string UserName { get; set; } 
    }
}
