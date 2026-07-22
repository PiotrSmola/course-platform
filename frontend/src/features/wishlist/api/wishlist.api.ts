import client from '@/shared/api/client'
import type { WishlistItemDto } from '@/features/wishlist/types/wishlist.types'

export async function getMyWishlist(): Promise<WishlistItemDto[]> {
  const response = await client.get('/wishlists/my')
  return response.data
}

export async function addToWishlist(courseId: string): Promise<string> {
  const response = await client.post(`/wishlists/${courseId}`)
  return response.data
}

export async function removeFromWishlist(courseId: string): Promise<void> {
  await client.delete(`/wishlists/${courseId}`)
}
