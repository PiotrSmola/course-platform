import client from '@/shared/api/client'
import type {
  NewsletterSubscribeRequest,
  NewsletterSubscriptionResponse,
  NewsletterTokenRequest
} from '@/features/newsletter/types/newsletter.types'

export async function subscribeNewsletter(
  data: NewsletterSubscribeRequest
): Promise<NewsletterSubscriptionResponse> {
  const response = await client.post('/newsletter/subscriptions', data)
  return response.data
}

export async function confirmNewsletterSubscription(data: NewsletterTokenRequest): Promise<NewsletterSubscriptionResponse> {
  const response = await client.post('/newsletter/subscriptions/confirm', data)
  return response.data
}

export async function unsubscribeNewsletter(data: NewsletterTokenRequest): Promise<NewsletterSubscriptionResponse> {
  const response = await client.post('/newsletter/subscriptions/unsubscribe', data)
  return response.data
}
