using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace CarRental.MVC.Models.Booking
{
    public class DriverInformationViewModel
    {
        public Guid CarId { get; set; }
        public string CarName { get; set; } = string.Empty;
        
        public bool IsSameAsRenter { get; set; } = true;

        [Display(Name = "H? và tên tài x?")]
        public string FullName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime DateOfBirth { get; set; }

        [Phone(ErrorMessage = "S? ?i?n tho?i không h?p l?")]
        [Display(Name = "S? ?i?n tho?i")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không h?p l?")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "S? b?ng lái tài x? là b?t bu?c")]
        [Display(Name = "S? b?ng lái")]
        public string LicenseNumber { get; set; } = string.Empty;

        [Display(Name = "?nh b?ng lái xe (File Upload)")]
        [JsonIgnore] // Không serialize vào session
        public IFormFile? LicenseImageFile { get; set; }

        [Display(Name = "?nh b?ng lái xe (URL ?? hi?n th?)")]
        public string? LicenseImage { get; set; } // Tr??ng này s? l?u URL ?? hi?n th?

        [Display(Name = "??a ch?")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Thành ph?")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "Qu?n/Huy?n")]
        public string District { get; set; } = string.Empty;

        [Display(Name = "Ph??ng/Xã")]
        public string Ward { get; set; } = string.Empty;

        // Method to validate if different from renter
        public bool IsValid()
        {
            if (IsSameAsRenter) return true;
            
            return !string.IsNullOrWhiteSpace(FullName) &&
                   !string.IsNullOrWhiteSpace(LicenseNumber) &&
                   !string.IsNullOrWhiteSpace(PhoneNumber) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Address) &&
                   DateOfBirth != default;
        }
    }
}