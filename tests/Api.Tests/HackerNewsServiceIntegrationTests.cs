using Api.Services;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Api.Tests
{
    public class HackerNewsServiceIntegrationTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<HackerNewsServiceClient> _logger;

        public HackerNewsServiceIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = XUnitLogger.CreateLogger<HackerNewsServiceClient>(_testOutputHelper);
        }

        [Fact]
        public async Task Should_Get_Best_Stories()
        {
            // Arrange
            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");

            var service = new HackerNewsServiceClient(client, _logger);

            // Act
            var data = await service.GetBestStoriesAsync();

            // Assert
            Assert.NotNull(data);
            Assert.NotEmpty(data);
        }

        [Fact]
        public async Task Should_Get_Item()
        {
            // Arrange
            var expectedId = 39653718;

            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");

            var service = new HackerNewsServiceClient(client, _logger);

            // Act
            var data = await service.GetItemAsync(expectedId);

            // Assert
            Assert.NotNull(data);
            Assert.Equal(expectedId, data.Id);
            // test some fields that should not change
            Assert.Equal("ulrischa", data.By);
            Assert.Equal("story", data.Type);
            Assert.Equal("Bruno: Fast and Git-friendly open-source API client (Postman alternative)", data.Title);
            Assert.Equal("https://www.usebruno.com/", data.Url);
            Assert.Equal(1710008996, data.Time);
        }

        [Fact]
        public async Task Should_Throw_Exception_When_HttpClient_Fails_On_GetBestStoriesAsync()
        {
            // Arrange
            var expectedException = new HttpRequestException("error");
            var handler = new FakeHttpMessageHandler(expectedException);

            using var client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");

            var service = new HackerNewsServiceClient(client, _logger);

            // Act
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetBestStoriesAsync());

            // Assert
            Assert.Equivalent(expectedException, exception);
        }

        [Fact]
        public async Task Should_Throw_Exception_When_HttpClient_Fails_On_GetItemAsync()
        {
            // Arrange
            var expectedException = new HttpRequestException("error");
            var handler = new FakeHttpMessageHandler(expectedException);

            using var client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");

            var service = new HackerNewsServiceClient(client, _logger);

            // Act
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetItemAsync(1));

            // Assert
            Assert.Equivalent(expectedException, exception);
        }
        public class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Exception _exception;

            public FakeHttpMessageHandler(Exception exception)
            {
                _exception = exception;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw _exception;
            }
        }
    }
}