using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTiemChungMinhXuan.Models;

namespace QuanLyTiemChungMinhXuan.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // 1. TRANG CHỦ: Hiển thị các đợt tiêm chủng đang mở tại Phường Minh Xuân
        public async Task<IActionResult> Index()
        {
            var danhSachDotTiem = await _context.DotTiemChungs
                .Include(d => d.MaVacXinNavigation)
                .Include(d => d.PhieuDangKies)
                .OrderBy(d => d.NgayTiem)
                .ToListAsync();
            return View(danhSachDotTiem);
        }

        // 2. ĐĂNG KÝ TIÊM TRỰC TUYẾN (GET & POST)
        [HttpGet]
        public async Task<IActionResult> DangKyTiem(int? maDot)
        {
            ViewBag.DotTiemList = await _context.DotTiemChungs
                .Include(d => d.MaVacXinNavigation)
                .ToListAsync();
            ViewBag.SelectedDot = maDot;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangKyTiem(string HoTen, DateOnly NgaySinh, string GioiTinh, string CCCD, string SoDienThoai, string ToDanPho, string DiaChiChiTiet, int MaDot, int MuiTiemSo)
        {
            // Kiểm tra xem người dân đã có hồ sơ theo CCCD chưa
            var nguoiDan = await _context.NguoiDans.FirstOrDefaultAsync(n => n.Cccd == CCCD);
            if (nguoiDan == null)
            {
                nguoiDan = new NguoiDan
                {
                    HoTen = HoTen,
                    NgaySinh = NgaySinh,
                    GioiTinh = GioiTinh,
                    Cccd = CCCD,
                    SoDienThoai = SoDienThoai,
                    ToDanPho = ToDanPho,
                    DiaChiChiTiet = DiaChiChiTiet
                };
                _context.NguoiDans.Add(nguoiDan);
                await _context.SaveChangesAsync();
            }

            // Tạo phiếu đăng ký tiêm
            var phieu = new PhieuDangKy
            {
                MaNguoiDan = nguoiDan.MaNguoiDan,
                MaDot = MaDot,
                MuiTiemSo = MuiTiemSo,
                NgayDangKy = DateTime.Now,
                TrangThai = "Chờ duyệt",
                TinhTrangSauTiem = "Chưa tiêm"
            };
            _context.PhieuDangKies.Add(phieu);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đăng ký tiêm thành công cho công dân {HoTen} (CCCD: {CCCD})! Mã phiếu của bạn là #{phieu.MaPhieu}.";
            return RedirectToAction("TraCuu", new { cccd = CCCD });
        }

        // 3. TRA CỨU SỔ TIÊM CHỦNG ĐIỆN TỬ THEO CCCD / SĐT
        [HttpGet]
        public async Task<IActionResult> TraCuu(string? cccd)
        {
            ViewBag.TuKhoa = cccd;
            if (string.IsNullOrEmpty(cccd))
            {
                return View(new List<PhieuDangKy>());
            }

            var ketQua = await _context.PhieuDangKies
                .Include(p => p.MaNguoiDanNavigation)
                .Include(p => p.MaDotNavigation)
                    .ThenInclude(d => d.MaVacXinNavigation)
                .Where(p => p.MaNguoiDanNavigation.Cccd == cccd || p.MaNguoiDanNavigation.SoDienThoai == cccd)
                .OrderByDescending(p => p.NgayDangKy)
                .ToListAsync();

            return View(ketQua);
        }

        // 4. ĐĂNG NHẬP HỆ THỐNG QUẢN TRỊ
        [HttpGet]
        public IActionResult DangNhap() => View();

        [HttpPost]
        public async Task<IActionResult> DangNhap(string tenDangNhap, string matKhau)
        {
            var tk = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap && t.MatKhau == matKhau);

            if (tk != null)
            {
                HttpContext.Session.SetString("TenDangNhap", tk.TenDangNhap);
                HttpContext.Session.SetString("HoTen", tk.HoTenHienThi ?? tk.TenDangNhap);
                HttpContext.Session.SetString("VaiTro", tk.VaiTro);

                if (tk.VaiTro == "Admin")
                    return RedirectToAction("Dashboard", "Admin");
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác!";
            return View();
        }

        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}