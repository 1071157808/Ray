using System.ComponentModel.DataAnnotations;

namespace Ray.WebApi.ViewModels
{
    public class TransferViewModel
    {
        [Required]
        public long ToAccountId { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }
    }
}
