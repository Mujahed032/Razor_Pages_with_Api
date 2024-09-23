using Application.FileManagement.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Threading.Tasks;

public class UploadFileModel : PageModel
{
    private readonly IMediator _mediator;

    public UploadFileModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Property to hold the uploaded Excel file. BindProperty attribute ensures that the file is bound to the model when the form is submitted.
    [BindProperty]
    public IFormFile ExcelFile { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ExcelFile == null || ExcelFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Please upload a valid Excel file.";
            return Page();
        }

        // Create a MemoryStream to store the content of the uploaded file in memory.
        using var memoryStream = new MemoryStream();

        // Copy the content of the uploaded Excel file to the MemoryStream asynchronously.
        await ExcelFile.CopyToAsync(memoryStream);

        // Use MediatR to send a command to process the file content (as a byte array).
        var result = await _mediator.Send(new CreateForm.UploadFilesCommand(memoryStream.ToArray()));

        if (result.Success)
        {
            TempData["SuccessMessage"] = "File(s) uploaded successfully!";
            return Page(); 
        }

        TempData["ErrorMessage"] = result.Message ?? "Failed to upload file(s).";
        return Page();
    }

}
