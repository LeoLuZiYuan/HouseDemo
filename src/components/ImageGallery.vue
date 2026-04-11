<template>
  <div class="relative">
    <!-- 主圖 -->
    <div class="relative overflow-hidden rounded-2xl aspect-video bg-gray-100">
      <img
        :src="images[current]"
        :alt="`圖片 ${current + 1}`"
        class="w-full h-full object-contain transition-opacity duration-300"
      />
      <!-- 左右切換按鈕 -->
      <button
        v-if="images.length > 1"
        @click="prev"
        class="absolute left-3 top-1/2 -translate-y-1/2 bg-white/80 hover:bg-white rounded-full w-9 h-9 flex items-center justify-center shadow transition"
        aria-label="上一張"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-gray-700" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
        </svg>
      </button>
      <button
        v-if="images.length > 1"
        @click="next"
        class="absolute right-3 top-1/2 -translate-y-1/2 bg-white/80 hover:bg-white rounded-full w-9 h-9 flex items-center justify-center shadow transition"
        aria-label="下一張"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-gray-700" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
        </svg>
      </button>
      <!-- 頁碼指示點 -->
      <div class="absolute bottom-3 left-1/2 -translate-x-1/2 flex gap-1.5">
        <button
          v-for="(_, i) in images"
          :key="i"
          @click="current = i"
          class="w-2 h-2 rounded-full transition"
          :class="i === current ? 'bg-white' : 'bg-white/50'"
          :aria-label="`第 ${i + 1} 張`"
        />
      </div>
    </div>
    <!-- 縮圖列 -->
    <div v-if="images.length > 1" class="flex gap-2 mt-3 overflow-x-auto pb-1">
      <button
        v-for="(img, i) in images"
        :key="i"
        @click="current = i"
        class="flex-none w-20 h-14 rounded-lg overflow-hidden border-2 transition"
        :class="i === current ? 'border-rose-500' : 'border-transparent'"
      >
        <img :src="img" :alt="`縮圖 ${i + 1}`" class="w-full h-full object-cover" loading="lazy" />
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const props = defineProps({
  images: {
    type: Array,
    required: true,
  },
})

const current = ref(0)

function prev() {
  current.value = (current.value - 1 + props.images.length) % props.images.length
}

function next() {
  current.value = (current.value + 1) % props.images.length
}
</script>
