using System.ComponentModel.DataAnnotations;

namespace Financal_API.Models
{
    public class VixIndex
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
