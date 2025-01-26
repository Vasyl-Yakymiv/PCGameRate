using System.ComponentModel.DataAnnotations;

namespace PCGameRate.Models
{
    public class Developer
    {
        [Key]
        public int DeveloperId { get; set; }
        public string DeveloperName { get; set; }
        public string? WebSite { get; set; }
    }
}
