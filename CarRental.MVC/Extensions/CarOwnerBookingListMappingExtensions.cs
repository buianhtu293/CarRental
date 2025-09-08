using CarRental.Application.DTOs.Booking;
using CarRental.Domain.Enums;
using CarRental.MVC.Models.Booking;

namespace CarRental.MVC.Extensions
{
    public static class CarOwnerBookingListMappingExtensions
    {
        public static BookingListViewModel ToViewModelCarOwner(this BookingListResponseDto dto)
        {
            return new BookingListViewModel
            {
                Bookings = dto.Bookings.Select(b => b.ToViewModelCarOwner()).ToList(),
                TotalCount = dto.TotalCount,
                CurrentPage = dto.CurrentPage,
                PageSize = dto.PageSize
            };
        }

        public static BookingItemViewModel ToViewModelCarOwner(this BookingListDto dto)
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
                BookingItems = dto.BookingItems.Select(bi => bi.ToViewModelCarOwner()).ToList()
            };

            // Determine overall status
            SetOverallStatus(viewModel);

            return viewModel;
        }

        public static BookingCarItemViewModel ToViewModelCarOwner(this BookingItemListDto dto)
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
                        Action = "ConfirmDeposit",
                        DisplayText = "Confirm Deposit",
                        ButtonClass = "btn-outline-warning",
                        Icon = "fas fa-wallet"
                    });
                    break;

                case BookingItemStatusEnum.Confirm:
                    break;

                case BookingItemStatusEnum.InProgress:
                    break;

                case BookingItemStatusEnum.PendingPayment:
                    actions.Add(new BookingActionViewModel
                    {
                        Action = "ConfirmPayment",
                        DisplayText = "Confirm Payment",
                        ButtonClass = "btn-outline-primary",
                        Icon = "fas fa-credit-card"
                    });
                    break;

                case BookingItemStatusEnum.Completed:
                    break;

                case BookingItemStatusEnum.Cancelled:
                    // Only view details
                    break;
            }

            return actions;
        }
    }
}
