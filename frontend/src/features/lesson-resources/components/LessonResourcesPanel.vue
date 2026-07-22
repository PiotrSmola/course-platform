<template>
  <section v-if="resources.length" class="lesson-resources glass">
    <h2>Materiały do pobrania</h2>
    <ul class="resource-list">
      <li v-for="resource in resources" :key="resource.id" class="resource-item">
        <div class="resource-info">
          <span class="resource-title">{{ resource.title }}</span>
          <span class="resource-meta">{{ formatSize(resource.sizeBytes) }} · {{ formatType(resource.contentType) }}</span>
        </div>
        <button
          type="button"
          class="btn btn-ghost"
          :disabled="download.isPending.value"
          @click="onDownload(resource.id)"
        >
          Pobierz
        </button>
      </li>
    </ul>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import {
  useLessonResourceDownload,
  useLessonResources
} from '@/features/lesson-resources/composables/useLessonResources'

const props = defineProps<{
  courseId: string
  lessonId: string
}>()

const resourcesQuery = useLessonResources(() => props.courseId, () => props.lessonId)
const download = useLessonResourceDownload(() => props.courseId, () => props.lessonId)

const resources = computed(() => resourcesQuery.data.value ?? [])

async function onDownload(resourceId: string) {
  const result = await download.mutateAsync(resourceId)
  window.open(result.url, '_blank', 'noopener,noreferrer')
}

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

function formatType(contentType: string): string {
  const map: Record<string, string> = {
    'application/pdf': 'PDF',
    'application/zip': 'ZIP',
    'text/plain': 'TXT',
    'application/json': 'JSON'
  }
  return map[contentType] ?? contentType
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.lesson-resources {
  margin-top: 16px;
  padding: 20px;
  border-radius: 20px;

  h2 {
    margin: 0 0 12px;
    font-size: 1.1rem;
  }
}

.resource-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.resource-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 12px;
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.resource-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.resource-title {
  font-weight: 500;
  color: $color-ink;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.resource-meta {
  font-size: 0.85rem;
  color: $color-muted;
}
</style>
