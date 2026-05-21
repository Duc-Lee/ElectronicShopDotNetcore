// E-Shop Interactive Cart & Voucher Engine
let activeCoupon = null;

const itemQuantityInputs = document.querySelectorAll('.cart-item-quantity-input');
const itemDeleteButtons = document.querySelectorAll('.cart-item-remove-btn');

// Load any previously applied coupon from sessionStorage (good UX!)
if (sessionStorage.getItem('eshop_coupon')) {
    activeCoupon = sessionStorage.getItem('eshop_coupon');
}

// Quantity inputs event listeners
itemQuantityInputs.forEach(input => {
    input.addEventListener('change', () => {
        const productId = parseInt(input.getAttribute('data-id'));
        let quantity = parseInt(input.value);
        
        if (isNaN(quantity) || quantity < 1) {
            quantity = 1;
            input.value = 1;
        }

        fetch(`/api/user/cart/item/${productId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: quantity
        })
        .then(res => {
            if (!res.ok) throw new Error("Gặp lỗi khi cập nhật.");
            return res.json();
        })
        .then(data => {
            const toast = {
                title: "Thành công",
                message: "Cập nhật số lượng thành công.",
                status: TOAST_STATUS.SUCCESS,
                timeout: 2000
            };

            if (!data || data.status != "success") {
                toast.title = "Lỗi";
                toast.message = "Gặp lỗi khi cập nhật số lượng.";
                toast.status = TOAST_STATUS.DANGER;
            }
            Toast.create(toast);
            
            if (data.status === "success") {
                refreshItemQuantity(productId);
                refreshProductPrice(productId, data.data.items);
                
                // Recalculate prices considering any active vouchers
                const updatedPrices = calculateDiscount(data.data);
                updateTotalPrices(updatedPrices);
            }
        })
        .catch(err => {
            Toast.create({
                title: "Lỗi",
                message: err.message || "Đã xảy ra lỗi kết nối.",
                status: TOAST_STATUS.DANGER,
                timeout: 2000
            });
        });
    });
});

// Delete buttons event listeners
itemDeleteButtons.forEach(button => {
    button.addEventListener('click', () => {
        const productId = parseInt(button.getAttribute('data-id'));

        Swal.fire({
            title: 'Xóa sản phẩm?',
            text: "Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3b82f6',
            cancelButtonColor: '#f43f5e',
            confirmButtonText: 'Đồng ý',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                fetch(`/api/user/cart/item/${productId}`, {
                    method: 'DELETE',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                })
                .then(res => res.json())
                .then(data => {
                    if (data.status === "success") {
                        removeCartItem(productId);
                        refreshCartProductAmount(data.data.itemsQuantity);
                        
                        // Recalculate prices
                        const updatedPrices = calculateDiscount(data.data);
                        updateTotalPrices(updatedPrices);

                        Swal.fire({
                            title: 'Đã xóa!',
                            text: 'Sản phẩm đã được xóa khỏi giỏ hàng.',
                            icon: 'success',
                            timer: 1500,
                            showConfirmButton: false
                        });

                        // Check if cart is empty
                        if (data.data.itemsQuantity === 0) {
                            displayEmptyCart();
                        }
                    } else {
                        Swal.fire('Lỗi', 'Không thể xóa sản phẩm.', 'error');
                    }
                })
                .catch(() => {
                    Swal.fire('Lỗi kết nối', 'Vui lòng kiểm tra lại đường truyền!', 'error');
                });
            }
        });
    });
});

// Coupon / Voucher System Implementation
const applyCouponBtn = document.querySelector('#apply-coupon-btn');
const couponInput = document.querySelector('#coupon-code');

if (applyCouponBtn && couponInput) {
    // Apply previously stored coupon on load
    if (activeCoupon) {
        couponInput.value = activeCoupon;
        setTimeout(() => {
            applyCoupon(true); // silent apply on load
        }, 500);
    }

    applyCouponBtn.addEventListener('click', () => {
        applyCoupon(false);
    });

    couponInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            applyCoupon(false);
        }
    });
}

function applyCoupon(silent = false) {
    const code = couponInput.value.trim().toUpperCase();
    const messageEl = document.querySelector('#coupon-message');
    
    if (!code) {
        if (!silent) {
            Swal.fire('Chú ý', 'Vui lòng nhập mã giảm giá.', 'info');
        }
        return;
    }

    // Supported vouchers: ESHOP10, FREESHIP, WELCOME50
    const validCodes = ['ESHOP10', 'FREESHIP', 'WELCOME50'];
    
    if (validCodes.includes(code)) {
        activeCoupon = code;
        sessionStorage.setItem('eshop_coupon', code);
        
        // Fetch current cart values to apply voucher on
        fetch('/api/user/cart')
        .then(res => res.json())
        .then(data => {
            if (data.status === "success") {
                const updatedPrices = calculateDiscount(data.data);
                updateTotalPrices(updatedPrices);
                
                messageEl.className = 'small mt-1 text-success fw-semibold';
                let promoDesc = "";
                if (code === 'ESHOP10') promoDesc = "Giảm 10% tổng giá trị tạm tính.";
                if (code === 'FREESHIP') promoDesc = "Miễn phí vận chuyển 100%.";
                if (code === 'WELCOME50') promoDesc = "Giảm ngay 50.000 ₫ cho khách hàng mới.";
                
                messageEl.innerHTML = `<i class="bi bi-patch-check-fill me-1"></i> Đã áp dụng mã <strong>${code}</strong>: ${promoDesc}`;
                messageEl.classList.remove('d-none');

                if (!silent) {
                    Swal.fire({
                        title: 'Áp dụng thành công!',
                        text: `Bạn đã được áp dụng mã giảm giá ${code}`,
                        icon: 'success',
                        confirmButtonColor: '#a855f7'
                    });
                }
            }
        });
    } else {
        activeCoupon = null;
        sessionStorage.removeItem('eshop_coupon');
        messageEl.className = 'small mt-1 text-danger fw-semibold';
        messageEl.innerHTML = `<i class="bi bi-exclamation-triangle-fill me-1"></i> Mã giảm giá không hợp lệ.`;
        messageEl.classList.remove('d-none');
        
        // Revert to original prices
        fetch('/api/user/cart')
        .then(res => res.json())
        .then(data => {
            if (data.status === "success") {
                updateTotalPrices(data.data);
                document.querySelector('#discount-row').classList.add('d-none');
            }
        });

        if (!silent) {
            Swal.fire('Thất bại', 'Mã giảm giá không hợp lệ hoặc đã hết hạn!', 'error');
        }
    }
}

// Helper to calculate discounts dynamically on client side
function calculateDiscount(cart) {
    const result = {
        subtotal: { raw: cart.subtotal.raw, formatted: cart.subtotal.formatted },
        vat: { raw: cart.vat.raw, formatted: cart.vat.formatted },
        shipping: { raw: cart.shipping.raw, formatted: cart.shipping.formatted },
        total: { raw: cart.total.raw, formatted: cart.total.formatted },
        discount: 0,
        discountFormatted: "0 ₫"
    };

    if (activeCoupon === 'ESHOP10') {
        result.discount = result.subtotal.raw * 0.1;
        result.discountFormatted = "-" + formatVND(result.discount);
        result.total.raw = Math.max(0, result.subtotal.raw + result.vat.raw + result.shipping.raw - result.discount);
        result.total.formatted = formatVND(result.total.raw);
    } else if (activeCoupon === 'FREESHIP') {
        result.discount = result.shipping.raw;
        result.discountFormatted = "-" + formatVND(result.discount);
        result.shipping.raw = 0;
        result.shipping.formatted = "0 ₫";
        result.total.raw = Math.max(0, result.subtotal.raw + result.vat.raw - result.discount);
        result.total.formatted = formatVND(result.total.raw);
    } else if (activeCoupon === 'WELCOME50') {
        result.discount = Math.min(result.subtotal.raw, 50000);
        result.discountFormatted = "-" + formatVND(result.discount);
        result.total.raw = Math.max(0, result.subtotal.raw + result.vat.raw + result.shipping.raw - result.discount);
        result.total.formatted = formatVND(result.total.raw);
    }

    return result;
}

function formatVND(value) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })
        .format(value)
        .replace(/₫/, '₫') // normalize spacing
        .trim();
}

function removeCartItem(productId) {
    const item = document.querySelector(`#cart-item-${productId}`);
    if (item) {
        item.style.opacity = '0';
        item.style.transform = 'scale(0.9)';
        setTimeout(() => {
            item.remove();
        }, 400);
    }
}

function refreshItemQuantity(productId) {
    const itemQuantityInput = document.querySelector(`#quantity-${productId}-input`);
    const itemQuantity = document.querySelector(`#quantity-${productId}`);
    if (itemQuantityInput && itemQuantity) {
        itemQuantity.textContent = itemQuantityInput.value;
    }
}

function refreshProductPrice(productId, cartItems) {
    const itemTotalPrice = document.querySelector(`#total-price-${productId}`);
    const cartItem = cartItems.find(item => item.productId == productId);
    if (itemTotalPrice && cartItem) {
        itemTotalPrice.textContent = cartItem.totalPrice.formatted;
    }
}

function refreshCartProductAmount(amount) {
    const badge = document.querySelector('#cart-items-count-badge');
    const navBadge = document.querySelector('#cart-amount');
    if (badge) badge.textContent = `${amount} sản phẩm`;
    if (navBadge) navBadge.textContent = amount;
}

function updateTotalPrices(cart) {
    document.querySelector('#total').textContent = cart.total.formatted;
    document.querySelector('#subtotal').textContent = cart.subtotal.formatted;
    document.querySelector('#vat').textContent = cart.vat.formatted;
    document.querySelector('#shipping').textContent = cart.shipping.formatted;

    const discountRow = document.querySelector('#discount-row');
    const discountAmount = document.querySelector('#discount-amount');
    const discountLabel = document.querySelector('#discount-label');

    if (discountRow && discountAmount && cart.discount > 0) {
        discountAmount.textContent = cart.discountFormatted;
        if (activeCoupon) {
            discountLabel.textContent = `Giảm giá (${activeCoupon}):`;
        }
        discountRow.classList.remove('d-none');
    } else if (discountRow) {
        discountRow.classList.add('d-none');
    }

    // Dynamic checkout URL updates based on active coupon
    const checkoutBtn = document.querySelector('#checkout-btn');
    if (checkoutBtn) {
        let baseHref = checkoutBtn.getAttribute('href') || '/ShoppingCart/Summary';
        baseHref = baseHref.split('?')[0];
        if (activeCoupon) {
            checkoutBtn.setAttribute('href', `${baseHref}?couponCode=${activeCoupon}`);
        } else {
            checkoutBtn.setAttribute('href', baseHref);
        }
    }
}

function displayEmptyCart() {
    const container = document.querySelector('#cart-items-container');
    const summary = document.querySelector('#cart-summary-container');
    
    if (summary) summary.remove();
    
    if (container) {
        container.className = "col-12";
        container.innerHTML = `
            <div class="glass-panel p-5 text-center" style="animation: fadeIn 0.8s ease;">
                <i class="bi bi-cart-x text-muted opacity-50" style="font-size: 5rem;"></i>
                <h3 class="mt-3 text-main">Giỏ hàng của bạn đang trống!</h3>
                <p class="text-muted">Hãy quay lại trang chủ và khám phá hàng ngàn sản phẩm công nghệ hấp dẫn.</p>
                <a class="btn btn-primary mt-4" href="/Home/Index">
                    <i class="bi bi-house me-2"></i> Quay về trang chủ
                </a>
            </div>
        `;
    }
}