import client from '@/shared/api/client'
import type { SupportMessageRequest } from '@/features/support/types/support.types'

export async function sendSupportMessage(data: SupportMessageRequest): Promise<void> {
  await client.post('/support/messages', data)
}
