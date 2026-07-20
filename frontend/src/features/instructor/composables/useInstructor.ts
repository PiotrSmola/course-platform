import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import {
  getInstructorCourses,
  getInstructorDashboard,
  createModule,
  updateModule,
  deleteModule,
  createLesson,
  updateLesson,
  deleteLesson,
  initiateLessonVideoUpload,
  presignLessonVideoPart,
  completeLessonVideoUpload,
  abortLessonVideoUpload,
  updateCourseStatus,
  deleteCourse,
  type CreateModuleRequest,
  type UpdateModuleRequest,
  type CreateLessonRequest,
  type UpdateLessonRequest
} from '@/features/instructor/api/instructor.api'
import { presignCourseThumbnailUpload, confirmCourseThumbnailUpload } from '@/features/courses/api/courses.api'
import { queryKeys } from '@/shared/queryKeys'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'
import type { CourseStatus } from '@/features/courses/types/course.types'

export function useInstructorCourses() {
  return useQuery({
    queryKey: queryKeys.instructorCourses(),
    queryFn: getInstructorCourses
  })
}

export function useInstructorDashboard() {
  return useQuery({
    queryKey: queryKeys.instructorDashboard(),
    queryFn: getInstructorDashboard
  })
}

export function useUpdateCourseStatus() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ courseId, status }: { courseId: string; status: CourseStatus }) =>
      updateCourseStatus(courseId, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.instructorCourses() })
      queryClient.invalidateQueries({ queryKey: queryKeys.instructorDashboard() })
      queryClient.invalidateQueries({ queryKey: queryKeys.coursesBrowseAll() })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses() })
      toast.success('Status kursu został zaktualizowany')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się zaktualizować statusu')
  })
}

export function useDeleteCourse() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (courseId: string) => deleteCourse(courseId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.instructorCourses() })
      queryClient.invalidateQueries({ queryKey: queryKeys.instructorDashboard() })
      queryClient.invalidateQueries({ queryKey: queryKeys.coursesBrowseAll() })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses() })
      toast.success('Kurs został usunięty')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się usunąć kursu')
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

export function useCourseThumbnailUpload(courseId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ file, onStatus }: { file: File; onStatus?: (status: string) => void }) => {
      onStatus?.('Generowanie URL do uploadu...')
      const presign = await presignCourseThumbnailUpload(courseId, file.type)

      onStatus?.('Upload miniaturki...')
      const res = await fetch(presign.url, { method: 'PUT', body: file, headers: { 'Content-Type': file.type } })
      if (!res.ok) throw new Error(`Thumbnail upload failed: ${res.status}`)

      onStatus?.('Zapis miniaturki...')
      await confirmCourseThumbnailUpload(courseId, presign.objectKey)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.course(courseId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.instructorCourses() })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses() })
      queryClient.invalidateQueries({ queryKey: queryKeys.coursesBrowseAll() })
      toast.success('Miniaturka została zapisana')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się wgrać miniaturki')
  })
}

export function useLessonVideoUpload(courseId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ lessonId, file, onStatus }: { lessonId: string; file: File; onStatus?: (status: string) => void }) => {
      onStatus?.('Inicjalizacja uploadu...')
      const init = await initiateLessonVideoUpload(courseId, lessonId, file.type)

      try {
        const totalParts = Math.ceil(file.size / init.partSizeBytes)
        if (totalParts > init.maxParts) {
          throw new Error('Plik wideo jest za duży.')
        }

        const parts: { partNumber: number; eTag: string }[] = []
        for (let partNumber = 1; partNumber <= totalParts; partNumber++) {
          onStatus?.(`Upload części ${partNumber}/${totalParts}...`)
          const start = (partNumber - 1) * init.partSizeBytes
          const chunk = file.slice(start, Math.min(start + init.partSizeBytes, file.size))

          const presign = await presignLessonVideoPart(courseId, lessonId, init.uploadId, partNumber)
          const res = await fetch(presign.url, { method: 'PUT', body: chunk })
          if (!res.ok) throw new Error(`Upload part failed: ${res.status}`)

          const eTag = res.headers.get('etag')
          if (!eTag) throw new Error('Brak nagłówka ETag w odpowiedzi storage.')
          parts.push({ partNumber, eTag })
        }

        onStatus?.('Finalizacja uploadu...')
        await completeLessonVideoUpload(courseId, lessonId, init.uploadId, parts)
      } catch (error) {
        await abortLessonVideoUpload(courseId, lessonId, init.uploadId).catch(() => undefined)
        throw error
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.course(courseId) })
      toast.success('Wideo zostało wgrane')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się wgrać wideo')
  })
}
