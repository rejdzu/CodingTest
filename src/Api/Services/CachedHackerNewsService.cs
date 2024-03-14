using System.Collections.Concurrent;
using Api.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Caching.Memory;

namespace Api.Services
{
    public class CachedHackerNewsService : IHackerNewsService
    {
        private const string BestStoriesKey = "beststories";
        private const string GetItemKeyPrefix = "getitem";

        private readonly IHackerNewsServiceClient _hackerNewsService;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, Task<int[]>> _cachedBestStoriesUrls;
        private readonly ConcurrentDictionary<string, Task<HackerNewsItemDto>> _cachedItemsUrls;

        private readonly MemoryCache _itemsCache;
        private readonly TimeSpan _cacheInterval = TimeSpan.FromSeconds(30);

        public CachedHackerNewsService(IHackerNewsServiceClient hackerNewsService, ILogger<CachedHackerNewsService> logger)
        {
            _hackerNewsService = hackerNewsService;
            _logger = logger;

            _cachedBestStoriesUrls = new ConcurrentDictionary<string, Task<int[]>>();
            _cachedItemsUrls = new ConcurrentDictionary<string, Task<HackerNewsItemDto>>();
            _itemsCache = new MemoryCache(new MemoryCacheOptions());
        }

        public async Task<int[]> GetBestStoriesAsync(CancellationToken cancellationToken = default)
        {
            if (_cachedBestStoriesUrls.TryGetValue(BestStoriesKey, out var cachedBestStoriesData))
            {
                _logger.LogDebug("Returning cached task for {key}", BestStoriesKey);
                return await cachedBestStoriesData;
            }

            var task = _hackerNewsService.GetBestStoriesAsync(cancellationToken);
            _cachedBestStoriesUrls[BestStoriesKey] = task;
            
            var data = await task;
            _logger.LogDebug("Fetched value for {key}", BestStoriesKey);

            _cachedItemsUrls.Remove(BestStoriesKey, out var _);
            _logger.LogDebug("Removed cached value from urls for {key}", BestStoriesKey);
            return data;
        }

        public async Task<HackerNewsItemDto> GetItemAsync(int itemId, CancellationToken cancellationToken = default)
        {
            var key = $"{GetItemKeyPrefix}-{itemId}";

            if (_itemsCache.TryGetValue(key, out HackerNewsItemDto? cachedItem) && cachedItem != null)
            {
                _logger.LogDebug("Returning cached value for {key}", key);
                return cachedItem;
            }

            if (_cachedItemsUrls.TryGetValue(key, out var cachedItemData))
            {
                _logger.LogDebug("Returning cached task for {key}", key);
                return await cachedItemData;
            }

            var tcs = new TaskCompletionSource<HackerNewsItemDto>();
            _cachedItemsUrls[key] = tcs.Task;

            var data = await _hackerNewsService.GetItemAsync(itemId, cancellationToken);
            _logger.LogDebug("Fetched value for {key}", key);
            tcs.SetResult(data);

            _itemsCache.Set(key, data, _cacheInterval);
            _logger.LogDebug("Added item to cache for {key} for {time}", key, _cacheInterval);

            _cachedItemsUrls.Remove(key, out var _);
            _logger.LogDebug("Removed cached value from url for {key}", key);
            return data;
        }
    }
}
