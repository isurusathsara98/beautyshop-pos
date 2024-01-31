using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WomenBeautyBoutique.Services
{
    public class AdminService
    {
        public bool IsAdmin { get; set; } = false;
        private readonly IJSRuntime JS;
        public event Action OnAuthenticationChanged;

        public AdminService(IJSRuntime jsRuntime)
        {
            JS = jsRuntime;
        }
        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            try
            {
                if (await JS.InvokeAsync<bool>("authenticateUser", username, password))
                {
                    IsAdmin = true;
                    OnAuthenticationChanged?.Invoke();
                    return true;
                }
                else
                {
                    IsAdmin = false;
                    return false;
                }

            }
            catch (JSException ex)
            {
                // Handle JavaScript interop exception
                Console.Error.WriteLine($"Error calling JavaScript function: {ex.Message}");
                return false;
            }
        }
    }
}