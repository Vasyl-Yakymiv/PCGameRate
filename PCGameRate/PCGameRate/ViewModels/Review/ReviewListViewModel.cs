namespace PCGameRate.ViewModels.Review
{
    public class ReviewListViewModel
    {
        public int? GameId { get; set; }
        public string GameTitle { get; set; }
        public string GameImage { get; set; }
        public string ReviewText { get; set; }
        public DateTime? DatePosted { get; set; }
    }
}
