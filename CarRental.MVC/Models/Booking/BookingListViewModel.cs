using CarRental.Domain.Enums;

namespace CarRental.MVC.Models.Booking
{
    /// <summary>
    /// ViewModel for booking list page, displays user's bookings with pagination and filtering.
    /// Contains pagination information, filtering options and sorting.
    /// </summary>
    public class BookingListViewModel
    {
        /// <summary>
        /// List of bookings in current page
        /// </summary>
        public List<BookingItemViewModel> Bookings { get; set; } = new();
        
        /// <summary>
        /// Total number of bookings (across all pages)
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// Current page
        /// </summary>
        public int CurrentPage { get; set; }
        
        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        
        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;
        
        /// <summary>
        /// Whether there is a next page
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;
        
        /// <summary>
        /// Current sorting criteria
        /// </summary>
        public string SortBy { get; set; } = "newest";
        
        /// <summary>
        /// Current status filter
        /// </summary>
        public BookingItemStatusEnum? StatusFilter { get; set; }
        
        /// <summary>
        /// Current search keyword
        /// </summary>
        public string SearchTerm { get; set; } = string.Empty;
        
        /// <summary>
        /// List of status options for filter dropdown
        /// </summary>
        public List<SelectItem> StatusOptions { get; set; } = new()
        {
            new SelectItem { Value = "", Text = "All Status" },
            new SelectItem { Value = ((int)BookingItemStatusEnum.PendingDeposit).ToString(), Text = "Pending Deposit" },
            new SelectItem { Value = ((int)BookingItemStatusEnum.Confirm).ToString(), Text = "Confirmed" },
            new SelectItem { Value = ((int)BookingItemStatusEnum.InProgress).ToString(), Text = "In Progress" },
            new SelectItem { Value = ((int)BookingItemStatusEnum.PendingPayment).ToString(), Text = "Pending Payment" },
            new SelectItem { Value = ((int)BookingItemStatusEnum.Completed).ToString(), Text = "Completed" },
            new SelectItem { Value = ((int)BookingItemStatusEnum.Cancelled).ToString(), Text = "Cancelled" }
        };
        
        /// <summary>
        /// List of sorting options for dropdown
        /// </summary>
        public List<SelectItem> SortOptions { get; set; } = new()
        {
            new SelectItem { Value = "newest", Text = "Newest to Latest" },
            new SelectItem { Value = "oldest", Text = "Oldest to Newest" },
            new SelectItem { Value = "highest", Text = "Highest to Lowest"},
            new SelectItem { Value = "lowest", Text = "Lowest to Highest"},
        };
    }

    /// <summary>
    /// ViewModel representing a booking in the list.
    /// Contains booking summary information and list of booking items (cars).
    /// </summary>
    public class BookingItemViewModel
    {
        /// <summary>
        /// Booking ID
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Booking number for display
        /// </summary>
        public string BookingNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Booking creation date
        /// </summary>
        public DateTime BookingDate { get; set; }
        
        /// <summary>
        /// Car pickup date
        /// </summary>
        public DateTime PickupDate { get; set; }
        
        /// <summary>
        /// Car return date
        /// </summary>
        public DateTime ReturnDate { get; set; }
        
        /// <summary>
        /// Number of rental days
        /// </summary>
        public decimal NumberOfDays { get; set; }
        
        /// <summary>
        /// Total rental amount
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Total deposit amount
        /// </summary>
        public decimal TotalDeposit { get; set; }
        
        /// <summary>
        /// Renter name
        /// </summary>
        public string RenterName { get; set; } = string.Empty;
        
        /// <summary>
        /// Renter email
        /// </summary>
        public string RenterEmail { get; set; } = string.Empty;
        
        /// <summary>
        /// Renter phone number
        /// </summary>
        public string RenterPhone { get; set; } = string.Empty;
        
        /// <summary>
        /// List of booking items (cars) in this booking
        /// </summary>
        public List<BookingCarItemViewModel> BookingItems { get; set; } = new();
        
        /// <summary>
        /// Overall booking status (based on item statuses)
        /// </summary>
        public string OverallStatus { get; set; } = string.Empty;
        
        /// <summary>
        /// CSS class for overall status display
        /// </summary>
        public string OverallStatusClass { get; set; } = string.Empty;
        
        /// <summary>
        /// Detailed renter information
        /// </summary>
        public RenterDetailViewModel? RenterInfo { get; set; }
        
        /// <summary>
        /// Detailed driver information
        /// </summary>
        public List<DriverDetailViewModel>? DriversInfo { get; set; } = new();
    }

    /// <summary>
    /// ViewModel representing a booking item (specific car) in booking.
    /// Contains car information, pricing, status and available actions.
    /// </summary>
    public class BookingCarItemViewModel
    {
        /// <summary>
        /// Booking item ID
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// ID of booking containing this item
        /// </summary>
        public Guid BookingId { get; set; }
        
        /// <summary>
        /// Car ID
        /// </summary>
        public Guid CarId { get; set; }
        
        /// <summary>
        /// Car brand
        /// </summary>
        public string CarBrand { get; set; } = string.Empty;
        
        /// <summary>
        /// Car model
        /// </summary>
        public string CarModel { get; set; } = string.Empty;
        
