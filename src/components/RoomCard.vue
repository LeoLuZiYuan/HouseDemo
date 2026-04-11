<template>
  <RouterLink
    :to="`/room/${room.id}`"
    class="group block bg-white rounded-2xl overflow-hidden shadow-sm hover:shadow-lg transition-shadow duration-300 border border-gray-100"
  >
    <div class="relative overflow-hidden aspect-[4/3]">
      <img
        :src="room.coverImage"
        :alt="room.name"
        class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
        loading="lazy"
      />
    </div>
    <div class="p-4">
      <div class="flex items-start justify-between gap-2">
        <h3 class="font-semibold text-gray-900 text-base leading-snug">{{ room.name }}</h3>
        <span class="text-xs text-gray-500 whitespace-nowrap mt-0.5">{{ room.area }}</span>
      </div>
      <p class="text-sm text-gray-500 mt-1 line-clamp-2">{{ room.shortDesc }}</p>
      <div class="mt-3 flex items-baseline gap-1">
        <span class="font-semibold text-gray-900">{{ room.monthlyRent }}</span>
        <span class="text-sm text-gray-400">/ 月</span>
      </div>
    </div>
  </RouterLink>
</template>

<script setup>
import { RouterLink } from 'vue-router'

defineProps({
  room: {
    type: Object,
    required: true,
  },
})
</script>
