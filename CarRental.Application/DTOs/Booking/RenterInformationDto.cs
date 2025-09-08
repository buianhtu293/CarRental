namespace CarRental.Application.DTOs.Booking
{
    /// <summary>
    /// DTO containing information of the car renter.
    /// Includes personal information, identification documents and contact address.
    /// </summary>
    public class RenterInformationDto
    {
        /// <summary>
        /// Full name of the car renter
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// Date of birth of the car renter
        /// </summary>
        public DateTime DateOfBirth { get; set; }
        
        /// <summary>
        /// Contact phone number of the car renter
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Email address of the car renter
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Driver's license number of the renter
        /// </summary>
        public string LicenseNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// URL of renter's driver license image
        /// </summary>
        public string LicenseImageUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Detailed address of the car renter
        /// </summary>
        public string Address { get; set; } = string.Empty;
        
        /// <summary>
        /// City/Province of the car renter
        /// </summary>
        public string City { get; set; } = string.Empty;
        
        /// <summary>
        /// District/County of the car renter
        /// </summary>
        public string District { get; set; } = string.Empty;
        
        /// <summary>
        /// Ward/Commune of the car renter
        /// </summary>
        public string Ward { get; set; } = string.Empty;
    }
}