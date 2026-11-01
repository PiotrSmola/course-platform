<template>
  <div class="empty-state">
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

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 12px;
  padding: 56px 24px;
  border: 1px dashed $color-hairline;
  border-radius: $radius-card;

  h3 {
    font-family: $font-display;
    font-size: 1.4rem;
    font-weight: 600;
    letter-spacing: -0.015em;
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
</style>
