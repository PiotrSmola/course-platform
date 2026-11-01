<template>
  <div
    class="skeleton-block"
    :class="{ rounded: rounded }"
    :style="{ height, width }"
  />
</template>

<script setup lang="ts">
withDefaults(defineProps<{
  height?: string
  width?: string
  rounded?: boolean
}>(), {
  height: '1rem',
  width: '100%',
  rounded: false
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.skeleton-block {
  background: linear-gradient(
    90deg,
    rgba($color-ink, 0.04) 0%,
    rgba($color-ink, 0.08) 50%,
    rgba($color-ink, 0.04) 100%
  );
  background-size: 200% 100%;
  animation: shimmer 1.4s ease-in-out infinite;
  border-radius: $radius-sm;
  box-shadow: inset 0 0 0 1px rgba($color-ink, 0.05);

  &.rounded {
    border-radius: 999px;
  }
}

@keyframes shimmer {
  0% {
    background-position: 200% 0;
  }
  100% {
    background-position: -200% 0;
  }
}
</style>
