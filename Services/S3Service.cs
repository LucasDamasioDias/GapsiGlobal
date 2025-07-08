using Amazon;
using Amazon.S3;
using Amazon.S3.Model;


namespace GapsiMVC.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly RegionEndpoint _region;
        
        public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client; 
            _bucketName = configuration["AWS:BucketName"];
            var regionName = configuration["AWS:Region"];

            if (string.IsNullOrEmpty(_bucketName) || string.IsNullOrEmpty(regionName))
            {
                throw new InvalidOperationException("AWS:BucketName e AWS:Region precisam estar configurados no appsettings.json.");
            }

            _region = RegionEndpoint.GetBySystemName(regionName);
        }

        public async Task<string> UploadFileAsync(IFormFile formFile, string targetFolder)
        {           
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(formFile.FileName)}";
            
            var objectKey = $"{targetFolder}/{fileName}";

            using var stream = formFile.OpenReadStream();

            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                InputStream = stream,
                ContentType = formFile.ContentType      
            };

            var response = await _s3Client.PutObjectAsync(putRequest);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {                
                return $"https://{_bucketName}.s3.{_region.SystemName}.amazonaws.com/{objectKey}";
            }
            else
            {
                throw new Exception($"Erro ao fazer upload para o S3. Status: {response.HttpStatusCode}");
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return false;

            try
            {
                var uri = new Uri(fileUrl);               
                var objectKey = uri.AbsolutePath.TrimStart('/');

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = objectKey
                };

                var response = await _s3Client.DeleteObjectAsync(deleteRequest);
                return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {                
                Console.WriteLine($"Erro ao deletar arquivo do S3: {ex.Message}");
                return false;
            }
        }
    }
}