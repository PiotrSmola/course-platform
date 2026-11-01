<template>
  <section class="revenue-panel">
    <div v-if="isLoading" class="state">Ładowanie przychodów...</div>
    <div v-else-if="isError" class="state">Nie udało się załadować przychodów.</div>
    <div v-else-if="data" class="stats-grid">
      <div class="stat-card">
        <span class="stat-value">{{ formatCurrency(data.totalRevenue) }}</span>
        <span class="stat-label">Łączny przychód</span>
      </div>
      <div class="stat-card">
        <span class="stat-value">{{ formatCurrency(data.courseSalesRevenue) }}</span>
        <span class="stat-label">Sprzedaż kursów ({{ data.completedCoursePayments }})</span>
      </div>
      <div class="stat-card">
        <span class="stat-value">{{ formatCurrency(data.subscriptionRevenue) }}</span>
        <span class="stat-label">Subskrypcje ({{ data.paidSubscriptionInvoices }} faktur)</span>
      </div>
      <div class="stat-card">
        <span class="stat-value">{{ data.activeSubscriptions }}</span>
        <span class="stat-label">Aktywne All-access</span>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { useAdminRevenueOverview } from '@/features/analytics/composables/useAnalytics'

const { data, isLoading, isError } = useAdminRevenueOverview()

function formatCurrency(value: number) {
  return new Intl.NumberFormat('pl-PL', {
    style: 'currency',
    currency: 'PLN',
    maximumFractionDigits: 0
  }).format(value)
}
</script>

<style lang="scss" scoped>
.revenue-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 12px 0 4px;
}

.stats-grid {
  display: flex;
  flex-wrap: wrap;
  overflow: hidden;
  border-top: 1px solid $color-hairline;
  border-bottom: 1px solid $color-hairline;
}

.stat-card {
  flex: 1 1 200px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin: -1px 0 0 -1px;
  padding: 24px 22px 20px;
  border-top: 1px solid $color-hairline;
  border-left: 1px solid $color-hairline;
}

.stat-value {
  font-family: $font-display;
  font-size: clamp(1.9rem, 3vw, 2.5rem);
  font-weight: 600;
  line-height: 1;
  letter-spacing: -0.015em;
  font-variant-numeric: lining-nums;
  color: $color-ink;
  white-space: nowrap;
}

.stat-label {
  @include mono-label;
  font-size: 0.62rem;
  color: $color-faint;
}

.state {
  @include mono-label;
  color: $color-muted;
}
</style>
