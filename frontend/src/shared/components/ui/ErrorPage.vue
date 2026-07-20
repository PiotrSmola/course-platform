<template>
  <div class="error-page">
    <div class="error-page__inner glass-card">
      <span class="error-page__code">{{ code }}</span>
      <h1>{{ title }}</h1>
      <p>{{ description }}</p>
      <div class="error-page__actions">
        <RouterLink v-if="primaryLabel && primaryTo" class="btn btn-primary" :to="primaryRoute">
          {{ primaryLabel }}
        </RouterLink>
        <RouterLink v-if="secondaryLabel && secondaryTo" class="btn btn-ghost" :to="secondaryRoute">
          {{ secondaryLabel }}
        </RouterLink>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'

const props = defineProps<{
  code: string | number
  title: string
  description: string
  primaryLabel?: string
  primaryTo?: string
  secondaryLabel?: string
  secondaryTo?: string
}>()

function resolveRoute(target?: string) {
  if (!target) return '/'
  if (target.startsWith('/')) return target
  return { name: target }
}

const primaryRoute = computed(() => resolveRoute(props.primaryTo))
const secondaryRoute = computed(() => resolveRoute(props.secondaryTo))
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.error-page {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 70vh;
  padding: 40px 24px;
}

.error-page__inner {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 16px;
  padding: 56px 48px;
  max-width: 520px;
  width: 100%;
}

.error-page__code {
  font-size: clamp(4rem, 12vw, 6rem);
  font-weight: 800;
  line-height: 1;
  color: $color-faint;
  letter-spacing: -0.04em;
}

h1 {
  font-size: 1.5rem;
  font-weight: 700;
}

p {
  color: $color-muted;
  font-size: 0.98rem;
  line-height: 1.65;
  max-width: 380px;
}

.error-page__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  justify-content: center;
  margin-top: 8px;
}
</style>
