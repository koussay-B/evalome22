import axios from 'axios'

const http = axios.create({
    baseURL: import.meta.env.VITE_API_URL,
    headers: {
        'Content-Type': 'application/json',
    },
})

// Inject JWT token on every request
http.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token')
        if (token) {
            config.headers.Authorization = `Bearer ${token}`
        }
        return config
    },
    (error) => Promise.reject(error)
)

// Redirect to login only on expired session (not failed login attempts)
http.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            const hadToken = !!localStorage.getItem('token')
            if (hadToken) {
                localStorage.removeItem('token')
                localStorage.removeItem('user')
                localStorage.removeItem('company')
                window.location.href = '/auth/login'
            }
        }
        return Promise.reject(error)
    }
)

export default http
