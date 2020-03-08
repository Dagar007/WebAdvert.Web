using System.ComponentModel.DataAnnotations;


namespace WebAdvert.Models.Accounts
{
    public class ConfirmModel
    {
        [Required]
        [EmailAddress]
        [Display(Name ="Email")]
        public string Email { get; set; }
        [Required(ErrorMessage ="Code is required")]
        public string Code { get; set; }
    }
}
