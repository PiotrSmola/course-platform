<template>
  <section class="revenue-panel">
    <div v-if="isLoading" class="state">Ładowanie przychodów...</div>
    <div v-else-if="isError" class="state">Nie udało się załadować przychodów.</div>
    <div v-else-if="data" class="stats-grid">
      <div class="stat-card glass-card">
        <span class="stat-value">{{ formatCurrency(data.totalRevenue) }}</span>
        <span class="stat-label">Łączny przychód</span>
      </div>
      <div class="stat-card glass-card">
        <span class="stat-value">{{ formatCurrency(data.courseSalesRevenue) }}</span>
        <span class="stat-label">Sprzedaż kursów ({{ data.completedCoursePayments }})</span>
      </div>
      <div class="stat-card glass-card">
        <span class="stat-value">{{ formatCurrency(data.subscriptionRevenue) }}</span>
        <span class="stat-label">Subskrypcje ({{ data.paidSubscriptionInvoices }} faktur)</span>
      </div>
      <div class="stat-card glass-card">
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
@use "@/assets/styles/abstracts/variables" as *;

.revenue-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 12px;
}

.stat-card {
  padding: 18px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.stat-value {
  font-size: 1.35rem;
  font-weight: 700;
}

.stat-label {
  color: $color-muted;
  font-size: 0.85rem;
}

.state {
  color: $color-muted;
}
</style>
