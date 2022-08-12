using System;
using System.ComponentModel.DataAnnotations;
using XCV.Entities.Enums;
using XCV.ValidationAttributes;

namespace XCV.InputModels
{
    public class EmployeeCreationModel
    {
        private const string InvalidNameCharactersErrorMessage = "Der Name darf keine Zahlen beinhalten.";
        private const string InvalidNameLengthsErrorMessage = "Der Name darf nicht länger als {1} Zeichen sein.";
        private const string InvalidYearsErrorMessage = "Die Anzahl an Jahren muss zwischen {1} und {2} liegen.";

        [Required(ErrorMessage = "Bitte geben Sie Ihren Vornamen an")]
        [StringLength(30, ErrorMessage = InvalidNameLengthsErrorMessage)]
        [RegularExpression("^\\D*$", ErrorMessage = InvalidNameCharactersErrorMessage)]
        [SqlInjectionValidation]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Bitte geben Sie Ihren Nachnamen an")]
        [StringLength(30, ErrorMessage = InvalidNameLengthsErrorMessage)]
        [RegularExpression("^\\D*$", ErrorMessage = InvalidNameCharactersErrorMessage)]
        public string SurName { get; set; } = null!;

        [Required(ErrorMessage = "Bitte geben Sie ein Username an")]
        [StringLength(30, ErrorMessage = InvalidNameLengthsErrorMessage)]
        [SqlInjectionValidation]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Bitte füllen Sie das Feld aus")]
        [DateTimeRange]
        public DateTime? EmployedSince { get; set; }

        [Required(ErrorMessage = "Bitte füllen Sie das Feld aus")]
        [Range(0, 100, ErrorMessage = InvalidYearsErrorMessage)]
        public int? WorkExperience { get; set; }

        [Required(ErrorMessage = "Bitte füllen Sie das Feld aus")]
        [Range(0, 100, ErrorMessage = InvalidYearsErrorMessage)]
        public int? ScientificAssistant { get; set; }

        [Required(ErrorMessage = "Bitte füllen Sie das Feld aus")]
        [Range(0, 100, ErrorMessage = InvalidYearsErrorMessage)]
        public int? StudentAssistant { get; set; }

        [Required(ErrorMessage = "Bitte füllen Sie das Feld aus")] public Authorizations? Authorization { get; set; } = Entities.Enums.Authorizations.Pleb;
        [Required(ErrorMessage = "Bitte füllen Sie das Feld aus")] public RateCardLevel? RateCardLevel { get; set; } = Entities.Enums.RateCardLevel.Level1;
        public byte[]? ProfilePicture { get; set; }
    }
}