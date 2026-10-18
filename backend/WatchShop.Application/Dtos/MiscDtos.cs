namespace WatchShop.Application.Dtos;

public record AdminStatsDto(int NewOrders, int UnreadMessages, int Products, int LowStock);

public record ContactMessageDto(int Id, string Name, string Phone, string? Email, string Message,
    DateTime CreatedAt, bool IsRead);
