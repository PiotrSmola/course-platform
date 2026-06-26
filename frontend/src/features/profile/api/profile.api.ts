import client from '@/shared/api/client'
import type { UserProfileDto, UpdateProfileRequest, UserProfileSummaryDto } from '@/features/profile/types/profile.types'

export async function getUserProfile(): Promise<UserProfileDto> {
  const response = await client.get('/users/me')
  return response.data
}

export async function updateUserProfile(data: UpdateProfileRequest): Promise<UserProfileSummaryDto> {
  const response = await client.put('/users/me', data)
  return response.data
}

export async function deleteUserAccount(): Promise<void> {
  await client.delete('/users/me')
}
