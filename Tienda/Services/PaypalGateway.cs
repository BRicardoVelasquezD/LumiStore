using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Tienda.Services
{
    internal sealed class PaypalGateway
    {
        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _baseUrl;

        public PaypalGateway()
        {
            _clientId = Environment.GetEnvironmentVariable("LUMISTORE_PAYPAL_CLIENT_ID");
            _clientSecret = Environment.GetEnvironmentVariable("LUMISTORE_PAYPAL_CLIENT_SECRET");
            _baseUrl = "https://api-m.sandbox.paypal.com";

            if (string.IsNullOrWhiteSpace(_clientId) || string.IsNullOrWhiteSpace(_clientSecret))
                throw new InvalidOperationException("Falta configurar LUMISTORE_PAYPAL_CLIENT_ID y LUMISTORE_PAYPAL_CLIENT_SECRET.");
        }

        public async Task<Tuple<string, string>> CreateOrderAsync(decimal amountUsd, string returnUrl, string cancelUrl)
        {
            string accessToken = await GetAccessTokenAsync();
            var payload = new JObject
            {
                ["intent"] = "CAPTURE",
                ["purchase_units"] = new JArray
                {
                    new JObject
                    {
                        ["amount"] = new JObject
                        {
                            ["currency_code"] = "USD",
                            ["value"] = amountUsd.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
                },
                ["application_context"] = new JObject
                {
                    ["brand_name"] = "Tienda Lumi",
                    ["user_action"] = "PAY_NOW",
                    ["return_url"] = returnUrl,
                    ["cancel_url"] = cancelUrl
                }
            };

            using (var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "/v2/checkout/orders"))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Add("PayPal-Request-Id", Guid.NewGuid().ToString("N"));
                request.Content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
                using (HttpResponseMessage response = await Http.SendAsync(request))
                {
                    string body = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                        throw new InvalidOperationException("PayPal rechazó la creación del pago.");

                    JObject result = JObject.Parse(body);
                    string id = (string)result["id"];
                    string approvalUrl = (string)result["links"]?.FirstOrDefault(x =>
                        (string)x["rel"] == "approve" || (string)x["rel"] == "payer-action")?["href"];
                    if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(approvalUrl))
                        throw new InvalidOperationException("PayPal no devolvió el enlace de aprobación.");
                    return Tuple.Create(id, approvalUrl);
                }
            }
        }

        public async Task<JObject> CaptureOrderAsync(string orderId)
        {
            string accessToken = await GetAccessTokenAsync();
            using (var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "/v2/checkout/orders/" + Uri.EscapeDataString(orderId) + "/capture"))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Add("PayPal-Request-Id", Guid.NewGuid().ToString("N"));
                request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
                using (HttpResponseMessage response = await Http.SendAsync(request))
                {
                    string body = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                        throw new InvalidOperationException("PayPal no pudo capturar el pago.");
                    return JObject.Parse(body);
                }
            }
        }

        public async Task<bool> RefundCaptureAsync(string captureId)
        {
            string accessToken = await GetAccessTokenAsync();
            using (var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "/v2/payments/captures/" + Uri.EscapeDataString(captureId) + "/refund"))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Add("PayPal-Request-Id", Guid.NewGuid().ToString("N"));
                request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
                using (HttpResponseMessage response = await Http.SendAsync(request))
                    return response.IsSuccessStatusCode;
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(_clientId + ":" + _clientSecret));
            using (var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "/v1/oauth2/token"))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
                using (HttpResponseMessage response = await Http.SendAsync(request))
                {
                    string body = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                        throw new InvalidOperationException("PayPal rechazó las credenciales configuradas.");
                    string accessToken = (string)JObject.Parse(body)["access_token"];
                    if (string.IsNullOrWhiteSpace(accessToken))
                        throw new InvalidOperationException("PayPal no devolvió un token de acceso.");
                    return accessToken;
                }
            }
        }
    }
}
