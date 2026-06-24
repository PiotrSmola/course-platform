import client from '@/shared/api/client'
import type { EnrollmentDto } from '@/features/enrollment/types/enrollment.types'

export async function enroll(courseId: string): Promise<string> {
  const response = await client.post('/enrollments', { courseId })
  return response.data
}

export async function getMyEnrollments(): Promise<EnrollmentDto[]> {
  const response = await client.get('/enrollments/my')
  return response.data
}
