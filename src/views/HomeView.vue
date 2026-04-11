<template>
  <main class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-gray-900">探索特色房型</h1>
      <p class="mt-2 text-gray-500">精選 {{ rooms.length }} 種風格各異的住宿空間</p>
    </div>
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      <RoomCard v-for="room in rooms" :key="room.id" :room="room" />
    </div>
  </main>
</template>

<script setup>
import RoomCard from '../components/RoomCard.vue'
import rooms from '../data/rooms.json'
</script>
