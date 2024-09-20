using System.ComponentModel.DataAnnotations;

namespace Domain.Concrete.Models
{
    public class FileUpload
    {
        [Key]
        public int Id { get; set; }
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
    }
}
