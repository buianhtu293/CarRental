namespace CarRental.MVC.Models.Booking
{
    public class CashPaymentMethodViewModel
    {
        public bool IsAvailable { get; set; } = true;
        public string Instructions { get; set; } = "Quý khách vui lòng thanh toán ti?n c?c b?ng ti?n m?t khi nh?n xe. Vui lòng mang theo gi?y t? tùy thân và b?ng lái xe h?p l?.";
        public string Note { get; set; } = "L?u ý: Booking s? chuy?n sang tr?ng thái 'Ch? thanh toán' cho ??n khi quý khách th?c hi?n thanh toán.";
    }
}