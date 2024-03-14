using System.Net;
using Api.Controllers;
using Api.Interfaces;
using Api.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Api.Tests
{
    public class BestStoriesControllerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly Mock<IBestStoriesService> _bestStoriesServiceMock;
        private readonly BestStoriesController _controller;
        private readonly Fixture _fixture;

        public BestStoriesControllerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _bestStoriesServiceMock = new Mock<IBestStoriesService>();
            var logger = XUnitLogger.CreateLogger<BestStoriesController>(_testOutputHelper);
            _controller = new BestStoriesController(_bestStoriesServiceMock.Object, logger);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task Should_Get_Best_N_Stories()
        {
            // Arrange
            var n = 50;
            var stories = _fixture.CreateMany<BestStoryDto>(n).ToArray();

            _bestStoriesServiceMock.Setup(x => x.GetNBestStoriesAsync(n, It.IsAny<CancellationToken>()))
                .ReturnsAsync(stories);

            // Act
            var result = await _controller.GetNBestStoriesAsync(n);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            var data = result.Value.ToArray();
            Assert.NotEmpty(data);

            var itemsInspector = new List<Action<BestStoryDto>>();

            foreach (var bestStoryDto in data)
            {
                itemsInspector.Add(_ => ElementInspector(bestStoryDto));
            }

            void ElementInspector(BestStoryDto actual)
            {
                var expectedItem = stories.FirstOrDefault(x => x.Title == actual.Title);

                Assert.NotNull(expectedItem);
                Assert.Equal(expectedItem.Title, actual.Title);
                Assert.Equal(expectedItem.Uri, actual.Uri);
                Assert.Equal(expectedItem.PostedBy, actual.PostedBy);
                Assert.Equal(expectedItem.Time, actual.Time);
                Assert.Equal(expectedItem.Score, actual.Score);
                Assert.Equal(expectedItem.CommentCount, actual.CommentCount);
            }

            Assert.Collection(data, itemsInspector.ToArray());
        }

        [Fact]
        public async Task Should_Return_500_StatusCode_When_Service_Throws()
        {
            // Arrange
            _bestStoriesServiceMock.Setup(x => x.GetNBestStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _controller.GetNBestStoriesAsync(1);

            // Assert
            Assert.Null(result.Value);
            Assert.NotNull(result.Result);
            var statusCodeResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            Assert.NotNull(statusCodeResult);
            Assert.NotNull(statusCodeResult.Value);
            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal("An error occured while processing your request.", statusCodeResult.Value.ToString());
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(500)]
        public async Task Should_Return_400_StatusCode_When_Query_Parameter_Is_Invalid(int value)
        {
            // Act
            var result = await _controller.GetNBestStoriesAsync(value);

            // Assert
            Assert.Null(result.Value);
            Assert.NotNull(result.Result);
            var statusCodeResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            Assert.NotNull(statusCodeResult);
            Assert.NotNull(statusCodeResult.Value);
            Assert.Equal((int)HttpStatusCode.BadRequest, statusCodeResult.StatusCode);
            Assert.Equal("Parameter n has to be between 0 and 200.", statusCodeResult.Value.ToString());
        }
    }
}