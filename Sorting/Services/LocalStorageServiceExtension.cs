using System.Text.Json;
namespace Sorting.Services;

public static class LocalStorageServiceExtension
{
    private static string GetSingletonKey<T>() => $"Singleton`{typeof(T).FullName}";

    public async static ValueTask<T?> Get<T>(this LocalStorageService localStorage, string key)
    {
        var str = await localStorage.Get(key);
        return JsonSerializer.Deserialize<T>(str);
    }

    public async static ValueTask Set<T>(this LocalStorageService localStorage, string key, T value)
    {
        var str = JsonSerializer.Serialize(value);
        await localStorage.Set(key, str);
    }
    
    public static ValueTask<T?> GetSingleton<T>(this LocalStorageService localStorage) => localStorage.Get<T>(GetSingletonKey<T>());

    public static ValueTask SetSingleton<T>(this LocalStorageService localStorage, T value) => localStorage.Set(GetSingletonKey<T>(), value);
}
