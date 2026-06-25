import { computed } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { getLearningPaths, getLearningPathBySlug } from '@/features/learning-paths/api/learningPaths.api'

export function useLearningPaths() {
  return useQuery({
    queryKey: ['learning-paths'],
    queryFn: getLearningPaths
  })
}

export function useLearningPathBySlug(slug: () => string) {
  return useQuery({
    queryKey: computed(() => ['learning-paths', slug()]),
    queryFn: () => getLearningPathBySlug(slug()),
    enabled: computed(() => !!slug())
  })
}