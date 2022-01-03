namespace Sorting.Services;

public delegate void LocalStorageEventHandler(object? sender, LocalStorageEventArgs args);


public enum LocalStorageEventCategory
{
    Add,
    Set,
    Remove,
    Clear
}

public class LocalStorageEventArgs : EventArgs
{
    public LocalStorageEventCategory Category { get; }
    public string? Key { get; }
    public string? OldValue { get; }
    public string? NewValue { get; }

    public LocalStorageEventArgs(LocalStorageEventCategory category, string? key, string? oldValue, string? newValue)
    {
        this.Category = category;
        this.Key = key;
        this.OldValue = oldValue;
        this.NewValue = newValue;
    }
}