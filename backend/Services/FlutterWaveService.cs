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

    public FlutterwaveService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _baseUrl = config["Flutter:BaseUrl"];
        _secretKey = config["Flutter:Secret_key"];
        
    }

    // Method to initialize a payment
    public async Task<string> InitializePayment(decimal amount,AppUser appUser, int id)
    {
        var redirectUrl = GenerateRedirectUrl(amount, id, appUser);
        Console.WriteLine(appUser.Id);
        var paymentData = new
        {
            tx_ref = GenerateUUID(),
            amount = amount,
            currency = "NGN",
            redirect_url = redirectUrl,
            customer = new
            {
                email = $"{appUser.MatricNo}@schoolLevy.com",
                user_id = appUser.Id,
                levy_id = id
            }
          
        };
        
        // Serialize payment data to JSON
        var json = JsonConvert.SerializeObject(paymentData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Set the authorization header for the request
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _secretKey);

        // Make the HTTP POST request to the Flutterwave payment endpoint
        var response = await _httpClient.PostAsync(_baseUrl, content);
        response.EnsureSuccessStatusCode(); // Throw if not a success code.

        // Read and return the response content
        var result = await response.Content.ReadAsStringAsync();
        return result; // Optionally parse the response to get the payment link if needed
        }

        private string GenerateRedirectUrl(decimal amount, int id, AppUser appUser)
        {
        // Implement your logic to generate the redirect URL
        // This might involve using UrlHelper or similar methods
        return $"http://localhost:5087/student/api/confirm/deposit/{amount}/{id}/{appUser}"; // Replace with actual logic
    
        }
          private static string GenerateUUID()
        {
            return Guid.NewGuid().ToString();
        }
}
