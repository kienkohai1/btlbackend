using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Thêm dòng này

namespace BTL.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sự kiện là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sự kiện không được vượt quá 100 ký tự.")]
        public string Name { get; set; }

        [Display(Name = "Ngày và Giờ")]
        [DataType(DataType.DateTime)]
        public DateTime DateTime { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Địa điểm là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Địa điểm không được vượt quá 200 ký tự.")]
        public string Location { get; set; }

        [Display(Name = "Hình ảnh")]
        public string? ImagePath { get; set; } // Đường dẫn lưu trữ hình ảnh (có thể null)

        [NotMapped]
        [Display(Name = "Chọn hình ảnh")]
        public IFormFile? ImageFile { get; set; }
    }
}