using GateHub.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace GateHub.repository
{
    public class PaymobService
    {
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;
        private readonly GateHubContext context;

        public PaymobService(HttpClient httpClient,IConfiguration configuration,GateHubContext context)
        {
            this.httpClient = httpClient;
            this.configuration = configuration;
            this.context = context;
        }

        public async Task<string> InitiatePayment(VehicleOwner owner,decimal amount,string vehicleEntryIds,string purpose)
        {
            //step 1: get authentication token from paymob
            var authRequest = new { api_key = configuration["Paymob:APIKey"] };
            var authResponse = await httpClient.PostAsync("https://accept.paymob.com/api/auth/tokens",
                new StringContent(JsonConvert.SerializeObject(authRequest),Encoding.UTF8, "application/json"));

            var authData = JsonConvert.DeserializeObject<dynamic>(await authResponse.Content.ReadAsStringAsync());
            string authToken = authData.token;

            //step 2: Register an order with Paymob 
            var orderRequest = new
            {
                auth_token = authToken,
                delivery_needed = "false",
                amount_cents = (int)(amount * 100),
                currency = "EGP",
                merchant_order_id = Guid.NewGuid().ToString(),
                items = new object[] { }
            };

            var orderResponse = await httpClient.PostAsync("https://accept.paymob.com/api/ecommerce/orders",
                new StringContent(JsonConvert.SerializeObject(orderRequest), Encoding.UTF8, "application/json"));

            var orderData = JsonConvert.DeserializeObject<dynamic>(await orderResponse.Content.ReadAsStringAsync());
            string orderId = orderData.id;

            //step 3: request a payment key
            var paymentRequest = new
            {
                auth_token = authToken,
                amount_cents = (int)(amount * 100),
                expiration = 3600,
                order_id = orderId,
                currency = "EGP",  
                billing_data = new
                {
                    first_name = owner.PhoneNumber ?? "Test",
                    last_name = "User",
                    email = "test@example.com",
                    phone_number = owner.PhoneNumber ,
                    street = "Test Street",
                    building = "123",
                    floor = "1",
                    apartment = "5A",
                    city = "Cairo",
                    country = "EG",
                    postal_code = "12345",
                    extra_description = vehicleEntryIds,
                },
                integration_id = configuration["Paymob:IntegrationID"]
            };

            var paymentResponse = await httpClient.PostAsync("https://accept.paymob.com/api/acceptance/payment_keys",
            new StringContent(JsonConvert.SerializeObject(paymentRequest), Encoding.UTF8, "application/json"));


            var paymentData = JsonConvert.DeserializeObject<dynamic>(await paymentResponse.Content.ReadAsStringAsync());
            string paymentKey = paymentData.token;

            //step 4: redirect user to paymob iframe for payment 
            return $"https://accept.paymob.com/api/acceptance/iframes/{configuration["Paymob:IframeID"]}?payment_token={paymentKey}";

        }
    }
}
