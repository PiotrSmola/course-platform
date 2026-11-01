<template>
  <div class="business-page">
    <div class="business-hero">
      <div class="container">
        <header class="section-head business-head">
          <span class="eyebrow">Dla firm</span>
          <h1 class="business-title">Szkolenia dla zespołów, które chcą <em>rosnąć</em></h1>
          <p class="business-subtitle">
            Wybierz plan dopasowany do skali Twojej organizacji. Daj zespołowi dostęp do pełnej biblioteki kursów, panel postępów i wsparcie, które rośnie razem z firmą.
          </p>
        </header>
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
@use "@/assets/styles/abstracts/mixins" as *;

.business-page {
  min-height: 100vh;
}

.business-hero {
  padding: 32px 0 0;
}

.business-head {
  margin-bottom: 48px;
}

.business-title {
  font-size: clamp(2.4rem, 4.6vw, 3.8rem);
  margin-top: 18px;
}

.business-subtitle {
  font-size: 0.98rem;
  line-height: 1.6;
}

.business-body {
  padding-bottom: 96px;
}

.plans-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 20px;
  align-items: stretch;

  @media (max-width: 960px) {
    grid-template-columns: 1fr;
    max-width: 560px;
  }
}

.business-loading,
.business-error {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 48px 0;
  color: $color-muted;
}

.business-loading p {
  @include mono-label;
  color: $color-faint;
}

.spinner {
  width: 22px;
  height: 22px;
  border: 2px solid rgba($color-ink, 0.1);
  border-top-color: $color-accent;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 560px) {
  .business-hero {
    padding-top: 40px;
  }

  .business-body {
    padding-bottom: 64px;
  }
}
</style>
