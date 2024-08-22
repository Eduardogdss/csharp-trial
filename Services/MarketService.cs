using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MeuPrograma
{
    public class MarketService
    {
        private static readonly HttpClient _httpClient;

        static MarketService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<float?> GetRegularMarketPriceAsync(string active)
        {
            string token = Environment.GetEnvironmentVariable("API_TOKEN") ?? string.Empty;
            string baseUrl = $"https://brapi.dev/api/quote/{active}?token={token}";
            try
            {
                HttpResponseMessage res = await _httpClient.GetAsync(baseUrl);
                res.EnsureSuccessStatusCode();
                string data = await res.Content.ReadAsStringAsync();
                var dataObj = JObject.Parse(data);
                var regularMarketPriceToken = dataObj["results"]?[0]?["regularMarketPrice"] ?? null;
                return regularMarketPriceToken?.Value<float>();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro ao obter o preço do mercado regular: {e.Message}");
                return null;
            }
        }
    }
}
