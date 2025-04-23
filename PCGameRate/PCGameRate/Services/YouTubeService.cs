using System.Text.Json;

namespace PCGameRate.Services
{
    public class YouTubeService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "AIzaSyBqoSsCOgWCns4FZIG21Ty-3rzFECQRJQo";

        public YouTubeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> GetTopLiveStreamEmbedAsync(string gameTitle)
        {
            var requestUri = $"https://www.googleapis.com/youtube/v3/search" +
                     $"?part=snippet" +
                     $"&q={Uri.EscapeDataString(gameTitle)}" +
                     $"&eventType=live" +
                     $"&type=video" +
                     $"&videoCategoryId=20" +
                     $"&order=viewCount" +
                     $"&key={_apiKey}";

            var response = await _httpClient.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);

            var items = json.RootElement.GetProperty("items");

            if (items.GetArrayLength() == 0)
                return null; 

            var firstItem = items.EnumerateArray().First();

            if (!firstItem.TryGetProperty("id", out var idElement) ||
                !idElement.TryGetProperty("videoId", out var videoIdElement))
                return null; 

            var videoId = videoIdElement.GetString();

            if (string.IsNullOrEmpty(videoId))
                return null;

            return $"<iframe width=\"600\" height=\"400\" src=\"https://www.youtube.com/embed/{videoId}\" frameborder=\"0\" allowfullscreen></iframe>";
        }

    }
}
