let dataTable;

function loadDataTable() {
    if ($('#tblData').length === 0) {
        console.warn('DataTable element not found');
        return;
    }

    dataTable = $('#tblData').DataTable({
        "ajax": { 
            url: '/admin/product/getall',
            dataSrc: 'data',
            // debug: log response to console for troubleshooting
            dataFilter: function(data) {
                try {
                    var json = JSON.parse(data);
                    console.log('GET /admin/product/getall response:', json);
                } catch (e) {
                    console.error('Failed to parse products response', e);
                }
                return data;
            },
            error: function(xhr, error, thrown) {
                console.error('Error loading products:', error, xhr);
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: 'Không thể tải dữ liệu sản phẩm. Vui lòng thử lại.'
                });
            }
        },
        "columns": [
            { data: 'author', defaultContent: '', "width": "12%" },
            { data: 'title', "width": "18%" },
            { data: 'isbn', defaultContent: '', "width": "10%" },
            { 
                data: 'price', 
                "width": "5%",
                "render": function(data) {
                    return new Intl.NumberFormat('vi-VN', { 
                        style: 'currency', 
                        currency: 'VND' 
                    }).format(data);
                }
            },
            { data: 'stock', defaultContent: 0, "width": "6%" },
            { data: 'category.name', defaultContent: '', "width": "10%" },
            {
                data: 'id',
                "width": "25%",
                "render": function (data) {
                    return `<div class="d-flex justify-content-center">
                    <div class="w-75 btn-group" role="group">
                    <a href="/admin/product/upsert?id=${data}" class="btn btn-primary"><i class="bi bi-pencil-square"></i> Sửa</a>
                    <button class="btn btn-danger delete-product-btn" data-id="${data}"><i class="bi bi-trash-fill"></i> Xóa</button>
                    </div>
                    </div>
                    `;
                }
            },
        ],
        "drawCallback": function (settings, json) {
            // Remove old event listeners to prevent duplicates
            $('.delete-product-btn').off('click').on('click', function() {
                const productId = $(this).data('id');
                if (productId) {
                    showDeleteModal(productId);
                } else {
                    console.error('Product ID not found');
                }
            });
        },
        "language": {
            "url": "//cdn.datatables.net/plug-ins/1.13.7/i18n/vi.json"
        }
    });
}

function showDeleteModal(id) {
    if (!id) {
        console.error('Invalid product ID');
        return;
    }

    Swal.fire({
        title: "Bạn có chắc chắn?",
        text: "Bạn sẽ không thể hoàn tác thao tác này!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Có, xóa!",
        cancelButtonText: "Hủy"
    }).then(async (result) => {
        if (result.isConfirmed) {
            try {
                const success = await deleteProduct(id);
                if (success) {
                    Swal.fire("Đã xóa!", "Sản phẩm đã được xóa thành công.", "success");
                    if (dataTable) {
                        dataTable.ajax.reload();
                    }
                } else {
                    Swal.fire("Lỗi!", "Đã xảy ra lỗi khi xóa sản phẩm.", "error");
                }
            } catch (error) {
                console.error('Error deleting product:', error);
                Swal.fire("Lỗi!", "Đã xảy ra lỗi khi xóa sản phẩm.", "error");
            }
        }
    });
}

async function deleteProduct(id) {
    try {
        const response = await fetch(`/admin/product/delete/${id}`, { 
            method: 'DELETE',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() || ''
            }
        });

        if (!response.ok) {
            console.error('Delete request failed with status:', response.status);
            return false;
        }

        const data = await response.json();
        
        if (data.success) {
            return true;
        } else {
            console.error('Delete failed:', data.message);
            return false;
        }
    } catch (error) {
        console.error('Error in deleteProduct:', error);
        return false;
    }
}

// Initialize DataTable when document is ready
$(document).ready(function() {
    loadDataTable();
});



