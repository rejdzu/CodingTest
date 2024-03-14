using Api.Interfaces;
using Api.Models;

namespace Api.Services
{
    public class HackerNewsService : IHackerNewsServiceClient
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public HackerNewsService(HttpClient client, ILogger<HackerNewsService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<int[]> GetBestStoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _client.GetFromJsonAsync<int[]>("v0/beststories.json", cancellationToken);
                return result;
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
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while fetching item with {id}", itemId);
                throw;
            }
        }
    }
}