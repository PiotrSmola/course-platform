import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import {
  getInstructorCourses,
  createModule,
  updateModule,
  deleteModule,
  createLesson,
  updateLesson,
  deleteLesson,
  type CreateModuleRequest,
  type UpdateModuleRequest,
  type CreateLessonRequest,
  type UpdateLessonRequest
} from '@/features/instructor/api/instructor.api'
import { queryKeys } from '@/shared/queryKeys'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'

export function useInstructorCourses() {
  return useQuery({
    queryKey: queryKeys.instructorCourses(),
    queryFn: getInstructorCourses
  })
}

export function useModuleMutations(courseId: string) {
  const queryClient = useQueryClient()

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: queryKeys.course(courseId) })
    queryClient.invalidateQueries({ queryKey: queryKeys.instructorCourses() })
  }

  const createModuleMutation = useMutation({
    mutationFn: (data: Omit<CreateModuleRequest, 'courseId'>) =>
      createModule({ ...data, courseId }),
    onSuccess: () => {
      invalidate()
      toast.success('Moduł został dodany')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się dodać modułu')
  })

  const updateModuleMutation = useMutation({
    mutationFn: (data: Omit<UpdateModuleRequest, 'courseId'>) =>
      updateModule({ ...data, courseId }),
    onSuccess: () => {
      invalidate()
      toast.success('Moduł został zaktualizowany')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się zaktualizować modułu')
  })

  const deleteModuleMutation = useMutation({
    mutationFn: (moduleId: string) => deleteModule(courseId, moduleId),
    onSuccess: () => {
      invalidate()
      toast.success('Moduł został usunięty')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się usunąć modułu')
  })

  const createLessonMutation = useMutation({
    mutationFn: (data: Omit<CreateLessonRequest, 'courseId'>) =>
      createLesson({ ...data, courseId }),
    onSuccess: () => {
      invalidate()
      toast.success('Lekcja została dodana')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się dodać lekcji')
  })

  const updateLessonMutation = useMutation({
    mutationFn: (data: Omit<UpdateLessonRequest, 'courseId'>) =>
      updateLesson({ ...data, courseId }),
    onSuccess: () => {
      invalidate()
      toast.success('Lekcja została zaktualizowana')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się zaktualizować lekcji')
  })

  const deleteLessonMutation = useMutation({
    mutationFn: ({ moduleId, lessonId }: { moduleId: string; lessonId: string }) =>
      deleteLesson(courseId, moduleId, lessonId),
    onSuccess: () => {
      invalidate()
      toast.success('Lekcja została usunięta')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się usunąć lekcji')
  })

  return {
    createModule: createModuleMutation,
    updateModule: updateModuleMutation,
    deleteModule: deleteModuleMutation,
    createLesson: createLessonMutation,
    updateLesson: updateLessonMutation,
    deleteLesson: deleteLessonMutation
  }
}
