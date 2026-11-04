<template>
  <div class="p-8">
    <div class="flex justify-between items-center mb-8">
      <h1 class="text-3xl font-bold">房東管理面板</h1>
      <button
        @click="logout"
        class="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
      >
        登出
      </button>
    </div>

    <div class="bg-white rounded-lg shadow p-6">
      <h2 class="text-xl font-semibold mb-4">房型列表</h2>
      <p class="text-gray-600">Phase 2 完成 — 認證系統運作中 ✓</p>
      <p class="text-sm text-gray-500 mt-2">Phase 3 將實現完整 CRUD 管理功能</p>
    </div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { useAuth } from '../composables/useAuth'

const router = useRouter()
const { logout: authLogout } = useAuth()

const logout = () => {
  authLogout()
  router.push('/login')
}
</script>
