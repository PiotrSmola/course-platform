<template>
  <section class="structure-editor">
    <div class="structure-head">
      <h3>Struktura kursu</h3>
      <button type="button" class="btn btn-ghost" @click="addModule">+ Moduł</button>
    </div>

    <p v-if="!modules.length" class="empty">Brak modułów. Dodaj pierwszy moduł, a następnie lekcje z adresem URL wideo (placeholder).</p>

    <div v-for="(module, mIndex) in modules" :key="module.id" class="module-block glass-card">
      <div class="module-row">
        <div class="form-group">
          <label :for="`module-title-${module.id}`">Tytuł modułu</label>
          <input
            :id="`module-title-${module.id}`"
            v-model="module.title"
            placeholder="Np. Wprowadzenie"
            @blur="saveModule(module, mIndex)"
          />
        </div>
        <div class="form-group form-group--narrow">
          <label :for="`module-order-${module.id}`">Kolejność</label>
          <input
            :id="`module-order-${module.id}`"
            v-model.number="module.order"
            type="number"
            min="0"
            @blur="saveModule(module, mIndex)"
          />
        </div>
        <div class="form-group form-group--action">
          <span class="label-spacer" aria-hidden="true">&nbsp;</span>
          <button type="button" class="btn btn-ghost danger" @click="removeModule(module.id)">Usuń moduł</button>
        </div>
      </div>

      <div class="lessons">
        <p class="lessons-label">Lekcje</p>
        <div v-for="(lesson, lIndex) in module.lessons" :key="lesson.id" class="lesson-row">
          <div class="form-group">
            <label :for="`lesson-title-${lesson.id}`">Tytuł lekcji</label>
            <input
              :id="`lesson-title-${lesson.id}`"
              v-model="lesson.title"
              placeholder="Np. Pierwsze kroki"
              @blur="saveLesson(module.id, lesson, lIndex)"
            />
          </div>
          <div class="form-group">
            <label :for="`lesson-video-file-${lesson.id}`">Wideo</label>
            <input
              :id="`lesson-video-file-${lesson.id}`"
              :ref="(el) => setVideoInputRef(el as HTMLInputElement, lesson.id)"
              type="file"
              accept="video/*"
              class="sr-only"
              @change="onVideoInputChange(module.id, lesson.id, $event)"
            />
            <button
              type="button"
              class="file-upload-btn"
              @click="videoInputRefs[lesson.id]?.click()"
            >
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
                <polyline points="17 8 12 3 7 8" />
                <line x1="12" y1="3" x2="12" y2="15" />
              </svg>
              <span>Wybierz wideo</span>
            </button>
            <div class="video-meta">
              <span v-if="selectedVideoFiles[lesson.id]" class="file-name">{{ selectedVideoFiles[lesson.id].name }}</span>
              <span v-else-if="lesson.videoObjectKey" class="file-hint">Wideo wgrane</span>
              <span v-if="uploadState[lesson.id]?.status" class="helper">
                {{ uploadState[lesson.id]?.status }}
              </span>
            </div>
          </div>
          <div class="form-group form-group--narrow">
            <label :for="`lesson-duration-${lesson.id}`">Czas (min)</label>
            <input
              :id="`lesson-duration-${lesson.id}`"
              v-model.number="lesson.duration"
              type="number"
              min="1"
              @blur="saveLesson(module.id, lesson, lIndex)"
            />
          </div>
          <div class="form-group form-group--narrow">
            <label :for="`lesson-order-${lesson.id}`">Kolejność</label>
            <input
              :id="`lesson-order-${lesson.id}`"
              v-model.number="lesson.order"
              type="number"
              min="0"
              @blur="saveLesson(module.id, lesson, lIndex)"
            />
          </div>
          <div class="form-group form-group--action">
            <span class="label-spacer" aria-hidden="true">&nbsp;</span>
            <button type="button" class="btn btn-ghost danger icon-btn" title="Usuń lekcję" @click="removeLesson(module.id, lesson.id)">×</button>
          </div>
        </div>
        <button type="button" class="btn btn-ghost add-lesson" @click="addLesson(module.id, module.lessons.length)">+ Lekcja</button>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { ModuleDto } from '@/features/courses/types/course.types'
