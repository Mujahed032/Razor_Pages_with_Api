using Application.Interface.Persistence;
using CommonX;
using MediatR;
using ClosedXML.Excel;
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

                if (!IsExcelFile(request.ExcelFileContent))
                {
                    return new ResponseDataModel<FileUpload>
                    {
                        Success = false,
                        Message = "Invalid file type. Please upload an Excel file (.xlsx)."
                    };
                }

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
                // Create a memory stream from the byte array to load the Excel file content.
                using var stream = new MemoryStream(fileContent);

                // Load the Excel file using ClosedXML
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.First(); // Get the first worksheet

                if (worksheet == null || worksheet.FirstRowUsed() == null)
                {
                    throw new InvalidDataException("The Excel file is empty or invalid.");
                }

                // Create an empty list to store `FileUpload` objects, which will hold file data extracted from the worksheet.
                var fileUploads = new List<FileUpload>();

                // Iterate over the rows starting from the second row (to skip header)
                var firstRow = worksheet.FirstRowUsed().RowNumber();
                var lastRow = worksheet.LastRowUsed().RowNumber();

                for (int row = firstRow + 1; row <= lastRow; row++) // Assuming first row is header
                {
                    // Extract the file name from the first column (column A or 1).
                    var fileName = worksheet.Cell(row, 1).GetString();

                    // Extract the file path from the second column (column B or 2).
                    var filePath = worksheet.Cell(row, 2).GetString();

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
            // This method checks whether the file content is an Excel file by checking its signature (magic number).
            private bool IsExcelFile(byte[] fileContent)
            {
                // Check if the file is in the ZIP format (Excel OpenXML .xlsx is a ZIP file)
                // ZIP files start with these bytes: 0x50, 0x4B (PK)
                return fileContent.Length >= 4 && fileContent[0] == 0x50 && fileContent[1] == 0x4B;
            }
        }
    }
}
