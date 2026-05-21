using ElectronicShopMVC.DataAccess.Repository.IRepository;
using ElectronicShopMVC.Model;
using ElectronicShopMVC.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class VoucherController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VoucherController> _logger;

        public VoucherController(IUnitOfWork unitOfWork, ILogger<VoucherController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Retrieve all vouchers using GetAllAsync with explicit non-deleted filter
                var vouchers = await _unitOfWork.Voucher.GetAllAsync(v => !v.IsDeleted);
                return View(vouchers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vouchers in Admin Index");
                TempData["error"] = "Không thể tải danh sách Voucher.";
                return View(new List<Voucher>());
            }
        }

        public IActionResult Create()
        {
            var voucher = new Voucher
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1),
                DiscountType = "Percentage",
                IsActive = true
            };
            return View(voucher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Voucher voucher)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check duplicate code
                    var existing = _unitOfWork.Voucher.Get(v => v.Code == voucher.Code && !v.IsDeleted);
                    if (existing != null)
                    {
                        ModelState.AddModelError("Code", "Mã giảm giá này đã tồn tại.");
                        return View(voucher);
                    }

                    voucher.Code = voucher.Code?.Trim().ToUpper();
                    voucher.CreatedBy = User.Identity?.Name ?? "Admin";
                    voucher.CreatedAt = DateTime.UtcNow;

                    _unitOfWork.Voucher.Add(voucher);
                    _unitOfWork.Save();

                    TempData["success"] = "Tạo mã giảm giá mới thành công.";
                    return RedirectToAction(nameof(Index));
                }
                return View(voucher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating voucher");
                TempData["error"] = "Đã xảy ra lỗi khi tạo mã giảm giá.";
                return View(voucher);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var voucher = await Task.Run(() => _unitOfWork.Voucher.Get(v => v.Id == id && !v.IsDeleted));
                if (voucher == null)
                {
                    return NotFound();
                }
                return View(voucher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading voucher for edit, ID: {VoucherId}", id);
                TempData["error"] = "Không thể tải thông tin mã giảm giá.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Voucher voucher)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existing = await Task.Run(() => _unitOfWork.Voucher.Get(v => v.Id == voucher.Id && !v.IsDeleted));
                    if (existing == null)
                    {
                        return NotFound();
                    }

                    // Check duplicate code if changed
                    var codeDuplicate = _unitOfWork.Voucher.Get(v => v.Code == voucher.Code && v.Id != voucher.Id && !v.IsDeleted);
                    if (codeDuplicate != null)
                    {
                        ModelState.AddModelError("Code", "Mã giảm giá này đã tồn tại.");
                        return View(voucher);
                    }

                    existing.Code = voucher.Code?.Trim().ToUpper();
                    existing.Title = voucher.Title;
                    existing.Description = voucher.Description;
                    existing.DiscountType = voucher.DiscountType;
                    existing.DiscountValue = voucher.DiscountValue;
                    existing.MinOrderAmount = voucher.MinOrderAmount;
                    existing.MaxDiscountAmount = voucher.MaxDiscountAmount;
                    existing.MaxUses = voucher.MaxUses;
                    existing.StartDate = voucher.StartDate;
                    existing.EndDate = voucher.EndDate;
                    existing.IsActive = voucher.IsActive;
                    
                    existing.UpdatedBy = User.Identity?.Name ?? "Admin";
                    existing.UpdatedAt = DateTime.UtcNow;

                    _unitOfWork.Voucher.Update(existing);
                    _unitOfWork.Save();

                    TempData["success"] = "Cập nhật mã giảm giá thành công.";
                    return RedirectToAction(nameof(Index));
                }
                return View(voucher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing voucher, ID: {VoucherId}", voucher.Id);
                TempData["error"] = "Đã xảy ra lỗi khi cập nhật mã giảm giá.";
                return View(voucher);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var voucher = _unitOfWork.Voucher.Get(v => v.Id == id && !v.IsDeleted);
                if (voucher == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy voucher hoặc đã bị xóa." });
                }

                // Remove triggers the EF Interceptor which updates IsDeleted = true
                _unitOfWork.Voucher.Remove(voucher);
                _unitOfWork.Save();

                _logger.LogInformation("Admin soft-deleted Voucher {VoucherId} ({Code})", id, voucher.Code);
                return Json(new { success = true, message = "Xóa mã giảm giá thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting voucher {VoucherId}", id);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa mã giảm giá." });
            }
        }
    }
}
