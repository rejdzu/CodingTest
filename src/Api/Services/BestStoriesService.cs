using Api.Interfaces;
using Api.Models;

namespace Api.Services
{
    public class BestStoriesService : IBestStoriesService
    {
        private readonly IHackerNewsService _hackerNewsService;

        public BestStoriesService(IHackerNewsService hackerNewsService)
        {
            _hackerNewsService = hackerNewsService;
        }

        public async Task<BestStoryDto[]> GetNBestStoriesAsync(int n, CancellationToken cancellationToken = default)
        {
            var bestStoriesIds = await _hackerNewsService.GetBestStoriesAsync(cancellationToken);

            var count = Math.Min(n, bestStoriesIds.Length);

            var result = new BestStoryDto[count];

            for (var i = 0; i < count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                var item = await _hackerNewsService.GetItemAsync(bestStoriesIds[i], cancellationToken);
                result[i] = Map(item);
            }

            return result.OrderByDescending(x => x.Score).ToArray();
        }

        private static BestStoryDto Map(HackerNewsItemDto item)
        {
            return new BestStoryDto
            {
                Title = item.Title,
                Uri = item.Url,
                PostedBy = item.By,
                Time = DateTimeOffset.FromUnixTimeSeconds(item.Time).UtcDateTime,
                Score = item.Score ?? 0,
                CommentCount = item.Kids?.Count ?? 0
            };
        }
    }
}