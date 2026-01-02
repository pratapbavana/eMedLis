class ApiService {
    constructor() {
        this.baseUrl = '/api';
        this.token = localStorage.getItem('auth_token');
    }

    // Login
    async login(username, password) {
        try {
            const response = await fetch(`${this.baseUrl}/auth/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ username, password })
            });

            const data = await response.json();

            if (data.success) {
                localStorage.setItem('auth_token', data.data.token);
                localStorage.setItem('user_info', JSON.stringify(data.data));
                this.token = data.data.token;
            }

            return data;
        } catch (error) {
            return { success: false, message: error.message };
        }
    }

    // Logout
    async logout() {
        try {
            const response = await fetch(`${this.baseUrl}/auth/logout`, {
                method: 'POST',
                headers: this.getAuthHeaders()
            });

            localStorage.removeItem('auth_token');
            localStorage.removeItem('user_info');
            this.token = null;

            return await response.json();
        } catch (error) {
            return { success: false, message: error.message };
        }
    }

    // Get samples
    async getSamples() {
        try {
            const response = await fetch(`${this.baseUrl}/samplecollection`, {
                method: 'GET',
                headers: this.getAuthHeaders()
            });

            return await response.json();
        } catch (error) {
            return { success: false, message: error.message };
        }
    }

    // Get auth headers
    getAuthHeaders() {
        const headers = {
            'Content-Type': 'application/json'
        };

        if (this.token) {
            headers['Authorization'] = `Bearer ${this.token}`;
        }

        return headers;
    }

    // Check if user is authenticated
    isAuthenticated() {
        return !!localStorage.getItem('auth_token');
    }

    // Get current user
    getCurrentUser() {
        const userInfo = localStorage.getItem('user_info');
        return userInfo ? JSON.parse(userInfo) : null;
    }
}

const apiService = new ApiService();
