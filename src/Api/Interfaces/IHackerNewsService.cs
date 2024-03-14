using Api.Models;

namespace Api.Interfaces
{
    public interface IHackerNewsService
    {
        Task<int[]> GetBestStoriesAsync(CancellationToken cancellationToken = default);
        Task<HackerNewsItemDto> GetItemAsync(int itemId, CancellationToken cancellationToken = default);
    }
}