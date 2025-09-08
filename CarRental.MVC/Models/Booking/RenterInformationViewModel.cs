using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace CarRental.MVC.Models.Booking
{
    public class RenterInformationViewModel
    {
        [Required(ErrorMessage = "H? tên là b?t bu?c")]
        [Display(Name = "H? và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là b?t bu?c")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "S? ?i?n tho?i là b?t bu?c")]
        [Phone(ErrorMessage = "S? ?i?n tho?i không h?p l?")]
        [Display(Name = "S? ?i?n tho?i")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là b?t bu?c")]
        [EmailAddress(ErrorMessage = "Email không h?p l?")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "S? b?ng lái là b?t bu?c")]
        [Display(Name = "S? b?ng lái")]
        public string LicenseNumber { get; set; } = string.Empty;

        [Display(Name = "?nh b?ng lái xe (File Upload)")]
        [JsonIgnore] // Không serialize vào session
        public IFormFile? LicenseImageFile { get; set; }

        [Display(Name = "?nh b?ng lái xe (URL ?? hi?n th?)")]
        public string? LicenseImage { get; set; } // Tr??ng này s? l?u URL ?? hi?n th?

        [Required(ErrorMessage = "??a ch? là b?t bu?c")]
        [Display(Name = "??a ch?")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Thành ph?")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "Qu?n/Huy?n")]
        public string District { get; set; } = string.Empty;

        [Display(Name = "Ph??ng/Xã")]
        public string Ward { get; set; } = string.Empty;
    }
}