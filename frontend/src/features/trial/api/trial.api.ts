import client from '@/shared/api/client'
import type {
  ActivateTrialRequest,
  TrialAccessDto,
  TrialEligibleCourseDto
} from '@/features/trial/types/trial.types'

export async function getTrialEligibleCourses(): Promise<TrialEligibleCourseDto[]> {
  const response = await client.get('/trials/eligible-courses')
  return response.data
}

export async function activateTrial(data: ActivateTrialRequest): Promise<TrialAccessDto> {
  const response = await client.post('/trials', data)
  return response.data
}
