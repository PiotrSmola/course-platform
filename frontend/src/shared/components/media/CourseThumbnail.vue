<template>
  <div class="thumb" :style="style" />
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  url?: string | null
}>()

const style = computed(() => {
  return props.url ? { backgroundImage: `url(${props.url})` } : undefined
})
</script>

<style lang="scss" scoped>
.thumb {
  background: linear-gradient(160deg, rgba(139, 92, 246, 0.2), rgba(34, 211, 238, 0.1));
  background-size: cover;
  background-position: center;
}
</style>
