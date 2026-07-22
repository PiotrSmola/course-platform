import client from '@/shared/api/client'
import type {
  AdminRevenueOverviewDto,
  InstructorAnalyticsDto
} from '@/features/analytics/types/analytics.types'

export async function getInstructorAnalytics(courseId?: string): Promise<InstructorAnalyticsDto> {
  const response = await client.get<InstructorAnalyticsDto>('/courses/instructor/analytics', {
    params: courseId ? { courseId } : undefined
  })
  return response.data
}

export async function getAdminRevenueOverview(): Promise<AdminRevenueOverviewDto> {
  const response = await client.get<AdminRevenueOverviewDto>('/admin/revenue')
  return response.data
}
