// Theme Toggle for Swagger UI
(function() {
    'use strict';
    
    // Wait for Swagger UI to load
    window.addEventListener('load', function() {
        setTimeout(function() {
            addThemeToggle();
            loadTheme();
        }, 500);
    });
    
    function addThemeToggle() {
        const topbar = document.querySelector('.topbar');
        if (!topbar || document.getElementById('theme-toggle-btn')) return;
        
        const toggleBtn = document.createElement('button');
        toggleBtn.id = 'theme-toggle-btn';
        toggleBtn.innerHTML = `
            <svg id="moon-icon" width="20" height="20" viewBox="0 0 24 24" fill="currentColor" style="display: none;">
                <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/>
            </svg>
            <svg id="sun-icon" width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                <circle cx="12" cy="12" r="5"/>
                <path d="M12 1v2m0 18v2M4.22 4.22l1.42 1.42m12.72 12.72l1.42 1.42M1 12h2m18 0h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42"/>
            </svg>
        `;
        
        toggleBtn.style.cssText = `
            position: fixed;
            top: 10px;
            right: 20px;
            z-index: 10000;
            background: #238636;
            border: none;
            border-radius: 8px;
            padding: 10px 16px;
            cursor: pointer;
            display: flex;
            align-items: center;
            gap: 8px;
            color: white;
            font-weight: 500;
            font-size: 14px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
            transition: all 0.3s ease;
        `;
        
        toggleBtn.onmouseover = function() {
            this.style.background = '#2ea043';
            this.style.transform = 'scale(1.05)';
        };
        
        toggleBtn.onmouseout = function() {
            this.style.background = '#238636';
            this.style.transform = 'scale(1)';
        };
        
        toggleBtn.onclick = toggleTheme;
        document.body.appendChild(toggleBtn);
    }
    
    function toggleTheme() {
        const isDark = document.body.classList.contains('dark-mode');
        if (isDark) {
            document.body.classList.remove('dark-mode');
            localStorage.setItem('swagger-theme', 'light');
            updateIcon(false);
        } else {
            document.body.classList.add('dark-mode');
            localStorage.setItem('swagger-theme', 'dark');
            updateIcon(true);
        }
    }
    
    function loadTheme() {
        const theme = localStorage.getItem('swagger-theme') || 'dark';
        if (theme === 'dark') {
            document.body.classList.add('dark-mode');
            updateIcon(true);
        } else {
            document.body.classList.remove('dark-mode');
            updateIcon(false);
        }
    }
    
    function updateIcon(isDark) {
        const moonIcon = document.getElementById('moon-icon');
        const sunIcon = document.getElementById('sun-icon');
        if (moonIcon && sunIcon) {
            if (isDark) {
                moonIcon.style.display = 'none';
                sunIcon.style.display = 'block';
            } else {
                moonIcon.style.display = 'block';
                sunIcon.style.display = 'none';
            }
        }
    }
})();
