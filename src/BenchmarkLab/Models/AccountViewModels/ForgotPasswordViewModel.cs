using System.ComponentModel.DataAnnotations;

namespace MeasureThat.Net.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