        /// <summary>
        /// Full car name
        /// </summary>
        public string CarName => $"{CarBrand} {CarModel}";
        
        /// <summary>
        /// Car license plate
        /// </summary>
        public string LicensePlate { get; set; } = string.Empty;
        
        /// <summary>
        /// Daily rental price
        /// </summary>
        public decimal PricePerDay { get; set; }
        
        /// <summary>
        /// Deposit amount
        /// </summary>
        public decimal Deposit { get; set; }
        
        /// <summary>
        /// Total amount for this item
        /// </summary>
        public decimal SubTotal { get; set; }
        
        /// <summary>
        /// Booking item status
        /// </summary>
        public BookingItemStatusEnum Status { get; set; }
        
        /// <summary>
        /// Status display (text format)
        /// </summary>
        public string StatusDisplay { get; set; } = string.Empty;
        
        /// <summary>
        /// CSS class for status display
        /// </summary>
        public string StatusClass { get; set; } = string.Empty;
        
        /// <summary>
        /// List of car image URLs
        /// </summary>
        public List<string> CarImages { get; set; } = new();
        
        /// <summary>
        /// Primary (first) car image
        /// </summary>
        public string PrimaryImage => CarImages.FirstOrDefault();
        
        /// <summary>
        /// Current image index (for carousel)
        /// </summary>
        public int CurrentImageIndex { get; set; } = 0;
        
        /// <summary>
        /// List of actions available based on status
        /// </summary>
        public List<BookingActionViewModel> AvailableActions { get; set; } = new();
    }

    /// <summary>
    /// ViewModel representing an action that can be performed on booking item.
    /// Examples: Cancel, Confirm Pickup, Return Car, etc.
    /// </summary>
    public class BookingActionViewModel
    {
        /// <summary>
        /// Action name (for logic processing)
        /// </summary>
        public string Action { get; set; } = string.Empty;
        
        /// <summary>
        /// Text displayed on button
        /// </summary>
        public string DisplayText { get; set; } = string.Empty;
        
        /// <summary>
        /// CSS class for button
        /// </summary>
        public string ButtonClass { get; set; } = string.Empty;
        
        /// <summary>
        /// Icon for button
        /// </summary>
        public string Icon { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether confirmation is required before execution
        /// </summary>
        public bool RequiresConfirmation { get; set; } = false;
        
        /// <summary>
        /// Confirmation message
        /// </summary>
        public string ConfirmationMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel for dropdown select items.
    /// Used for filter status and sort options.
    /// </summary>
    public class SelectItem
    {
        /// <summary>
        /// Option value
        /// </summary>
        public string Value { get; set; } = string.Empty;
        
        /// <summary>
        /// Option display text
        /// </summary>
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel containing detailed car renter information.
    /// Displays complete personal information and address.
    /// </summary>
    public class RenterDetailViewModel
    {
        /// <summary>
        /// Renter full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// Renter email
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Renter phone number
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Renter date of birth
        /// </summary>
        public DateTime DateOfBirth { get; set; }
        
        /// <summary>
        /// Driver license number
        /// </summary>
        public string LicenseNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Driver license image URL
        /// </summary>
        public string? LicenseImageUrl { get; set; }
        
        /// <summary>
        /// Detailed address
        /// </summary>
        public string Address { get; set; } = string.Empty;
        
        /// <summary>
        /// City/Province
        /// </summary>
        public string City { get; set; } = string.Empty;
        
        /// <summary>
        /// District/County
        /// </summary>
        public string District { get; set; } = string.Empty;
        
        /// <summary>
        /// Ward/Commune
        /// </summary>
        public string Ward { get; set; } = string.Empty;
        
        /// <summary>
        /// Full address (combined Ward, District, City)
        /// </summary>
        public string FullAddress => $"{Ward}, {District}, {City}".Trim().TrimStart(',').TrimEnd(',');
    }

    /// <summary>
    /// ViewModel containing detailed driver information.
    /// Similar to RenterDetailViewModel but with additional renter relationship information.
    /// </summary>
    public class DriverDetailViewModel
    {
        /// <summary>
        /// Driver full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// Driver email
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Driver phone number
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Driver date of birth
        /// </summary>
        public DateTime DateOfBirth { get; set; }
        
        /// <summary>
        /// Driver license number
        /// </summary>
        public string LicenseNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Driver license image URL
        /// </summary>
        public string? LicenseImageUrl { get; set; }
        
        /// <summary>
        /// Detailed address
        /// </summary>
        public string Address { get; set; } = string.Empty;
        
        /// <summary>
        /// City/Province
        /// </summary>
        public string City { get; set; } = string.Empty;
        
        /// <summary>
        /// District/County
        /// </summary>
        public string District { get; set; } = string.Empty;
        
        /// <summary>
        /// Ward/Commune
        /// </summary>
        public string Ward { get; set; } = string.Empty;
        
        /// <summary>
        /// Indicates whether driver is the same as renter
        /// </summary>
        public bool IsSameAsRenter { get; set; }
        
        /// <summary>
        /// Full address (combined Ward, District, City)
        /// </summary>
        public string FullAddress => $"{Ward}, {District}, {City}".Trim().TrimStart(',').TrimEnd(',');
    }
}