using Amazon.S3;
using Amazon.S3.Model;
using ImageManagement.Controllers;
using ImageManagement.Data.Repositories;
using ImageManagement.Models;
using ImageManagement.Tests.Mocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ImageManagement.Tests.Controllers
{
    public class ImageFilesControllerTests
    {
        private readonly Mock<IImageFileRepository> _mockRepository;
        private readonly Mock<IAmazonS3> _mockS3Client;
        private readonly Mock<ILogger<ImageFilesController>> _mockLogger;
        private readonly ImageFilesController _controller;

        public ImageFilesControllerTests()
        {
            var imageFiles = new List<ImageFile>
            {
                new ImageFile { Name = "Test1", Url = "url1" },
                new ImageFile { Name = "Test2", Url = "url2" }
            };

            _mockRepository = RepositoryMock.GetImageFileRepositoryMock(imageFiles);
            _mockS3Client = new Mock<IAmazonS3>();
            var lazyS3Client = new Lazy<Task<IAmazonS3>>(() => Task.FromResult(_mockS3Client.Object));

            _mockLogger = new Mock<ILogger<ImageFilesController>>();
            _controller = new ImageFilesController(_mockRepository.Object, lazyS3Client, _mockLogger.Object);
        }

        [Fact]
        public void GetImages_ReturnsCorrectNumberOfImages()
        {
            // Arrange & Act 
            var result = _controller.GetImages();
            // Assert
            var okResult = result as OkObjectResult;
            var value = okResult.Value as ImageFilesResponse;

            Assert.Equal(2, value.Data.Count);
        }
        [Fact]
        public async Task PostUploadInformationAsync_ReturnsSuccessMessage()
        {
            // Arrange
            var fileItem = new FileUploadedItem { FileName = "TestFile1", S3Location = "s3://location1/" };
            var fileItesms = new List<FileUploadedItem>();
            fileItesms.Add(fileItem);
            var postUploadInfoModel = new PostUploadInfoModel
            {
                FileUploadedItems = fileItesms.ToArray(),
            };

            // Mock S3 response
            _mockS3Client.Setup(s3 => s3.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                         .ReturnsAsync(new GetObjectResponse());

            // Act
            var result = await _controller.PostUploadInformationAsync(postUploadInfoModel);

            // Assert
            var okResult = result as OkObjectResult;
            var output = okResult?.Value as PostUploadResponse;
            Assert.Equal(true, output?.IsSuccessfully);
        }

        [Fact]
        public void UpdateImageAsync_FileNotExists_ReturnsNotFound()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetImageFileById(It.IsAny<string>())).Returns((ImageFile)null);

            // Act
            var result = _controller.UpdateImageAsync("someId", new ImageFileUpdateModel { Description = "Updated" });

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteImageAsync_FileNotExists_ReturnsNotFound()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetImageFileById(It.IsAny<string>())).Returns((ImageFile)null);

            // Act
            var result = await _controller.DeleteImageAsync("someId");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
