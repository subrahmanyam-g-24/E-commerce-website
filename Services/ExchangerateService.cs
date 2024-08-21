using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Ecommerce_App_prac1.Services
{
    public class ExchangeRateService
    {
        private readonly HttpClient _client;
        private readonly string _apiKey;

        public ExchangeRateService(string apiKey)
        {
            _client = new HttpClient();
            _apiKey = apiKey;
        }

        public async Task<JObject> GetLatestRatesAsync(string baseCurrency)
        {
            string baseUrl = "https://v6.exchangerate-api.com/v6/";
            string endpoint = $"{baseUrl}{_apiKey}/latest/{baseCurrency}";

            HttpResponseMessage response = await _client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            return JObject.Parse(responseBody);
        }
    }
}
