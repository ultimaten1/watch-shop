# Website bán đồng hồ (ASP.NET Core Web API + React)

Website giới thiệu và bán đồng hồ cho cửa hàng gia đình.

- **Backend:** ASP.NET Core 10 Web API, Entity Framework Core, SQL Server, đăng nhập quản trị bằng JWT
- **Frontend:** React 18 + TypeScript + Vite
- **Khách xem được:** trang chủ, danh sách và lọc sản phẩm, chi tiết sản phẩm, giỏ hàng, đặt hàng, tra cứu đơn, giới thiệu, liên hệ
- **Thanh toán:** chuyển khoản bằng mã QR VietQR (điền sẵn số tiền và nội dung), trả tiền khi nhận hàng, hoặc để cửa hàng liên hệ riêng
- **Quản trị (`/admin`):** quản lý sản phẩm và ảnh, hãng, đơn hàng, tin nhắn khách

> Lưu ý: mã nguồn backend chưa được biên dịch thử khi tạo (môi trường tạo không có .NET SDK). Frontend đã build thành công. Nếu `dotnet build` báo lỗi, hãy gửi lại thông báo lỗi để sửa.

## Cần cài

- .NET 10 SDK
- Node.js 20 trở lên
- SQL Server (bản Developer, Express hoặc LocalDB đều được)

## Chạy thử trên máy

### 1. Cấu hình backend

Mở `backend/WatchShop.Api/appsettings.json` và sửa:

| Mục                         | Cần làm                                                                                                                                                                                                |
| --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `ConnectionStrings:Default` | Chuỗi kết nối SQL Server của bạn. Mặc định dùng đăng nhập Windows vào `localhost`. Nếu dùng tài khoản `sa`: `Server=localhost;Database=WatchShop;User Id=sa;Password=...;TrustServerCertificate=True;` |
| `Jwt:Key`                   | Đổi thành chuỗi ngẫu nhiên dài từ 32 ký tự trở lên                                                                                                                                                     |
| `Admin`                     | Tên đăng nhập và mật khẩu quản trị đầu tiên. **Đổi mật khẩu trước khi chạy lần đầu**, vì tài khoản chỉ được tạo một lần                                                                                |
| `Shop`                      | Tên, số điện thoại, Zalo, địa chỉ, giờ mở cửa của cửa hàng                                                                                                                                             |
| `Payment`                   | Ngân hàng nhận tiền. `BankId` là mã ngân hàng theo VietQR (ví dụ VCB, MB, TCB, ACB), kèm số tài khoản và tên chủ tài khoản viết hoa không dấu                                                          |

### 2. Chạy backend

```bash
cd backend/WatchShop.Api
dotnet run
```

Lần chạy đầu tự tạo database `WatchShop` và nạp sẵn vài hãng, sản phẩm mẫu. API chạy ở `http://localhost:5080`, tài liệu Swagger ở `http://localhost:5080/swagger`.

### 3. Chạy frontend

```bash
cd frontend
npm install
npm run dev
```

Mở `http://localhost:5173`. Trang quản trị ở `http://localhost:5173/admin`.

### 4. Việc làm ngay sau khi chạy

1. Đăng nhập `/admin`, xoá hoặc sửa các sản phẩm mẫu, thêm sản phẩm thật kèm ảnh chụp thật.
2. Sửa nội dung trang `frontend/src/pages/About.tsx` thành câu chuyện thật của gia đình.
3. Gửi thử một đơn hàng chuyển khoản, quét mã QR bằng app ngân hàng để chắc chắn đúng tài khoản.

## Code First với migrations của EF Core

Cấu trúc database được định nghĩa bằng các class trong `WatchShop.Domain/Entities` và cấu hình trong `WatchShop.Infrastructure/Persistence/AppDbContext.cs`. Migrations nằm ở `WatchShop.Infrastructure/Persistence/Migrations`. Khi app khởi động sẽ tự chạy `Migrate()`.

Khi sửa entity, tạo migration mới (tắt backend trước):

```bash
dotnet tool install --global dotnet-ef   # chỉ cần lần đầu
cd backend
dotnet ef migrations add TenThayDoi --project WatchShop.Infrastructure --startup-project WatchShop.Api --output-dir Persistence/Migrations
dotnet ef database update --project WatchShop.Infrastructure --startup-project WatchShop.Api
```

Chuỗi kết nối **phải có tên database** (`Initial Catalog=WatchShop`), nếu thiếu SQL Server sẽ dùng database `master`.

