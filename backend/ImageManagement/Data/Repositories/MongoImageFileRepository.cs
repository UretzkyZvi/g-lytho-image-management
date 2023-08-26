using ImageManagement.Models;
using MongoDB.Driver;

namespace ImageManagement.Data.Repositories
{
    public class MongoImageFileRepository : IImageFileRepository
    {
        private readonly MongoDbContext _context;

        public MongoImageFileRepository(MongoDbContext context)
        {
            _context = context;
        }

        public IEnumerable<ImageFile> GetAllImageFiles()
        {
            return _context.ImageFiles.Find(_ => true).ToList();
        }

        public ImageFile GetImageFileById(string id)
        {
            return _context.ImageFiles.Find(file => file.Id == id).FirstOrDefault();
        }

        public void AddImageFile(ImageFile imageFile)
        {
            _context.ImageFiles.InsertOne(imageFile);
        }

        public void UpdateImageFile(ImageFile imageFile)
        {
            _context.ImageFiles.ReplaceOne(file => file.Id == imageFile.Id, imageFile);
        }
        public bool UpdateImageFileDescription(string id, string description)
        {
            var filter = Builders<ImageFile>.Filter.Eq(f => f.Id, id);
            var update = Builders<ImageFile>.Update.Set(f => f.Description, description);

            var updateResult = _context.ImageFiles.UpdateOne(filter, update);
            return updateResult.ModifiedCount > 0;
        }

        public void DeleteImageFileById(string id)
        {
            _context.ImageFiles.DeleteOne(file => file.Id == id);
        }
        public void BulkInsert(IEnumerable<ImageFile> imageFiles)
        {
            _context.ImageFiles.InsertMany(imageFiles);
        }
    }
}
