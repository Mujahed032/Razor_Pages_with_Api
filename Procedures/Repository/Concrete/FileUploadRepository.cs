using Application.Interface.Persistence;
using CommonX;
using Domain.Concrete.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Concrete
{
    public class FileUploadRepository : IFileUploadRepository
    {
        private readonly ApplicationDbContext _context;

        public FileUploadRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IResponseDataModel<FileUpload>> AddFileAsync(List<FileUpload> files)
        {
            var response = new ResponseDataModel<FileUpload>();
            try
            {
                var filesToUpload = new List<FileUpload>();

                foreach (var file in files)
                {
                    var existingFile = await _context.Files
                        .FirstOrDefaultAsync(f => f.FilePath == file.FilePath);

                    if (existingFile != null)
                    {
                        // File already exists, skip adding this file
                        response.Message = $"File '{file.FileName}' already exists, skipping upload.";
                        continue;
                    }

                    // If file doesn't exist, add it to the list for uploading
                    filesToUpload.Add(file);
                }

                if (filesToUpload.Any())
                {
                    await _context.Files.AddRangeAsync(filesToUpload);
                    await _context.SaveChangesAsync();

                    response.Data = filesToUpload;
                    response.Success = true;
                    response.Message = "File(s) uploaded successfully.";
                }
                else
                {
                    response.Success = false;
                    response.Message = "No new files to upload.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Failed to upload files: {ex.Message}";
            }
            return response;
        }
    }
}