## Đưa lên mạng

Cách đơn giản nhất là chạy chung một ứng dụng:

```bash
cd frontend
npm run build
# chép toàn bộ nội dung thư mục dist vào backend/WatchShop.Api/wwwroot
cd ../backend/WatchShop.Api
dotnet publish -c Release -o publish
```

Backend phục vụ luôn website React và ảnh upload. Khi lên server thật:

- Đặt `ASPNETCORE_ENVIRONMENT=Production`, đặt `ConnectionStrings`, `Jwt:Key`, `Admin` bằng biến môi trường hoặc `appsettings.Production.json` (đừng đưa mật khẩu lên GitHub).
- Dùng HTTPS (Cloudflare, IIS, Nginx hoặc dịch vụ như Azure App Service đều xử lý được).
- Sao lưu định kỳ database và thư mục `wwwroot/uploads` (chứa ảnh sản phẩm).
- Nếu frontend chạy ở tên miền khác backend, thêm tên miền đó vào `Cors:Origins` và đặt `VITE_API_BASE` khi build frontend.

## Giúp khách tìm thấy cửa hàng

- Điền thông tin thật trong `Shop`, đăng ký **Google Business Profile** cho cửa hàng và dán link bản đồ nhúng vào `Shop:MapEmbedUrl`.
- Mỗi sản phẩm cần tên rõ ràng, mô tả riêng và ảnh thật. Trang chi tiết đã có sẵn tiêu đề, mô tả và dữ liệu cấu trúc cho Google.
- Gắn link website vào Facebook, Zalo OA, TikTok của cửa hàng.
- Giới hạn hiện tại: React chạy phía trình duyệt nên Google có thể lập chỉ mục chậm hơn trang render sẵn. Bước nâng cấp sau này: dùng Blazor Server/SSR hoặc Next.js nếu cần SEO mạnh hơn.

## Cổng thanh toán trực tuyến (VNPay, MoMo, PayOS)

Bản này dùng chuyển khoản QR nên **không cần hợp đồng cổng thanh toán**, đơn được đối chiếu thủ công trong `/admin/don-hang` (đổi trạng thái sang "Đã thanh toán"). Khi muốn tự động, thêm một `PaymentController` nhận webhook từ cổng thanh toán và cập nhật `Order.Status`. Các cổng này đều yêu cầu đăng ký merchant bằng giấy tờ kinh doanh.

## Cấu trúc thư mục

Backend theo Clean Architecture (phụ thuộc đi một chiều: Api → Infrastructure → Application → Domain):

```
backend/
  WatchShop.Domain          Entities (Brand, Product, Order, Customer, AdminUser...), Enums. Không phụ thuộc gì.
  WatchShop.Application     Nghiệp vụ, không biết EF Core hay HTTP
    Abstractions/           IRepository, IUnitOfWork, IPasswordHasher, ITokenService
    Services/               ProductService, OrderService, AuthService, AccountService...
    Dtos/                   dữ liệu vào/ra của API
    Common/                 lỗi nghiệp vụ (NotFound, BadRequest...), Slugger
  WatchShop.Infrastructure  Cài đặt chi tiết
    Persistence/            AppDbContext, Repositories, UnitOfWork, DbSeeder, Migrations
    Security/               BCryptPasswordHasher, JwtTokenService
  WatchShop.Api             Lớp giao tiếp HTTP
    Controllers/            mỏng, chỉ nhận request và gọi Service
    Common/                 middleware chuyển lỗi nghiệp vụ thành mã HTTP
    Program.cs              cấu hình, JWT, CORS, rate limit
frontend/src
  pages/         các trang khách xem
  admin/         trang quản trị
  components/    LiveClock, ProductCard, Layout...
  api.ts         gọi backend
  styles.css     giao diện
```

## Những điều server tự bảo vệ

- Giá và tồn kho luôn tính lại ở server khi đặt hàng, khách không sửa giá được.
- Mật khẩu quản trị và khách hàng lưu bằng BCrypt; đăng nhập, đăng ký, form liên hệ và đặt hàng có giới hạn tần suất.
- Phân quyền: khách (`Customer`), nhân viên (`Admin`), chủ cửa hàng (`Admin` + `Owner`). Token của khách không gọi được API quản trị.
- Upload ảnh kiểm tra đuôi file, dung lượng và nội dung thật của file.
- Tra cứu đơn cần cả mã đơn và số điện thoại.
