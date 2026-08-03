<template>
  <div class="business-page">
    <div class="business-hero">
      <div class="container">
        <span class="eyebrow">Dla firm</span>
        <h1 class="business-title">Szkolenia dla zespołów, które chcą rosnąć</h1>
        <p class="business-subtitle">
          Wybierz plan dopasowany do skali Twojej organizacji. Daj zespołowi dostęp do pełnej biblioteki kursów, panel postępów i wsparcie, które rośnie razem z firmą.
        </p>
      </div>
    </div>

    <div class="container business-body">
      <div v-if="isLoading" class="business-loading">
        <div class="spinner" />
        <p>Ładowanie planów...</p>
      </div>

      <div v-else-if="isError" class="business-error">
        <p>Nie udało się pobrać planów. Spróbuj ponownie.</p>
      </div>

      <div v-else class="plans-grid">
        <PlanCard v-for="plan in items" :key="plan.id" :plan="plan" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useBusinessPlans } from '@/features/business/composables/useBusiness'
import PlanCard from '@/features/business/components/PlanCard.vue'

const { isLoading, isError, data } = useBusinessPlans()
const items = computed(() => data.value?.items ?? [])
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.business-page {
  padding-top: $header-height;
  min-height: 100vh;
}

.business-hero {
  padding: 64px 0 40px;
  border-bottom: 1px solid $color-hairline;
  text-align: center;
}

.eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 6px 14px;
  font-size: 0.78rem;
  font-weight: 600;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: $color-ink;
  background: rgba(34, 211, 238, 0.14);
  border: 1px solid rgba(34, 211, 238, 0.3);
  border-radius: 999px;
  margin-bottom: 16px;
}

.business-title {
  font-family: $font-display;
  font-size: $font-size-xl;
  font-weight: 600;
  letter-spacing: -0.02em;
  margin-bottom: 14px;
  background: $color-grad-text;
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}

.business-subtitle {
  color: $color-muted;
  font-size: 1.05rem;
  max-width: 720px;
  margin: 0 auto;
  line-height: 1.55;
}

.business-body {
  padding-top: 56px;
  padding-bottom: 96px;
}

.plans-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 24px;
  align-items: stretch;

  @media (max-width: 960px) {
    grid-template-columns: 1fr;
    max-width: 480px;
    margin: 0 auto;
  }
}

.business-loading,
.business-error {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  padding: 80px 20px;
  text-align: center;
  color: $color-muted;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid rgba(255, 255, 255, 0.1);
  border-top-color: $color-gold;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
.eyebrow {
  --lg-blur: 12px;
  background: rgba(34, 211, 238, 0.14);
  -webkit-backdrop-filter: blur(12px) saturate(170%);
  backdrop-filter: blur(12px) saturate(170%);
  box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.28), inset 0 0 0 1px rgba(34, 211, 238, 0.32);
}

@media (max-width: 560px) {
  .business-hero {
    padding: 42px 0 30px;
  }

  .business-body {
    padding-top: 40px;
    padding-bottom: 64px;
  }
}

</style>