<template>
  <div class="paths-page">
    <div class="paths-hero">
      <div class="container">
        <span class="eyebrow">Ścieżki nauki</span>
        <h1 class="paths-title">Zaplanowane ścieżki rozwoju</h1>
        <p class="paths-subtitle">
          Kuratorowane zestawy kursów, które prowadzą od podstaw do zaawansowanych tematów. Wybierz ścieżkę dopasowaną do Twojego celu — frontend, backend albo pełen stos.
        </p>
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
        <PathCard v-for="path in items" :key="path.id" :path="path" />
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

.paths-page {
  padding-top: $header-height;
  min-height: 100vh;
}

.paths-hero {
  padding: 56px 0 36px;
  border-bottom: 1px solid $color-hairline;
  text-align: center;
}

.eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 6px 14px;
  font-size: 0.78rem;
  font-weight: 600;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: $color-ink;
  background: rgba(139, 92, 246, 0.14);
  border: 1px solid rgba(139, 92, 246, 0.3);
  border-radius: 999px;
  margin-bottom: 16px;
}

.paths-title {
  font-family: $font-display;
  font-size: $font-size-xl;
  font-weight: 600;
  letter-spacing: -0.02em;
  margin-bottom: 12px;
  background: $color-grad-text;
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}

.paths-subtitle {
  color: $color-muted;
  font-size: 1.02rem;
  max-width: 680px;
  margin: 0 auto;
  line-height: 1.55;
}

.paths-body {
  padding-top: 48px;
  padding-bottom: 80px;
}

.paths-loading,
.paths-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  padding: 80px 20px;
  text-align: center;
  color: $color-muted;
}

.paths-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 24px;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid rgba(255, 255, 255, 0.1);
  border-top-color: $color-gold;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
.eyebrow {
  --lg-blur: 12px;
  background: rgba(139, 92, 246, 0.14);
  -webkit-backdrop-filter: blur(12px) saturate(170%);
  backdrop-filter: blur(12px) saturate(170%);
  box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.28), inset 0 0 0 1px rgba(139, 92, 246, 0.32);
}

@media (max-width: 560px) {
  .paths-hero {
    padding: 40px 0 28px;
  }

  .paths-grid {
    grid-template-columns: 1fr;
  }
}

</style>