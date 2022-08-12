using System.ComponentModel.DataAnnotations;
using XCV.ValidationAttributes;

namespace XCV.InputModels
{
    public class SignInModel
    {
        [Required] [SqlInjectionValidation] public string Login { get; set; }

        [Required] public string Password { get; set; }
    }
}