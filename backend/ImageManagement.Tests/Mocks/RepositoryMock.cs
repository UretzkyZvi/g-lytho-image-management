using ImageManagement.Data.Repositories;
using ImageManagement.Models;
using Moq;

namespace ImageManagement.Tests.Mocks
{
    public static class RepositoryMock
    {
        public static Mock<IImageFileRepository> GetImageFileRepositoryMock(List<ImageFile> imageFiles)
        {
            var mockRepository = new Mock<IImageFileRepository>();

            mockRepository.Setup(repo => repo.GetAllImageFiles())
                           .Returns(imageFiles);

            mockRepository.Setup(repo => repo.GetImageFileById(It.IsAny<string>()))
                .Returns((string id) => imageFiles.FirstOrDefault(img => img.Id == id));

            return mockRepository;
        }
    }
}
