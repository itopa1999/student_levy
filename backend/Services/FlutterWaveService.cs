using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using backend.Interfaces;
using backend.models;
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
    public async Task<string> InitializePayment(decimal amount,AppUser appUser)
    {
        var redirectUrl = GenerateRedirectUrl(amount);

        var paymentData = new
        {
            tx_ref = GenerateUUID(),
            amount = amount,
            currency = "NGN",
            redirect_url = redirectUrl,
            customer = new
            {
                email = $"{appUser.MatricNo}@schoolLevy.com"
            }
          
        };

        // Serialize payment data to JSON
        var json = JsonConvert.SerializeObject(paymentData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Set the authorization header for the request
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _secretKey);

        // Make the HTTP POST request to the Flutterwave payment endpoint
        var response = await _httpClient.PostAsync($"{_baseUrl}", content);
        response.EnsureSuccessStatusCode(); // Throw if not a success code.

        // Read and return the response content
        var result = await response.Content.ReadAsStringAsync();
        return result; // Optionally parse the response to get the payment link if needed
        }

        private string GenerateRedirectUrl(decimal amount)
        {
        // Implement your logic to generate the redirect URL
        // This might involve using UrlHelper or similar methods
        return $"http://localhost:5000/api/transactions/confirm/deposit/{amount}"; // Replace with actual logic
    
        }
          private static string GenerateUUID()
        {
            return Guid.NewGuid().ToString();
        }
}
