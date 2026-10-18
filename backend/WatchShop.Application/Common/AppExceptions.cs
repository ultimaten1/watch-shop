namespace WatchShop.Application.Common;

/// <summary>Lỗi nghiệp vụ. Lớp Api chuyển thành mã HTTP tương ứng.</summary>
public abstract class AppException(string message) : Exception(message);

/// <summary>Dữ liệu gửi lên không hợp lệ (HTTP 400).</summary>
public class BadRequestException(string message) : AppException(message);

/// <summary>Không tìm thấy (HTTP 404).</summary>
public class NotFoundException(string message = "Không tìm thấy") : AppException(message);

/// <summary>Chưa đăng nhập hoặc sai thông tin đăng nhập (HTTP 401).</summary>
public class UnauthorizedException(string message) : AppException(message);
