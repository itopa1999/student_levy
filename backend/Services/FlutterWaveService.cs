using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

public class FlutterwaveService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _secretKey;

    public FlutterwaveService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["Flutterwave:BaseUrl"];
        _secretKey = configuration["Flutterwave:SecretKey"];
    }

    // Method to initialize a payment
    public async Task<string> InitializePayment(decimal amount, string currency, string redirectUrl, string email, string txRef)
    {
        var paymentData = new
        {
            tx_ref = txRef, // A unique reference for each transaction
            amount = amount,
            currency = currency,
            redirect_url = redirectUrl,
            customer = new
            {
                email = email
            }
        };

        var json = JsonConvert.SerializeObject(paymentData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _secretKey);

        var response = await _httpClient.PostAsync($"{_baseUrl}/payments", content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();
        return result; // Parse response to get the payment link if needed
    }
}
