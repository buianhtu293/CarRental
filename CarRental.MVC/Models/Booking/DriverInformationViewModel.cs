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

        [Display(Name = "H? v� t�n t�i x?")]
        public string FullName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Ng�y sinh")]
        public DateTime DateOfBirth { get; set; }

        [Phone(ErrorMessage = "S? ?i?n tho?i kh�ng h?p l?")]
        [Display(Name = "S? ?i?n tho?i")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email kh�ng h?p l?")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "S? b?ng l�i t�i x? l� b?t bu?c")]
        [Display(Name = "S? b?ng l�i")]
        public string LicenseNumber { get; set; } = string.Empty;

        [Display(Name = "?nh b?ng l�i xe (File Upload)")]
        [JsonIgnore] // Kh�ng serialize v�o session
        public IFormFile? LicenseImageFile { get; set; }

        [Display(Name = "?nh b?ng l�i xe (URL ?? hi?n th?)")]
        public string? LicenseImage { get; set; } // Tr??ng n�y s? l?u URL ?? hi?n th?

        [Display(Name = "??a ch?")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Th�nh ph?")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "Qu?n/Huy?n")]
        public string District { get; set; } = string.Empty;

        [Display(Name = "Ph??ng/X�")]
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