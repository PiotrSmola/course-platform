<template>
  <div class="plan-card" :class="[`is-${variant}`, surfaceClass, { featured: plan.isFeatured }]">
    <span v-if="plan.isFeatured" class="featured-badge">Najpopularniejszy</span>

    <div class="plan-top">
      <span class="plan-no">Plan {{ number }}</span>
      <h3 class="plan-name">{{ plan.name }}</h3>
    </div>
    <p class="plan-desc">{{ plan.shortDescription }}</p>

    <div class="plan-price">
      <span v-if="priceLabel" class="price-value">{{ priceLabel }}</span>
      <span v-else class="price-empty">—</span>
    </div>

    <ul class="plan-features">
      <li v-for="(feature, i) in plan.features" :key="feature.id">
        <span v-if="variant === 'paper'" class="feature-mark is-num">{{ String(i + 1).padStart(2, '0') }}</span>
        <span v-else-if="variant === 'outline'" class="feature-mark is-dash">—</span>
        <svg v-else class="feature-mark" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12" /></svg>
        <span>{{ feature.text }}</span>
      </li>
    </ul>

    <a class="btn plan-cta" :class="plan.isFeatured ? 'btn-primary' : 'btn-ghost'" :href="plan.callToActionUrl">{{ plan.callToActionText }}</a>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { BusinessPlan } from '@/features/business/api/business.api'
import { formatPlanPrice } from '@/features/business/composables/useBusiness'

const props = withDefaults(
  defineProps<{ plan: BusinessPlan; variant?: 'glass' | 'paper' | 'outline'; index?: number }>(),
  { variant: 'glass', index: undefined }
)

const priceLabel = computed(() => formatPlanPrice(props.plan))

const number = computed(() => (props.index === undefined ? '—' : String(props.index + 1).padStart(2, '0')))

const surfaceClass = computed(() => {
  switch (props.variant) {
    case 'paper': return 'paper-card'
    case 'outline': return 'outline-card'
    default: return 'glass-card is-static'
  }
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

$paper-ink: #15171b;
$paper-line: rgba(21, 23, 27, 0.16);

.plan-card {
  display: flex;
  flex-direction: column;
  min-width: 0;
  padding: 26px 26px 26px;
}

.is-glass {
  --lg-blur: 10px;
}

.is-paper {
  padding: 34px 30px 30px;
  color: $paper-ink;
  transform: rotate(-1.2deg) translateY(-14px);
  z-index: 1;

  @media (max-width: 960px) {
    transform: rotate(-0.6deg);
  }
}

.featured-badge {
  @include tag;
  position: absolute;
  top: -12px;
  right: 22px;
  color: $color-on-accent;
  background: $color-accent;
  box-shadow: 0 8px 18px -8px rgba(0, 0, 0, 0.45);
  transform: rotate(2deg);
}

.plan-top {
  padding-bottom: 16px;
  margin-bottom: 16px;
  border-bottom: 1px solid $color-hairline;

  .is-paper & {
    border-bottom-color: $paper-ink;
  }
}

.plan-no {
  @include mono-label;
  display: block;
  font-size: 0.62rem;
  color: $color-faint;
  margin-bottom: 10px;

  .is-paper & {
    color: rgba($paper-ink, 0.55);
  }
}

.plan-name {
  font-family: $font-display;
  font-size: 1.6rem;
  font-weight: 600;

  .is-paper & {
    font-size: 2rem;
    color: $paper-ink;
  }
}

.plan-desc {
  font-size: 0.92rem;
  color: $color-muted;
  line-height: 1.55;
  min-height: 60px;
  margin-bottom: 18px;

  .is-paper & {
    color: rgba($paper-ink, 0.72);
  }
}

.plan-price {
  display: flex;
  align-items: baseline;
  gap: 6px;
  padding: 16px 0;
  border-top: 1px dashed $color-hairline;
  border-bottom: 1px dashed $color-hairline;
  margin-bottom: 20px;

  .is-paper & {
    border-color: $paper-line;
  }

  .price-value {
    font-family: $font-display;
    font-size: 2.2rem;
    font-weight: 600;
    line-height: 1;
    color: $color-ink;

    .is-paper & {
      font-size: 2.8rem;
      color: $paper-ink;
    }

    .is-outline & {
      font-size: 1.9rem;
      font-weight: 500;
    }
  }

  .price-empty {
    font-family: $font-mono;
    color: $color-faint;
    font-size: 1.4rem;
  }

  .is-outline & {
    border-top-style: solid;
    border-bottom-style: solid;
  }
}

.plan-features {
  list-style: none;
  padding: 0;
  margin: 0 0 28px;
  display: flex;
  flex-direction: column;
  gap: 11px;
  flex: 1;

  li {
    display: flex;
    align-items: flex-start;
    gap: 10px;
    font-size: 0.92rem;
    color: $color-ink;
  }

  .is-paper & {
    gap: 0;

    li {
      padding: 8px 0;
      border-bottom: 1px dashed $paper-line;
      color: $paper-ink;
    }
  }

  .is-outline & li {
    color: $color-muted;
  }
}

.feature-mark {
  flex-shrink: 0;
  margin-top: 4px;
  color: $color-accent;

  &.is-num {
    @include mono-label;
    min-width: 22px;
    margin-top: 3px;
    color: rgba($paper-ink, 0.5);
  }

  &.is-dash {
    margin-top: 0;
    font-family: $font-mono;
  }
}

.plan-cta {
  width: 100%;
  margin-top: auto;
}
</style>
