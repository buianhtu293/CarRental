using System;
using System.ComponentModel.DataAnnotations;

namespace App.Areas.Identity.Models.ManageViewModels
{
    public class EditExtraProfileModel : IValidatableObject
    {
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Display(Name = "User Email")]
        public string UserEmail { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        // Province
        [Display(Name = "Province")]
        public string ProvinceName { get; set; }

        // District
        [Display(Name = "District")]
        public string DistrictName { get; set; }

        // Ward
        [Display(Name = "Ward")]
        public string WardName { get; set; }

        // Full address (optional if you still want to store it as one field)
        [Display(Name = "Full Address")]
        [StringLength(400)]
        public string HomeAdress { get; set; }

		[Display(Name = "License ID")]
		public string LicenseId { get; set; }

		[Display(Name = "License Image")]
		public IFormFile? LicenseImage { get; set; }

		public string? LicenseImageUrl { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			// Require at least one image (existing or newly uploaded)
			if (string.IsNullOrEmpty(LicenseImageUrl) && LicenseImage == null)
			{
				yield return new ValidationResult(
					"Please upload a license image.",
					new[] { nameof(LicenseImage) }
				);
			}
		}

	}

}