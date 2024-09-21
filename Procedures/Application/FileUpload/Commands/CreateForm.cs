using Application.Interface.Persistence;
using CommonX;
using MediatR;
using OfficeOpenXml;
using Domain.Concrete.Models;
using System.IO;

namespace Application.FileManagement.Commands
{
    public static class CreateForm
    {
        public record UploadFilesCommand(byte[] ExcelFileContent) : IRequest<ResponseDataModel<FileUpload>>;

        public class UploadFilesCommandHandler : IRequestHandler<UploadFilesCommand, ResponseDataModel<FileUpload>>
        {
            private readonly IFileUploadRepository _fileUploadRepository;

            public UploadFilesCommandHandler(IFileUploadRepository fileUploadRepository)
            {
                _fileUploadRepository = fileUploadRepository;
            }

            public async Task<ResponseDataModel<FileUpload>> Handle(UploadFilesCommand request, CancellationToken cancellationToken)
            {
                // Process Excel file to extract file names and paths
                var filesToUpload = ProcessExcelFile(request.ExcelFileContent);

                if (filesToUpload == null || !filesToUpload.Any())
                {
                    return new ResponseDataModel<FileUpload> { Success = false, Message = "No valid files to upload." };
                }

                var result = await _fileUploadRepository.AddFileAsync(filesToUpload);

                // Map IResponseDataModel<FileUpload> to ResponseDataModel<FileUpload>
                var response = new ResponseDataModel<FileUpload>
                {
                    Data = result.Data,
                    Success = result.Success,
                    Message = result.Message
                };

                return response;
            }

            private List<FileUpload> ProcessExcelFile(byte[] fileContent)
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the license context

                using var stream = new MemoryStream(fileContent);
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.First();

                if (worksheet == null || worksheet.Dimension == null)
                {
                    throw new InvalidDataException("The Excel file is empty or invalid.");
                }

                var fileUploads = new List<FileUpload>();

                for (int row = 2; row <= worksheet.Dimension.Rows; row++)  // Assuming first row is header
                {
                    var fileName = worksheet.Cells[row, 1].Text;
                    var filePath = worksheet.Cells[row, 2].Text;

                    if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    {
                        continue; // Skip rows with invalid data
                    }

                    var fileBytes = File.ReadAllBytes(filePath);

                    fileUploads.Add(new FileUpload
                    {
                        FileName = fileName,
                        FileContent = fileBytes,
                        FilePath = filePath
                    });
                }

                return fileUploads;
            }
        }
    }
}
