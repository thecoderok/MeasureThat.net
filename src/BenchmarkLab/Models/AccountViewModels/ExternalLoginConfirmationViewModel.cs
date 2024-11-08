using System.ComponentModel.DataAnnotations;

namespace MeasureThat.Net.Models.AccountViewModels
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        public string Email
        {
            get; set;
        }
    }
}
