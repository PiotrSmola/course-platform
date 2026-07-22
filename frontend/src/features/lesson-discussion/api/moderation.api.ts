import client from '@/shared/api/client'
import type {
  ModerationQueueDto,
  ModerationQueueFilters
} from '@/features/lesson-discussion/types/moderation.types'

export async function getModerationQueue(
  filters: ModerationQueueFilters = {}
): Promise<ModerationQueueDto> {
  const response = await client.get<ModerationQueueDto>('/moderation/qa', {
    params: {
      courseId: filters.courseId || undefined,
      unansweredByInstructorOnly: filters.unansweredByInstructorOnly ?? false,
      pageNumber: filters.pageNumber ?? 1,
      pageSize: filters.pageSize ?? 20
    }
  })
  return response.data
}
