using CarRental.Application.DTOs.Booking;
using CarRental.Domain.Enums;
using CarRental.MVC.Models.Booking;

namespace CarRental.MVC.Extensions
{
    public static class BookingListMappingExtensions
    {
        public static BookingListViewModel ToViewModel(this BookingListResponseDto dto)
        {
            return new BookingListViewModel
            {
                Bookings = dto.Bookings.Select(b => b.ToViewModel()).ToList(),
                TotalCount = dto.TotalCount,
                CurrentPage = dto.CurrentPage,
                PageSize = dto.PageSize
            };
        }

        public static BookingItemViewModel ToViewModel(this BookingListDto dto)
        {
            var viewModel = new BookingItemViewModel
            {
                Id = dto.Id,
                BookingNumber = dto.BookingNumber,
                BookingDate = dto.BookingDate,
                PickupDate = dto.PickupDate,
                ReturnDate = dto.ReturnDate,
                NumberOfDays = dto.NumberOfDays,
                TotalAmount = dto.TotalAmount,
                TotalDeposit = dto.TotalDeposit,
                RenterName = dto.RenterName,
                RenterEmail = dto.RenterEmail,
                RenterPhone = dto.RenterPhone,
                BookingItems = dto.BookingItems.Select(bi => bi.ToViewModel()).ToList()
            };

            // Determine overall status
            SetOverallStatus(viewModel);

            return viewModel;
        }

        public static BookingCarItemViewModel ToViewModel(this BookingItemListDto dto)
        {
            var viewModel = new BookingCarItemViewModel
            {
                Id = dto.Id,
                BookingId = dto.BookingId,
                CarId = dto.CarId,
                CarBrand = dto.CarBrand,
                CarModel = dto.CarModel,
                LicensePlate = dto.LicensePlate,
                PricePerDay = dto.PricePerDay,
                Deposit = dto.Deposit,
                SubTotal = dto.SubTotal,
                Status = dto.Status,
                StatusDisplay = dto.StatusDisplay,
                StatusClass = dto.StatusClass,
                CarImages = dto.CarImages
            };

            // Set available actions based on status
            viewModel.AvailableActions = GetAvailableActions(dto.Status);

            return viewModel;
        }

        public static BookingEditViewModel ToEditViewModel(this BookingDetailDto dto)
        {
            return new BookingEditViewModel
            {
                Id = dto.Id,
                BookingId = dto.BookingId,
                BookingNumber = dto.BookingNumber,
                BookingDate = dto.BookingDate,
                Status = dto.Status.ToString(),
                IsEditable = dto.IsEditable,
                PaymentMethod = dto.PaymentMethod,
                TotalAmount = dto.TotalAmount,
                TotalDeposit = dto.TotalDeposit,
                NumberOfDays = dto.NumberOfDays,
                
                CarInfo = new CarInformationViewModel
                {
                    Id = dto.CarInfo.Id,
                    Brand = dto.CarInfo.Brand,
                    Model = dto.CarInfo.Model,
                    LicensePlate = dto.CarInfo.LicensePlate,
                    ImageUrl = dto.CarInfo.ImageUrl,
                    PricePerDay = dto.CarInfo.PricePerDay,
                    RequiredDeposit = dto.CarInfo.RequiredDeposit,
                    Location = dto.CarInfo.Location,
                    Color = dto.CarInfo.Color,
                    Seats = dto.CarInfo.Seats,
                    Transmission = dto.CarInfo.Transmission,
                    FuelType = dto.CarInfo.FuelType
                },

                Information = new BookingInformationEditViewModel
                {
                    PickupDate = dto.PickupDate,
                    ReturnDate = dto.ReturnDate,
                    Renter = new RenterInformationViewModel
                    {
                        FullName = dto.RenterInfo.FullName,
                        Email = dto.RenterInfo.Email,
                        PhoneNumber = dto.RenterInfo.PhoneNumber,
                        DateOfBirth = dto.RenterInfo.DateOfBirth,
                        LicenseNumber = dto.RenterInfo.LicenseNumber,
                        LicenseImage = dto.RenterInfo.LicenseImageUrl,
                        Address = dto.RenterInfo.Address,
                        City = dto.RenterInfo.City,
                        District = dto.RenterInfo.District,
                        Ward = dto.RenterInfo.Ward
                    },
                    Drivers = dto.Drivers.Select((driver, index) => new DriverInformationViewModel
                    {
                        FullName = driver.FullName,
                        Email = driver.Email,
                        PhoneNumber = driver.PhoneNumber,
                        DateOfBirth = driver.DateOfBirth,
                        LicenseNumber = driver.LicenseNumber,
                        LicenseImage = driver.LicenseImageUrl,
                        Address = driver.Address,
                        City = driver.City,
                        District = driver.District,
                        Ward = driver.Ward,
                        IsSameAsRenter = driver.IsSameAsRenter
                    }).ToList()
                },

                Progress = new BookingProgressViewModel
                {
                    CurrentStep = 1,
                    TotalSteps = 3,
                    BookingSessionId = dto.Id.ToString()
                }
            };
        }

