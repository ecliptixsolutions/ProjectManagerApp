$(function () {
    initTheme();
    initSidebar();
    initToasts();
    initCharts();
    initDataTables();
});

function initTheme() {
    const theme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-bs-theme', theme);
    $('.theme-toggle').html(theme === 'dark' ? '<i class="fas fa-sun"></i>' : '<i class="fas fa-moon"></i>');
    $(document).on('click', '.theme-toggle', function () {
        const current = document.documentElement.getAttribute('data-bs-theme');
        const next = current === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-bs-theme', next);
        localStorage.setItem('theme', next);
        $(this).html(next === 'dark' ? '<i class="fas fa-sun"></i>' : '<i class="fas fa-moon"></i>');
    });
}

function initSidebar() {
    const currentPath = window.location.pathname.toLowerCase();
    $('.sidebar-nav .nav-link').each(function () {
        const href = $(this).attr('href').toLowerCase();
        if (currentPath === href || (href !== '/' && currentPath.startsWith(href))) {
            $(this).addClass('active');
        }
    });
    $(document).on('click', '.sidebar-toggle, .sidebar-overlay', function () {
        $('.sidebar').toggleClass('show');
        $('.sidebar-overlay').toggleClass('show');
    });
    $(document).on('click', '.sidebar-nav .nav-link', function () {
        if (window.innerWidth <= 768) {
            $('.sidebar').removeClass('show');
            $('.sidebar-overlay').removeClass('show');
        }
    });
}

function initToasts() {
    const success = $('#toast-success').val();
    const error = $('#toast-error').val();
    if (success && success.trim()) showToast('success', success);
    if (error && error.trim()) showToast('error', error);
}

function showToast(type, message) {
    const bg = type === 'success' ? '#10b981' : '#ef4444';
    const icon = type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle';
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 4000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });
    Toast.fire({ icon: type, title: message, background: bg, color: '#fff', iconColor: '#fff' });
}

function initCharts() {
    if (typeof Chart === 'undefined') return;

    $('.chart-revenue-expense').each(function () {
        const labels = $(this).data('labels') || [];
        const revenue = $(this).data('revenue') || [];
        const expense = $(this).data('expense') || [];
        new Chart(this, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    { label: 'Revenue', data: revenue, backgroundColor: 'rgba(16,185,129,.7)', borderRadius: 6 },
                    { label: 'Expense', data: expense, backgroundColor: 'rgba(239,68,68,.7)', borderRadius: 6 }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: true, position: 'top' } },
                scales: { y: { beginAtZero: true, grid: { color: 'rgba(0,0,0,.05)' } } }
            }
        });
    });

    $('.chart-project-status').each(function () {
        const labels = $(this).data('labels') || [];
        const data = $(this).data('values') || [];
        new Chart(this, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{ data: data, backgroundColor: ['#4f46e5','#10b981','#f59e0b','#ef4444','#94a3b8'], borderWidth: 0 }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { position: 'bottom' } },
                cutout: '70%'
            }
        });
    });

    $('.chart-profit-loss').each(function () {
        const labels = $(this).data('labels') || [];
        const profit = $(this).data('profit') || [];
        new Chart(this, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{ label: 'Net Profit', data: profit, borderColor: '#4f46e5', backgroundColor: 'rgba(79,70,229,.1)', fill: true, tension: .4 }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true, grid: { color: 'rgba(0,0,0,.05)' } } }
            }
        });
    });
}

function initDataTables() {
    if ($.fn.DataTable && $('.datatable').length) {
        $('.datatable').DataTable({
            pageLength: 25,
            lengthMenu: [[10, 25, 50, -1], [10, 25, 50, 'All']],
            language: { search: '', searchPlaceholder: 'Search...', lengthMenu: '_MENU_ per page' },
            dom: '<"row g-2 mb-3"<"col-sm-6"l><"col-sm-6"f>>rt<"row g-2 mt-3"<"col-sm-6"i><"col-sm-6"p>>',
            order: []
        });
    }
}
