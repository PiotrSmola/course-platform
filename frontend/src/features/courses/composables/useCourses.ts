import { useQuery, useMutation } from '@tanstack/vue-query'
import { useRouter } from 'vue-router'
import { getCourses, getCourseDetails, createCourse, updateCourse } from '@/features/courses/api/courses.api'
import { toast } from 'vue3-toastify'

export function useCourses() {
  const { isLoading, isError, error, data } = useQuery({
    queryKey: ['courses'],
    queryFn: () => getCourses()
  })

  return { isLoading, isError, error, data }
}

export function useCourseDetails(courseId: string) {
  return useQuery({
    queryKey: ['course', courseId],
    queryFn: () => getCourseDetails(courseId),
    enabled: !!courseId
  })
}

export function useCreateCourse() {
  const router = useRouter()

  return useMutation({
    mutationFn: createCourse,
    onSuccess: () => {
      toast.success('Course created')
      router.push({ name: 'InstructorDashboard' })
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.error || 'Failed to create course')
    }
  })
}

export function useUpdateCourse() {
  return useMutation({
    mutationFn: updateCourse,
    onSuccess: () => {
      toast.success('Course updated')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.error || 'Failed to update course')
    }
  })
}
