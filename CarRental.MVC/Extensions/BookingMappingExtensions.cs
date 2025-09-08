using CarRental.Application.DTOs.Booking;
using CarRental.Domain.Enums;
using CarRental.MVC.Models.Booking;

namespace CarRental.MVC.Extensions
{
    public static class BookingMappingExtensions
    {
        #region DTO to ViewModel Mappings

        public static BookingInformationViewModel ToViewModel(this BookingInformationDto dto)
        {
            return new BookingInformationViewModel
            {
                Progress = dto.Progress.ToViewModel(),
                Summary = dto.Summary.ToViewModel(),
                //Renter = dto.Renter.ToViewModel(),
                Drivers = dto.Drivers.Select(d => d.ToViewModel()).ToList(),
                BookingSessionId = dto.BookingSessionId,
                SelectedCarIds = dto.SelectedCarIds,
                PickupDate = dto.PickupDate,
                ReturnDate = dto.ReturnDate
            };
        }

        public static BookingProgressViewModel ToViewModel(this BookingProgressDto dto)
        {
            return new BookingProgressViewModel
            {
                CurrentStep = dto.CurrentStep,
                TotalSteps = dto.TotalSteps,
                BookingSessionId = dto.BookingSessionId
            };
        }

        public static CarInformationViewModel ToViewModel(this CarInformationDto dto)
        {
            return new CarInformationViewModel
            {
                Id = dto.Id,
                Brand = dto.Brand,
                Model = dto.Model,
                LicensePlate = dto.LicensePlate,
                ImageUrl = dto.ImageUrl,
                PricePerDay = dto.PricePerDay,
                RequiredDeposit = dto.RequiredDeposit,
                AverageRating = dto.AverageRating,
                TotalTrips = dto.TotalTrips,
                Location = dto.Location,
                Status = dto.Status,
                Color = dto.Color,
                Seats = dto.Seats,
                Transmission = dto.Transmission,
                FuelType = dto.FuelType
            };
        }

        public static BookingSummaryViewModel ToViewModel(this BookingSummaryDto dto)
        {
            return new BookingSummaryViewModel
            {
                NumberOfDays = dto.NumberOfDays,
                PricePerDay = dto.PricePerDay,
                TotalAmount = dto.TotalAmount,
                TotalDeposit = dto.TotalDeposit,
                PickupDate = dto.PickupDate,
                ReturnDate = dto.ReturnDate,
                NumberOfCars = dto.NumberOfCars,
                CarItems = dto.CarItems.Select(c => c.ToViewModel()).ToList()
            };
        }

        public static CarSummaryItem ToViewModel(this CarSummaryItemDto dto)
        {
            return new CarSummaryItem
            {
                CarId = dto.CarId,
                CarName = dto.CarName,
                PricePerDay = dto.PricePerDay,
                Deposit = dto.Deposit,
                SubTotal = dto.SubTotal,
                Transmission = dto.Transmission,
                Seats = dto.Seats,
                AverageRating = dto.AverageRating,
                Location = dto.Location,
                LicensePlate = dto.LicensePlate,
                FuelType = dto.FuelType,
                TotalTrips = dto.TotalTrips
            };
        }

        public static DriverInformationViewModel ToViewModel(this DriverInformationDto dto)
        {
            return new DriverInformationViewModel
            {
                CarId = dto.CarId,
                CarName = dto.CarName,
                IsSameAsRenter = dto.IsSameAsRenter,
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                LicenseNumber = dto.LicenseNumber,
                LicenseImage = dto.LicenseImageUrl, // Map LicenseImageUrl t? DTO sang LicenseImage c?a ViewModel
                Address = dto.Address,
                City = dto.City, // This should work now
                District = dto.District, // This should work now
                Ward = dto.Ward // Added Ward field
            };
        }

        public static RenterInformationViewModel ToViewModel(this RenterInformationDto dto)
        {
            return new RenterInformationViewModel
            {
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                LicenseNumber = dto.LicenseNumber,
                LicenseImage = dto.LicenseImageUrl, // Map LicenseImageUrl t? DTO sang LicenseImage c?a ViewModel
                Address = dto.Address,
                City = dto.City,
                District = dto.District,
                Ward = dto.Ward
            };
        }

        public static PaymentViewModel ToViewModel(this PaymentDto dto)
        {
            return new PaymentViewModel
            {
                Progress = dto.Progress.ToViewModel(),
                BookingSummary = dto.BookingSummary.ToViewModel(),
                PaymentMethods = dto.PaymentMethods.ToViewModel(),
                SelectedPaymentMethod = (PaymentMethodTypeEnum)(int)dto.SelectedPaymentMethod,
                BookingSessionId = dto.BookingSessionId,
                UserWallet = dto.UserWallet.ToViewModel()
            };
        }

