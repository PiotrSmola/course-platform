import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import {
  confirmLessonResourceUpload,
  deleteLessonResource,
  getLessonResourceDownloadUrl,
  getLessonResources,
  getLessonResourcesForInstructor,
  presignLessonResourceUpload
} from '@/features/lesson-resources/api/lessonResources.api'
import {
  ALLOWED_LESSON_RESOURCE_CONTENT_TYPES,
  MAX_LESSON_RESOURCE_BYTES
} from '@/features/lesson-resources/types/lessonResource.types'
import { queryKeys } from '@/shared/queryKeys'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'

export function useLessonResources(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>
) {
  return useQuery({
    queryKey: computed(() =>
      queryKeys.lessonResources(toValue(courseId), toValue(lessonId))
    ),
    queryFn: () => getLessonResources(toValue(courseId), toValue(lessonId)),
    enabled: computed(() => !!toValue(courseId) && !!toValue(lessonId))
  })
}

export function useLessonResourcesForInstructor(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>,
  enabled: MaybeRefOrGetter<boolean> = true
) {
  return useQuery({
    queryKey: computed(() =>
      queryKeys.lessonResourcesManage(toValue(courseId), toValue(lessonId))
    ),
    queryFn: () => getLessonResourcesForInstructor(toValue(courseId), toValue(lessonId)),
    enabled: computed(
      () => !!toValue(courseId) && !!toValue(lessonId) && toValue(enabled)
    )
  })
}

export function useLessonResourceUpload(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({
      file,
      title,
      onStatus
    }: {
      file: File
      title: string
      onStatus?: (status: string) => void
    }) => {
      if (!ALLOWED_LESSON_RESOURCE_CONTENT_TYPES.includes(file.type as typeof ALLOWED_LESSON_RESOURCE_CONTENT_TYPES[number])) {
        throw new Error('Nieobsługiwany typ pliku. Dozwolone: PDF, ZIP, TXT, JSON.')
      }

      if (file.size > MAX_LESSON_RESOURCE_BYTES) {
        throw new Error('Plik przekracza maksymalny rozmiar 25 MB.')
      }

      onStatus?.('Generowanie URL do uploadu...')
      const presign = await presignLessonResourceUpload(
        toValue(courseId),
        toValue(lessonId),
        file.type
      )

      onStatus?.('Upload materiału...')
      const res = await fetch(presign.url, {
        method: 'PUT',
        body: file,
        headers: { 'Content-Type': file.type }
      })
      if (!res.ok) throw new Error(`Resource upload failed: ${res.status}`)

      onStatus?.('Zapis materiału...')
      await confirmLessonResourceUpload(toValue(courseId), toValue(lessonId), {
        resourceId: presign.resourceId,
        objectKey: presign.objectKey,
        title: title.trim() || file.name,
        contentType: file.type
      })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.lessonResourcesManage(toValue(courseId), toValue(lessonId))
      })
      queryClient.invalidateQueries({
        queryKey: queryKeys.lessonResources(toValue(courseId), toValue(lessonId))
      })
      toast.success('Materiał został dodany')
    },
    onError: (error) =>
      toast.error(getApiErrorMessage(error) || 'Nie udało się wgrać materiału')
  })
}

export function useDeleteLessonResource(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (resourceId: string) =>
      deleteLessonResource(toValue(courseId), toValue(lessonId), resourceId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.lessonResourcesManage(toValue(courseId), toValue(lessonId))
      })
      queryClient.invalidateQueries({
        queryKey: queryKeys.lessonResources(toValue(courseId), toValue(lessonId))
      })
      toast.success('Materiał usunięty')
    },
    onError: (error) =>
      toast.error(getApiErrorMessage(error) || 'Nie udało się usunąć materiału')
  })
}

export function useLessonResourceDownload(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>
) {
  return useMutation({
    mutationFn: (resourceId: string) =>
      getLessonResourceDownloadUrl(toValue(courseId), toValue(lessonId), resourceId),
    onError: (error) =>
      toast.error(getApiErrorMessage(error) || 'Nie udało się pobrać materiału')
  })
}
