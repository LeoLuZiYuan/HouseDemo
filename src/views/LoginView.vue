<template>
  <div class="min-h-screen bg-gradient-to-b from-blue-50 to-white flex items-center justify-center p-4">
    <div class="w-full max-w-md bg-white rounded-lg shadow-lg p-8">
      <h1 class="text-3xl font-bold text-center mb-2">HouseDemo</h1>
      <p class="text-center text-gray-600 mb-8">房東管理面板</p>

      <form @submit.prevent="handleLogin" class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">
            郵箱
          </label>
          <input
            v-model="form.email"
            type="email"
            required
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            placeholder="landlord@housedemoll.com"
          />
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">
            密碼
          </label>
          <input
            v-model="form.password"
            type="password"
            required
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            placeholder="請輸入密碼"
          />
        </div>

        <button
          type="submit"
          :disabled="loading"
          class="w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 rounded-lg transition disabled:opacity-50"
        >
          {{ loading ? '登入中...' : '登入' }}
        </button>
      </form>

      <div v-if="error" class="mt-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
        {{ error }}
      </div>

      <div class="mt-6 p-3 bg-blue-50 border border-blue-200 rounded text-sm text-gray-700">
        <p><strong>測試帳號：</strong></p>
        <p>郵箱: landlord@housedemoll.com</p>
        <p>密碼: demo123456</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthApi } from '../composables/useAuthApi'

const router = useRouter()
const { login } = useAuthApi()
const form = ref({ email: '', password: '' })
const loading = ref(false)
const error = ref('')

const handleLogin = async () => {
  loading.value = true
  error.value = ''

  try {
    const data = await login(form.value.email, form.value.password)
    localStorage.setItem('auth_token', data.token)
    router.push('/admin')
  } catch (err) {
    error.value = '郵箱或密碼不正確'
  } finally {
    loading.value = false
  }
}
</script>
