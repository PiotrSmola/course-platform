<template>
  <div class="error-state">
    <h3>{{ title }}</h3>
    <p v-if="description">{{ description }}</p>
    <RouterLink v-if="actionLabel && actionTo" class="btn btn-primary" :to="actionRoute">
      {{ actionLabel }}
    </RouterLink>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'

const props = defineProps<{
  title: string
  description?: string
  actionLabel?: string
  actionTo?: string
}>()

const actionRoute = computed(() => {
  if (!props.actionTo) return '/'
  if (props.actionTo.startsWith('/')) {
    return props.actionTo
  }
  return { name: props.actionTo }
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 12px;
  padding: 48px 24px;

  h3 {
    font-size: 1.15rem;
    font-weight: 600;
    color: #f87171;
  }

  p {
    color: $color-muted;
    font-size: 0.94rem;
    max-width: 420px;
    line-height: 1.6;
  }
}
</style>
