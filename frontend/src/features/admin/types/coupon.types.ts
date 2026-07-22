export type DiscountType = 'Percentage' | 'FixedAmount' | 0 | 1

export interface CouponDto {
  id: string
  code: string
  discountType: DiscountType
  value: number
  startsAt: string
  expiresAt: string | null
  maxRedemptions: number | null
  redeemedCount: number
  isActive: boolean
  courseIds: string[]
}

export interface CreateCouponRequest {
  code: string
  discountType: number
  value: number
  startsAt: string
  expiresAt: string | null
  maxRedemptions: number | null
  isActive: boolean
  courseIds: string[]
}

export interface UpdateCouponRequest {
  discountType: number
  value: number
  startsAt: string
  expiresAt: string | null
  maxRedemptions: number | null
  isActive: boolean
  courseIds: string[]
}
