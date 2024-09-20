using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonX
{
    public interface IResponseModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public int StatusCode { get; set; }
    }

    public interface IResponseDataModel<T> : IResponseModel
    {
        public List<T> Data { get; set; }
    }
}
