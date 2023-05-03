using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryApplication
{
    public class Borrow
    {
        [Required]
        public int ReaderNumber { get; set; }

        [Required]
        public int InventoryNumber { get; set; }

        [Required]
        public DateTime BorrowDate { get; set; }

        [Required]
        [DateComparison("BorrowDate")]
        public DateTime ReturnDate { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BorrowNumber { get; set; }
    }
}

public class DateComparisonAttribute : ValidationAttribute
{
    private readonly string _comparisonProperty;

    public DateComparisonAttribute(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

        if (property == null)
        {
            return new ValidationResult($"Property {_comparisonProperty} not found.");
        }

        var comparisonValue = property.GetValue(validationContext.ObjectInstance, null);

        if (value == null || comparisonValue == null)
        {
            return ValidationResult.Success;
        }

        if (DateTime.Compare((DateTime)value, (DateTime)comparisonValue) < 0)
        {
            return new ValidationResult($"The {validationContext.DisplayName} must be greater than {_comparisonProperty}.");
        }

        return ValidationResult.Success;
    }
}

