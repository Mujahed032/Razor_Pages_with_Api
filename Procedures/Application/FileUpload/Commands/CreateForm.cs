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

            // This method processes an Excel file (passed as a byte array) to extract file names and paths.
            private List<FileUpload> ProcessExcelFile(byte[] fileContent)
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the license context

                // Create a memory stream from the byte array to load the Excel file content.
                using var stream = new MemoryStream(fileContent);

                // Load the ExcelPackage using the memory stream.
                using var package = new ExcelPackage(stream);

                // Get the first worksheet from the Excel workbook.
                var worksheet = package.Workbook.Worksheets.First();

                if (worksheet == null || worksheet.Dimension == null)
                {
                    throw new InvalidDataException("The Excel file is empty or invalid.");
                }

                // Create an empty list to store `FileUpload` objects, which will hold file data extracted from the worksheet.
                var fileUploads = new List<FileUpload>();

                for (int row = 2; row <= worksheet.Dimension.Rows; row++)  // Assuming first row is header
                {
                    // Extract the file name from the first column (assumed to be in column 1).
                    var fileName = worksheet.Cells[row, 1].Text;

                    // Extract the file path from the second column (assumed to be in column 2).
                    var filePath = worksheet.Cells[row, 2].Text;

                    if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    {
                        continue; // Skip rows with invalid data
                    }

                    // Read the file bytes from the file path.
                    var fileBytes = File.ReadAllBytes(filePath);

                    // Create a new `FileUpload` object to hold the file name, file content (as bytes), and file path.
                    // Add it to the list of file uploads.
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
