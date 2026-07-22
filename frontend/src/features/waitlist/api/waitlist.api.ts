import client from '@/shared/api/client'

export async function joinWaitlist(courseId: string): Promise<string> {
  const response = await client.post(`/waitlists/${courseId}`)
  return response.data
}

export async function leaveWaitlist(courseId: string): Promise<void> {
  await client.delete(`/waitlists/${courseId}`)
}
