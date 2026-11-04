import { ref, computed } from 'vue'

export function useAuth() {
  const token = ref(localStorage.getItem('auth_token'))
  const apiBase = import.meta.env.VITE_API_BASE

  const isAuthenticated = computed(() => !!token.value)

  const login = async (email, password) => {
    const response = await fetch(`${apiBase}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    })

    if (!response.ok) throw new Error('Login failed')

    const data = await response.json()
    token.value = data.token
    localStorage.setItem('auth_token', data.token)
    return data
  }

  const logout = () => {
    token.value = null
    localStorage.removeItem('auth_token')
  }

  return { token, isAuthenticated, login, logout }
}
