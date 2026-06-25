import { computed } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { getBusinessPlans } from '@/features/business/api/business.api'
import type { BusinessPlan } from '@/features/business/api/business.api'

export function useBusinessPlans() {
  return useQuery({
    queryKey: ['business-plans'],
    queryFn: getBusinessPlans
  })
}

export function formatPlanPrice(plan: BusinessPlan): string {
  if (plan.priceLabel) return plan.priceLabel
  if (plan.price === null || plan.price === undefined) return ''
  const formatted = plan.price.toFixed(2).replace(/\.00$/, '')
  const period = plan.billingPeriod === 0 ? '/mies.' : plan.billingPeriod === 1 ? '/rok' : ''
  return `${formatted} ${plan.currency ?? ''}${period}`.trim()
}

export function useFeaturedPlan() {
  const { data } = useBusinessPlans()
  return computed(() => data.value?.items.find((p) => p.isFeatured) ?? null)
}