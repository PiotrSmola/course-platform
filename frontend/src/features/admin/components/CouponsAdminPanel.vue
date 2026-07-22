<template>
  <div class="coupons-panel">
    <form class="create-form" @submit.prevent="onCreate">
      <h3>Nowy kupon</h3>
      <div class="form-grid">
        <input v-model="form.code" type="text" placeholder="Kod (np. PROMO20)" required />
        <select v-model.number="form.discountType">
          <option :value="0">Procent</option>
          <option :value="1">Kwota stała</option>
        </select>
        <input v-model.number="form.value" type="number" min="0.01" step="0.01" placeholder="Wartość" required />
        <input v-model="form.startsAt" type="datetime-local" required />
        <input v-model="form.expiresAt" type="datetime-local" />
        <input v-model.number="form.maxRedemptions" type="number" min="1" placeholder="Limit użyć (opcjonalnie)" />
        <label class="checkbox">
          <input v-model="form.isActive" type="checkbox" />
          Aktywny
        </label>
      </div>
      <button class="btn btn-primary" type="submit" :disabled="createCoupon.isPending.value">
        Utwórz kupon
      </button>
    </form>

    <div v-if="isLoading" class="loading">Ładowanie...</div>
    <div v-else class="table-scroll">
      <table class="data-table">
        <thead>
          <tr>
            <th>Kod</th>
            <th>Typ</th>
            <th>Wartość</th>
            <th>Użycia</th>
            <th>Status</th>
            <th>Akcje</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="coupon in coupons" :key="coupon.id">
            <td>{{ coupon.code }}</td>
            <td>{{ discountLabel(coupon.discountType) }}</td>
            <td>{{ coupon.value }}</td>
            <td>
              {{ coupon.redeemedCount }}
              <span v-if="coupon.maxRedemptions != null">/ {{ coupon.maxRedemptions }}</span>
            </td>
            <td>{{ coupon.isActive ? 'Aktywny' : 'Nieaktywny' }}</td>
            <td class="actions-cell">
              <button
                type="button"
                class="btn btn-ghost"
                @click="toggleActive(coupon)"
              >
                {{ coupon.isActive ? 'Dezaktywuj' : 'Aktywuj' }}
              </button>
              <button
                type="button"
                class="btn btn-ghost danger"
                @click="remove(coupon.id)"
              >
                Usuń
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive } from 'vue'
import { useAdminCoupons, useAdminMutations } from '@/features/admin/composables/useAdmin'
import type { CouponDto } from '@/features/admin/types/coupon.types'

const { data, isLoading } = useAdminCoupons()
const { createCoupon, updateCoupon, removeCoupon } = useAdminMutations()
const coupons = computed(() => data.value ?? [])

const form = reactive({
  code: '',
  discountType: 0,
  value: 20,
  startsAt: toLocalInput(new Date()),
  expiresAt: '',
  maxRedemptions: null as number | null,
  isActive: true
})

function toLocalInput(date: Date) {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`
}

function toIso(local: string) {
  return new Date(local).toISOString()
}

function discountLabel(type: CouponDto['discountType']) {
  return type === 1 || type === 'FixedAmount' ? 'Kwota' : 'Procent'
}

async function onCreate() {
  await createCoupon.mutateAsync({
    code: form.code,
    discountType: form.discountType,
    value: form.value,
    startsAt: toIso(form.startsAt),
    expiresAt: form.expiresAt ? toIso(form.expiresAt) : null,
    maxRedemptions: form.maxRedemptions,
    isActive: form.isActive,
    courseIds: []
  })
  form.code = ''
  form.value = 20
  form.expiresAt = ''
  form.maxRedemptions = null
}

function toggleActive(coupon: CouponDto) {
  updateCoupon.mutate({
    couponId: coupon.id,
    data: {
      discountType: typeof coupon.discountType === 'number'
        ? coupon.discountType
        : coupon.discountType === 'FixedAmount'
          ? 1
          : 0,
      value: coupon.value,
      startsAt: coupon.startsAt,
      expiresAt: coupon.expiresAt,
      maxRedemptions: coupon.maxRedemptions,
      isActive: !coupon.isActive,
      courseIds: coupon.courseIds
    }
  })
}

function remove(couponId: string) {
  if (!confirm('Usunąć kupon?')) return
  removeCoupon.mutate(couponId)
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.coupons-panel {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.create-form {
  display: flex;
  flex-direction: column;
  gap: 12px;

  h3 {
    margin: 0;
  }
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 10px;

  input,
  select {
    border-radius: 12px;
    border: 1px solid rgba(255, 255, 255, 0.12);
    background: rgba(255, 255, 255, 0.04);
    color: inherit;
    padding: 10px 12px;
  }
}

.checkbox {
  display: flex;
  align-items: center;
  gap: 8px;
}

.table-scroll {
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;

  th,
  td {
    text-align: left;
    padding: 12px 10px;
    border-bottom: 1px solid rgba(255, 255, 255, 0.08);
  }
}

.actions-cell {
  display: flex;
  gap: 8px;
}

.danger {
  color: #f87171;
}

.loading {
  color: $color-muted;
}
</style>
