# BÁO CÁO DỰ ÁN: WEBSITE THƯƠNG MẠI ĐIỆN TỬ ELECTRONICSHOP

Dự án này là một ứng dụng web thương mại điện tử được xây dựng bằng ASP.NET Core 8, mô phỏng một cửa hàng bán linh kiện điện tử trực tuyến.

## 1. Giới thiệu tổng quan

**ElectronicShopDotNETcore** là một ứng dụng web thương mại điện tử hiện đại, đầy đủ tính năng được xây dựng trên nền tảng ASP.NET Core 8. Dự án cung cấp một trải nghiệm mua sắm trực tuyến hoàn chỉnh, từ việc duyệt sản phẩm, quản lý giỏ hàng đến quy trình thanh toán an toàn. Dự án được thiết kế theo kiến trúc N-Tier (nhiều lớp) rõ ràng, giúp dễ dàng bảo trì, mở rộng và phát triển trong tương lai.

## 2. Các tính năng chính

Ứng dụng cung cấp đầy đủ các chức năng cần thiết cho một hệ thống E-commerce chuyên nghiệp:

*   **Phía khách hàng (Client):**
    *   **Trang chủ động:** Hiển thị sản phẩm nổi bật, sản phẩm mới và các danh mục.
    *   **Duyệt sản phẩm:** Người dùng có thể xem sản phẩm theo danh mục, tìm kiếm sản phẩm.
    *   **Chi tiết sản phẩm:** Xem thông tin chi tiết, hình ảnh, mô tả và giá của từng sản phẩm.
    *   **Giỏ hàng (Shopping Cart):** Thêm sản phẩm vào giỏ, cập nhật số lượng hoặc xóa sản phẩm khỏi giỏ hàng. Giỏ hàng được cập nhật real-time bằng JavaScript.
    *   **Thanh toán (Checkout):** Cung cấp quy trình thanh toán an toàn, cho phép người dùng nhập thông tin giao hàng.
    *   **Tích hợp cổng thanh toán VNPay:** Cho phép người dùng thanh toán đơn hàng trực tuyến một cách an toàn và tiện lợi.
    *   **Xác thực và phân quyền:** Người dùng có thể đăng ký tài khoản, đăng nhập. Hệ thống sử dụng ASP.NET Core Identity để quản lý.
    *   **Lịch sử đơn hàng:** Người dùng đã đăng nhập có thể xem lại lịch sử các đơn hàng đã đặt.

*   **Phía quản trị (Admin - yêu cầu đăng nhập với tài khoản Admin):**
    *   **Quản lý sản phẩm:** Thêm, xóa, sửa thông tin sản phẩm (tên, mô tả, giá, hình ảnh, số lượng tồn kho).
    *   **Quản lý danh mục:** Thêm, xóa, sửa các danh mục sản phẩm.
    *   **Quản lý đơn hàng:** Xem danh sách đơn hàng, cập nhật trạng thái đơn hàng (chờ xử lý, đang giao, đã giao, đã hủy).
    *   *(Hàm ý)*: Kiến trúc đã sẵn sàng để mở rộng các chức năng quản trị khác như quản lý người dùng, xem báo cáo thống kê.

## 3. Kiến trúc phần mềm

Ứng dụng được xây dựng theo **kiến trúc N-Tier**, chia thành các project (lớp) riêng biệt với các trách nhiệm rõ ràng. Điều này giúp tăng tính module hóa, dễ bảo trì và tái sử dụng code.

*   **`ElectronicShopMVC` (Presentation Layer - Tầng trình diễn):**
    *   Đây là project chính, chứa giao diện người dùng (UI) và xử lý các yêu cầu HTTP.
    *   Sử dụng ASP.NET Core MVC và Razor Pages để xây dựng giao diện.
    *   **Controllers:** Xử lý logic nghiệp vụ từ yêu cầu của người dùng (`HomeController`, `ShoppingCartController`, `ProductController`).
    *   **Views & Pages:** Các file `.cshtml` định nghĩa HTML được trả về cho trình duyệt.
    *   **ViewComponents:** Các thành phần UI có thể tái sử dụng (`MenuViewComponent`, `FooterViewComponent`).
    *   **wwwroot:** Chứa các tài sản tĩnh như CSS, JavaScript, hình ảnh.

*   **`ElectronicShopMVC.DataAccess` (Data Access Layer - Tầng truy cập dữ liệu):**
    *   Chịu trách nhiệm giao tiếp với cơ sở dữ liệu.
    *   Sử dụng **Entity Framework Core 8** làm ORM.
    *   Áp dụng các mẫu thiết kế (Design Pattern) **Repository** và **Unit of Work** để trừu tượng hóa việc truy cập dữ liệu.
        *   **Repository Pattern:** Cung cấp một lớp trừu tượng (`IRepository<T>`, `ProductRepository`) để che giấu logic truy vấn dữ liệu thô.
        *   **Unit of Work Pattern (`IUnitOfWork`):** Nhóm các thao tác ghi (thêm, sửa, xóa) vào một giao dịch duy nhất, đảm bảo tính toàn vẹn dữ liệu.

