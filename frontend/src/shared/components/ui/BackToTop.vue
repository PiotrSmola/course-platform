<template>
  <button
    v-show="isVisible"
    class="back-to-top glass"
    type="button"
    aria-label="Przewiń na początek strony"
    @click="scrollToTop"
  >
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
      <path d="m18 15-6-6-6 6" />
    </svg>
  </button>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'

const isVisible = ref(false)

function updateVisibility() {
  isVisible.value = window.scrollY > 360
}

function scrollToTop() {
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

onMounted(() => {
  updateVisibility()
  window.addEventListener('scroll', updateVisibility, { passive: true })
})

onBeforeUnmount(() => {
  window.removeEventListener('scroll', updateVisibility)
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.back-to-top {
  position: fixed;
  right: 24px;
  bottom: 24px;
  z-index: 90;
  display: grid;
  width: 46px;
  height: 46px;
  place-items: center;
  border: 0;
  color: $color-ink;
  cursor: pointer;
  box-shadow: 0 12px 28px rgba(3, 6, 24, 0.34);
  transition: transform 160ms ease, opacity 160ms ease, box-shadow 160ms ease;

  &:hover {
    transform: translateY(-3px);
    box-shadow: 0 16px 32px rgba(3, 6, 24, 0.42);
  }

  svg {
    width: 20px;
    height: 20px;
  }

  @media (max-width: 560px) {
    right: 16px;
    bottom: 16px;
  }
}
</style>
