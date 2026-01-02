// Change this ONE variable to update API URL everywhere
const API_CONFIG = {
    // Change port here only (e.g., 5000, 5001, 5002)
    baseUrl: 'https://localhost:44390',

    // API endpoints (relative to baseUrl)
    endpoints: {
        login: '/api/auth/login',
        register: '/api/auth/register',
        validate: '/api/auth/validate',
        logout: '/api/auth/logout'
    }
};

// Helper function to get full API URL
function getApiUrl(endpoint) {
    return API_CONFIG.baseUrl + API_CONFIG.endpoints[endpoint];
}

// Export for use in other scripts
window.API_CONFIG = API_CONFIG;
window.getApiUrl = getApiUrl;