*   **`ElectronicShopMVC.Model` (Domain Layer - Tầng miền):**
    *   Chứa các mô hình dữ liệu của ứng dụng.
    *   **Entities (POCOs):** Các lớp C# thuần túy ánh xạ tới các bảng trong cơ sở dữ liệu (`Product`, `Category`, `Order`, `ApplicationUser`).
    *   **ViewModels:** Các lớp được thiết kế riêng để phục vụ cho một View cụ thể, giúp truyền dữ liệu từ Controller sang View (`ProductVM`, `HomeVM`).
    *   **Data Transfer Objects (DTOs):** Các đối tượng dùng để truyền dữ liệu giữa các lớp, đặc biệt hữu ích trong các kịch bản API (`CartDTO`, `ProductDTO`).

*   **`ElectronicShopMVC.Utility` (Shared/Utility Layer - Tầng tiện ích):**
    *   Một project dùng chung chứa các lớp tiện ích, hằng số, và các hàm hỗ trợ có thể được sử dụng trên toàn bộ giải pháp.
    *   Ví dụ: `StaticDetails` (chứa các hằng số về vai trò người dùng, trạng thái đơn hàng), `EmailSender`.

## 4. Thiết kế Cơ sở dữ liệu

Cơ sở dữ liệu của dự án được thiết kế theo phương pháp Code-First sử dụng Entity Framework Core. Lược đồ được sinh ra từ các lớp entity trong project `ElectronicShopMVC.Model`.

**Các bảng chính bao gồm:**

*   **`Categories`**: Lưu trữ thông tin về các danh mục sản phẩm.
    *   `Id`, `Name`, `DisplayOrder`.
*   **`Products`**: Lưu trữ thông tin chi tiết về sản phẩm.
    *   `Id`, `Name`, `Description`, `Price`, `Stock`, `ImageUrl`.
    *   Có một khóa ngoại `CategoryId` liên kết với bảng `Categories` (mối quan hệ một-nhiều: một danh mục có nhiều sản phẩm).
*   **`AspNetUsers`**: Bảng do ASP.NET Core Identity quản lý, lưu trữ thông tin người dùng (email, mật khẩu đã hash, thông tin cá nhân như tên, địa chỉ).
*   **`Orders`**: Lưu thông tin đầu mục của đơn hàng.
    *   `Id`, `OrderDate`, `OrderTotal`, `OrderStatus`, `Name`, `PhoneNumber`, `Address`.
    *   Có một khóa ngoại `ApplicationUserId` liên kết tới bảng `AspNetUsers`.
*   **`OrderItems`**: Lưu thông tin chi tiết các sản phẩm trong một đơn hàng.
    *   Là bảng nối trong mối quan hệ nhiều-nhiều giữa `Orders` và `Products`.
    *   `Id`, `OrderId`, `ProductId`, `Quantity`, `Price`.
*   **`Carts` & `CartItems`**: Các bảng dùng để quản lý trạng thái giỏ hàng cho người dùng.

## 5. Công nghệ sử dụng

*   **Nền tảng backend:** ASP.NET Core 8, C# 12
*   **Cơ sở dữ liệu:** Microsoft SQL Server
*   **ORM (Object-Relational Mapping):** Entity Framework Core 8
*   **Giao diện frontend:** Razor Pages, HTML5, CSS3, JavaScript (ES6)
*   **Framework CSS:** Bootstrap 5
*   **Thư viện JavaScript:** jQuery
*   **Xác thực & Phân quyền:** ASP.NET Core Identity
*   **Cổng thanh toán:** VNPay

## 6. Hướng dẫn cài đặt và chạy dự án

