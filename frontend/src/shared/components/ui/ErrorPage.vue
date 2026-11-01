<template>
  <div class="error-page">
    <div class="error-page__inner">
      <span class="eyebrow">Kod błędu</span>
      <span class="error-page__code">{{ code }}</span>
      <div class="error-page__body">
        <h1>{{ title }}</h1>
        <div>
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
@use "@/assets/styles/abstracts/mixins" as *;

.error-page {
  display: flex;
  align-items: center;
  min-height: 70vh;
  padding: 40px 0 72px;
}

.error-page__inner {
  @include container;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.error-page__code {
  display: block;
  margin: 18px 0 0 -0.02em;
  font-family: $font-display;
  font-size: clamp(7rem, 24vw, 17rem);
  font-weight: 600;
  font-variant-numeric: lining-nums;
  line-height: 0.85;
  letter-spacing: -0.015em;
  color: $color-ink;
}

.error-page__body {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 440px);
  gap: 20px 56px;
  align-items: start;
  width: 100%;
  margin-top: 36px;
  padding-top: 28px;
  border-top: 1px solid $color-hairline;

  @media (max-width: 860px) {
    grid-template-columns: 1fr;
  }
}

h1 {
  @include highlight;
  font-size: clamp(2rem, 4vw, 3rem);
  font-weight: 600;
  line-height: 1.08;
  letter-spacing: -0.01em;
}

p {
  color: $color-muted;
  font-size: 0.98rem;
  line-height: 1.65;
}

.error-page__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-top: 22px;
}
</style>
