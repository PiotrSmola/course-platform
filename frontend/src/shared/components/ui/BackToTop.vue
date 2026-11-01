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
  --lg-r: 999px;
  --lg-blur: 6px;
  --lg-tint: rgba(13, 15, 19, 0.4);
  position: fixed;
  right: 24px;
  bottom: 24px;
  z-index: 90;
  display: grid;
  width: 44px;
  height: 44px;
  place-items: center;
  border: 0;
  background: none;
  color: $color-ink;
  cursor: pointer;
  transition: transform 0.45s $ease-jelly;

  &:hover {
    --lg-glow: 0.2;
    transform: translateY(-3px);
  }

  &:active {
    transform: scale(0.95);
  }

  svg {
    width: 18px;
    height: 18px;
  }

  @media (max-width: 560px) {
    right: 16px;
    bottom: 16px;
  }
}
</style>
