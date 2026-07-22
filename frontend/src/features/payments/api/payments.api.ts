import client from '@/shared/api/client'
import type { CheckoutSessionDto, PaymentStatusDto, PurchaseDto } from '@/features/payments/types/payment.types'

export async function createCheckoutSession(
  courseId: string,
  couponCode?: string | null
): Promise<CheckoutSessionDto> {
  const response = await client.post('/payments/checkout', {
    courseId,
    couponCode: couponCode || undefined
  })
  return response.data
}

export async function previewCoupon(
  courseId: string,
  couponCode: string
): Promise<{ originalAmount: number; discountAmount: number; finalAmount: number; code: string }> {
  const response = await client.post('/coupons/preview', { courseId, couponCode })
  return response.data
}

export async function getPaymentStatus(sessionId: string): Promise<PaymentStatusDto> {
  const response = await client.get(`/payments/${encodeURIComponent(sessionId)}`)
  return response.data
}

export async function getMyPurchases(limit = 5): Promise<PurchaseDto[]> {
  const response = await client.get('/payments/my', { params: { limit } })
  return response.data
}