import { useModuleMutations } from '@/features/instructor/composables/useInstructor'
import { initiateLessonVideoUpload, presignLessonVideoPart, completeLessonVideoUpload } from '@/features/instructor/api/instructor.api'

const props = defineProps<{
  courseId: string
  courseModules: ModuleDto[]
}>()

const {
  createModule,
  updateModule,
  deleteModule,
  createLesson,
  updateLesson,
  deleteLesson
} = useModuleMutations(props.courseId)

const localModules = ref<ModuleDto[]>([])

watch(
  () => props.courseModules,
  (value) => {
    localModules.value = value.map((m) => ({
      ...m,
      lessons: m.lessons.map((l) => ({
        ...l,
        videoObjectKey: l.videoObjectKey ?? ''
      }))
    }))
  },
  { immediate: true, deep: true }
)

const modules = computed({
  get: () => localModules.value,
  set: (v) => { localModules.value = v }
})

function addModule() {
  createModule.mutate({
    title: `Moduł ${modules.value.length + 1}`,
    order: modules.value.length
  })
}

function saveModule(module: ModuleDto, index: number) {
  updateModule.mutate({
    moduleId: module.id,
    title: module.title,
    order: module.order ?? index
  })
}

function removeModule(moduleId: string) {
  if (!confirm('Usunąć moduł wraz z lekcjami?')) return
  deleteModule.mutate(moduleId)
}

function addLesson(moduleId: string, lessonCount: number) {
  createLesson.mutate({
    moduleId,
    title: `Lekcja ${lessonCount + 1}`,
    description: '',
    videoObjectKey: '',
    duration: 10,
    order: lessonCount
  })
}

function saveLesson(moduleId: string, lesson: ModuleDto['lessons'][number], index: number) {
  updateLesson.mutate({
    moduleId,
    lessonId: lesson.id,
    title: lesson.title,
    description: lesson.description ?? '',
    videoObjectKey: lesson.videoObjectKey || '',
    duration: lesson.duration || 1,
    order: lesson.order ?? index
  })
}

function removeLesson(moduleId: string, lessonId: string) {
  if (!confirm('Usunąć lekcję?')) return
  deleteLesson.mutate({ moduleId, lessonId })
}

const uploadState = ref<Record<string, { status?: string }>>({})
const videoInputRefs = ref<Record<string, HTMLInputElement | null>>({})
const selectedVideoFiles = ref<Record<string, File>>({})

function setVideoInputRef(el: HTMLInputElement | null, lessonId: string) {
  videoInputRefs.value[lessonId] = el
}

function onVideoInputChange(moduleId: string, lessonId: string, event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  selectedVideoFiles.value[lessonId] = file
  void onSelectVideoFile(moduleId, lessonId, file)
}

async function onSelectVideoFile(moduleId: string, lessonId: string, file: File) {
  uploadState.value[lessonId] = { status: 'Inicjalizacja uploadu...' }

  const init = await initiateLessonVideoUpload(props.courseId, lessonId, file.type || 'application/octet-stream')
  const partSize = init.partSizeBytes
  const parts: { partNumber: number; eTag: string }[] = []

  const totalParts = Math.ceil(file.size / partSize)
  for (let partNumber = 1; partNumber <= totalParts; partNumber++) {
    uploadState.value[lessonId] = { status: `Upload part ${partNumber}/${totalParts}...` }
    const start = (partNumber - 1) * partSize
    const end = Math.min(start + partSize, file.size)
    const chunk = file.slice(start, end)

    const presign = await presignLessonVideoPart(props.courseId, lessonId, init.uploadId, partNumber)
    const res = await fetch(presign.url, { method: 'PUT', body: chunk })
    if (!res.ok) throw new Error(`Upload part failed: ${res.status}`)
    const eTag = res.headers.get('etag') ?? ''
    parts.push({ partNumber, eTag })
  }

  uploadState.value[lessonId] = { status: 'Finalizacja uploadu...' }
  await completeLessonVideoUpload(props.courseId, lessonId, init.uploadId, parts)
  uploadState.value[lessonId] = { status: 'Gotowe' }
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.structure-editor {
  margin-top: 48px;
  margin-inline: auto;
  max-width: 960px;
}

.structure-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;

  h3 {
    font-size: 1.2rem;
  }
}

