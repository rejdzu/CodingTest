using System.Collections.Concurrent;
using Api.Interfaces;
using Api.Models;

namespace Api.Services
{
    public class BestStoriesService : IBestStoriesService
    {
        private const int MaxParallelItemsFetch = 10;
        private readonly IHackerNewsService _hackerNewsService;

        public BestStoriesService(IHackerNewsService hackerNewsService)
        {
            _hackerNewsService = hackerNewsService;
        }

        public async Task<BestStoryDto[]> GetNBestStoriesAsync(int n, CancellationToken cancellationToken = default)
        {
            var bestStoriesIds = await _hackerNewsService.GetBestStoriesAsync(cancellationToken);

            var count = Math.Min(n, bestStoriesIds.Length);

            var result = new ConcurrentBag<BestStoryDto>();

            await Parallel.ForEachAsync(bestStoriesIds.Take(count),
                new ParallelOptions { MaxDegreeOfParallelism = MaxParallelItemsFetch, CancellationToken = cancellationToken },
                async (i, token) =>
                {
                    if (token.IsCancellationRequested)
                    {
                        throw new OperationCanceledException();
                    }

                    var item = await _hackerNewsService.GetItemAsync(i, cancellationToken);
                    result.Add(Map(item));
                });

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