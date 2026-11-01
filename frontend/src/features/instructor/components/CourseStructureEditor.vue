<template>
  <section class="structure-editor">
    <div class="structure-head">
      <h3>Struktura kursu</h3>
      <button type="button" class="btn btn-ghost" @click="addModule">+ Moduł</button>
    </div>

    <p v-if="!modules.length" class="empty">Brak modułów. Dodaj pierwszy moduł, a następnie lekcje z adresem URL wideo (placeholder).</p>

    <div v-for="(module, mIndex) in modules" :key="module.id" class="module-block glass">
      <span class="module-no">{{ String(mIndex + 1).padStart(2, '0') }}</span>
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
          <button type="button" class="btn danger" @click="removeModule(module.id)">Usuń moduł</button>
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
              accept="video/mp4,video/webm,video/quicktime"
              class="sr-only"
              @change="onVideoInputChange(module.id, lesson.id, $event)"
            />
            <button
              type="button"
              class="btn btn-ghost file-upload-btn"
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
            <button type="button" class="btn danger icon-btn" title="Usuń lekcję" @click="removeLesson(module.id, lesson.id)">×</button>
          </div>
          <LessonQuizEditor
            v-if="!lesson.id.startsWith('temp-')"
            :course-id="courseId"
            :lesson-id="lesson.id"
          />
          <LessonResourcesEditor
            v-if="!lesson.id.startsWith('temp-')"
            :course-id="courseId"
            :lesson-id="lesson.id"
          />
        </div>
        <button type="button" class="btn btn-ghost add-lesson" @click="addLesson(module.id, module.lessons.length)">+ Lekcja</button>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { ModuleDto } from '@/features/courses/types/course.types'
import { useModuleMutations, useLessonVideoUpload } from '@/features/instructor/composables/useInstructor'
import LessonQuizEditor from '@/features/quizzes/components/LessonQuizEditor.vue'
import LessonResourcesEditor from '@/features/lesson-resources/components/LessonResourcesEditor.vue'

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

const videoUpload = useLessonVideoUpload(props.courseId)

function setVideoInputRef(el: HTMLInputElement | null, lessonId: string) {
  videoInputRefs.value[lessonId] = el
}

function onVideoInputChange(moduleId: string, lessonId: string, event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  input.value = ''
  if (!file) return
  selectedVideoFiles.value[lessonId] = file
  videoUpload.mutate(
    { lessonId, file, onStatus: (status) => { uploadState.value[lessonId] = { status } } },
    {
      onSuccess: () => {
        uploadState.value[lessonId] = { status: 'Gotowe' }
      },
      onError: () => {
        uploadState.value[lessonId] = {}
        delete selectedVideoFiles.value[lessonId]
      }
    }
  )
}
</script>

<style lang="scss" scoped>
.structure-editor {
  margin-top: 56px;
  margin-inline: auto;
  max-width: 960px;
}

.structure-head {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 24px;
  padding-bottom: 18px;
  border-bottom: 1px solid $color-hairline;

  h3 {
    margin: 0;
    font-size: clamp(1.6rem, 2.6vw, 2.2rem);
    font-weight: 600;
  }

  .btn {
    padding: 10px 20px;
    font-size: 0.86rem;
  }
}

.empty {
  color: $color-muted;
  margin-bottom: 20px;
}

.module-block {
  --lg-r: #{$radius-card};
  --lg-blur: 12px;
  --lg-tint: rgba(13, 15, 19, 0.28);
  padding: 22px 24px 24px;
  margin-bottom: 20px;

  @media (max-width: 560px) {
    padding: 18px 14px;
  }
}

.module-no {
  display: block;
  margin-bottom: 14px;
  font-family: $font-display;
  font-size: 1.7rem;
  font-weight: 600;
  line-height: 1;
  color: $color-accent;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 0;

  label {
    @include mono-label;
    font-size: 0.62rem;
    color: $color-muted;
  }

  input {
    width: 100%;
    height: 46px;
    padding: 0 16px;
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

  &--narrow {
    max-width: 110px;
  }

  &--action {
    flex-shrink: 0;
    align-self: end;
  }

  .helper {
    min-height: 1.1em;
    font-family: $font-mono;
    font-size: 0.68rem;
    line-height: 1.3;
    color: $color-faint;
  }
}

.video-meta {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-height: 2.4em;
}

.file-name {
  font-family: $font-mono;
  font-size: 0.72rem;
  color: $color-muted;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.file-hint {
  @include mono-label;
  font-size: 0.6rem;
  color: $color-accent;
}

.label-spacer {
  display: block;
  height: 1.05rem;
  visibility: hidden;
}

.module-row {
  display: grid;
  grid-template-columns: 1fr 110px auto;
  gap: 16px;
  align-items: end;
  margin-bottom: 22px;

  @media (max-width: 720px) {
    grid-template-columns: 1fr;
  }
}

.lessons {
  padding-top: 18px;
  border-top: 1px solid $color-hairline;
}

.lessons-label {
  @include mono-label;
  margin-bottom: 16px;
  font-size: 0.62rem;
  color: $color-faint;
}

.lesson-row {
  display: grid;
  grid-template-columns: 1fr 1.2fr 110px 110px auto;
  gap: 16px;
  align-items: start;
  margin-bottom: 22px;
  padding-bottom: 22px;
  border-bottom: 1px dashed $color-hairline;

  .form-group--action {
    align-self: start;
  }

  @media (max-width: 900px) {
    grid-template-columns: 1fr 1fr;
  }

  @media (max-width: 560px) {
    grid-template-columns: 1fr;
  }
}

.btn {
  padding: 10px 18px;
  font-size: 0.84rem;
}

.add-lesson {
  margin-top: 2px;
}

.btn.danger {
  --lg-tint: #{rgba($color-danger, 0.22)};

  &:hover {
    --lg-tint: #{rgba($color-danger, 0.32)};
  }
}

.icon-btn {
  min-width: 46px;
  height: 46px;
  padding: 0;
  font-size: 1.2rem;
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
  align-self: flex-start;
  height: 46px;
  padding: 0 18px;

  svg {
    color: $color-accent;
    flex-shrink: 0;
  }
}
</style>
