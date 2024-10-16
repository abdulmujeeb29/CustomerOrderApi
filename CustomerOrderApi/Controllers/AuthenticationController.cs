using CustomerOrderApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CustomerOrderApi.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        public AuthenticationController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"];
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            
            var jsonContent = new StringContent(JsonSerializer.Serialize(user),Encoding.UTF8,"application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/Auth/register", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }
            ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
            return View(user);

        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/Auth/login", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                // Extract the token from the response
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var token = JsonSerializer.Deserialize<string>(jsonResponse);  // assuming token is returned as string

                // Store token in session or cookie
                HttpContext.Session.SetString("JWTToken", token); // Store in session

                return RedirectToAction("Index");
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your credentials and try again.");
            return View(user);


        }
        public IActionResult Logout()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> LogoutApi()
        {
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/Auth/logout", null);

            if (response.IsSuccessStatusCode)
            {
                // Clear JWT from session or cookie here
                return RedirectToAction("Index");
            }

            // Handle logout failure
            return RedirectToAction("Index");
        }

    }
}
