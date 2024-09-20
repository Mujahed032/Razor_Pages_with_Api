using Application.Interface.Persistence;
using CommonX;
using Domain.Concrete.Models;
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
        public async Task<IResponseDataModel<FileUpload>> AddFileAsync(List<FileUpload> file)
        {
            var response = new ResponseDataModel<FileUpload>();
            try
            {
                await _context.Files.AddRangeAsync(file);
                await _context.SaveChangesAsync();

                response.Data = file;
                response.Success = true;
                response.Message = "File uploaded successfully";
            }
            catch
            {
                response.Success = false;
                response.Message = "Failed to upload file";
            }
            return response;
        }
    }
}
