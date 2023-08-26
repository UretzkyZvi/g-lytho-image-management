namespace ImageManagement.Models
{
    public class FileUploadedItem {
        public string FileName { get; set; }
        public string S3Location { get; set; }
    }

    public class PostUploadInfoModel
    {
       public FileUploadedItem[] FileUploadedItems { get; set; }
 
    }
}