        public static BookingItemViewModel ToDetailsViewModel(this BookingDetailDto dto)
        {
            var viewModel = new BookingItemViewModel
            {
                Id = dto.BookingId, // Use BookingId instead of Id for the main booking
                BookingNumber = dto.BookingNumber,
                BookingDate = dto.BookingDate,
                PickupDate = dto.PickupDate,
                ReturnDate = dto.ReturnDate,
                NumberOfDays = dto.NumberOfDays,
                TotalAmount = dto.TotalAmount,
                TotalDeposit = dto.TotalDeposit,
                RenterName = dto.RenterInfo.FullName,
                RenterEmail = dto.RenterInfo.Email,
                RenterPhone = dto.RenterInfo.PhoneNumber,
                
                // Map detailed renter information
                RenterInfo = new RenterDetailViewModel
                {
                    FullName = dto.RenterInfo.FullName,
                    Email = dto.RenterInfo.Email,
                    PhoneNumber = dto.RenterInfo.PhoneNumber,
                    DateOfBirth = dto.RenterInfo.DateOfBirth,
                    LicenseNumber = dto.RenterInfo.LicenseNumber,
                    LicenseImageUrl = dto.RenterInfo.LicenseImageUrl,
                    Address = dto.RenterInfo.Address,
                    City = dto.RenterInfo.City,
                    District = dto.RenterInfo.District,
                    Ward = dto.RenterInfo.Ward
                },
                
                // Map drivers information
                DriversInfo = dto.Drivers?.Select(driver => new DriverDetailViewModel
                {
                    FullName = driver.FullName,
                    Email = driver.Email,
                    PhoneNumber = driver.PhoneNumber,
                    DateOfBirth = driver.DateOfBirth,
                    LicenseNumber = driver.LicenseNumber,
                    LicenseImageUrl = driver.LicenseImageUrl,
                    Address = driver.Address,
                    City = driver.City,
                    District = driver.District,
                    Ward = driver.Ward,
                    IsSameAsRenter = driver.IsSameAsRenter
                }).ToList() ?? new List<DriverDetailViewModel>(),
                
                // Create a single BookingCarItemViewModel for the car
                BookingItems = new List<BookingCarItemViewModel>
                {
                    new BookingCarItemViewModel
                    {
                        Id = dto.Id, // This is the BookingItem ID
                        BookingId = dto.BookingId,
                        CarId = dto.CarInfo.Id,
                        CarBrand = dto.CarInfo.Brand,
                        CarModel = dto.CarInfo.Model,
                        LicensePlate = dto.CarInfo.LicensePlate,
                        PricePerDay = dto.CarInfo.PricePerDay,
                        Deposit = dto.CarInfo.RequiredDeposit,
                        SubTotal = dto.TotalAmount,
                        Status = dto.Status,
                        StatusDisplay = GetStatusDisplay(dto.Status),
                        StatusClass = GetStatusClass(dto.Status),
                        CarImages = new List<string>(), // Empty for now, could be populated from CarInfo if available
                        AvailableActions = GetAvailableActions(dto.Status)
                    }
                }
            };

            // Set overall status based on the single item
            viewModel.OverallStatus = GetStatusDisplay(dto.Status);
            viewModel.OverallStatusClass = GetStatusClass(dto.Status);

            return viewModel;
        }

        public static BookingListRequestDto ToDto(this BookingListViewModel viewModel, Guid? userId = null)
        {
            return new BookingListRequestDto
            {
                Page = viewModel.CurrentPage,
                PageSize = viewModel.PageSize,
                SortBy = viewModel.SortBy,
                StatusFilter = viewModel.StatusFilter,
                SearchTerm = viewModel.SearchTerm,
                UserId = userId
            };
        }

