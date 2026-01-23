/* ============================================
   Gymunity Admin Dashboard - JavaScript
   ============================================ */

document.addEventListener('DOMContentLoaded', function () {
    // Initialize Components
    initSidebarToggle();
    initCurrentTime();
    setActiveNavLink();
    initAlertDismiss();
});

/* ============================================
   SIDEBAR TOGGLE
   ============================================ */

function initSidebarToggle() {
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');

    if (!sidebarToggle || !sidebar) return;

    sidebarToggle.addEventListener('click', function (e) {
        e.preventDefault();
        sidebar.classList.toggle('translate-x-0');
        sidebar.classList.toggle('translate-x-full');
    });

    // Close sidebar when clicking a link
    const navLinks = document.querySelectorAll('.sidebar-item .nav-link');
    navLinks.forEach(link => {
        link.addEventListener('click', function () {
            if (window.innerWidth < 1024) {
                sidebar.classList.add('translate-x-full');
                sidebar.classList.remove('translate-x-0');
            }
        });
    });

    // Close sidebar when clicking outside on mobile
    document.addEventListener('click', function (e) {
        if (window.innerWidth < 1024 && !sidebar.classList.contains('translate-x-full')) {
            if (!sidebar.contains(e.target) && !sidebarToggle.contains(e.target)) {
                sidebar.classList.add('translate-x-full');
                sidebar.classList.remove('translate-x-0');
            }
        }
    });
}

/* ============================================
   ACTIVE NAV LINK
   ============================================ */

function setActiveNavLink() {
    const currentUrl = window.location.pathname.toLowerCase();
    const navLinks = document.querySelectorAll('.sidebar-item .nav-link');

    navLinks.forEach(link => {
        const href = link.getAttribute('href')?.toLowerCase() || '';
        if (currentUrl.includes(href) && href !== '/') {
            link.classList.add('bg-blue-50', 'text-blue-600', 'border-l-blue-600');
            link.classList.remove('text-gray-600', 'border-l-transparent');
            link.setAttribute('aria-current', 'page');
        } else {
            link.classList.remove('bg-blue-50', 'text-blue-600', 'border-l-blue-600');
            link.classList.add('text-gray-600', 'border-l-transparent');
            link.removeAttribute('aria-current');
        }
    });
}

/* ============================================
   CURRENT TIME
   ============================================ */

function initCurrentTime() {
    const timeElement = document.getElementById('currentTime');
    if (!timeElement) return;

    function updateTime() {
        const now = new Date();
        timeElement.textContent = now.toLocaleTimeString();
    }

    updateTime();
    setInterval(updateTime, 1000);
}

/* ============================================
   ALERT AUTO-DISMISS
   ============================================ */

function initAlertDismiss() {
    const alerts = document.querySelectorAll('[class*="bg-"][class*="-50"]');
    alerts.forEach(alert => {
        const dismissBtn = alert.querySelector('[onclick*="remove"]') || alert.querySelector('button');
        if (dismissBtn) {
            dismissBtn.addEventListener('click', function () {
                alert.remove();
            });
        }
    });
}

/* ============================================
   FORM VALIDATION
   ============================================ */

function validateForm(formId) {
    const form = document.getElementById(formId);
    if (!form) return false;

    if (!form.checkValidity()) {
        event.preventDefault();
        event.stopPropagation();
    }

    form.classList.add('validated');
    return form.checkValidity();
}

/* ============================================
   LOADING SPINNER
   ============================================ */

function showLoadingSpinner(buttonId = null) {
    if (buttonId) {
        const button = document.getElementById(buttonId);
        if (button) {
            const originalContent = button.innerHTML;
            button.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Loading...';
            button.disabled = true;
            button.dataset.originalContent = originalContent;
        }
    }
}

function hideLoadingSpinner(buttonId = null) {
    if (buttonId) {
        const button = document.getElementById(buttonId);
        if (button) {
            button.innerHTML = button.dataset.originalContent || 'Submit';
            button.disabled = false;
        }
    }
}

/* ============================================
   CONFIRMATION DIALOG
   ============================================ */

function confirmAction(message = 'Are you sure?') {
    return new Promise((resolve) => {
        if (confirm(message)) {
            resolve(true);
        } else {
            resolve(false);
        }
    });
}

/* ============================================
   COPY TO CLIPBOARD
   ============================================ */

function copyToClipboard(text, feedbackId = null) {
    navigator.clipboard.writeText(text).then(() => {
        if (feedbackId) {
            const element = document.getElementById(feedbackId);
            if (element) {
                const originalText = element.textContent;
                element.textContent = 'Copied!';
                setTimeout(() => {
                    element.textContent = originalText;
                }, 2000);
            }
        }
        showNotification('Copied to clipboard!', 'success');
    }).catch(err => {
        showNotification('Failed to copy', 'error');
    });
}

/* ============================================
   NOTIFICATIONS
   ============================================ */

