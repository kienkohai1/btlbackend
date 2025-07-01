using System;
using System.Collections.Generic; // Cần thêm dòng này để sử dụng List<T>
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // Cần thêm dòng này để sử dụng IFormFile

namespace BTL.Models
{
    public class Event
    {
        // Constructor được viết đúng cú pháp như thế này
        public Event()
        {
            Tickets = new List<Ticket>();
        }

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

        public List<Ticket> Tickets { get; set; }

        [NotMapped]
        [Display(Name = "Chọn hình ảnh")]
        public IFormFile? ImageFile { get; set; }
    }
}