const apiBase = 'http://localhost:5088/api'

export function useAuthApi() {
  const login = async (email, password) => {
    const response = await fetch(`${apiBase}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    })

    if (!response.ok) {
      throw new Error('登入失敗')
    }

    return await response.json()
  }

  return { login }
}
