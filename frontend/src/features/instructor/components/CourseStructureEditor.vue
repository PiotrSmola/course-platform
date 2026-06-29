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
            <label :for="`lesson-video-${lesson.id}`">URL wideo</label>
            <input
              :id="`lesson-video-${lesson.id}`"
              v-model="lesson.videoUrl"
              placeholder="https://placeholder.local/video.mp4"
              @blur="saveLesson(module.id, lesson, lIndex)"
            />
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
        videoUrl: l.videoUrl ?? ''
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
    videoUrl: 'https://placeholder.local/videos/lesson.mp4',
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
    videoUrl: lesson.videoUrl || 'https://placeholder.local/videos/lesson.mp4',
    duration: lesson.duration || 1,
    order: lesson.order ?? index
  })
}

function removeLesson(moduleId: string, lessonId: string) {
  if (!confirm('Usunąć lekcję?')) return
  deleteLesson.mutate({ moduleId, lessonId })
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.structure-editor {
  margin-top: 48px;
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
  align-items: end;
  margin-bottom: 16px;

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
</style>