### Yêu cầu
*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Visual Studio 2022](https://visualstudio.microsoft.com/) (khuyến nghị) hoặc một code editor khác.
*   [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (phiên bản Express, Developer, hoặc cloud).

### Các bước cài đặt

1.  **Clone repository về máy:**
    ```bash
    git clone https://github.com/your-username/ElectronicShopDotNETcore.git
    cd ElectronicShopDotNETcore
    ```

2.  **Cấu hình chuỗi kết nối cơ sở dữ liệu:**
    *   Mở file `ElectronicShopMVC/appsettings.json`.
    *   Tìm đến mục `ConnectionStrings` và cập nhật chuỗi `DefaultConnection` để trỏ tới instance SQL Server của bạn.

    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=TEN_SERVER_CUA_BAN;Database=ElectronicShopDb;Trusted_Connection=True;TrustServerCertificate=True"
    }
    ```
    *Lưu ý: Thay `TEN_SERVER_CUA_BAN` thành tên SQL Server instance của bạn (ví dụ: `.` hoặc `localhost` nếu dùng local).*

3.  **Áp dụng Database Migrations:**
    *   Mở **Package Manager Console** trong Visual Studio.
    *   Chọn project mặc định là `ElectronicShopMVC.DataAccess`.
    *   Chạy lệnh sau để tạo cơ sở dữ liệu và các bảng:
    ```powershell
    Update-Database
    ```
    *   *Ngoài ra, ứng dụng được cấu hình để tự động áp dụng migration khi khởi động, nên CSDL có thể sẽ được tự tạo ở lần chạy đầu tiên.*

4.  **Chạy ứng dụng:**
    *   Nhấn `F5` trong Visual Studio để bắt đầu debug.
    *   Hoặc sử dụng .NET CLI:
    ```bash
    dotnet run --project ElectronicShopMVC/ElectronicShopMVC.csproj
    ```

### Tài khoản Admin mặc định

Hệ thống sẽ tự động tạo một tài khoản quản trị viên khi khởi chạy lần đầu. Thông tin đăng nhập được cấu hình trong `appsettings.json`:

*   **Email:** `admin@example.com`
*   **Mật khẩu:** `Admin@123`

## 7. Hướng dẫn sử dụng

*   **Trang cho khách hàng:** Truy cập vào URL gốc của ứng dụng (ví dụ: `https://localhost:7000`) để duyệt sản phẩm, thêm vào giỏ hàng và tiến hành thanh toán.
*   **Trang quản trị:** Để truy cập các chức năng quản lý, đăng nhập bằng tài khoản admin đã cung cấp. Các chức năng quản trị nằm dưới route `/Admin` (ví dụ: `/Admin/Product`, `/Admin/Category`).

## 8. Triển khai với Docker

Dự án đi kèm một `Dockerfile` được tối ưu hóa để đóng gói ứng dụng thành một container image.

### Phân tích `Dockerfile`

`Dockerfile` sử dụng kỹ thuật **multi-stage build** để tạo ra một image nhẹ và an toàn:

1.  **Stage `build`**:
    *   Sử dụng image `.NET SDK` (`mcr.microsoft.com/dotnet/sdk:8.0`).
    *   Sao chép các file `.csproj` và `.sln` trước để tận dụng cơ chế caching của Docker, giúp tăng tốc độ build khi các dependency không thay đổi.
    *   Thực hiện `dotnet restore` để tải về các NuGet packages.
    *   Sao chép toàn bộ source code và thực hiện `dotnet publish` để biên dịch ứng dụng ở chế độ `Release`.

2.  **Stage `final`**:
    *   Sử dụng image ASP.NET runtime nhỏ gọn hơn (`mcr.microsoft.com/dotnet/aspnet:8.0`).
    *   Sao chép các file đã được publish từ stage `build`.
    *   Mở port `5000` (HTTP) và `5001` (HTTPS) để cho phép truy cập từ bên ngoài.
    *   Sử dụng `ENTRYPOINT` để khởi chạy ứng dụng.

### Các lệnh Docker

1.  **Build Docker image:**
    Mở terminal tại thư mục gốc của dự án và chạy lệnh sau:
    ```bash
    docker build -t electronicshop .
    ```

2.  **Run Docker container:**
    Sau khi build thành công, chạy container với lệnh sau. Lưu ý quan trọng là bạn phải cung cấp chuỗi kết nối CSDL thông qua biến môi trường.
    ```bash
    docker run -p 8080:5000 -e "ConnectionStrings:DefaultConnection"="Server=YOUR_SERVER_IP_ADDRESS;Database=ElectronicShopDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;Trusted_Connection=False;TrustServerCertificate=True" -d electronicshop
    ```
    *   **Giải thích:**
        *   `-p 8080:5000`: Ánh xạ cổng `8080` trên máy host vào cổng `5000` của container.
        *   `-e "ConnectionStrings:DefaultConnection"="..."`: Ghi đè chuỗi kết nối trong `appsettings.json` bằng biến môi trường. **Bạn phải thay đổi giá trị này** để trỏ đến SQL Server của bạn. Container cần có khả năng truy cập đến địa chỉ IP của SQL Server.
        *   `-d`: Chạy container ở chế độ detached (chạy nền).
        *   `electronicshop`: Tên của image đã build.

    Sau khi container khởi động, bạn có thể truy cập ứng dụng tại `http://localhost:8080`.