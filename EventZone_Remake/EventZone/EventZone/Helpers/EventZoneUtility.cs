using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using EventZone.Models;
namespace EventZone.Helpers
{
    public class EventZoneUtility : SingletonBase<EventZoneUtility>
    {
        private EventZoneUtility() { }
        public string HashPassword(string password)
        {
            // byte array representation of that string
            byte[] encodedPassword = new UTF8Encoding().GetBytes(password);

            // need MD5 to calculate the hash
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);

            // string representation (similar to UNIX format)
            string encoded = BitConverter.ToString(hash)
                // without dashes
               .Replace("-", string.Empty)
                // make lowercase
               .ToLower();
            return encoded;
        }
        /// <summary>
        /// upload image to s3
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="fileName"></param>
        /// <param name="fileContent"></param>
        /// <param name="isPublic"></param>
        /// <param name="s3Client"></param>
        public static bool FileUploadToS3(string bucketName, string fileName, Stream fileContent, bool isPublic, AmazonS3Client s3Client)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                return false;
            }
            try
            {
                PutObjectRequest putObjectRequest = new PutObjectRequest();
                putObjectRequest.BucketName = bucketName;
                putObjectRequest.CannedACL = S3CannedACL.PublicRead;
                putObjectRequest.Key = fileName;
                putObjectRequest.InputStream = fileContent;
                PutObjectResponse response = s3Client.PutObject(putObjectRequest);
                return true;
            }
            catch {
                
            }
            return false;
        }

      

    }
}