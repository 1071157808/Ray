using System.ComponentModel.DataAnnotations;

namespace Ray.WebApi.ViewModels
{
    public class AddAmountViewModel
    {
        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }
    }
}
