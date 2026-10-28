using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTiemChungMinhXuan.Models
{
    // 1. BẢNG TÀI KHOẢN
    [Table("TaiKhoan")]
    public class TaiKhoan
    {
        [Key]
        public int MaTK { get; set; }
        public string TenDangNhap { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
        public string? HoTenHienThi { get; set; }
        public string VaiTro { get; set; } = "NguoiDan";
    }

    // 2. BẢNG NGƯỜI DÂN
    [Table("NguoiDan")]
    public class NguoiDan
    {
        [Key]
        public int MaNguoiDan { get; set; }
        public string HoTen { get; set; } = null!;
        public DateOnly NgaySinh { get; set; }
        public string GioiTinh { get; set; } = null!;
        [Column("CCCD")]
        public string Cccd { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string ToDanPho { get; set; } = null!;
        public string? DiaChiChiTiet { get; set; }
        public int? MaTK { get; set; }
    }

    // 3. BẢNG VẮC-XIN
    [Table("VacXin")]
    public class VacXin
    {
        [Key]
        public int MaVacXin { get; set; }
        public string TenVacXin { get; set; } = null!;
        public string PhongBenh { get; set; } = null!;
        public string NhaSanXuat { get; set; } = null!;
        public string SoLo { get; set; } = null!;
        public int SoLuongTon { get; set; }
        public DateOnly HanSuDung { get; set; }
    }

    // 4. BẢNG ĐỢT TIÊM CHỦNG
    [Table("DotTiemChung")]
    public class DotTiemChung
    {
        [Key]
        public int MaDot { get; set; }
        public string TenDot { get; set; } = null!;
        public int MaVacXin { get; set; }
        public DateOnly NgayTiem { get; set; }
        public string? DiaDiem { get; set; } = "Trạm Y tế Phường Minh Xuân";
        public int SoLuongToiDa { get; set; } = 100;
        public string? GhiChu { get; set; }

        [ForeignKey("MaVacXin")]
        public virtual VacXin? MaVacXinNavigation { get; set; }
        public virtual ICollection<PhieuDangKy> PhieuDangKies { get; set; } = new List<PhieuDangKy>();
    }

    // 5. BẢNG PHIẾU ĐĂNG KÝ
    [Table("PhieuDangKy")]
    public class PhieuDangKy
    {
        [Key]
        public int MaPhieu { get; set; }
        public int MaNguoiDan { get; set; }
        public int MaDot { get; set; }
        public int MuiTiemSo { get; set; } = 1;
        public DateTime NgayDangKy { get; set; } = DateTime.Now;
        public string TrangThai { get; set; } = "Chờ duyệt";
        public string? TinhTrangSauTiem { get; set; } = "Chưa tiêm";
        public string? CanBoTiem { get; set; }

        [ForeignKey("MaNguoiDan")]
        public virtual NguoiDan? MaNguoiDanNavigation { get; set; }

        [ForeignKey("MaDot")]
        public virtual DotTiemChung? MaDotNavigation { get; set; }
    }

    // 6. CẤU HÌNH DBCONTEXT KẾT NỐI SQL SERVER
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<NguoiDan> NguoiDans { get; set; }
        public DbSet<VacXin> VacXins { get; set; }
        public DbSet<DotTiemChung> DotTiemChungs { get; set; }
        public DbSet<PhieuDangKy> PhieuDangKies { get; set; }
    }
}