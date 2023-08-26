using Amazon.S3.Model;
using Amazon.S3;
using ImageManagement.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ImageManagement.Tests.Controllers
{
    public class S3ControllerTests
    {
        private readonly Mock<IAmazonS3> _mockS3Client;
        private readonly AWSS3Controller _controller;

        public S3ControllerTests()
        {
            _mockS3Client = new Mock<IAmazonS3>();
            var lazyS3Client = new Lazy<Task<IAmazonS3>>(() => Task.FromResult(_mockS3Client.Object));

            _controller = new AWSS3Controller(lazyS3Client);
        }

        [Fact]
        public void GeneratePresignedUrls_ReturnsListOfUrls()
        {
            // Arrange
            var fileNames = new List<string> { "file1.jpg", "file2.jpg" };
            _mockS3Client.Setup(s3 => s3.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
                         .Returns((GetPreSignedUrlRequest req) => $"https://fakeurl/{req.Key}");

            // Act
            var result = _controller.GeneratePresignedUrls(new FileRequest { FileNames = fileNames });

            // Assert
            var okResult = result as OkObjectResult;
            var response = okResult.Value as List<PreSignedUrlResponse>;
            Assert.Equal(fileNames.Count, response.Count);
            Assert.True(response.All(r => fileNames.Contains(r.FileName)));
        }

        [Fact]
        public void GeneratePresignedUrls_EmptyFileNames_ReturnsBadRequest()
        {
            // Act
            var result = _controller.GeneratePresignedUrls(new FileRequest { FileNames = new List<string>() });

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal("FileNames are required.", badRequestResult.Value);
        }
    }
}
