<template>
  <div class="paths-page">
    <div class="paths-hero">
      <div class="container">
        <header class="section-head paths-head">
          <span class="eyebrow">Ścieżki nauki</span>
          <h1 class="paths-title">Zaplanowane ścieżki <em>rozwoju</em></h1>
          <p class="paths-subtitle">
            Kuratorowane zestawy kursów, które prowadzą od podstaw do zaawansowanych tematów. Wybierz ścieżkę dopasowaną do Twojego celu — frontend, backend albo pełen stos.
          </p>
        </header>
      </div>
    </div>

    <div class="container paths-body">
      <div v-if="isLoading" class="paths-loading">
        <div class="spinner" />
        <p>Ładowanie ścieżek...</p>
      </div>

      <div v-else-if="isError" class="paths-empty">
        <p>Nie udało się pobrać ścieżek. Spróbuj ponownie.</p>
      </div>

      <div v-else-if="items.length === 0" class="paths-empty">
        <p>Wkrótce pojawią się tutaj nowe ścieżki.</p>
      </div>

      <div v-else class="paths-grid">
        <PathCard v-for="(path, index) in items" :key="path.id" :path="path" :index="index" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useLearningPaths } from '@/features/learning-paths/composables/useLearningPaths'
import PathCard from '@/features/learning-paths/components/PathCard.vue'

const { isLoading, isError, data } = useLearningPaths()
const items = computed(() => data.value?.items ?? [])
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.paths-page {
  min-height: 100vh;
}

.paths-hero {
  padding: 32px 0 0;
}

.paths-head {
  margin-bottom: 40px;
}

.paths-title {
  font-size: $font-size-2xl;
  margin-top: 18px;
}

.paths-subtitle {
  font-size: 0.98rem;
  line-height: 1.6;
}

.paths-body {
  padding-bottom: 96px;
}

.paths-loading,
.paths-empty {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 48px 0;
  color: $color-muted;
}

.paths-loading p {
  @include mono-label;
  color: $color-faint;
}

.paths-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 24px;
}

.spinner {
  width: 22px;
  height: 22px;
  border: 2px solid rgba($color-ink, 0.1);
  border-top-color: $color-accent;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 560px) {
  .paths-hero {
    padding-top: 40px;
  }

  .paths-grid {
    grid-template-columns: 1fr;
  }
}
</style>
