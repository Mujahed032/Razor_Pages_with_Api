using CommonX;
using Domain.Concrete.Models;

namespace Application.Interface.Persistence
{
    public interface IFileUploadRepository
    {
        Task<IResponseDataModel<Domain.Concrete.Models.FileUpload>> AddFileAsync(List<FileUpload> file);
    }
}
