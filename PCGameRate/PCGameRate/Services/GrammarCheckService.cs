using Newtonsoft.Json.Linq;
using PCGameRate.Interfaces;

namespace PCGameRate.Services
{ 
    public class GrammarCheckService : IGrammarCheckService
    {
        private readonly HttpClient _httpClient;
        public GrammarCheckService( HttpClient httpClient)
        {
                _httpClient = httpClient;
        }
        public async Task<List<string>> CheckGrammarAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

           

            var form = new Dictionary<string, string>
            {
                { "text", text },
                { "language", "uk" }
            };

            var content = new FormUrlEncodedContent(form);
            var response = await _httpClient.PostAsync("https://api.languagetool.org/v2/check", content);
            var json = await response.Content.ReadAsStringAsync();

            var matches = JObject.Parse(json)["matches"];
            var errors = new List<string>();

            foreach (var match in matches)
            {
                errors.Add(match["message"]?.ToString());
            }

            return errors;
        }
    }
}
