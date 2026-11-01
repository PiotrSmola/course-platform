<template>
  <div class="plan-card glass-card" :class="{ featured: plan.isFeatured }">
    <div class="plan-top">
      <h3 class="plan-name">{{ plan.name }}</h3>
      <span v-if="plan.isFeatured" class="featured-badge">Najpopularniejszy</span>
    </div>
    <p class="plan-desc">{{ plan.shortDescription }}</p>

    <div class="plan-price">
      <span v-if="priceLabel" class="price-value">{{ priceLabel }}</span>
      <span v-else class="price-empty">—</span>
    </div>

    <ul class="plan-features">
      <li v-for="feature in plan.features" :key="feature.id">
        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
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

const props = defineProps<{ plan: BusinessPlan }>()

const priceLabel = computed(() => formatPlanPrice(props.plan))
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.plan-card {
  --lg-blur: 10px;
  display: flex;
  flex-direction: column;
  padding: 24px 26px 26px;

  &.featured {
    --lg-tint: rgba(13, 15, 19, 0.4);
    box-shadow:
      0 0 0 1px rgba($color-accent, 0.45),
      0 28px 60px -24px rgba($color-accent, 0.28),
      0 12px 32px -8px rgba(0, 0, 0, 0.45);
  }
}

.plan-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  padding-bottom: 14px;
  margin-bottom: 14px;
  border-bottom: 1px solid $color-hairline;
}

.featured-badge {
  @include tag;
  flex-shrink: 0;
  color: $color-on-accent;
  background: $color-accent;
  box-shadow: none;
}

.plan-name {
  font-family: $font-display;
  font-size: 1.5rem;
  font-weight: 600;
}

.plan-desc {
  font-size: 0.92rem;
  color: $color-muted;
  line-height: 1.55;
  min-height: 60px;
  margin-bottom: 18px;
}

.plan-price {
  display: flex;
  align-items: baseline;
  gap: 6px;
  padding: 16px 0;
  border-top: 1px dashed $color-hairline;
  border-bottom: 1px dashed $color-hairline;
  margin-bottom: 20px;

  .price-value {
    font-family: $font-display;
    font-size: 2.4rem;
    font-weight: 600;
    line-height: 1;
    color: $color-ink;
  }

  .price-empty {
    font-family: $font-mono;
    color: $color-faint;
    font-size: 1.4rem;
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

    svg {
      color: $color-accent;
      flex-shrink: 0;
      margin-top: 4px;
    }
  }
}

.plan-cta {
  width: 100%;
  margin-top: auto;
}
</style>
