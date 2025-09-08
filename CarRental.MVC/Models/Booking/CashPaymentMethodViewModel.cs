namespace CarRental.MVC.Models.Booking
{
    public class CashPaymentMethodViewModel
    {
        public bool IsAvailable { get; set; } = true;
        public string Instructions { get; set; } = "Qu� kh�ch vui l�ng thanh to�n ti?n c?c b?ng ti?n m?t khi nh?n xe. Vui l�ng mang theo gi?y t? t�y th�n v� b?ng l�i xe h?p l?.";
        public string Note { get; set; } = "L?u �: Booking s? chuy?n sang tr?ng th�i 'Ch? thanh to�n' cho ??n khi qu� kh�ch th?c hi?n thanh to�n.";
    }
}