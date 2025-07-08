namespace GapsiMVC.Services
{
    public interface IS3Service
    {
        Task<string> UploadFileAsync(IFormFile formFile, string targetFolder);

        Task<bool> DeleteFileAsync(string fileUrl);
    }
}