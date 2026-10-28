using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTiemChungMinhXuan.Models;

namespace QuanLyTiemChungMinhXuan.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("VaiTro") == "Admin";

        // 1. DASHBOARD THỐNG KÊ TỔNG QUAN
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");

            ViewBag.TongNguoiDan = await _context.NguoiDans.CountAsync();
            ViewBag.TongVacXin = await _context.VacXins.SumAsync(v => v.SoLuongTon);
            ViewBag.TongPhieuCho = await _context.PhieuDangKies.CountAsync(p => p.TrangThai == "Chờ duyệt");
            ViewBag.TongDaTiem = await _context.PhieuDangKies.CountAsync(p => p.TrangThai == "Đã tiêm");

            var phieuMoiNhat = await _context.PhieuDangKies
                .Include(p => p.MaNguoiDanNavigation)
                .Include(p => p.MaDotNavigation)
                    .ThenInclude(d => d.MaVacXinNavigation)
                .OrderByDescending(p => p.NgayDangKy)
                .ToListAsync();

            return View(phieuMoiNhat);
        }

        // 2. DUYỆT PHIẾU & CẬP NHẬT KẾT QUẢ TIÊM (TRỪ TỒN KHO VẮC-XIN TỰ ĐỘNG)
        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThaiPhieu(int maPhieu, string trangThaiMoi, string tinhTrangSauTiem, string canBoTiem)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");

            var phieu = await _context.PhieuDangKies
                .Include(p => p.MaDotNavigation)
                .FirstOrDefaultAsync(p => p.MaPhieu == maPhieu);

            if (phieu != null && phieu.MaDotNavigation != null)
            {
                // Nếu chuyển sang trạng thái "Đã tiêm" lần đầu -> Tự động trừ 1 liều vắc-xin trong kho
                if (phieu.TrangThai != "Đã tiêm" && trangThaiMoi == "Đã tiêm")
                {
                    var vacXin = await _context.VacXins.FindAsync(phieu.MaDotNavigation.MaVacXin);
                    if (vacXin != null && vacXin.SoLuongTon > 0)
                    {
                        vacXin.SoLuongTon -= 1;
                    }
                }

                phieu.TrangThai = trangThaiMoi;
                phieu.TinhTrangSauTiem = string.IsNullOrEmpty(tinhTrangSauTiem) ? "Bình thường" : tinhTrangSauTiem;
                phieu.CanBoTiem = string.IsNullOrEmpty(canBoTiem) ? HttpContext.Session.GetString("HoTen") : canBoTiem;

                await _context.SaveChangesAsync();
                TempData["Msg"] = $"Đã cập nhật phiếu #{maPhieu} sang trạng thái: {trangThaiMoi}!";
            }
            return RedirectToAction("Dashboard");
        }

        // 3. QUẢN LÝ KHO VẮC-XIN
        public async Task<IActionResult> QuanLyVacXin()
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");
            return View(await _context.VacXins.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> ThemVacXin(string TenVacXin, string PhongBenh, string NhaSanXuat, string SoLo, int SoLuongTon, DateOnly HanSuDung)
        {
            var vx = new VacXin
            {
                TenVacXin = TenVacXin,
                PhongBenh = PhongBenh,
                NhaSanXuat = NhaSanXuat,
                SoLo = SoLo,
                SoLuongTon = SoLuongTon,
                HanSuDung = HanSuDung
            };
            _context.VacXins.Add(vx);
            await _context.SaveChangesAsync();
            TempData["Msg"] = "Đã nhập thêm lô vắc-xin mới vào kho!";
            return RedirectToAction("QuanLyVacXin");
        }

        [HttpPost]
        public async Task<IActionResult> XoaVacXin(int id)
        {
            var vx = await _context.VacXins.FindAsync(id);
            if (vx != null)
            {
                _context.VacXins.Remove(vx);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("QuanLyVacXin");
        }

        // 4. QUẢN LÝ ĐỢT TIÊM CHỦNG
        public async Task<IActionResult> QuanLyDotTiem()
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");
            ViewBag.VacXins = await _context.VacXins.ToListAsync();
            var list = await _context.DotTiemChungs
                .Include(d => d.MaVacXinNavigation)
                .OrderByDescending(d => d.NgayTiem)
                .ToListAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> ThemDotTiem(string TenDot, int MaVacXin, DateOnly NgayTiem, string DiaDiem, int SoLuongToiDa, string GhiChu)
        {
            var dot = new DotTiemChung
            {
                TenDot = TenDot,
                MaVacXin = MaVacXin,
                NgayTiem = NgayTiem,
                DiaDiem = DiaDiem,
                SoLuongToiDa = SoLuongToiDa,
                GhiChu = GhiChu
            };
            _context.DotTiemChungs.Add(dot);
            await _context.SaveChangesAsync();
            TempData["Msg"] = "Đã tạo đợt tiêm chủng mới thành công!";
            return RedirectToAction("QuanLyDotTiem");
        }
    }
}