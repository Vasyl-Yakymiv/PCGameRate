using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCGameRate.Tests.Data
{
    public class PopularGameDto
    {
        public int GameId { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string RatingAverage { get; set; }
        public string ReleaseYear { get; set; }
    }
}
