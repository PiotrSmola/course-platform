import { useQuery } from '@tanstack/vue-query'
import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import { getCourseThumbnailUrl } from '@/features/courses/api/courses.api'

export function useCourseThumbnailUrl(courseId: MaybeRefOrGetter<string>) {
  return useQuery({
    queryKey: computed(() => ['course', toValue(courseId), 'thumbnailUrl']),
    queryFn: () => getCourseThumbnailUrl(toValue(courseId)).then((r) => r.url),
    enabled: computed(() => !!toValue(courseId))
  })
}

