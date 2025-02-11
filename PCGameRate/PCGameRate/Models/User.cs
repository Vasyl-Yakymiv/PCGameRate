using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PCGameRate.Models
{
    public class User : IdentityUser 
    {
   
        public string? ProfileImageUrl { get; set; }
        public string? FullName { get; set; }
        public ICollection<Review> Reviews { get; set; }

    }
}
