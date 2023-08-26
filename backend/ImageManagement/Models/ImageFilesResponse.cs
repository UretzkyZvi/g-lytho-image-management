namespace ImageManagement.Models
{
    public class ImageFilesResponse
    {
        public  List<ImageFileWithSignedUrl> Data { get; set; } 
        public int CurrentPage { get; set; }
        public double TotalPages { get; set; }
        public string? NextPageLink { get; set; }
        public int Limit { get; set; }
        public int TotalRecords { get; set; }
    }
}
