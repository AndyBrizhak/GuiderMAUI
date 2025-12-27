// Файл: wwwroot/js/theme.js

// 1. Применяет тему
window.applyTheme = function (theme) {
    document.documentElement.className = theme;
    localStorage.setItem('theme', theme);
    document.cookie = `theme=${theme};path=/;max-age=31536000`;
}; // <--- Здесь добавлена точка с запятой

// 2. Загружает сохраненную тему
window.loadTheme = function () {
    var theme = localStorage.getItem('theme') || 'light-theme';
    document.documentElement.className = theme;
    return theme === 'dark-theme';
}; // <--- ВАЖНО: Здесь обязательно нужна точка с запятой перед (function...)

// 3. (Секция удалена для MAUI Hybrid)

// 4. Применяем тему немедленно
(function () {
    try {
        var theme = localStorage.getItem('theme') || 'light-theme';
        document.documentElement.className = theme;
    } catch (e) {
        console.warn('Не удалось загрузить тему из LocalStorage:', e);
    }
})();