function showNotification(message, type = 'info', duration = 3000) {
    const typeClasses = {
        'success': { bg: 'bg-green-50', border: 'border-green-200', text: 'text-green-700', icon: 'check-circle' },
        'danger': { bg: 'bg-red-50', border: 'border-red-200', text: 'text-red-700', icon: 'exclamation-circle' },
        'warning': { bg: 'bg-yellow-50', border: 'border-yellow-200', text: 'text-yellow-700', icon: 'exclamation-triangle' },
        'info': { bg: 'bg-blue-50', border: 'border-blue-200', text: 'text-blue-700', icon: 'info-circle' }
    };

    const config = typeClasses[type] || typeClasses['info'];

    const alertHTML = `
        <div class="fixed bottom-5 right-5 z-50 ${config.bg} border ${config.border} rounded-lg ${config.text} p-4 flex items-start gap-3 max-w-sm animate-slideInUp shadow-lg">
            <i class="fas fa-${config.icon} flex-shrink-0 mt-0.5"></i>
            <div class="flex-grow">${message}</div>
            <button class="text-current opacity-50 hover:opacity-75 ml-2" type="button" onclick="this.parentElement.remove()">
                <i class="fas fa-times"></i>
            </button>
        </div>
    `;

    const container = document.createElement('div');
    container.innerHTML = alertHTML;
    document.body.appendChild(container);

    const alert = container.querySelector('div');
    setTimeout(() => {
        alert.remove();
    }, duration);
}

/* ============================================
   TABLE ROW ACTIONS
   ============================================ */

function setupTableRowActions() {
    const rows = document.querySelectorAll('tbody tr');

    rows.forEach(row => {
        row.addEventListener('mouseenter', function () {
            this.classList.add('bg-gray-50');
        });

        row.addEventListener('mouseleave', function () {
            this.classList.remove('bg-gray-50');
        });
    });
}

/* ============================================
   DATE FORMATTING
   ============================================ */

function formatDate(dateString, format = 'short') {
    const date = new Date(dateString);
    const options = format === 'short'
        ? { year: 'numeric', month: 'short', day: 'numeric' }
        : { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' };

    return date.toLocaleDateString('en-US', options);
}

/* ============================================
   CURRENCY FORMATTING
   ============================================ */

function formatCurrency(amount, currency = 'EGP') {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: currency,
    }).format(amount);
}

/* ============================================
   NUMBER FORMATTING
   ============================================ */

function formatNumber(number) {
    return new Intl.NumberFormat('en-US').format(number);
}

/* ============================================
   SEARCH & FILTER
   ============================================ */

function initSearchFilter(inputId, tableId) {
    const searchInput = document.getElementById(inputId);
    const table = document.getElementById(tableId);

    if (!searchInput || !table) return;

    searchInput.addEventListener('keyup', function () {
        const filter = this.value.toUpperCase();
        const rows = table.querySelectorAll('tbody tr');

        rows.forEach(row => {
            const text = row.textContent.toUpperCase();
            row.style.display = text.includes(filter) ? '' : 'none';
        });
    });
}

/* ============================================
   EXPORT TABLE TO CSV
   ============================================ */

function exportTableToCSV(tableId, filename = 'export.csv') {
    const table = document.getElementById(tableId);
    if (!table) return;

    let csv = [];
    const rows = table.querySelectorAll('tr');

    rows.forEach(row => {
        const rowData = [];
        row.querySelectorAll('th, td').forEach(cell => {
            rowData.push('"' + cell.textContent.trim().replace(/"/g, '""') + '"');
        });
        csv.push(rowData.join(','));
    });

    const csvContent = csv.join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(url);
}

/* ============================================
   PRINT PAGE
   ============================================ */

function printPage() {
    window.print();
}

/* ============================================
   PAGE UTILITIES
   ============================================ */

function scrollToTop() {
    window.scrollTo({
        top: 0,
        behavior: 'smooth'
    });
}

// Scroll to top button
document.addEventListener('DOMContentLoaded', function () {
    const scrollToTopBtn = document.createElement('button');
    scrollToTopBtn.id = 'scrollToTopBtn';
    scrollToTopBtn.className = 'fixed bottom-5 right-5 hidden w-12 h-12 bg-blue-600 text-white rounded-full hover:bg-blue-700 transition z-50 items-center justify-center shadow-lg';
    scrollToTopBtn.innerHTML = '<i class="fas fa-arrow-up"></i>';
    document.body.appendChild(scrollToTopBtn);

    window.addEventListener('scroll', function () {
        if (window.scrollY > 300) {
            scrollToTopBtn.classList.remove('hidden');
            scrollToTopBtn.classList.add('flex');
        } else {
            scrollToTopBtn.classList.add('hidden');
            scrollToTopBtn.classList.remove('flex');
        }
    });

    scrollToTopBtn.addEventListener('click', scrollToTop);
});

/* ============================================
   API HELPERS
   ============================================ */

async function fetchWithAuth(url, options = {}) {
    const defaultOptions = {
        headers: {
            'Content-Type': 'application/json',
        }
    };

    const finalOptions = { ...defaultOptions, ...options };

    try {
        const response = await fetch(url, finalOptions);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('Fetch error:', error);
        showNotification('An error occurred. Please try again.', 'danger');
        throw error;
    }
}

/* ============================================
   DEBUG MODE
   ============================================ */

window.adminDebug = {
    enableDebugMode: function () {
        localStorage.setItem('debugMode', 'true');
        console.log('Debug mode enabled');
    },
    disableDebugMode: function () {
        localStorage.removeItem('debugMode');
        console.log('Debug mode disabled');
    },
    isDebugMode: function () {
        return localStorage.getItem('debugMode') === 'true';
    },
    log: function (message, data) {
        if (this.isDebugMode()) {
            console.log(`[ADMIN DEBUG] ${message}`, data);
        }
    }
};

// Expose utilities to window
window.adminUtils = {
    showLoadingSpinner,
    hideLoadingSpinner,
    confirmAction,
    copyToClipboard,
    showNotification,
    formatDate,
    formatCurrency,
    formatNumber,
    validateForm,
    exportTableToCSV,
    printPage,
    scrollToTop,
    fetchWithAuth,
    setupTableRowActions,
    initSearchFilter
};
