import client from '@/shared/api/client'

export interface CreateGiftCheckoutRequest {
  courseId: string
  recipientEmail: string
}

export interface GiftCheckoutSessionDto {
  redirectUrl: string
}
export interface GiftRedemptionDto {
  courseId: string
  courseTitle: string
}

export async function redeemGiftCode(code: string): Promise<GiftRedemptionDto> {
  const response = await client.post('/gifts/redeem', { code })
  return response.data
}


export async function createGiftCheckoutSession(
  request: CreateGiftCheckoutRequest
): Promise<GiftCheckoutSessionDto> {
  const response = await client.post('/gifts/checkout', request)
  return response.data
}
