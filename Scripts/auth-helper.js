// Get JWT token
function getAuthToken() {
    return localStorage.getItem('jwtToken');
}

// Get current user
function getCurrentUser() {
    const userData = localStorage.getItem('userData');
    return userData ? JSON.parse(userData) : null;
}

// Check if authenticated
function isAuthenticated() {
    return !!getAuthToken();
}

// Get authorization header
function getAuthHeader() {
    const token = getAuthToken();
    return {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    };
}

// Fetch with authentication
async function fetchWithAuth(url, options = {}) {
    const headers = {
        ...getAuthHeader(),
        ...options.headers
    };

    const response = await fetch(url, {
        ...options,
        headers: headers
    });

    // If 401, token expired - redirect to login
    if (response.status === 401) {
        localStorage.removeItem('jwtToken');
        localStorage.removeItem('userData');
        window.location.href = '/Account/Login';
    }

    return response;
}

// Logout
function logoutUser() {
    localStorage.removeItem('jwtToken');
    localStorage.removeItem('userData');
    window.location.href = '/Account/Login';
}
