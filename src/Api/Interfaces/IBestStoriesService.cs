using Api.Models;

namespace Api.Interfaces
{
    public interface IBestStoriesService
    {
        Task<BestStoryDto[]> GetNBestStoriesAsync(int n, CancellationToken cancellationToken = default);
    }
}