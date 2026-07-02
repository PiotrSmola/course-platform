<template>
  <div class="thumb" :style="style" />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useCourseThumbnailUrl } from '@/features/courses/composables/useCourseAssets'

const props = defineProps<{
  courseId: string
}>()

const query = useCourseThumbnailUrl(computed(() => props.courseId))
const style = computed(() => {
  const url = query.data.value
  return url ? { backgroundImage: `url(${url})` } : undefined
})
</script>

<style lang="scss" scoped>
.thumb {
  background: linear-gradient(160deg, rgba(139, 92, 246, 0.2), rgba(34, 211, 238, 0.1));
  background-size: cover;
  background-position: center;
}
</style>

