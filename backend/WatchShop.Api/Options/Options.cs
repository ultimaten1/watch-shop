namespace WatchShop.Api.Options;

public class JwtOptions
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Key { get; set; } = "";
    public int ExpireHours { get; set; } = 12;
}

public class ShopOptions
{
    public string Name { get; set; } = "";
    public string Slogan { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Zalo { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";
    public string OpeningHours { get; set; } = "";
    public string FacebookUrl { get; set; } = "";
    public string MapEmbedUrl { get; set; } = "";
}

public class PaymentOptions
{
    public string BankId { get; set; } = "";
    public string BankName { get; set; } = "";
    public string AccountNumber { get; set; } = "";
    public string AccountName { get; set; } = "";
}
