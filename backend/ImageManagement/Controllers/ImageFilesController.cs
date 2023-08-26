using Amazon.S3;
using Amazon.S3.Model;
using ImageManagement.Data;
using ImageManagement.Models;
using ImageManagement.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Linq;
using ImageManagement.Data.Repositories;
using Bogus;

namespace ImageManagement.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageFilesController : ControllerBase
    {
        private readonly IImageFileRepository _repository;
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger _logger;

        public ImageFilesController(IImageFileRepository repository, Lazy<Task<IAmazonS3>> s3Client, ILogger<ImageFilesController> logger)
        {
            _repository = repository;
            _s3Client = s3Client.Value.ConfigureAwait(false).GetAwaiter().GetResult();
            _logger = logger;
        }

        [HttpGet]
        public IActionResult  GetImages(int page = 1, int limit = 10, string sortBy = "Name", string order = "asc")
        {
            var totalFiles = _repository.GetAllImageFiles().Count();
            // Calculate the number of documents to skip
            int skip = (page - 1) * limit;

            var filteredFiles = _repository.GetAllImageFiles();

            // Sort
            if (order.ToLower() == "desc")
            {
                switch (sortBy)
                {
                    case "Name":
                        filteredFiles = filteredFiles.OrderByDescending(file => file.Name);
                        break;
                    case "Date":
                        filteredFiles = filteredFiles.OrderByDescending(file => file.UpdatedAt);
                        break;
                    // ... add other cases as necessary
                    default:
                        throw new ArgumentException($"Unknown sort field: {sortBy}");
                }
            }
            else
            {
                switch (sortBy)
                {
                    case "Name":
                        filteredFiles = filteredFiles.OrderBy(file => file.Name);
                        break;
                    case "Date":
                        filteredFiles = filteredFiles.OrderBy(file => file.UpdatedAt);
                        break;
                    // ... add other cases as necessary
                    default:
                        throw new ArgumentException($"Unknown sort field: {sortBy}");
                }
            }

            var totalRecords = filteredFiles.Count();

            var paginatedFiles = filteredFiles.Skip(skip).Take(limit).ToList();

            var results = paginatedFiles.Select(file =>
            {
                var signedUrl = GenerateSignedUrlForFile(file.Name);
                return new ImageFileWithSignedUrl
                {
                    ImageFile = file,
                    SignedUrl = signedUrl
                };
            }).ToList();

            string? nextPageLink = null;
            if (page * limit < totalFiles)
            {
                nextPageLink = Url.ActionLink(nameof(GetImages), "ImageFiles", new { page = page + 1, limit = limit, sortBy = sortBy, order = order });
            }

            return Ok(new ImageFilesResponse
            {
                Data = results,
                TotalRecords=totalRecords,
                CurrentPage = page,
                TotalPages = Math.Ceiling((double)totalRecords / limit),
                Limit=limit,
                NextPageLink=nextPageLink
            });
        }

        [HttpPost("postUpload")]
        public async Task<IActionResult> PostUploadInformationAsync([FromBody] PostUploadInfoModel model)
        {
            _logger.Log(LogLevel.Information, "Post upload process begin");
            var tasks = model.FileUploadedItems.Select(async FileUploadedItem =>
            {
                try
                {
                    // 1 get the file from s3
                    GetObjectRequest request = new GetObjectRequest
                    {
                        BucketName = "images-g-lytho",
                        Key = FileUploadedItem.FileName
                    };

                    using GetObjectResponse response = await _s3Client.GetObjectAsync(request);
                    using Stream responseStream = response.ResponseStream;
                    MemoryStream memoryStream = new MemoryStream();
                    await responseStream.CopyToAsync(memoryStream);

                    // Get file size from the ContentLength property
                    long fileSizeInBytes = response.ContentLength;
                    double fileSizeInMB = ImageUtilities.BytesToMB(fileSizeInBytes);

                    // 2 extract metadata
                    var metadata = await ImageUtilities.GetMetaDataFromStreamAsync(memoryStream);

                    // 3 store the received data in the MongoDB
                    var fakeUploderName = new Faker().Person.FullName;
                    var fakeDescription = new Faker().Commerce.ProductDescription();
                    return new ImageFile
                    {
                        Name = FileUploadedItem.FileName,
                        Url = FileUploadedItem.S3Location,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        UploadedBy = fakeUploderName,
                        Description = $"{fakeDescription}:: It's a fake description, please change me!",
                        Metadata = new MetaData
                        {
                            Height = metadata.Height,
                            Width = metadata.Width,
                            PixelType = metadata.PixelType
                        },
                        Size = fileSizeInMB

                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing file {FileUploadedItem.FileName}: {ex.Message}");
                    return null;  // or handle the error as appropriate for your application
                }
            }).ToList();

            var imageFiles = await Task.WhenAll(tasks);
            _logger.Log(LogLevel.Information, "Post upload process ended");

            _repository.BulkInsert(imageFiles.Where(file => file != null));

            return Ok(new PostUploadResponse { Message = "Information processed successfully!", IsSuccessfully = true });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateImageAsync(string id, [FromBody] ImageFileUpdateModel updateModel)
        {
            try
            {
                // 1. Check if the file exists in the repository
                var fileInDb = _repository.GetImageFileById(id);
                if (fileInDb == null)
                {
                    return NotFound(new { message = "File not found in database." });
                }

                // 2. Update the file's properties using the repository
                var isUpdated = _repository.UpdateImageFileDescription(id, updateModel.Description);

                if (!isUpdated)
                {
                    throw new Exception("Failed to update the file description.");
                }

                return Ok(new { message = "File updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating file id {id}: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImageAsync(string id)
        {
            try
            {
                // 1. Check if the file exists in the MongoDB database
                var fileInDb = _repository.GetImageFileById(id);
                if (fileInDb == null)
                {
                    return NotFound(new { message = "File not found in database." });
                }

                // 2. Delete the file from Amazon S3
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = "images-g-lytho",
                    Key = fileInDb.Name
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);

                // 3. Delete the file reference from MongoDB
                _repository.DeleteImageFileById(id);

                return Ok(new { message = "File deleted successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting file id {id}: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        private string GenerateSignedUrlForFile(string fileName)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = "images-g-lytho",
                Key = fileName,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(10) // or however long you want the URL to be valid for
            };

            return _s3Client.GetPreSignedURL(request);
        }
    }
}
