namespace RamsElec.App.Services;

public class AuthService
{
    private readonly ApiClient _apiClient;

    public AuthService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public bool IsLoggedIn { get; private set; }
    public string? DisplayName { get; private set; }
    public string? Role { get; private set; }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var result = await _apiClient.LoginAsync(new Shared.DTOs.LoginDto
        {
            Email = email,
            Password = password
        });

        if (result == null) return false;

        _apiClient.SetToken(result.Token);
        await SecureStorage.SetAsync("auth_token", result.Token);
        await SecureStorage.SetAsync("display_name", result.DisplayName);
        await SecureStorage.SetAsync("role", result.Role);

        IsLoggedIn = true;
        DisplayName = result.DisplayName;
        Role = result.Role;
        return true;
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        var token = await SecureStorage.GetAsync("auth_token");
        if (string.IsNullOrEmpty(token)) return false;

        _apiClient.SetToken(token);
        DisplayName = await SecureStorage.GetAsync("display_name");
        Role = await SecureStorage.GetAsync("role");
        IsLoggedIn = true;
        return true;
    }

    public void Logout()
    {
        _apiClient.ClearToken();
        SecureStorage.RemoveAll();
        IsLoggedIn = false;
        DisplayName = null;
        Role = null;
    }
}
