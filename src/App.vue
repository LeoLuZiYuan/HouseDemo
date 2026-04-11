<template>
  <div class="min-h-screen bg-gray-50">
    <AppHeader />
    <RouterView />
  </div>
</template>

<script setup>
import AppHeader from './components/AppHeader.vue'
import { RouterView } from 'vue-router'
</script>
