using Api.Interfaces;
using Api.Models;
using Api.Services;
using AutoFixture;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Api.Tests
{
    public class CachedHackerNewsServiceDecoratorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly Mock<IHackerNewsServiceClient> _mockHackerNewsService;
        private readonly CachedHackerNewsServiceDecorator _service;
        private readonly Fixture _fixture;

        public CachedHackerNewsServiceDecoratorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _mockHackerNewsService = new Mock<IHackerNewsServiceClient>();
            var logger = XUnitLogger.CreateLogger<CachedHackerNewsServiceDecorator>(_testOutputHelper);
            _service = new CachedHackerNewsServiceDecorator(_mockHackerNewsService.Object, logger);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task Should_Get_Best_Stories()
        {
            // Arrange
            var expectedData = new[] { 1, 2, 3 };

            _mockHackerNewsService.Setup(x => x.GetBestStoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expectedData);

            // Act
            var result = await _service.GetBestStoriesAsync();

            // Assert
            Assert.Equivalent(expectedData, result);
        }

        [Fact]
        public async Task Should_Get_Item()
        {
            // Arrange
            var expectedData = _fixture.Create<HackerNewsItemDto>();

            _mockHackerNewsService.Setup(x => x.GetItemAsync(expectedData.Id, It.IsAny<CancellationToken>())).ReturnsAsync(expectedData);

            // Act
            var result = await _service.GetItemAsync(expectedData.Id);

            // Assert
            Assert.Equivalent(expectedData, result);
        }

        [Fact]
        public async Task Should_Coalesce_Same_Requests_For_Get_Best_Stories()
        {
            // Arrange
            var expectedData = new[] { 1, 2, 3 };

            _mockHackerNewsService.Setup(x => x.GetBestStoriesAsync(It.IsAny<CancellationToken>())).Returns(async () =>
            {
                await Task.Delay(10);
                return expectedData;
            });

            // Act
            var tasks = Enumerable.Range(0, 100).Select(x => _service.GetBestStoriesAsync()).ToArray();
            var result = await Task.WhenAll(tasks);

            // Assert
            _mockHackerNewsService.Verify(x => x.GetBestStoriesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.All(result, ints => Assert.Equivalent(expectedData, ints));
        }

        [Fact]
        public async Task Should_Coalesce_Same_Requests_For_Get_Item()
        {
            // Arrange
            var expectedData = _fixture.Create<HackerNewsItemDto>();

            _mockHackerNewsService.Setup(x => x.GetItemAsync(expectedData.Id, It.IsAny<CancellationToken>())).Returns(async (int id, CancellationToken _) =>
            {
                await Task.Delay(10);
                return expectedData;
            });

            // Act
            var tasks = Enumerable.Range(0, 100).Select(x => _service.GetItemAsync(expectedData.Id)).ToArray();
            var result = await Task.WhenAll(tasks);

            _mockHackerNewsService.Verify(x => x.GetItemAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.All(result, item => Assert.Equivalent(expectedData, item));
        }

        [Fact]
        public async Task Should_Get_Exception_When_HackerNewsService_Throws_On_GetBestStoriesAsync()
        {
            // Arrange
            var expectedException = new HttpRequestException("error");
            _mockHackerNewsService.Setup(x => x.GetBestStoriesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetBestStoriesAsync());

            // Assert
            Assert.Equivalent(expectedException, exception);
        }

        [Fact]
        public async Task Should_Get_Exception_When_HackerNewsService_Throws_On_GetItemAsync()
        {
            // Arrange
            var expectedException = new HttpRequestException("error");
            _mockHackerNewsService.Setup(x => x.GetItemAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetItemAsync(1));

            // Assert
            Assert.Equivalent(expectedException, exception);
        }
    }
}