        public static PaymentMethodsViewModel ToViewModel(this PaymentMethodsDto dto)
        {
            return new PaymentMethodsViewModel
            {
                Wallet = dto.Wallet.ToViewModel(),
                Cash = dto.Cash.ToViewModel(),
                BankTransfer = dto.BankTransfer.ToViewModel()
            };
        }

        public static WalletPaymentMethodViewModel ToViewModel(this WalletPaymentMethodDto dto)
        {
            return new WalletPaymentMethodViewModel
            {
                IsAvailable = dto.IsAvailable,
                CurrentBalance = dto.CurrentBalance,
                RequiredAmount = dto.RequiredAmount
            };
        }

        public static CashPaymentMethodViewModel ToViewModel(this CashPaymentMethodDto dto)
        {
            return new CashPaymentMethodViewModel
            {
                IsAvailable = dto.IsAvailable,
                Instructions = dto.Instructions,
                Note = dto.Note
            };
        }

        public static BankTransferPaymentMethodViewModel ToViewModel(this BankTransferPaymentMethodDto dto)
        {
            return new BankTransferPaymentMethodViewModel
            {
                IsAvailable = dto.IsAvailable,
                Instructions = dto.Instructions,
                Note = dto.Note
            };
        }

        public static WalletInformationViewModel ToViewModel(this WalletInformationDto dto)
        {
            return new WalletInformationViewModel
            {
                Balance = dto.Balance,
                HasWallet = dto.HasWallet
            };
        }

        public static BookingFinishViewModel ToViewModel(this BookingFinishDto dto)
        {
            return new BookingFinishViewModel
            {
                Progress = dto.Progress.ToViewModel(),
                BookingNumber = dto.BookingNumber,
                BookingStatus = dto.BookingStatus,
                BookingDate = dto.BookingDate,
                BookingSummary = dto.BookingSummary.ToViewModel(),
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = dto.PaymentStatus,
                NextStepsTitle = dto.NextStepsTitle,
                NextSteps = dto.NextSteps,
                ContactPhone = dto.ContactPhone,
                ContactEmail = dto.ContactEmail,
                SuccessMessage = dto.SuccessMessage,
                DetailMessage = dto.DetailMessage,
                ActionButtons = dto.ActionButtons
            };
        }

        #endregion

        #region ViewModel to DTO Mappings

        public static BookingInformationDto ToDto(this BookingInformationViewModel viewModel)
        {
            return new BookingInformationDto
            {
                Progress = viewModel.Progress.ToDto(),
                Summary = viewModel.Summary.ToDto(),
                Renter = viewModel.Renter.ToDto(),
                Drivers = viewModel.Drivers.Select(d => d.ToDto()).ToList(),
                BookingSessionId = viewModel.BookingSessionId,
                SelectedCarIds = viewModel.SelectedCarIds,
                PickupDate = viewModel.PickupDate,
                ReturnDate = viewModel.ReturnDate,
                
            };
        }

        public static BookingProgressDto ToDto(this BookingProgressViewModel viewModel)
        {
            return new BookingProgressDto
            {
                CurrentStep = viewModel.CurrentStep,
                TotalSteps = viewModel.TotalSteps,
                BookingSessionId = viewModel.BookingSessionId
            };
        }

        public static CarInformationDto ToDto(this CarInformationViewModel viewModel)
        {
            return new CarInformationDto
            {
                Id = viewModel.Id,
                Brand = viewModel.Brand,
                Model = viewModel.Model,
                LicensePlate = viewModel.LicensePlate,
                ImageUrl = viewModel.ImageUrl,
                PricePerDay = viewModel.PricePerDay,
                RequiredDeposit = viewModel.RequiredDeposit,
                AverageRating = viewModel.AverageRating,
                TotalTrips = viewModel.TotalTrips,
                Location = viewModel.Location,
                Status = viewModel.Status,
                Color = viewModel.Color,
                Seats = viewModel.Seats,
                Transmission = viewModel.Transmission,
                FuelType = viewModel.FuelType,
                
            };
        }

