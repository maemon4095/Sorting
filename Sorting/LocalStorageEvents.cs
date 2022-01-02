namespace Sorting.Storage;

public delegate void LocalStorageEventHandler(object sender, LocalStorageEventArgs? args);

public enum LocalStorageEventCategory
{
    Set,
    Remove,
    Clear
}

public class LocalStorageEventArgs : EventArgs
{
    public LocalStorageEventCategory Category { get; }
    public string? Key { get; }
    public string? Value { get; }

    public LocalStorageEventArgs(LocalStorageEventCategory category, string? key, string? value)
    {
        this.Category = category;
        this.Key = key;
        this.Value = value;
    }
}