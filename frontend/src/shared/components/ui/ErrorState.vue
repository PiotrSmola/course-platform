<template>
  <div class="error-state">
    <span class="error-state__label">Błąd</span>
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
@use "@/assets/styles/abstracts/mixins" as *;

.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 12px;
  padding: 48px 24px;

  h3 {
    font-family: $font-display;
    font-size: 1.4rem;
    font-weight: 600;
    letter-spacing: -0.015em;
    color: $color-ink;
  }

  p {
    color: $color-muted;
    font-size: 0.94rem;
    max-width: 420px;
    line-height: 1.6;
  }

  .btn {
    margin-top: 8px;
  }
}

.error-state__label {
  @include tag;
  color: $color-danger;
  box-shadow: inset 0 0 0 1px rgba($color-danger, 0.35);
}
</style>
