using System.Text.Json.Serialization;

namespace GateHub.Dtos
{
    public class PaymobWebhookDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("obj")]
        public PaymobTransaction Obj { get; set; }
    }

    public class PaymobTransaction
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("amount_cents")]
        public int AmountCents { get; set; }

        [JsonPropertyName("order")]
        public PaymobOrder Order { get; set; }

        [JsonPropertyName("payment_key_claims")]
        public PaymobPaymentKeyClaims PaymentKeyClaims { get; set; }
    }

    public class PaymobOrder
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("merchant_order_id")]
        public string MerchantOrderId { get; set; }
    }

    public class PaymobPaymentKeyClaims
    {
        [JsonPropertyName("billing_data")]
        public PaymobBillingData BillingData { get; set; }
    }

    public class PaymobBillingData
    {
        [JsonPropertyName("extra_description")]
        public string ExtraDescription { get; set; }

        [JsonPropertyName("first_name")]
        public string Nat_Id { get; set; }
    }
}
