window.DimensionExtension = {
    dotnetReference: null,

    GetClientDimensions: () => {
        return {
            Width: document.body.clientWidth,
            Height: document.body.clientHeight
        };
    },

    GetInnerWindowDimensions: () => {
        return {
            Width: window.innerWidth,
            Height: window.innerHeight
        };
    },

    SetResizeEventHandle: (reference) => {
        window.DimensionExtension.dotnetReference = reference;
        window.addEventListener('resize', InvokeResizeEvent, false);
    },

    ClearResizeEventHandle: () => {
        window.DimensionExtension.dotnetReference = null;
        window.removeEventListener('resize', InvokeResizeEvent, false);
    },
}

function InvokeResizeEvent(event) {
    const ref = window.DimensionExtension.dotnetReference;
    if (!ref) return;
    ref.invokeMethodAsync('FireResizeWindow');
}