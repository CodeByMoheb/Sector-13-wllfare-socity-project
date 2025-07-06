using System.Text.Json.Serialization;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class SSLCommerzResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("failedreason")]
        public string FailedReason { get; set; } = string.Empty;

        [JsonPropertyName("sessionkey")]
        public string SessionKey { get; set; } = string.Empty;

        [JsonPropertyName("GatewayPageURL")]
        public string GatewayPageURL { get; set; } = string.Empty;

        [JsonPropertyName("redirectGatewayURL")]
        public string RedirectGatewayURL { get; set; } = string.Empty;

        [JsonPropertyName("directPaymentURL")]
        public string DirectPaymentURL { get; set; } = string.Empty;

        [JsonPropertyName("directPaymentURLBank")]
        public string DirectPaymentURLBank { get; set; } = string.Empty;

        [JsonPropertyName("directPaymentURLCard")]
        public string DirectPaymentURLCard { get; set; } = string.Empty;

        [JsonPropertyName("redirectGatewayURLFailed")]
        public string RedirectGatewayURLFailed { get; set; } = string.Empty;

        [JsonPropertyName("storeBanner")]
        public string StoreBanner { get; set; } = string.Empty;

        [JsonPropertyName("storeLogo")]
        public string StoreLogo { get; set; } = string.Empty;

        [JsonPropertyName("is_direct_pay_enable")]
        public string IsDirectPayEnable { get; set; } = string.Empty;

        [JsonPropertyName("gw")]
        public GatewayInfo? Gw { get; set; }

        [JsonPropertyName("desc")]
        public List<PaymentMethod>? Desc { get; set; }
    }

    public class GatewayInfo
    {
        [JsonPropertyName("visa")]
        public string Visa { get; set; } = string.Empty;

        [JsonPropertyName("master")]
        public string Master { get; set; } = string.Empty;

        [JsonPropertyName("amex")]
        public string Amex { get; set; } = string.Empty;

        [JsonPropertyName("othercards")]
        public string OtherCards { get; set; } = string.Empty;

        [JsonPropertyName("internetbanking")]
        public string InternetBanking { get; set; } = string.Empty;

        [JsonPropertyName("mobilebanking")]
        public string MobileBanking { get; set; } = string.Empty;
    }

    public class PaymentMethod
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("logo")]
        public string Logo { get; set; } = string.Empty;

        [JsonPropertyName("gw")]
        public string Gw { get; set; } = string.Empty;

        [JsonPropertyName("r_flag")]
        public string RFlag { get; set; } = string.Empty;

        [JsonPropertyName("redirectGatewayURL")]
        public string RedirectGatewayURL { get; set; } = string.Empty;
    }
} 