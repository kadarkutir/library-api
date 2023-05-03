using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryApplication
{
    public class User
    {
        [Required]
        [InvalidCharacter("!?_-:;#")]
        [StringLength(20)]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReaderNumber { get; set; }

        public DateTime BirthDate { get; set; }
    }
}

public class InvalidCharacterAttribute : ValidationAttribute
{
    private readonly string _invalidCharacters;

    public InvalidCharacterAttribute(string invalidCharacters)
    {
        _invalidCharacters = invalidCharacters;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult($"{validationContext.DisplayName} cant be empty");
        }

        var invalidCharacters = _invalidCharacters.ToCharArray();

        if (invalidCharacters.Any(c => ((string)value).Contains(c)))
        {
            return new ValidationResult($"{validationContext.DisplayName} contains invalid characters.");
        }

        return ValidationResult.Success;
    }
}
