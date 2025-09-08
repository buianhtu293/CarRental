using CarRental.Domain.Enums;

namespace CarRental.Application.DTOs.Booking
{
    /// <summary>
    /// DTO containing summary information of a booking in the booking list.
    /// Used to display booking information in user's booking list page.
    /// </summary>
    public class BookingListDto
    {
        /// <summary>
        /// Unique identifier of the booking
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Booking number displayed to user
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
        /// Car renter name
        /// </summary>
        public string RenterName { get; set; } = string.Empty;
        
        /// <summary>
        /// Car renter email
        /// </summary>
        public string RenterEmail { get; set; } = string.Empty;
        
        /// <summary>
        /// Car renter phone number
        /// </summary>
        public string RenterPhone { get; set; } = string.Empty;
        
        /// <summary>
        /// List of booking items (cars) in this booking
        /// </summary>
        public List<BookingItemListDto> BookingItems { get; set; } = new();
    }

    /// <summary>
    /// DTO containing information of a booking item (specific car) in the list.
    /// Displays car information, pricing, status and images.
    /// </summary>
    public class BookingItemListDto
    {
        /// <summary>
        /// Unique identifier of the booking item
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Identifier of the main booking
        /// </summary>
        public Guid BookingId { get; set; }
        
        /// <summary>
        /// Car identifier
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
        /// Full car name (combined brand and model)
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
        /// Total amount for this booking item
        /// </summary>
        public decimal SubTotal { get; set; }
        
        /// <summary>
        /// Current status of the booking item
        /// </summary>
        public BookingItemStatusEnum Status { get; set; }
        
        /// <summary>
        /// Status display for user (text format)
        /// </summary>
        public string StatusDisplay => GetStatusDisplay();
        
        /// <summary>
        /// CSS class for status display
        /// </summary>
        public string StatusClass => GetStatusClass();
        
        /// <summary>
        /// List of car image URLs
        /// </summary>
        public List<string> CarImages { get; set; } = new();
        
        /// <summary>
        /// Primary car image (first image or default image)
        /// </summary>
        public string PrimaryImage => CarImages.FirstOrDefault() ?? "/img/car-rent-1.png";
        
        /// <summary>
        /// Converts status enum to display text
        /// </summary>
        /// <returns>Status description text</returns>
        private string GetStatusDisplay()
        {
            return Status switch
            {
                BookingItemStatusEnum.PendingDeposit => "Pending Deposit",
                BookingItemStatusEnum.Confirm => "Confirmed",
                BookingItemStatusEnum.InProgress => "In Progress",
                BookingItemStatusEnum.PendingPayment => "Pending Payment",
                BookingItemStatusEnum.Completed => "Completed",
                BookingItemStatusEnum.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }
        
        /// <summary>
        /// Gets appropriate CSS class for each status
        /// </summary>
        /// <returns>CSS class for status badge display</returns>
        private string GetStatusClass()
        {
            return Status switch
            {
                BookingItemStatusEnum.PendingDeposit => "badge-warning",
                BookingItemStatusEnum.Confirm => "badge-success",
                BookingItemStatusEnum.InProgress => "badge-info",
                BookingItemStatusEnum.PendingPayment => "badge-warning",
                BookingItemStatusEnum.Completed => "badge-success",
                BookingItemStatusEnum.Cancelled => "badge-danger",
                _ => "badge-secondary"
            };
        }
    }

    /// <summary>
    /// DTO containing parameters for filtering and paginating booking list.
    /// Used when user searches, filters by status, sorts booking list.
    /// </summary>
    public class BookingListRequestDto
    {
        /// <summary>
        /// Current page number (starting from 1)
        /// </summary>
        public int Page { get; set; } = 1;
        
        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; } = 10;
        
        /// <summary>
        /// Sorting criteria (newest, oldest)
        /// </summary>
        public string SortBy { get; set; } = "newest";
        
        /// <summary>
        /// Filter by booking status (null = all statuses)
        /// </summary>
        public BookingItemStatusEnum? StatusFilter { get; set; }
        
        /// <summary>
        /// Search keyword (search in booking number, car name, etc.)
        /// </summary>
        public string SearchTerm { get; set; } = string.Empty;
        
        /// <summary>
        /// User ID (to filter bookings of specific user)
        /// </summary>
        public Guid? UserId { get; set; }
    }

    /// <summary>
    /// DTO containing booking list result with pagination information.
    /// Includes booking list and information about total count, current page.
    /// </summary>
    public class BookingListResponseDto
    {
        /// <summary>
        /// List of bookings in current page
        /// </summary>
        public List<BookingListDto> Bookings { get; set; } = new();
        
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
    }
}