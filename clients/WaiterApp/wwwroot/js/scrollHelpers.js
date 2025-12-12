window.scrollToBottom = (el) => {
    try {
        if (!el) return;
        el.scrollTop = el.scrollHeight;
    } catch (e) {
        console.warn('scrollToBottom failed', e);
    }
};
