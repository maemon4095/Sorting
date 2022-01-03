window.LocalStorageExtension = {
    dotnetReference: null,
    LocalStorageContains: (key) => {
        return !!localStorage.getItem(key);
    },

    SetLocalStorage: (key, value) => {
        const old = localStorage.getItem(key);
        localStorage.setItem(key, value);

        InvokeLocalStorageEvent({
            Key: key,
            Category: old === null ? 'Add' : 'Set',
            OldValue: old,
            NewValue: key
        });
    },

    GetLocalStorage: (key) => {
        return localStorage.getItem(key);
    },

    RemoveLocalStorage: (key) => {
        const old = localStorage.getItem(key);
        localStorage.removeItem(key);
        InvokeLocalStorageEvent({
            Key: key,
            Category: 'Remove',
            OldValue: old,
            NewValue: null
        });
    },

    LocalStorageIsAvailable: () => {
        return !!window.localStorage;
    },

    ClearLocalStorage: () => {
        localStorage.clear();
        this.InvokeLocalStorageEvent({
            Key: key,
            Category: 'Clear',
            OldValue: null,
            NewValue: null
        });
    },

    SetLocalStorageEventHandle: (reference) => {
        window.LocalStorageExtension.dotnetReference = reference;
        window.addEventListener('storage', FireLocalStorageEvent, false);
    },

    ClearLocalStorageEventHandle: () => {
        window.LocalStorageExtension.dotnetReference = null;
        window.removeEventListener('storage', FireLocalStorageEvent, false);
    },
}

function FireLocalStorageEvent(event) {
    const oldValue = event.oldValue;
    const newValue = event.newValue;
    const args = {
        Key: event.key,
        Category: (oldValue === null && newValue !== null ? 'Add'
            : oldValue !== null && newValue === null ? 'Remove'
                : oldValue !== null && newValue !== null ? 'Set'
                    : 'Clear'),
        OldValue: oldValue,
        NewValue: newValue
    };
    this.InvokeResizeEvent(args);
}

function InvokeLocalStorageEvent(args) {
    const ref = window.LocalStorageExtension.dotnetReference;
    if (!ref) return;
    ref.invokeMethodAsync('FireStorageChanged', args.Category, args.Key, args.OldValue, args.NewValue);
}