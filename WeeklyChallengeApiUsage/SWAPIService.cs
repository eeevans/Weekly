using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WeeklyChallengeApiUsage
{
    public class SwapiService
    {
        private readonly HttpClient _client;
        private readonly Dictionary<int, People> _cache;

        public SwapiService(HttpClient client)
        {
            _client = client;
            _cache = new Dictionary<int, People>();
        }

        public async Task<ISwapiServiceResponse> GetPersonById(int id)
        {
            var jsonSerializer = new JsonSerializer();

            try
            {
                if (_cache.ContainsKey(id))
                    return SwapiServiceResponse.Ok(_cache[id]);

                var request = CreateRequest(id);
                var response = await _client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"{response.StatusCode} - {response.ReasonPhrase}");

                await using var responseStream = await response.Content.ReadAsStreamAsync();
                using var streamReader = new StreamReader(responseStream);
                using var jsonTextReader = new JsonTextReader(streamReader);
                var people = jsonSerializer.Deserialize<People>(jsonTextReader);
                _cache.Add(id, people);
                return SwapiServiceResponse.Ok(people);
            }
            catch (Exception e)
            {
                return SwapiServiceResponse.Error(e);
            }
        }

        private HttpRequestMessage CreateRequest(int id)
        {
            return new HttpRequestMessage(HttpMethod.Get, $"{id}/");
        }
    }

    public interface ISwapiServiceResponse
    {
        bool Success { get; set; }
        People ServicePeople { get; set; }
        Exception ServiceException { get; set; }
    }

    public class SwapiServiceResponse : ISwapiServiceResponse
    {
        public static SwapiServiceResponse Ok(People people)
        {
            return new SwapiServiceResponse() { Success = true, ServicePeople = people };
        }

        public static SwapiServiceResponse Error(Exception ex)
        {
            return new SwapiServiceResponse() { Success = false, ServiceException = ex };
        }

        public bool Success { get; set; }
        public People ServicePeople { get; set; }
        public Exception ServiceException { get; set; }
    }

    public class People
    {
        [JsonProperty(PropertyName="name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; set; }
    }
}