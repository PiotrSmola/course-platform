import client from '@/shared/api/client'

export interface CreateReviewRequest {
  courseId: string
  rating: number
  comment: string
}

export async function createReview(data: CreateReviewRequest): Promise<string> {
  const response = await client.post(`/courses/${data.courseId}/reviews`, data)
  return response.data
}
