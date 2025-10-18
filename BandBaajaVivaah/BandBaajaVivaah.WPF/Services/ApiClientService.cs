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

        public ApiClientService(string baseApiUrl)
        {
            _httpClient = new HttpClient();
            // IMPORTANT: Replace this port with the one your API is running on.
            // You can find this in your API project's launchSettings.json file.
            _httpClient.BaseAddress = new Uri(baseApiUrl);
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

        public async Task<bool> DeleteWeddingAsync(int weddingId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/weddings/{weddingId}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteGuestAsync(int guestId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/guests/{guestId}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/tasks/{taskId}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<WeddingDto?> CreateWeddingAsync(CreateWeddingDto createDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/weddings", createDto);
                if (response.IsSuccessStatusCode)
                {
                    var wedding = await response.Content.ReadFromJsonAsync<WeddingDto>();
                    return wedding;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
            return null;
        }

        public async Task<bool> UpdateWeddingAsync(int weddingId, CreateWeddingDto updateDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/weddings/{weddingId}", updateDto);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<IEnumerable<GuestDto>> GetGuestsAsync(int weddingId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/guests/wedding/{weddingId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<IEnumerable<GuestDto>>() ?? new List<GuestDto>();
                }
            }
            catch (HttpRequestException)
            {
                return Enumerable.Empty<GuestDto>();
            }
            return new List<GuestDto>();
        }

        public async Task<IEnumerable<TaskDto>> GetTasksAsync(int weddingId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/tasks/wedding/{weddingId}");
                if (response.IsSuccessStatusCode)
                {
                    var tasks = await response.Content.ReadFromJsonAsync<IEnumerable<TaskDto>>();
                    return tasks ?? new List<TaskDto>();
                }
            }
            catch (HttpRequestException)
            {
                return [];
            }
            return [];
        }

        public async Task<GuestDto?> CreateGuestAsync(CreateGuestDto createDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/guests", createDto);
                if (response.IsSuccessStatusCode)
                {
                    var guest = await response.Content.ReadFromJsonAsync<GuestDto>();
                    return guest;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
            return null;
        }

        public async Task<TaskDto?> CreateTaskAsync(CreateTaskDto createDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/tasks", createDto);
                if (response.IsSuccessStatusCode)
                {
                    var task = await response.Content.ReadFromJsonAsync<TaskDto>();
                    return task;
                }
            }
            catch (HttpRequestException ex)
            {
                return null;
            }
            return null;
        }

        public async Task<bool> UpdateGuestAsync(int guestId, CreateGuestDto updateDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/guests/{guestId}", updateDto);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<bool> UpdateTaskAsync(int taskId, CreateTaskDto updateDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/tasks/{taskId}", updateDto);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                return false;
            }
        }

        public async Task<IEnumerable<ExpenseDto>> GetExpensesAsync(int weddingId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/expenses/wedding/{weddingId}");
                if (response.IsSuccessStatusCode)
                {
                    var expenses = await response.Content.ReadFromJsonAsync<IEnumerable<ExpenseDto>>();
                    return expenses ?? new List<ExpenseDto>();
                }
            }
            catch (HttpRequestException)
            {
                return Enumerable.Empty<ExpenseDto>();
            }
            return new List<ExpenseDto>();
        }

        public async Task<ExpenseDto?> CreateExpenseAsync(CreateExpenseDto createDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/expenses", createDto);
                if (response.IsSuccessStatusCode)
                {
                    var expense = await response.Content.ReadFromJsonAsync<ExpenseDto>();
                    return expense;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
            return null;
        }

        public async Task<bool> UpdateExpenseAsync(int expenseId, CreateExpenseDto updateDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/expenses/{expenseId}", updateDto);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteExpenseAsync(int expenseId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/expenses/{expenseId}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/admin/users");
                if (response.IsSuccessStatusCode)
                {
                    var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
                    return users ?? new List<UserDto>();
                }
            }
            catch (HttpRequestException)
            {
                return Enumerable.Empty<UserDto>();
            }
            return new List<UserDto>();
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/admin/users/{userId}/role", new { role = newRole });
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/admin/users/{userId}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<IEnumerable<WeddingDto>?> GetWeddingsForUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Admin/users/{userId}/weddings");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<IEnumerable<WeddingDto>>();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<WeddingDto?> AddWeddingForUserAsync(int userId, CreateWeddingDto weddingDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/Admin/users/{userId}/weddings", weddingDto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<WeddingDto>();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> DeleteWeddingByAdminAsync(int weddingId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Admin/weddings/{weddingId}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<bool> UpdateWeddingByAdminAsync(int weddingId, CreateWeddingDto updateDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Admin/weddings/{weddingId}", updateDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
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
