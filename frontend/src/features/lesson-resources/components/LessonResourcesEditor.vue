<template>
  <div class="resources-editor">
    <button type="button" class="btn btn-ghost" @click="open = !open">
      {{ open ? 'Ukryj materiały' : 'Materiały do pobrania' }}
    </button>

    <div v-if="open" class="editor-panel">
      <div v-if="resourcesQuery.isLoading.value" class="state">Ładowanie materiałów...</div>
      <template v-else>
        <ul v-if="resources.length" class="resource-list">
          <li v-for="resource in resources" :key="resource.id" class="resource-row">
            <div class="resource-info">
              <span class="resource-title">{{ resource.title }}</span>
              <span class="resource-meta">{{ formatSize(resource.sizeBytes) }}</span>
            </div>
            <button
              type="button"
              class="btn btn-ghost danger"
              :disabled="remove.isPending.value"
              @click="remove.mutate(resource.id)"
            >
              Usuń
            </button>
          </li>
        </ul>
        <p v-else class="empty">Brak materiałów dla tej lekcji.</p>

        <div class="upload-row">
          <div class="form-group">
            <label :for="`resource-title-${lessonId}`">Tytuł</label>
            <input
              :id="`resource-title-${lessonId}`"
              v-model="title"
              type="text"
              placeholder="Np. Notatki z lekcji"
            />
          </div>
          <div class="form-group">
            <label :for="`resource-file-${lessonId}`">Plik</label>
            <input
              :id="`resource-file-${lessonId}`"
              ref="fileInputRef"
              type="file"
              :accept="LESSON_RESOURCE_ACCEPT"
              class="sr-only"
              @change="onFileSelected"
            />
            <button type="button" class="file-upload-btn" @click="fileInputRef?.click()">
              Wybierz plik
            </button>
            <span v-if="selectedFile" class="file-name">{{ selectedFile.name }}</span>
          </div>
          <button
            type="button"
            class="btn btn-primary"
            :disabled="!selectedFile || upload.isPending.value"
            @click="uploadFile"
          >
            Dodaj materiał
          </button>
          <span v-if="uploadStatus" class="helper">{{ uploadStatus }}</span>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import {
  useDeleteLessonResource,
  useLessonResourceUpload,
  useLessonResourcesForInstructor
} from '@/features/lesson-resources/composables/useLessonResources'
import { LESSON_RESOURCE_ACCEPT } from '@/features/lesson-resources/types/lessonResource.types'

const props = defineProps<{
  courseId: string
  lessonId: string
}>()

const open = ref(false)
const title = ref('')
const selectedFile = ref<File | null>(null)
const uploadStatus = ref('')
const fileInputRef = ref<HTMLInputElement | null>(null)

const resourcesQuery = useLessonResourcesForInstructor(
  () => props.courseId,
  () => props.lessonId,
  open
)
const upload = useLessonResourceUpload(() => props.courseId, () => props.lessonId)
const remove = useDeleteLessonResource(() => props.courseId, () => props.lessonId)

const resources = computed(() => resourcesQuery.data.value ?? [])

function onFileSelected(event: Event) {
  const input = event.target as HTMLInputElement
  selectedFile.value = input.files?.[0] ?? null
  if (selectedFile.value && !title.value) {
    title.value = selectedFile.value.name.replace(/\.[^.]+$/, '')
  }
}

async function uploadFile() {
  if (!selectedFile.value) return

  uploadStatus.value = 'Przygotowanie...'
  await upload.mutateAsync({
    file: selectedFile.value,
    title: title.value,
    onStatus: (status) => {
      uploadStatus.value = status
    }
  })

  title.value = ''
  selectedFile.value = null
  uploadStatus.value = ''
  if (fileInputRef.value) {
    fileInputRef.value.value = ''
  }
}

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.resources-editor {
  margin-top: 8px;
  grid-column: 1 / -1;
}

.editor-panel {
  margin-top: 12px;
  padding: 16px;
  border-radius: 16px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.resource-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.resource-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 8px 10px;
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.06);
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
}

.resource-meta {
  font-size: 0.85rem;
  color: $color-muted;
}

.upload-row {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  align-items: flex-end;
  padding-top: 8px;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 180px;

  input[type='text'] {
    border-radius: 12px;
    border: 1px solid rgba(255, 255, 255, 0.1);
    background: rgba(255, 255, 255, 0.04);
    color: $color-ink;
    padding: 10px 12px;
  }
}

.file-upload-btn {
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  background: rgba(255, 255, 255, 0.04);
  color: $color-ink;
  padding: 10px 12px;
  cursor: pointer;
}

.file-name {
  font-size: 0.85rem;
  color: $color-muted;
}

.helper {
  font-size: 0.85rem;
  color: $color-muted;
  align-self: center;
}

.empty,
.state {
  color: $color-muted;
  margin: 0;
}

.danger {
  color: #f87171;
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}
</style>
