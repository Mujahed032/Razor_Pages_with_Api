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

    [BindProperty]
    public IFormFile ExcelFile { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ExcelFile == null || ExcelFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Please upload a valid Excel file.";
            return Page();
        }

        using var memoryStream = new MemoryStream();
        await ExcelFile.CopyToAsync(memoryStream);

        var result = await _mediator.Send(new CreateForm.UploadFilesCommand(memoryStream.ToArray()));

        if (result.Success)
        {
            TempData["SuccessMessage"] = "File(s) uploaded successfully!";
            return RedirectToPage("Success");  // or use Page() to stay on the same page
        }

        TempData["ErrorMessage"] = result.Message ?? "Failed to upload file(s).";
        return Page();
    }

}
