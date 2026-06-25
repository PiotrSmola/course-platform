import client from '@/shared/api/client'

export interface BusinessPlanFeature {
  id: string
  text: string
  displayOrder: number
}

export interface BusinessPlan {
  id: string
  name: string
  slug: string
  shortDescription: string
  price: number | null
  currency: string | null
  billingPeriod: number | null
  priceLabel: string | null
  callToActionText: string
  callToActionUrl: string
  isFeatured: boolean
  displayOrder: number
  features: BusinessPlanFeature[]
}

export async function getBusinessPlans(): Promise<{ items: BusinessPlan[] }> {
  const response = await client.get('/business-plans')
  return response.data
}