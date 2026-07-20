import client from '@/shared/api/client'

export interface CreateReviewRequest {
  courseId: string
  rating: number
  comment: string
}

export interface UpdateReviewRequest {
  courseId: string
  reviewId: string
  rating: number
  comment: string
}

export async function createReview(data: CreateReviewRequest): Promise<string> {
  const response = await client.post(`/courses/${data.courseId}/reviews`, data)
  return response.data
}

export async function updateReview(data: UpdateReviewRequest): Promise<void> {
  await client.put(`/courses/${data.courseId}/reviews/${data.reviewId}`, data)
}

export async function deleteReview(courseId: string, reviewId: string): Promise<void> {
  await client.delete(`/courses/${courseId}/reviews/${reviewId}`)
}
