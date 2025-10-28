const API_URL = 'https://localhost:7179/api'

export const apiFetch = async (endpoint, options = {}) => {
    const res = await fetch(`${API_URL}${endpoint}`, {
        ...options,
        credentials: 'include',
        headers: {
            'Content-Type': 'application/json',
            ...(options.headers || {}),
        },
    })

    if (res.status === 401) {
        throw new Error("Unauthorized");
    }

    return res.ok ? await res.json() : Promise.reject(await res.json());
};