using ImageManagement.Models;

namespace ImageManagement.Data.Repositories
{
    public interface IImageFileRepository
    {
        IEnumerable<ImageFile> GetAllImageFiles();
        ImageFile GetImageFileById(string id);
        void AddImageFile(ImageFile imageFile);
        void UpdateImageFile(ImageFile imageFile);
        void DeleteImageFileById(string id);
        bool UpdateImageFileDescription(string id, string description);

        void BulkInsert(IEnumerable<ImageFile> imageFiles);
    }
}
