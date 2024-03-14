using Api.Interfaces;
using Api.Models;
using Api.Services;
using AutoFixture;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Api.Tests
{
    public class BestStoriesServiceTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly Mock<IHackerNewsServiceClient> _hackerNewsServiceMock;
        private readonly IFixture _fixture;
        private readonly BestStoriesService _service;

        public BestStoriesServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _hackerNewsServiceMock = new Mock<IHackerNewsServiceClient>();
            var logger = XUnitLogger.CreateLogger<CachedHackerNewsService>(_testOutputHelper);
            var cachedService = new CachedHackerNewsService(_hackerNewsServiceMock.Object, logger);
            _fixture = new Fixture();
            _service = new BestStoriesService(cachedService);
        }

        [Theory]
        [InlineData(15, 50)]
        [InlineData(50, 50)]
        [InlineData(100, 50)]
        public async Task Should_Get_N_Best_Stories(int n, int totalStories)
        {
            // Arrange
            var stories = _fixture.CreateMany<HackerNewsItemDto>(totalStories).ToDictionary(x => x.Id, x => x);

            var bestStoriesIds = stories.Keys.ToArray();

            _hackerNewsServiceMock.Setup(x => x.GetBestStoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(bestStoriesIds);

            _hackerNewsServiceMock.Setup(x => x.GetItemAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) => stories[id]);

            // Act
            var data = await _service.GetNBestStoriesAsync(n);

            // Assert
            Assert.NotNull(data);

            // prepare inspector for every item in returned collection
            var itemsInspector = new List<Action<BestStoryDto>>();

            foreach (var bestStoryDto in data)
            {
                itemsInspector.Add(_ => ElementInspector(bestStoryDto));
            }

            void ElementInspector(BestStoryDto actual)
            {
                var expectedItem = stories.Values.FirstOrDefault(x => x.Title == actual.Title);

                Assert.NotNull(expectedItem);
                Assert.Equal(expectedItem.Title, actual.Title);
                Assert.Equal(expectedItem.Url, actual.Uri);
                Assert.Equal(expectedItem.By, actual.PostedBy);
                Assert.Equal(expectedItem.Time, new DateTimeOffset(actual.Time).ToUnixTimeSeconds());
                Assert.Equal(expectedItem.Score, actual.Score);
                Assert.Equal(expectedItem.Kids?.Count, actual.CommentCount);
            }

            Assert.Collection(data, itemsInspector.ToArray());

            // check descending order
            for (var i = 0; i < data.Length - 1; i++)
            {
                Assert.True(data[i].Score >= data[i + 1].Score);
            }
        }
    }
}