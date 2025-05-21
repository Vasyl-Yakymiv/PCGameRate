using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PCGameRate.Interfaces;


namespace PCGameRate.Services
{
    public class CaptchaService : ICaptchaService
    {
        private readonly string _secretKey;
        private readonly HttpClient _httpClient;

        public CaptchaService(IConfiguration configuration)
        {
            _secretKey = "6Ldv_doqAAAAADp6eRH75Q_6m7twqK4G93i8a0Do";
            _httpClient = new HttpClient();
        }

        public virtual async Task<bool> IsCaptchaValid(string captchaResponse)
        {
            if (string.IsNullOrEmpty(captchaResponse))
                return false;

            var response = await _httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={_secretKey}&response={captchaResponse}", null);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonData = JObject.Parse(jsonResponse);
            return jsonData["success"]?.Value<bool>() ?? false;
        }
    }
}
