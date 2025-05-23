using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace CocktailDebacle.Server.Service
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
    }

    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> cloudinaryConfig)
        {
            var account = new Account(
                cloudinaryConfig.Value.CloudName,
                cloudinaryConfig.Value.ApiKey,
                cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }
        
        public async Task<string?> UploadImageAsync(IFormFile file, string publicId)
        {
            if (file.Length == 0) return null;

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = publicId,
                Type = "authenticated",
                Overwrite = true,
                Invalidate = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl?.ToString();
        }

        public async Task<string?> UploadImageAsyncUrl(string url, string publicId)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(url),
                PublicId = publicId,
                Type = "authenticated",
                Overwrite = true,
                Invalidate = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl?.ToString();
        }

        public string GeneratePrivateImageUrl(string publicId, int expireSeconds = 3600)
        {
            return _cloudinary.Api.UrlImgUp
                .Secure(true)
                .Signed(true)
                .Type("authenticated")
                .Transform(new Transformation().Flags("authenticated"))
                .BuildUrl(publicId);
        }

        public async Task<string> DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image,
                Type = "authenticated"
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result;
        }
    }


}