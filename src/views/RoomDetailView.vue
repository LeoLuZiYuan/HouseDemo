<template>
  <main class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
    <!-- 找不到房型 -->
    <div v-if="!room" class="text-center py-20">
      <p class="text-gray-400 text-lg">找不到此房型</p>
      <RouterLink to="/" class="mt-4 inline-block text-rose-500 hover:underline">← 回到首頁</RouterLink>
    </div>

    <template v-else>
      <!-- 返回按鈕 -->
      <RouterLink to="/" class="inline-flex items-center gap-1 text-sm text-gray-500 hover:text-rose-500 transition-colors mb-6">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
        </svg>
        所有房型
      </RouterLink>

      <!-- 圖片輪播 -->
      <ImageGallery :images="[room.coverImage, ...room.images]" />

      <!-- 標題區 -->
      <div class="mt-8 flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4">
        <div>
          <h1 class="text-2xl sm:text-3xl font-bold text-gray-900">{{ room.name }}</h1>
          <p class="mt-1 text-gray-500">{{ room.area }} · 台灣</p>
        </div>
        <div class="text-right flex-none">
          <div class="text-2xl font-bold text-gray-900">{{ room.monthlyRent }}</div>
          <div class="text-sm text-gray-400">每月租金（不含管理費）</div>
        </div>
      </div>

      <hr class="my-6 border-gray-200" />

      <!-- 特色標籤 -->
      <div>
        <h2 class="text-lg font-semibold text-gray-900 mb-3">房型特色</h2>
        <div class="flex flex-wrap gap-2">
          <span
            v-for="feature in room.features"
            :key="feature"
            class="px-3 py-1 bg-rose-50 text-rose-600 text-sm rounded-full"
          >
            {{ feature }}
          </span>
        </div>
      </div>

      <hr class="my-6 border-gray-200" />

      <!-- 詳細說明 -->
      <div>
        <h2 class="text-lg font-semibold text-gray-900 mb-3">關於這個空間</h2>
        <p class="text-gray-600 leading-relaxed whitespace-pre-line">{{ room.description }}</p>
      </div>
    </template>
  </main>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import ImageGallery from '../components/ImageGallery.vue'
import { usePropertyApi } from '../composables/usePropertyApi'

const route = useRoute()
const room = ref(null)
const { getPublicPropertyById } = usePropertyApi()

onMounted(async () => {
  room.value = await getPublicPropertyById(route.params.id)
})
</script>
