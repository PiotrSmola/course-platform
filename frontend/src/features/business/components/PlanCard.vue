<template>
  <div class="plan-card glass-card" :class="{ featured: plan.isFeatured }">
    <span v-if="plan.isFeatured" class="featured-badge">Najpopularniejszy</span>
    <h3 class="plan-name">{{ plan.name }}</h3>
    <p class="plan-desc">{{ plan.shortDescription }}</p>

    <div class="plan-price">
      <span v-if="priceLabel" class="price-value">{{ priceLabel }}</span>
      <span v-else class="price-empty">—</span>
    </div>

    <ul class="plan-features">
      <li v-for="feature in plan.features" :key="feature.id">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
        <span>{{ feature.text }}</span>
      </li>
    </ul>

    <a class="btn plan-cta" :href="plan.callToActionUrl">{{ plan.callToActionText }}</a>
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
@use "sass:color";

.plan-card {
  --lg-r: 22px;
  --lg-blur: 0px;
  position: relative;
  padding: 28px 24px;
  display: flex;
  flex-direction: column;
  gap: 18px;
  transition: transform 0.3s, box-shadow 0.3s, border-color 0.3s;

  &.featured {
    border-color: rgba(245, 158, 11, 0.4);
    box-shadow:
      0 24px 60px rgba(3, 6, 24, 0.55),
      0 4px 14px rgba(3, 6, 24, 0.35),
      0 0 0 1px rgba(245, 158, 11, 0.4),
      inset 0 1px 1px rgba(255, 255, 255, 0.2);
    background:
      linear-gradient(180deg, rgba(245, 158, 11, 0.05), transparent 60%),
      var(--lg-tint);
    transform: translateY(-6px);
  }

  &:hover:not(.featured) {
    transform: translateY(-4px);
    border-color: rgba(255, 255, 255, 0.18);
    box-shadow:
      0 30px 70px rgba(3, 6, 24, 0.55),
      0 6px 18px rgba(3, 6, 24, 0.4),
      inset 0 1px 1px rgba(255, 255, 255, 0.2);
  }
}

.featured-badge {
  position: absolute;
  top: -12px;
  left: 50%;
  transform: translateX(-50%);
  padding: 5px 14px;
  font-size: 0.72rem;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: #1a1208;
  background: $color-grad;
  border-radius: 999px;
  box-shadow: 0 8px 22px rgba(245, 158, 11, 0.45);
}

.plan-name {
  font-family: $font-display;
  font-size: 1.4rem;
  font-weight: 600;
  letter-spacing: -0.01em;
}

.plan-desc {
  font-size: 0.92rem;
  color: $color-muted;
  line-height: 1.55;
  min-height: 60px;
}

.plan-price {
  display: flex;
  align-items: baseline;
  gap: 6px;
  padding: 12px 0;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);

  .price-value {
    font-family: $font-display;
    font-size: 1.6rem;
    font-weight: 700;
    color: $color-ink;
  }

  .price-empty {
    color: $color-faint;
    font-size: 1.4rem;
  }
}

.plan-features {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
  flex: 1;

  li {
    display: flex;
    align-items: flex-start;
    gap: 10px;
    font-size: 0.92rem;
    color: $color-ink;

    svg {
      color: $color-gold;
      flex-shrink: 0;
      margin-top: 4px;
    }
  }
}

.plan-cta {
  @include liquid-glass;
  --lg-r: 999px;
  --lg-blur: 0px;
  --lg-tint: rgba(255, 255, 255, 0.06);
  width: 100%;
  padding: 12px 18px;
  border: none;
  text-align: center;
  text-decoration: none;
  font-weight: 600;
  font-size: 0.95rem;
  color: $color-ink;
  cursor: pointer;
  box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.2), inset 0 0 0 1px rgba(255, 255, 255, 0.08);
  transition: --lg-tint 0.3s, transform 0.2s, box-shadow 0.3s;
  margin-top: auto;

  &:hover {
    --lg-tint: rgba(245, 158, 11, 0.18);
    color: $color-ink;
    transform: translateY(-2px);
    box-shadow:
      0 10px 24px rgba(245, 158, 11, 0.3),
      inset 0 1px 1px rgba(255, 255, 255, 0.3),
      inset 0 0 0 1px rgba(245, 158, 11, 0.4);
  }
}

.plan-card.featured .plan-cta {
  --lg-tint: rgba(245, 158, 11, 0.85);
  color: #1a1208;
  box-shadow:
    0 12px 28px rgba(245, 158, 11, 0.4),
    inset 0 1px 1px rgba(255, 255, 255, 0.4),
    inset 0 0 0 1px rgba(245, 158, 11, 0.6);

  &:hover {
    --lg-tint: rgba(245, 158, 11, 1);
    color: #1a1208;
    transform: translateY(-2px);
  }
}
</style>