/**
 * 城市智慧路灯管理信息系统 - API 请求封装
 */
const API_BASE = location.port === '3000' ? 'http://localhost:8080/api' : '/api';

const api = {
    /**
     * 通用请求方法
     */
    async request(url, options = {}) {
        const token = localStorage.getItem('token');
        const headers = {
            'Content-Type': 'application/json',
            ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
            ...options.headers
        };

        try {
            const response = await fetch(`${API_BASE}${url}`, {
                ...options,
                headers
            });

            // 401 未授权处理
            if (response.status === 401) {
                localStorage.removeItem('token');
                localStorage.removeItem('user');
                if (!window.location.pathname.includes('login')) {
                    window.location.href = '/pages/login.html';
                }
                throw new Error('登录已过期，请重新登录');
            }

            const data = await response.json();

            if (data.code !== 200) {
                throw new Error(data.message || '请求失败');
            }

            return data;
        } catch (error) {
            if (error.message === 'Failed to fetch') {
                throw new Error('网络连接失败，请检查后端服务');
            }
            throw error;
        }
    },

    get(url, params) {
        let queryStr = '';
        if (params) {
            const searchParams = new URLSearchParams();
            Object.entries(params).forEach(([key, value]) => {
                if (value !== null && value !== undefined && value !== '') {
                    searchParams.append(key, value);
                }
            });
            queryStr = '?' + searchParams.toString();
        }
        return this.request(url + queryStr);
    },

    post(url, body) {
        return this.request(url, {
            method: 'POST',
            body: JSON.stringify(body)
        });
    },

    put(url, body) {
        return this.request(url, {
            method: 'PUT',
            body: JSON.stringify(body)
        });
    },

    delete(url) {
        return this.request(url, { method: 'DELETE' });
    }
};

// ====== Auth API ======
const authApi = {
    login: (data) => api.post('/auth/login', data),
    register: (data) => api.post('/auth/register', data),
    getInfo: () => api.get('/auth/info'),
    changePassword: (data) => api.put('/auth/password', data),
    updateProfile: (data) => api.put('/auth/profile', data),
};

// ====== Statistics API ======
const statsApi = {
    overview: () => api.get('/statistics/overview'),
    energyDaily: (start, end) => api.get('/statistics/energy/daily', { startDate: start, endDate: end }),
    energyByArea: (start, end) => api.get('/statistics/energy/by-area', { startDate: start, endDate: end }),
    alarmByType: () => api.get('/statistics/alarm/by-type'),
    alarmByLevel: () => api.get('/statistics/alarm/by-level'),
    deviceByType: () => api.get('/statistics/device/by-type'),
    deviceByArea: () => api.get('/statistics/device/by-area'),
};

// ====== Streetlight API ======
const lightApi = {
    list: (params) => api.get('/streetlights', params),
    all: (areaId) => api.get('/streetlights/all', { areaId }),
    detail: (id) => api.get(`/streetlights/${id}`),
    create: (data) => api.post('/streetlights', data),
    update: (id, data) => api.put(`/streetlights/${id}`, data),
    delete: (id) => api.delete(`/streetlights/${id}`),
};

// ====== Area API ======
const areaApi = {
    list: () => api.get('/areas'),
    tree: () => api.get('/areas/tree'),
    children: (parentId) => api.get(`/areas/children/${parentId}`),
};

// ====== Cabinet API ======
const cabinetApi = {
    list: (params) => api.get('/cabinets', params),
    detail: (id) => api.get(`/cabinets/${id}`),
};

// ====== Alarm API ======
const alarmApi = {
    list: (params) => api.get('/alarms', params),
    detail: (id) => api.get(`/alarms/${id}`),
    handle: (id, data) => api.put(`/alarms/${id}/handle`, data),
};

// ====== Work Order API ======
const orderApi = {
    list: (params) => api.get('/work-orders', params),
    detail: (id) => api.get(`/work-orders/${id}`),
    create: (data) => api.post('/work-orders', data),
    update: (id, data) => api.put(`/work-orders/${id}`, data),
    updateStatus: (id, data) => api.put(`/work-orders/${id}/status`, data),
};

// ====== Strategy API ======
const strategyApi = {
    list: (params) => api.get('/strategies', params),
    detail: (id) => api.get(`/strategies/${id}`),
    create: (data) => api.post('/strategies', data),
    update: (id, data) => api.put(`/strategies/${id}`, data),
    toggle: (id) => api.put(`/strategies/${id}/toggle`),
    delete: (id) => api.delete(`/strategies/${id}`),
};

// ====== Control API ======
const controlApi = {
    execute: (data) => api.post('/control/execute', data),
    logs: (params) => api.get('/control/logs', params),
};

// ====== Repair API ======
const repairApi = {
    list: (params) => api.get('/repair-reports', params),
    my: (params) => api.get('/repair-reports/my', params),
    detail: (id) => api.get(`/repair-reports/${id}`),
    create: (data) => api.post('/repair-reports', data),
    handle: (id, data) => api.put(`/repair-reports/${id}/handle`, data),
};

// ====== Announcement API ======
const announceApi = {
    published: () => api.get('/announcements/published'),
    list: (params) => api.get('/announcements', params),
    detail: (id) => api.get(`/announcements/${id}`),
    create: (data) => api.post('/announcements', data),
    update: (id, data) => api.put(`/announcements/${id}`, data),
    publish: (id) => api.put(`/announcements/${id}/publish`),
    withdraw: (id) => api.put(`/announcements/${id}/withdraw`),
    delete: (id) => api.delete(`/announcements/${id}`),
};

// ====== User API (Admin) ======
const userApi = {
    list: (params) => api.get('/users', params),
    detail: (id) => api.get(`/users/${id}`),
    update: (id, data) => api.put(`/users/${id}`, data),
    delete: (id) => api.delete(`/users/${id}`),
    resetPassword: (id) => api.put(`/users/${id}/reset-password`),
};

// ====== Role API ======
const roleApi = {
    list: () => api.get('/roles'),
};
