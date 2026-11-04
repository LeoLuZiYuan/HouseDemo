<template>
  <main class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-gray-900">探索特色房型</h1>
      <p class="mt-2 text-gray-500">精選 {{ properties.length }} 種風格各異的住宿空間</p>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="flex justify-center py-20">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-rose-500"></div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-lg p-6 text-center py-20">
      <p class="text-red-600 font-semibold mb-4">{{ error }}</p>
      <button @click="fetchProperties" class="px-4 py-2 bg-rose-500 text-white rounded hover:bg-rose-600 transition">
        重新加載
      </button>
    </div>

    <!-- Empty State -->
    <div v-else-if="properties.length === 0" class="text-center py-20">
      <p class="text-gray-400 text-lg">目前沒有可用的房型</p>
    </div>

    <!-- Properties Grid -->
    <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      <RoomCard v-for="room in properties" :key="room.id" :room="room" />
    </div>
  </main>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import RoomCard from '../components/RoomCard.vue'
import { usePropertyApi } from '../composables/usePropertyApi'

const properties = ref([])
const isLoading = ref(true)
const error = ref(null)
const { getProperties } = usePropertyApi()

const fetchProperties = async () => {
  isLoading.value = true
  error.value = null
  try {
    properties.value = await getProperties()
  } catch (err) {
    error.value = 'Failed to load properties. Please try again.'
    console.error('Error fetching properties:', err)
  } finally {
    isLoading.value = false
  }
}

onMounted(fetchProperties)
</script>
