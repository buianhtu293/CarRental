using System.ComponentModel.DataAnnotations;

namespace CarRental.Domain.Enums
{
    public enum BookingItemStatusEnum
    {
        [Display(Name = "PendingDeposit")]
        PendingDeposit = 1,

        [Display(Name = "Confirm")]
        Confirm = 2,

        [Display(Name = "InProgress")]
        InProgress = 3,

        [Display(Name = "PendingPayment")]
        PendingPayment = 4,

        [Display(Name = "Completed")]
        Completed = 5,

        [Display(Name = "Cancelled")]
        Cancelled = 6,
    }
}
