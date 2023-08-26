using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Bogus;
using ImageManagement.Models;
using ImageManagement.Utilities;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ImageManagement.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        public IMongoCollection<ImageFile> ImageFiles => _database.GetCollection<ImageFile>("ImageFiles");
        public MongoDbContext(Lazy<Task<IMongoDatabase>> databaseTask)
        {
            _database = databaseTask.Value.Result;
        
        }
        public async Task InitializeAsync()
        {
            await SetupIndexesAsync();
        }
        private async Task SetupIndexesAsync()
        {
            var collection = ImageFiles;
            var indexKeys = Builders<ImageFile>.IndexKeys.Ascending(img => img.Name);
            collection.Indexes.CreateOne(new CreateIndexModel<ImageFile>(indexKeys));
            if (collection.CountDocuments(new BsonDocument()) <=2)
            {
                var imageUrl = new Faker().Image.PicsumUrl();
                var metadata = await ImageUtilities.GetMetaDataFromURLAsync(imageUrl);
                var faker = new Faker<ImageFile>()
                            .RuleFor(o => o.Id, f => ObjectId.GenerateNewId().ToString())
                            .RuleFor(o=>o.Name, f => f.Name.Random.RandomLocale())
                            .RuleFor(o => o.Size, f => f.Random.Double(1, 1000))
                            .RuleFor(o => o.CreatedAt, f => f.Date.Past())
                            .RuleFor(o => o.UpdatedAt, f => f.Date.Recent())
                            .RuleFor(o => o.UploadedBy, f => f.Name.FullName()) 
                            .RuleFor(o => o.Description, f => f.Lorem.Sentence());

                var imageFile = faker.Generate();
                imageFile.Url= imageUrl;
                imageFile.Metadata= new MetaData { 
                    Height = metadata.Height,
                    Width = metadata.Width,
                    PixelType = metadata.PixelType                  
                };

                collection.InsertOne(imageFile);
            }
           
        }


    }
}
