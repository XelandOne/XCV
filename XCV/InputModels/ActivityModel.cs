using System.ComponentModel.DataAnnotations;

namespace XCV.InputModels
{
    public class ActivityModel
    {
        private const string InvalidLengthMessage = "Der Name darf nur {1} Zeichen lang sein";

        [Required(ErrorMessage = "Bitte geben Sie einen Namen ein")]
        [StringLength(100, ErrorMessage = InvalidLengthMessage)]
        public string Name { get; set; } = null!;
    }
}