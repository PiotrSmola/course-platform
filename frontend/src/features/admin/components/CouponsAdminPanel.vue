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
            <td class="code">{{ coupon.code }}</td>
            <td>{{ discountLabel(coupon.discountType) }}</td>
            <td class="num">{{ coupon.value }}</td>
            <td class="num">
              {{ coupon.redeemedCount }}
              <span v-if="coupon.maxRedemptions != null">/ {{ coupon.maxRedemptions }}</span>
            </td>
            <td><span class="chip" :class="{ 'is-active': coupon.isActive }">{{ coupon.isActive ? 'Aktywny' : 'Nieaktywny' }}</span></td>
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
                class="btn danger"
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
.coupons-panel {
  display: flex;
  flex-direction: column;
  gap: 28px;
  padding-top: 12px;
}

.create-form {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 16px;
  padding-bottom: 24px;
  border-bottom: 1px solid $color-hairline;

  h3 {
    margin: 0;
    font-size: 1.3rem;
    font-weight: 600;
  }

  .btn {
    padding: 10px 20px;
    font-size: 0.86rem;
  }
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 10px;
  width: 100%;

  input:not([type='checkbox']),
  select {
    height: 44px;
    padding: 0 14px;
    border: none;
    border-radius: $radius-md;
    background: rgba(13, 15, 19, 0.45);
    box-shadow: inset 0 0 0 1px $color-hairline;
    color: $color-ink;
    font: inherit;
    font-size: 0.9rem;
    transition: box-shadow 0.2s;

    &::placeholder {
      color: $color-faint;
    }

    &:focus {
      outline: none;
      box-shadow: inset 0 0 0 1px $color-hairline, 0 0 0 2px rgba($color-accent, 0.6);
    }
  }

  input[type='datetime-local'] {
    font-family: $font-mono;
    font-size: 0.8rem;
    color-scheme: dark;
  }
}

.checkbox {
  @include mono-label;
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 0.66rem;
  color: $color-muted;

  input {
    accent-color: $color-accent;
  }
}

.table-scroll {
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;

  th,
  td {
    text-align: left;
    padding: 13px 14px;
    border-bottom: 1px solid $color-hairline;
    vertical-align: middle;

    &:first-child {
      padding-left: 4px;
    }
  }

  th {
    @include mono-label;
    padding-top: 14px;
    padding-bottom: 12px;
    font-size: 0.62rem;
    color: $color-faint;
    white-space: nowrap;
  }

  tbody tr:last-child td {
    border-bottom: none;
  }

  tbody tr {
    transition: background 0.2s;

    &:hover {
      background: rgba($color-ink, 0.03);
    }
  }
}

.code {
  font-family: $font-mono;
  font-size: 0.82rem;
  letter-spacing: 0.04em;
  color: $color-ink;
}

.num {
  font-family: $font-mono;
  font-size: 0.82rem;
  font-variant-numeric: tabular-nums;
  color: $color-muted;
}

.chip {
  @include tag;

  &.is-active {
    color: $color-accent;
    box-shadow: inset 0 0 0 1px rgba($color-accent, 0.35);
  }
}

.actions-cell {
  display: flex;
  gap: 8px;

  .btn {
    padding: 7px 14px;
    font-size: 0.8rem;
    white-space: nowrap;
  }
}

.btn.danger {
  --lg-tint: #{rgba($color-danger, 0.22)};

  &:hover {
    --lg-tint: #{rgba($color-danger, 0.32)};
  }
}

.loading {
  @include mono-label;
  color: $color-muted;
}
</style>
