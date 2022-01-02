window.LocalStorageExtension = {
    LocalStorageContains: (key) => {
        return !!localStorage.getItem(key);
    },

    SetLocalStorage: (key, value) => {
        localStorage.setItem(key, value);
    },

    GetLocalStorage: (key) => {
        return localStorage.getItem(key);
    },

    RemoveLocalStorage: (key) => {
        localStorage.removeItem(key);
    },

    LocalStorageIsAvailable: () => {
        return !!window.localStorage;
    },

    ClearLocalStorage: () => {
        localStorage.clear();
    }
}