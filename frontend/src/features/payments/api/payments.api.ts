import client from '@/shared/api/client'
import type { CheckoutSessionDto, PaymentStatusDto, PurchaseDto } from '@/features/payments/types/payment.types'

export async function createCheckoutSession(courseId: string): Promise<CheckoutSessionDto> {
  const response = await client.post('/payments/checkout', { courseId })
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
