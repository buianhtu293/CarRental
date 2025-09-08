namespace CarRental.Application.DTOs.Booking
{
    /// <summary>
    /// DTO containing information of a driver authorized to drive the car in booking.
    /// Each car in booking must have one designated driver (can be the renter or someone else).
    /// </summary>
    public class DriverInformationDto
    {
        /// <summary>
        /// Identifier of the car this driver will drive
        /// </summary>
        public Guid CarId { get; set; }
        
        /// <summary>
        /// Car name (brand + model) for display
        /// </summary>
        public string CarName { get; set; } = string.Empty;
        
        /// <summary>
        /// Indicates whether the driver is the same as the renter
        /// </summary>
        public bool IsSameAsRenter { get; set; } = true;
        
        /// <summary>
        /// Full name of the driver
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// Date of birth of the driver
        /// </summary>
        public DateTime DateOfBirth { get; set; }
        
        /// <summary>
        /// Phone number of the driver
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Email address of the driver
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Driver's license number
        /// </summary>
        public string LicenseNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// URL of driver's license image
        /// </summary>
        public string LicenseImageUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Detailed address of the driver
        /// </summary>
        public string Address { get; set; } = string.Empty;
        
        /// <summary>
        /// City/Province of the driver
        /// </summary>
        public string City { get; set; } = string.Empty;
        
        /// <summary>
        /// District/County of the driver
        /// </summary>
        public string District { get; set; } = string.Empty;
        
        /// <summary>
        /// Ward/Commune of the driver
        /// </summary>
        public string Ward { get; set; } = string.Empty;
    }
}