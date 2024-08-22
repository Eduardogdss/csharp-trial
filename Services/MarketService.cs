using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MeuPrograma
{
    public class MarketService
    {
        public async Task<float?> GetRegularMarketPriceAsync(string active)
        {
            string token = Environment.GetEnvironmentVariable("API_TOKEN");
            string baseUrl = $"https://brapi.dev/api/quote/{active}?token={token}";
            Console.WriteLine($"Consultando a URL: {baseUrl}");
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage res = await client.GetAsync(baseUrl);
                    res.EnsureSuccessStatusCode();
                    string data = await res.Content.ReadAsStringAsync();
                    var dataObj = JObject.Parse(data);
                    var regularMarketPriceToken = dataObj["results"]?[0]?["regularMarketPrice"];
                    return regularMarketPriceToken?.Value<float>();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro: {e.Message}");
                return null;
            }
        }
    }
}
