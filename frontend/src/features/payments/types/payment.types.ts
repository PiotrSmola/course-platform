export enum PaymentStatus {
  Pending = 0,
  Completed = 1,
  Failed = 2,
  Expired = 3
}

export enum SubscriptionStatus {
  Active = 0,
  PastDue = 1,
  Canceled = 2,
  Incomplete = 3
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

export interface PurchaseHistoryItemDto {
  kind: 'course' | 'subscription' | 'gift'
  id: string
  courseId: string | null
  courseTitle: string | null
  recipientEmail: string | null
  amount: number
  currency: string
  completedAt: string
  giftCode: string | null
}

export interface SubscriptionCheckoutSessionDto {
  redirectUrl: string
}

export interface BillingPortalSessionDto {
  redirectUrl: string
}

export interface MySubscriptionDto {
  hasSubscription: boolean
  hasActiveAccess: boolean
  status: SubscriptionStatus | null
  currentPeriodEnd: string | null
  canManageInPortal: boolean
  latestInvoicePaidAt: string | null
  latestInvoiceAmount: number | null
  latestInvoiceCurrency: string | null
}

export interface SubscriptionOfferDto {
  monthlyPricePln: number
}