        public static BookingSummaryDto ToDto(this BookingSummaryViewModel viewModel)
        {
            return new BookingSummaryDto
            {
                NumberOfDays = viewModel.NumberOfDays,
                PricePerDay = viewModel.PricePerDay,
                TotalAmount = viewModel.TotalAmount,
                TotalDeposit = viewModel.TotalDeposit,
                PickupDate = viewModel.PickupDate,
                ReturnDate = viewModel.ReturnDate,
                NumberOfCars = viewModel.NumberOfCars,
                CarItems = viewModel.CarItems.Select(c => c.ToDto()).ToList()
            };
        }

        public static CarSummaryItemDto ToDto(this CarSummaryItem viewModel)
        {
            return new CarSummaryItemDto
            {
                CarId = viewModel.CarId,
                CarName = viewModel.CarName,
                PricePerDay = viewModel.PricePerDay,
                Deposit = viewModel.Deposit,
                SubTotal = viewModel.SubTotal,
                LicensePlate = viewModel.LicensePlate,
                AverageRating = viewModel.AverageRating,
                FuelType = viewModel.FuelType,
                Location = viewModel.Location,
                Seats = viewModel.Seats,
                TotalTrips = viewModel.TotalTrips,
                Transmission = viewModel.Transmission
            };
        }

        public static RenterInformationDto ToDto(this RenterInformationViewModel viewModel)
        {
            return new RenterInformationDto
            {
                FullName = viewModel.FullName,
                DateOfBirth = viewModel.DateOfBirth,
                PhoneNumber = viewModel.PhoneNumber,
                Email = viewModel.Email,
                LicenseNumber = viewModel.LicenseNumber,
                LicenseImageUrl = viewModel.LicenseImage ?? string.Empty, // S? d?ng LicenseImage thay vì LicenseImageUrl
                Address = viewModel.Address,
                City = viewModel.City,
                District = viewModel.District,
                Ward = viewModel.Ward
            };
        }

        public static DriverInformationDto ToDto(this DriverInformationViewModel viewModel)
        {
            return new DriverInformationDto
            {
                CarId = viewModel.CarId,
                CarName = viewModel.CarName,
                IsSameAsRenter = viewModel.IsSameAsRenter,
                FullName = viewModel.FullName,
                DateOfBirth = viewModel.DateOfBirth,
                PhoneNumber = viewModel.PhoneNumber,
                Email = viewModel.Email,
                LicenseNumber = viewModel.LicenseNumber,
                LicenseImageUrl = viewModel.LicenseImage ?? string.Empty, // S? d?ng LicenseImage thay vì LicenseImageUrl
                Address = viewModel.Address,
                City = viewModel.City,
                District = viewModel.District,
                Ward = viewModel.Ward // Added Ward field
            };
        }

        public static PaymentDto ToDto(this PaymentViewModel viewModel)
        {
            return new PaymentDto
            {
                Progress = viewModel.Progress.ToDto(),
                BookingSummary = viewModel.BookingSummary.ToDto(),
                PaymentMethods = viewModel.PaymentMethods.ToDto(),
                SelectedPaymentMethod = (PaymentMethodTypeEnum)(int)viewModel.SelectedPaymentMethod,
                BookingSessionId = viewModel.BookingSessionId,
                UserWallet = viewModel.UserWallet.ToDto()
            };
        }

        public static PaymentMethodsDto ToDto(this PaymentMethodsViewModel viewModel)
        {
            return new PaymentMethodsDto
            {
                Wallet = viewModel.Wallet.ToDto(),
                Cash = viewModel.Cash.ToDto(),
                BankTransfer = viewModel.BankTransfer.ToDto()
            };
        }

        public static WalletPaymentMethodDto ToDto(this WalletPaymentMethodViewModel viewModel)
        {
            return new WalletPaymentMethodDto
            {
                IsAvailable = viewModel.IsAvailable,
                CurrentBalance = viewModel.CurrentBalance,
                RequiredAmount = viewModel.RequiredAmount
            };
        }

        public static CashPaymentMethodDto ToDto(this CashPaymentMethodViewModel viewModel)
        {
            return new CashPaymentMethodDto
            {
                IsAvailable = viewModel.IsAvailable,
                Instructions = viewModel.Instructions,
                Note = viewModel.Note
            };
        }

        public static BankTransferPaymentMethodDto ToDto(this BankTransferPaymentMethodViewModel viewModel)
        {
            return new BankTransferPaymentMethodDto
            {
                IsAvailable = viewModel.IsAvailable,
                Instructions = viewModel.Instructions,
                Note = viewModel.Note
            };
        }

        public static WalletInformationDto ToDto(this WalletInformationViewModel viewModel)
        {
            return new WalletInformationDto
            {
                Balance = viewModel.Balance,
                HasWallet = viewModel.HasWallet
            };
        }

        #endregion
    }
}