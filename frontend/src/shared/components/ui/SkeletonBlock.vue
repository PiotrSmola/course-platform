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
.skeleton-block {
  background: linear-gradient(
    90deg,
    rgba(255, 255, 255, 0.04) 0%,
    rgba(255, 255, 255, 0.08) 50%,
    rgba(255, 255, 255, 0.04) 100%
  );
  background-size: 200% 100%;
  animation: shimmer 1.4s ease-in-out infinite;
  border-radius: 8px;

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
