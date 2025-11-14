// Dark Mode Toggle Script
(function() {
    'use strict';

    const DARK_MODE_KEY = 'darkMode';
    const DARK_MODE_CLASS = 'dark-mode';

    // Lấy trạng thái dark mode từ localStorage hoặc system preference
    function getDarkModePreference() {
        const saved = localStorage.getItem(DARK_MODE_KEY);
        if (saved !== null) {
            return saved === 'true';
        }
        // Nếu chưa có preference, dùng system preference
        return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    }

    // Áp dụng dark mode
    function applyDarkMode(isDark) {
        if (isDark) {
            document.documentElement.classList.add(DARK_MODE_CLASS);
            document.body.classList.add(DARK_MODE_CLASS);
        } else {
            document.documentElement.classList.remove(DARK_MODE_CLASS);
            document.body.classList.remove(DARK_MODE_CLASS);
        }
        localStorage.setItem(DARK_MODE_KEY, isDark.toString());
        updateToggleButton(isDark);
    }

    // Cập nhật icon của nút toggle
    function updateToggleButton(isDark) {
        const toggleBtn = document.getElementById('darkModeToggle');
        if (toggleBtn) {
            const icon = toggleBtn.querySelector('i, svg');
            if (icon) {
                if (isDark) {
                    // Icon mặt trời (light mode)
                    if (icon.tagName === 'I') {
                        icon.className = 'bi bi-sun-fill';
                    } else {
                        icon.innerHTML = `
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z"></path>
                        `;
                    }
                    toggleBtn.setAttribute('aria-label', 'Chuyển sang chế độ sáng');
                } else {
                    // Icon mặt trăng (dark mode)
                    if (icon.tagName === 'I') {
                        icon.className = 'bi bi-moon-fill';
                    } else {
                        icon.innerHTML = `
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z"></path>
                        `;
                    }
                    toggleBtn.setAttribute('aria-label', 'Chuyển sang chế độ tối');
                }
            }
        }
    }

    // Khởi tạo dark mode khi trang load
    function initDarkMode() {
        const isDark = getDarkModePreference();
        applyDarkMode(isDark);

        // Lắng nghe sự kiện click trên nút toggle
        const toggleBtn = document.getElementById('darkModeToggle');
        if (toggleBtn) {
            toggleBtn.addEventListener('click', function() {
                const currentIsDark = document.documentElement.classList.contains(DARK_MODE_CLASS);
                applyDarkMode(!currentIsDark);
            });
        }

        // Lắng nghe thay đổi system preference (nếu chưa có preference được lưu)
        if (window.matchMedia) {
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function(e) {
                if (localStorage.getItem(DARK_MODE_KEY) === null) {
                    applyDarkMode(e.matches);
                }
            });
        }
    }

    // Chạy khi DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initDarkMode);
    } else {
        initDarkMode();
    }
})();

