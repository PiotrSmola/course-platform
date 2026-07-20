import { computed } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { getLearningPaths, getLearningPathBySlug } from '@/features/learning-paths/api/learningPaths.api'

import { queryKeys } from '@/shared/queryKeys'

export function useLearningPaths() {
  return useQuery({
    queryKey: queryKeys.learningPaths(),
    queryFn: getLearningPaths
  })
}

export function useLearningPathBySlug(slug: () => string) {
  return useQuery({
    queryKey: computed(() => queryKeys.learningPath(slug())),
    queryFn: () => getLearningPathBySlug(slug()),
    enabled: computed(() => !!slug())
  })
}