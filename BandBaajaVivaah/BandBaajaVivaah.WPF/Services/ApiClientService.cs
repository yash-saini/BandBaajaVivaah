using BandBaajaVivaah.Contracts.DTO;
using BandBaajaVivaah.Contracts.DTOs;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BandBaajaVivaah.WPF.Services
{
    public class ApiClientService
    {
        private readonly HttpClient _httpClient;

        public ApiClientService()
        {
            _httpClient = new HttpClient();
            // IMPORTANT: Replace this port with the one your API is running on.
            // You can find this in your API project's launchSettings.json file.
            _httpClient.BaseAddress = new Uri("https://localhost:7159/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Sets the authorization token for all subsequent requests.
        /// </summary>
        /// <param name="token">The JWT token received from the login endpoint.</param>
        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Logs a user in and returns the authentication token.
        /// </summary>
        public async Task<string?> LoginAsync(UserLoginDto loginDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (!string.IsNullOrEmpty(result?.Token))
                    {
                        SetAuthToken(result.Token); // Store the token for future requests
                        return result.Token;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions (e.g., API is not running)
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Fetches the list of weddings for the authenticated user.
        /// </summary>
        public async Task<IEnumerable<WeddingDto>?> GetWeddingsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/weddings");
                if (response.IsSuccessStatusCode)
                {
                    var weddings = await response.Content.ReadFromJsonAsync<IEnumerable<WeddingDto>>();
                    return weddings;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <returns>True if registration is successful, otherwise false.</returns>
        public async Task<RegistrationResult> RegisterAsync(UserRegisterDto registerDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerDto);

                if (response.IsSuccessStatusCode)
                {
                    return RegistrationResult.Success;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    return RegistrationResult.EmailAlreadyExists;
                }

                return RegistrationResult.Failure;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return RegistrationResult.Failure;
            }
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", new { email });
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", resetDto);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// A helper class to deserialize the JSON response from the login endpoint.
    /// </summary>
    public class LoginResponse
    {
        public string? Token { get; set; }
    }

    public enum RegistrationResult
    {
        Success,
        EmailAlreadyExists,
        Failure
    }
}
