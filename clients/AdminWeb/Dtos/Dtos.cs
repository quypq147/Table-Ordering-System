using System.ComponentModel.DataAnnotations;

namespace AdminWeb.Dtos
{
    // Re-export or temporary keep validation attribute; move if needed later
    public sealed class GuidNotEmptyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext _)
            => value is Guid g && g != Guid.Empty
               ? ValidationResult.Success
               : new ValidationResult("Vui lòng chọn danh mục hợp lệ.");
    }
}
