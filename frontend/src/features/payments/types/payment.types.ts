export enum PaymentStatus {
  Pending = 0,
  Completed = 1,
  Failed = 2,
  Expired = 3
}

export interface CheckoutSessionDto {
  redirectUrl: string | null
  enrolled: boolean
}

export interface PaymentStatusDto {
  status: PaymentStatus
  courseId: string
  courseTitle: string
}

export interface PurchaseDto {
  courseId: string
  courseTitle: string
  amount: number
  currency: string
  completedAt: string
}
