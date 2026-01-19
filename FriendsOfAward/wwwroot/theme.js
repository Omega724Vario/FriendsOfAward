window.themeManager = {
    setTheme: function (theme) {
        if (theme) {
            document.documentElement.setAttribute("data-theme", theme);
            localStorage.setItem("theme", theme);
        } else {
            document.documentElement.removeAttribute("data-theme");
            localStorage.removeItem("theme");
        }
    },
    getTheme: function () {
        return document.documentElement.getAttribute("data-theme");
    },
    getStoredTheme: function () {
        return localStorage.getItem("theme");
    },
    initTheme: function () {
        // This runs on page load to restore theme from storage if available
        const stored = localStorage.getItem("theme");
        if (stored) {
            this.setTheme(stored);
        }
    },
    ensureTheme: function () {
        // Ensure the theme attribute persists
        const stored = localStorage.getItem("theme");
        const current = document.documentElement.getAttribute("data-theme");
        if (stored && stored !== current) {
            this.setTheme(stored);
        }
    }
};

// Initialize theme on page load
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function () {
        window.themeManager.initTheme();
    });
} else {
    window.themeManager.initTheme();
}

// For Blazor enhanced navigation - reapply theme on every navigation
if (window.Blazor) {
    window.Blazor.addEventListener('enhancedload', function () {
        window.themeManager.ensureTheme();
    });
}

// Mutation observer to detect when data-theme is removed
const observer = new MutationObserver(function (mutations) {
    mutations.forEach(function (mutation) {
        if (mutation.type === 'attributes' && mutation.attributeName === 'data-theme') {
            const stored = localStorage.getItem("theme");
            const current = document.documentElement.getAttribute("data-theme");
            
            // If there's a stored theme but it's not on the element, reapply it
            if (stored && !current) {
                setTimeout(function () {
                    window.themeManager.setTheme(stored);
                }, 0);
            }
        }
    });
});

// Start observing the document element for attribute changes
observer.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ['data-theme']
});
