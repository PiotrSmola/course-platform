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
              class="btn danger"
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
            <button type="button" class="btn btn-ghost file-upload-btn" @click="fileInputRef?.click()">
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
.resources-editor {
  grid-column: 1 / -1;
}

.editor-panel {
  margin-top: 12px;
  padding: 18px;
  border-radius: $radius-sm;
  box-shadow: inset 0 0 0 1px $color-hairline;
  background: rgba(13, 15, 19, 0.2);
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.resource-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
}

.resource-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 0;
  border-bottom: 1px solid $color-hairline;

  &:first-child {
    padding-top: 0;
  }
}

.resource-info {
  display: flex;
  flex-direction: column;
  gap: 3px;
  min-width: 0;
}

.resource-title {
  font-weight: 500;
  color: $color-ink;
}

.resource-meta {
  font-family: $font-mono;
  font-size: 0.7rem;
  color: $color-faint;
}

.upload-row {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  align-items: flex-end;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 180px;

  label {
    @include mono-label;
    font-size: 0.62rem;
    color: $color-muted;
  }

  input[type='text'] {
    height: 40px;
    padding: 0 14px;
    border: none;
    border-radius: $radius-md;
    background: rgba(13, 15, 19, 0.45);
    box-shadow: inset 0 0 0 1px $color-hairline;
    color: $color-ink;
    font: inherit;
    font-size: 0.92rem;
    transition: box-shadow 0.2s;

    &::placeholder {
      color: $color-faint;
    }

    &:focus {
      outline: none;
      box-shadow: inset 0 0 0 1px $color-hairline, 0 0 0 2px rgba($color-accent, 0.6);
    }
  }
}

.btn {
  padding: 8px 16px;
  font-size: 0.8rem;
}

.file-upload-btn {
  align-self: flex-start;
  height: 40px;
  padding: 0 16px;
}

.file-name,
.helper {
  font-family: $font-mono;
  font-size: 0.72rem;
  color: $color-muted;
}

.helper {
  align-self: center;
}

.empty,
.state {
  margin: 0;
  color: $color-muted;
  font-size: 0.88rem;
}

.btn.danger {
  --lg-tint: #{rgba($color-danger, 0.22)};

  &:hover {
    --lg-tint: #{rgba($color-danger, 0.32)};
  }
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
