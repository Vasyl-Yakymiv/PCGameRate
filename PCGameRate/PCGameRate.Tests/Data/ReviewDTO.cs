using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCGameRate.Tests.Data
{
    public class ReviewDto
    {
        public int GameId { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string FullName { get; set; }
        public string ReviewText { get; set; }
    }
}
