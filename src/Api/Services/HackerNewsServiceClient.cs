using System.Text.Json;
using Api.Interfaces;
using Api.Models;

namespace Api.Services
{
    public class HackerNewsServiceClient : IHackerNewsServiceClient
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public HackerNewsServiceClient(HttpClient client, ILogger<HackerNewsServiceClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<int[]> GetBestStoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _client.GetFromJsonAsync<int[]>("v0/beststories.json", cancellationToken);
                return result ?? throw new JsonException("Failed to deserialize best stories");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while fetching best stories");
                throw;
            }
        }

        public async Task<HackerNewsItemDto> GetItemAsync(int itemId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _client.GetFromJsonAsync<HackerNewsItemDto>($"v0/item/{itemId}.json", cancellationToken);
                return result ?? throw new JsonException($"Failed to deserialize item with id {itemId}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while fetching item with {id}", itemId);
                throw;
            }
        }
    }
}