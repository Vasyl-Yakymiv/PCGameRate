namespace PCGameRate.Interfaces
{
    public interface IYouTubeService
    {
        Task<string?> GetTopLiveStreamEmbedAsync(string gameTitle);
    }
}
