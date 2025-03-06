using System.Text.Json.Serialization;

namespace GateHub.Dtos
{
    public class PaymobWebhookDto
    {
        [JsonPropertyName("obj")]
        public PaymobWebhookObject Obj { get; set; }
    }

    public class PaymobWebhookObject
    {
        [JsonPropertyName("success")]
        public string Success { get; set; }

        [JsonPropertyName("amount_cents")]
        public int AmountCents { get; set; }

        [JsonPropertyName("order")]
        public PaymobOrder Order { get; set; }

        [JsonPropertyName("id")]
        public string TransactionId { get; set; }
    }

    public class PaymobOrder
    {
        [JsonPropertyName("merchant_order_id")]
        public string MerchantOrderId { get; set; }
    }
}