        private static void SetOverallStatus(BookingItemViewModel viewModel)
        {
            if (!viewModel.BookingItems.Any())
            {
                viewModel.OverallStatus = "No Items";
                viewModel.OverallStatusClass = "badge-secondary";
                return;
            }

            // Check if all items have the same status
            var statuses = viewModel.BookingItems.Select(bi => bi.Status).Distinct().ToList();
            
            if (statuses.Count == 1)
            {
                var singleStatus = statuses.First();
                viewModel.OverallStatus = viewModel.BookingItems.First().StatusDisplay;
                viewModel.OverallStatusClass = viewModel.BookingItems.First().StatusClass;
            }
            else
            {
                // Mixed statuses
                if (statuses.Contains(BookingItemStatusEnum.Cancelled))
                {
                    viewModel.OverallStatus = "Partially Cancelled";
                    viewModel.OverallStatusClass = "badge-warning";
                }
                else if (statuses.Contains(BookingItemStatusEnum.Completed))
                {
                    viewModel.OverallStatus = "Partially Completed";
                    viewModel.OverallStatusClass = "badge-info";
                }
                else
                {
                    viewModel.OverallStatus = "Mixed Status";
                    viewModel.OverallStatusClass = "badge-secondary";
                }
            }
        }

        private static List<BookingActionViewModel> GetAvailableActions(BookingItemStatusEnum status)
        {
            var actions = new List<BookingActionViewModel>
            {
                // Always add View Details
                new() {
                    Action = "ViewDetails",
                    DisplayText = "View Details",
                    ButtonClass = "btn-outline-info",
                    Icon = "fas fa-eye"
                }
            };

            // Add status-specific actions
            switch (status)
            {
                case BookingItemStatusEnum.PendingDeposit:
                    actions.Add(new BookingActionViewModel
                    {
                        Action = "Edit",
                        DisplayText = "Edit Booking",
                        ButtonClass = "btn-outline-warning",
                        Icon = "fas fa-edit"
                    });
                    actions.Add(new BookingActionViewModel
                    {
                        Action = "Cancel",
                        DisplayText = "Cancel",
                        ButtonClass = "btn-outline-danger",
                        Icon = "fas fa-times",
                        RequiresConfirmation = true,
                        ConfirmationMessage = "Are you sure you want to cancel this booking?"
                    });
                    break;

                case BookingItemStatusEnum.Confirm:
                    actions.Add(new BookingActionViewModel
                    {
                        Action = "Edit",
                        DisplayText = "Edit Booking",
                        ButtonClass = "btn-outline-warning",
                        Icon = "fas fa-edit"
                    });
                    actions.Add(new BookingActionViewModel
                    {
                        Action = "ConfirmPickup",
                        DisplayText = "Confirm Pick-up",
                        ButtonClass = "btn-outline-success",
                        Icon = "fas fa-car"
                    });
                    actions.Add(new BookingActionViewModel
                    {
                        Action = "Cancel",
                        DisplayText = "Cancel",
                        ButtonClass = "btn-outline-danger",
                        Icon = "fas fa-times",
                        RequiresConfirmation = true,
                        ConfirmationMessage = "Are you sure you want to cancel this booking?"
                    });
                    break;

                case BookingItemStatusEnum.InProgress:
                    actions.Add(new BookingActionViewModel
                    {
                        Action = "ReturnCar",
                        DisplayText = "Return Car",
                        ButtonClass = "btn-outline-warning",
                        Icon = "fas fa-undo"
                    });
                    break;

                case BookingItemStatusEnum.PendingPayment:
                case BookingItemStatusEnum.Completed:
                case BookingItemStatusEnum.Cancelled:
                    // Only view details
                    break;
            }

            return actions;
        }

        private static string GetStatusDisplay(BookingItemStatusEnum status)
        {
            return status switch
            {
                BookingItemStatusEnum.PendingDeposit => "Pending Deposit",
                BookingItemStatusEnum.Confirm => "Confirmed",
                BookingItemStatusEnum.InProgress => "In Progress",
                BookingItemStatusEnum.PendingPayment => "Pending Payment",
                BookingItemStatusEnum.Completed => "Completed",
                BookingItemStatusEnum.Cancelled => "Cancelled",
                _ => status.ToString()
            };
        }

        private static string GetStatusClass(BookingItemStatusEnum status)
        {
            return status switch
            {
                BookingItemStatusEnum.PendingDeposit => "badge-warning",
                BookingItemStatusEnum.Confirm => "badge-info",
                BookingItemStatusEnum.InProgress => "badge-primary",
                BookingItemStatusEnum.PendingPayment => "badge-warning",
                BookingItemStatusEnum.Completed => "badge-success",
                BookingItemStatusEnum.Cancelled => "badge-danger",
                _ => "badge-secondary"
            };
        }
    }
}