using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.DTOs
{
    public class SearchFormModel
    {
        [Display(Name = "Địa chỉ cụ thể (số nhà, tên đường)")]
        public string? Address { get; set; }

        [Display(Name = "Tỉnh/Thành phố")]
        public string? ProvinceName { get; set; }

        [Display(Name = "Mã Tỉnh/Thành phố")] 
        public int? ProvinceId { get; set; }

        [Display(Name = "Quận/Huyện")]
        public string? DistrictName { get; set; }

        [Display(Name = "Mã Quận/Huyện")] 
        public int? DistrictId { get; set; }

        [Display(Name = "Phường/Xã")]
        public string? WardName { get; set; }

        [Display(Name = "Mã Phường/Xã")]
        public int? WardId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày nhận xe.")]
        [Display(Name = "Ngày nhận")]
        public DateTime? PickupDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ nhận xe.")]
        [Display(Name = "Giờ nhận")]
        [Range(0, 23, ErrorMessage = "Giờ không hợp lệ.")]
        public int? PickupTime { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày trả xe.")]
        [Display(Name = "Ngày trả")]
        public DateTime? ReturnDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ trả xe.")]
        [Display(Name = "Giờ trả")]
        [Range(0, 23, ErrorMessage = "Giờ không hợp lệ.")]
        public int? ReturnTime { get; set; }

        /// <summary>
        /// Thuộc tính tính toán để kết hợp ngày và giờ nhận xe.
        /// Controller và Service sẽ sử dụng thuộc tính này.
        /// </summary>
        public DateTime? CombinedPickupDateTime => PickupDate.HasValue && PickupTime.HasValue
            ? PickupDate.Value.Date.AddHours(PickupTime.Value)
            : null;

        /// <summary>
        /// Thuộc tính tính toán để kết hợp ngày và giờ trả xe.
        /// Controller và Service sẽ sử dụng thuộc tính này.
        /// </summary>
        public DateTime? CombinedReturnDateTime => ReturnDate.HasValue && ReturnTime.HasValue
            ? ReturnDate.Value.Date.AddHours(ReturnTime.Value)
            : null;
    }
}
