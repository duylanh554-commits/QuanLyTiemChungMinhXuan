using Microsoft.EntityFrameworkCore;
using QuanLyTiemChungMinhXuan.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// LƯU Ý: Chuỗi kết nối dùng LocalDB có sẵn của Visual Studio (chạy 100% trên mọi máy mà không cần cấu hình tên Server)
// Hoặc nếu bạn muốn nhìn thấy trong SSMS thì đổi "(localdb)\\MSSQLLocalDB" thành ".\\SQLEXPRESS" hoặc "localhost"
var connectionString = "Server=.\\SQLEXPRESS;Database=QuanLyTiemChung_MinhXuan;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// TỰ ĐỘNG TẠO BẢNG VÀ NẠP DỮ LIỆU MẪU NGAY KHI BẤM CHẠY WEB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // Tự động tạo Database nếu chưa có

    if (!db.TaiKhoans.Any())
    {
        db.TaiKhoans.AddRange(
            new TaiKhoan { TenDangNhap = "admin", MatKhau = "123456", HoTenHienThi = "Trạm Trưởng Y Tế Minh Xuân", VaiTro = "Admin" },
            new TaiKhoan { TenDangNhap = "congdan1", MatKhau = "123456", HoTenHienThi = "Nguyễn Văn An", VaiTro = "NguoiDan" }
        );
        db.SaveChanges();

        var vx1 = new VacXin { TenVacXin = "Vaxigrip Tetra", PhongBenh = "Cúm mùa 4 chủng", NhaSanXuat = "Sanofi Pasteur (Pháp)", SoLo = "VX-2026-01", SoLuongTon = 150, HanSuDung = new DateOnly(2027, 12, 31) };
        var vx2 = new VacXin { TenVacXin = "Engerix-B", PhongBenh = "Viêm gan B", NhaSanXuat = "GSK (Bỉ)", SoLo = "EB-2026-09", SoLuongTon = 85, HanSuDung = new DateOnly(2027, 6, 30) };
        var vx3 = new VacXin { TenVacXin = "Gardasil 9", PhongBenh = "Ung thư cổ tử cung (HPV)", NhaSanXuat = "MSD (Mỹ)", SoLo = "GD-2026-05", SoLuongTon = 40, HanSuDung = new DateOnly(2028, 1, 15) };
        db.VacXins.AddRange(vx1, vx2, vx3);
        db.SaveChanges();

        var dot1 = new DotTiemChung { TenDot = "Chiến dịch tiêm phòng Cúm mùa Đợt 1 - P. Minh Xuân", MaVacXin = vx1.MaVacXin, NgayTiem = new DateOnly(2026, 11, 15), DiaDiem = "Trạm Y tế Phường Minh Xuân (Bàn tiêm số 1)", SoLuongToiDa = 100, GhiChu = "Mang theo CCCD và thẻ BHYT" };
        var dot2 = new DotTiemChung { TenDot = "Tiêm phòng Viêm gan B định kỳ tháng 11", MaVacXin = vx2.MaVacXin, NgayTiem = new DateOnly(2026, 11, 20), DiaDiem = "Trạm Y tế Phường Minh Xuân (Bàn tiêm số 2)", SoLuongToiDa = 50, GhiChu = "Khám sàng lọc miễn phí trước tiêm" };
        var dot3 = new DotTiemChung { TenDot = "Tiêm vắc-xin HPV cho thanh thiếu niên", MaVacXin = vx3.MaVacXin, NgayTiem = new DateOnly(2026, 11, 25), DiaDiem = "Nhà văn hóa Tổ 5 - Phường Minh Xuân", SoLuongToiDa = 80, GhiChu = "Ưu tiên các hộ dân Tổ 1 đến Tổ 10" };
        db.DotTiemChungs.AddRange(dot1, dot2, dot3);
        db.SaveChanges();

        var nd1 = new NguoiDan { HoTen = "Nguyễn Văn An", NgaySinh = new DateOnly(1998, 5, 12), GioiTinh = "Nam", Cccd = "008098001234", SoDienThoai = "0912345678", ToDanPho = "Tổ 5", DiaChiChiTiet = "Số 12 Đường Bình Thuận, P. Minh Xuân" };
        db.NguoiDans.Add(nd1);
        db.SaveChanges();

        db.PhieuDangKies.Add(new PhieuDangKy { MaNguoiDan = nd1.MaNguoiDan, MaDot = dot1.MaDot, MuiTiemSo = 1, TrangThai = "Đã tiêm", TinhTrangSauTiem = "Bình thường (Đã theo dõi 30 phút)", CanBoTiem = "BS. Lê Hoàng Nam" });
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();