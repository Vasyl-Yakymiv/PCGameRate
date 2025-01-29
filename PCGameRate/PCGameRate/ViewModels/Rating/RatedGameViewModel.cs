namespace PCGameRate.ViewModels.Rating
{
    public class RatedGameViewModel
    {
        public int? GameId { get; set; }
        public string GameTitle { get; set; }
        public string GameImage { get; set; }
        public string ReleaseDate { get; set; }
        public float? RatingAverage { get; set; }
        public int? RatingValue { get; set; }
        public string Genre { get; set; }
        public string Developer { get; set; }
        public DateTime? RatingDate { get; set; }
    }
}
