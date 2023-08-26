using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ImageManagement.Controllers
{
    public class FileRequest
    {
        public List<string> FileNames { get; set; }
    }

    public class PreSignedUrlResponse
    {
        public string FileName { get; set; }
        public string Url { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class AWSS3Controller : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;

        public AWSS3Controller(Lazy<Task<IAmazonS3>> s3Client)
        {
            _s3Client = s3Client.Value.Result;
        }

        [HttpPost("presignedUrls")]
        public IActionResult GeneratePresignedUrls([FromBody] FileRequest request)
        {
            if (request?.FileNames == null || !request.FileNames.Any())
            {
                return BadRequest("FileNames are required.");
            }

            var responseList = new List<PreSignedUrlResponse>();

            foreach (var fileName in request.FileNames)
            {
                var urlRequest = new GetPreSignedUrlRequest
                {
                    BucketName = "images-g-lytho",
                    Key = fileName,
                    Verb = HttpVerb.PUT,
                    Expires = DateTime.UtcNow.AddMinutes(10)  // will be valid for 10 min
                };

                var url = _s3Client.GetPreSignedURL(urlRequest);
                responseList.Add(new PreSignedUrlResponse
                {
                    FileName = fileName,
                    Url = url
                });
            }

            return Ok(responseList);
        }
    }
}