.empty {
  color: $color-muted;
  margin-bottom: 20px;
}

.module-block {
  padding: 24px;
  margin-bottom: 16px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 0;

  label {
    font-size: 0.88rem;
    font-weight: 600;
    color: $color-muted;
  }

  input {
    height: 48px;
    padding: 0 18px;
    border-radius: 16px;
    border: none;
    background: rgba(255, 255, 255, 0.04);
    box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.3), inset 0 0 0 1px rgba(255, 255, 255, 0.1);
    color: $color-ink;
    font: inherit;
    font-size: 0.94rem;
    outline: none;
    transition: background 0.3s, box-shadow 0.3s;
    width: 100%;

    &::placeholder {
      color: $color-faint;
    }

    &:focus {
      background: rgba(255, 255, 255, 0.07);
      box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.35), inset 0 0 0 1px rgba(167, 139, 250, 0.5), 0 0 0 4px rgba(139, 92, 246, 0.18);
    }
  }

  &--narrow {
    max-width: 110px;
  }

  &--action {
    flex-shrink: 0;
    align-self: end;
  }

  .helper {
    min-height: 1.1em;
    color: $color-faint;
    font-size: 0.75rem;
    font-weight: 500;
    line-height: 1.2;
  }
}

.video-meta {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-height: 2.4em;
}

.file-name {
  color: $color-muted;
  font-size: 0.8rem;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.file-hint {
  color: $color-faint;
  font-size: 0.8rem;
  font-weight: 500;
}

.label-spacer {
  display: block;
  height: 1.32rem;
  visibility: hidden;
}

.module-row {
  display: grid;
  grid-template-columns: 1fr 110px auto;
  gap: 16px;
  align-items: end;
  margin-bottom: 20px;

  @media (max-width: 720px) {
    grid-template-columns: 1fr;
  }
}

.lessons {
  padding-top: 8px;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
}

.lessons-label {
  font-size: 0.82rem;
  font-weight: 600;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: $color-faint;
  margin-bottom: 12px;
}

.lesson-row {
  display: grid;
  grid-template-columns: 1fr 1.2fr 110px 110px auto;
  gap: 16px;
  align-items: start;
  margin-bottom: 28px;

  @media (max-width: 900px) {
    grid-template-columns: 1fr 1fr;
  }

  @media (max-width: 560px) {
    grid-template-columns: 1fr;
  }
}

.add-lesson {
  margin-top: 4px;
}

.danger {
  color: #f87171;
}

.icon-btn {
  min-width: 48px;
  height: 48px;
  padding: 0;
  font-size: 1.25rem;
  line-height: 1;
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

.file-upload-btn {
  @include liquid-glass;
  --lg-r: 14px;
  --lg-blur: 0px;
  --lg-tint: rgba(255, 255, 255, 0.04);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  height: 48px;
  padding: 0 20px;
  border: none;
  font: inherit;
  font-size: 0.9rem;
  font-weight: 600;
  color: $color-ink;
  cursor: pointer;
  background: none;
  box-shadow:
    0 10px 30px rgba(3, 6, 24, 0.35),
    0 2px 8px rgba(3, 6, 24, 0.22),
    0 18px 30px -22px rgba(170, 200, 255, 0.35),
    inset 0 1px 1px rgba(255, 255, 255, 0.3),
    inset 0 0 0 1px rgba(255, 255, 255, 0.1);
  transition: --lg-tint 0.35s, box-shadow 0.35s, transform 0.2s;

  &:hover {
    --lg-tint: rgba(245, 158, 11, 0.08);
    transform: translateY(-2px);
    box-shadow:
      0 10px 30px rgba(3, 6, 24, 0.4),
      0 2px 8px rgba(3, 6, 24, 0.25),
      0 18px 30px -22px rgba(245, 158, 11, 0.4),
      inset 0 1px 1px rgba(255, 255, 255, 0.35),
      inset 0 0 0 1px rgba(245, 158, 11, 0.3);
  }

  &:focus-visible {
    outline: none;
  }

  svg {
    color: $color-gold;
    flex-shrink: 0;
  }
}
</style>
