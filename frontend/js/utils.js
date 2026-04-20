/**
 * 城市智慧路灯管理信息系统 - 通用工具函数
 */

// ====== Toast 通知 ======
function showToast(message, type = 'info', duration = 3000) {
    let container = document.querySelector('.toast-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'toast-container';
        document.body.appendChild(container);
    }
    const toast = document.createElement('div');
    const iconNames = { success: 'check-circle', error: 'x-circle', warning: 'alert-triangle', info: 'info' };
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `<span><i data-lucide="${iconNames[type] || 'info'}" style="width:16px;height:16px"></i></span><span>${message}</span>`;
    if (window.lucide) lucide.createIcons({ nodes: [toast] });
    container.appendChild(toast);
    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(100%)';
        setTimeout(() => toast.remove(), 300);
    }, duration);
}

// ====== Auth 工具 ======
function getUser() {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
}

function isLoggedIn() {
    return !!localStorage.getItem('token');
}

function hasRole(role) {
    const user = getUser();
    return user && user.roles && user.roles.includes(role);
}

function requireAuth() {
    if (!isLoggedIn()) {
        window.location.href = '/pages/login.html';
        return false;
    }
    return true;
}

function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/pages/login.html';
}

// ====== 日期格式化 ======
function formatDate(dateStr) {
    if (!dateStr) return '-';
    const d = new Date(dateStr);
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

function formatDateTime(dateStr) {
    if (!dateStr) return '-';
    const d = new Date(dateStr);
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')} ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
}

// ====== 状态标签工厂 ======
const STATUS_MAP = {
    onlineStatus: { 0: { text: '离线', class: 'tag-default' }, 1: { text: '在线', class: 'tag-success' } },
    lightStatus: { 0: { text: '关灯', class: 'tag-default' }, 1: { text: '亮灯', class: 'tag-success' } },
    deviceStatus: {
        0: { text: '故障', class: 'tag-danger' }, 1: { text: '在线正常', class: 'tag-success' },
        2: { text: '在线异常', class: 'tag-warning' }, 3: { text: '离线', class: 'tag-default' },
        4: { text: '待检修', class: 'tag-info' }, 5: { text: '暂停运行', class: 'tag-default' }
    },
    alarmType: {
        1: '灯具故障', 2: '电压异常', 3: '电流异常', 4: '通信故障', 5: '线缆异常', 6: '温度异常', 7: '漏电告警', 8: '其他'
    },
    alarmLevel: {
        1: { text: '低', class: 'tag-info' }, 2: { text: '中', class: 'tag-warning' },
        3: { text: '高', class: 'tag-danger' }, 4: { text: '紧急', class: 'tag-danger' }
    },
    alarmStatus: {
        0: { text: '未处理', class: 'tag-danger' }, 1: { text: '处理中', class: 'tag-warning' },
        2: { text: '已处理', class: 'tag-success' }, 3: { text: '已忽略', class: 'tag-default' }
    },
    workOrderStatus: {
        0: { text: '待分配', class: 'tag-default' }, 1: { text: '已分配', class: 'tag-info' },
        2: { text: '处理中', class: 'tag-warning' }, 3: { text: '已完成', class: 'tag-success' },
        4: { text: '已关闭', class: 'tag-default' }
    },
    workOrderPriority: {
        1: { text: '低', class: 'tag-info' }, 2: { text: '中', class: 'tag-warning' },
        3: { text: '高', class: 'tag-danger' }, 4: { text: '紧急', class: 'tag-danger' }
    },
    repairStatus: {
        0: { text: '待审核', class: 'tag-warning' }, 1: { text: '已审核', class: 'tag-success' },
        2: { text: '处理中', class: 'tag-warning' }, 3: { text: '已完成', class: 'tag-success' },
        4: { text: '已驳回', class: 'tag-danger' }
    },
    strategyType: { 1: '定时策略', 2: '光控策略', 3: '节假日策略', 4: '人流策略' },
    cabinetStatus: {
        0: { text: '故障', class: 'tag-danger' }, 1: { text: '正常', class: 'tag-success' },
        2: { text: '维护中', class: 'tag-warning' }
    },
    announcementType: { 1: '系统通知', 2: '维护公告', 3: '政策通知' },
    announcementStatus: {
        0: { text: '草稿', class: 'tag-default' }, 1: { text: '已发布', class: 'tag-success' },
        2: { text: '已撤回', class: 'tag-warning' }
    }
};

function statusTag(mapKey, value) {
    const map = STATUS_MAP[mapKey];
    if (!map) return value;
    const item = map[value];
    if (!item) return value;
    if (typeof item === 'string') return item;
    return `<span class="tag ${item.class}">${item.text}</span>`;
}

// ====== 分页渲染 ======
function renderPagination(containerId, total, page, size, onPageChange) {
    const container = document.getElementById(containerId);
    if (!container) return;
    const totalPages = Math.ceil(total / size);
    if (totalPages <= 1) { container.innerHTML = ''; return; }

    let html = `<div class="pagination">
        <span class="pagination-info">共 ${total} 条，第 ${page}/${totalPages} 页</span>
        <div class="pagination-btns">
            <button class="page-btn" ${page <= 1 ? 'disabled' : ''} onclick="window._paginate(${page - 1})">‹</button>`;

    const start = Math.max(1, page - 2);
    const end = Math.min(totalPages, page + 2);
    for (let i = start; i <= end; i++) {
        html += `<button class="page-btn ${i === page ? 'active' : ''}" onclick="window._paginate(${i})">${i}</button>`;
    }

    html += `<button class="page-btn" ${page >= totalPages ? 'disabled' : ''} onclick="window._paginate(${page + 1})">›</button>
        </div></div>`;

    container.innerHTML = html;
    window._paginate = onPageChange;
}

// ====== 侧边栏生成 ======
function renderSidebar(activePage) {
    const user = getUser();
    const isAdmin = hasRole('ADMIN');
    const isOperator = hasRole('OPERATOR');

    const menuItems = [
        { section: '总览' },
        { icon: 'layout-dashboard', text: '仪表盘', href: '/pages/dashboard.html', id: 'dashboard' },
        { section: '监控管理' },
        { icon: 'map-pin', text: '设备地图', href: '/pages/map.html', id: 'map' },
        { icon: 'lightbulb', text: '路灯设备', href: '/pages/device.html', id: 'device' },
        { icon: 'server', text: '电柜管理', href: '/pages/cabinet.html', id: 'cabinet' },
        { section: '控制策略' },
        { icon: 'sliders-horizontal', text: '实时控制', href: '/pages/control.html', id: 'control', roles: ['ADMIN', 'OPERATOR'] },
        { icon: 'clipboard-list', text: '策略管理', href: '/pages/strategy.html', id: 'strategy' },
        { section: '运维管理' },
        { icon: 'bell-ring', text: '告警中心', href: '/pages/alarm.html', id: 'alarm' },
        { icon: 'wrench', text: '维修工单', href: '/pages/workorder.html', id: 'workorder' },
        { icon: 'file-text', text: '报修管理', href: '/pages/repair.html', id: 'repair' },
        { section: '数据分析' },
        { icon: 'bar-chart-3', text: '数据统计', href: '/pages/statistics.html', id: 'statistics' },
        { section: '系统管理', roles: ['ADMIN'] },
        { icon: 'users', text: '用户管理', href: '/pages/users.html', id: 'users', roles: ['ADMIN'] },
        { icon: 'megaphone', text: '公告管理', href: '/pages/announcement.html', id: 'announcement', roles: ['ADMIN'] },
        { section: 'IoT通信', roles: ['ADMIN', 'OPERATOR'] },
        { icon: 'radio', text: 'MQTT通信', href: '/pages/mqtt.html', id: 'mqtt', roles: ['ADMIN', 'OPERATOR'] },
    ];

    let html = `
    <div class="sidebar-brand">
        <div class="brand-icon"><i data-lucide="lamp" style="width:20px;height:20px;color:#fff"></i></div>
        <h1>智慧路灯<small>管理信息系统</small></h1>
    </div>
    <nav class="sidebar-nav">`;

    for (const item of menuItems) {
        if (item.roles && user) {
            const hasAccess = item.roles.some(r => user.roles.includes(r));
            if (!hasAccess) continue;
        }
        if (item.section) {
            html += `<div class="nav-section">${item.section}</div>`;
        } else {
            html += `<a class="nav-item ${activePage === item.id ? 'active' : ''}" href="${item.href}">
                <span class="nav-icon"><i data-lucide="${item.icon}" style="width:18px;height:18px"></i></span>${item.text}
            </a>`;
        }
    }

    html += '</nav>';
    return html;
}

// ====== 顶栏生成 ======
function renderHeader(title, breadcrumb) {
    const user = getUser();
    return `
    <div class="header-left">
        <div class="breadcrumb">
            <a href="/pages/dashboard.html">首页</a>
            <span>/</span>
            <span>${breadcrumb || title}</span>
        </div>
    </div>
    <div class="header-right">
        <button class="header-btn" title="告警" onclick="location.href='/pages/alarm.html'">
            <i data-lucide="bell" style="width:18px;height:18px"></i><span class="badge" id="alarmBadge" style="display:none">0</span>
        </button>
        <div class="user-menu" id="userMenuTrigger">
            <div class="user-avatar">${user ? (user.realName || user.username).charAt(0) : '?'}</div>
            <span class="user-name">${user ? (user.realName || user.username) : '未登录'}</span>
            <span style="margin-left:2px;display:flex"><i data-lucide="chevron-down" style="width:14px;height:14px;color:var(--text-muted)"></i></span>
        </div>
        <div id="userDropdown" class="user-dropdown" style="display:none;position:absolute;top:52px;right:20px;background:#fff;border-radius:10px;box-shadow:0 8px 24px rgba(0,0,0,0.15);padding:6px;z-index:200;min-width:160px;border:1px solid var(--border-light)">
            <a href="/pages/profile.html" style="display:flex;align-items:center;gap:8px;padding:10px 14px;border-radius:8px;font-size:13px;color:#1E293B;transition:background 0.2s" onmouseover="this.style.background='#F1F5F9'" onmouseout="this.style.background='transparent'"><i data-lucide="user" style="width:15px;height:15px"></i> 个人中心</a>
            <div style="border-top:1px solid #E2E8F0;margin:4px 8px"></div>
            <a href="#" onclick="logout();return false" style="display:flex;align-items:center;gap:8px;padding:10px 14px;border-radius:8px;font-size:13px;color:#EF4444;transition:background 0.2s" onmouseover="this.style.background='rgba(239,68,68,0.06)'" onmouseout="this.style.background='transparent'"><i data-lucide="log-out" style="width:15px;height:15px"></i> 退出登录</a>
        </div>
    </div>`;
}

// Toggle user dropdown - use consistent style.display
document.addEventListener('click', (e) => {
    const dropdown = document.getElementById('userDropdown');
    const trigger = document.getElementById('userMenuTrigger');
    if (!dropdown) return;
    if (trigger && trigger.contains(e.target)) {
        // Toggle dropdown visibility
        dropdown.style.display = dropdown.style.display === 'none' ? 'block' : 'none';
    } else {
        // Click outside - close dropdown
        dropdown.style.display = 'none';
    }
});

// ====== 初始化页面框架 ======
function initPage(pageId, pageTitle) {
    if (!requireAuth()) return false;
    const sidebar = document.getElementById('sidebar');
    const header = document.getElementById('topHeader');
    if (sidebar) sidebar.innerHTML = renderSidebar(pageId);
    if (header) header.innerHTML = renderHeader(pageTitle);
    // Render Lucide icons in dynamically generated content
    if (window.lucide) lucide.createIcons();
    return true;
}
