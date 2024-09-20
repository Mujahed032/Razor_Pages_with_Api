using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonX
{
    public class ResponseModel : IResponseModel
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class ResponseDataModel<T> : ResponseModel, IResponseDataModel<T>
    {
        public List<T> Data { get; set; }
    }
}
