namespace Sorting.Services;

public delegate void LocalStorageEventHandler(object? sender, LocalStorageEvent args);


public enum LocalStorageEventCategory
{
    Add,
    Set,
    Remove,
    Clear
}

public class LocalStorageEvent : EventArgs
{
    public LocalStorageEventCategory Category { get; }
    public string? Key { get; }
    public string? OldValue { get; }
    public string? NewValue { get; }

    public bool IsRelatedTo(string key)
    {
        if (this.Category == LocalStorageEventCategory.Clear) return true;
        if (this.Key == key) return true;
        return false;
    }

    public LocalStorageEvent(LocalStorageEventCategory category, string? key, string? oldValue, string? newValue)
    {
        this.Category = category;
        this.Key = key;
        this.OldValue = oldValue;
        this.NewValue = newValue;
    }
}