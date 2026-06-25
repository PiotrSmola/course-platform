<template>
  <div class="course-editor">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">{{ isNew ? 'Nowy kurs' : 'Edycja kursu' }}</span>
        <h2>{{ isNew ? 'Utwórz kurs' : 'Edytuj kurs' }}</h2>
      </div>

      <form @submit.prevent="submit" class="editor-form glass">
        <div class="form-group">
          <label>Tytuł</label>
          <input v-model="form.title" required placeholder="Np. Vue 3 Mastery" />
        </div>
        <div class="form-group">
          <label>Opis</label>
          <textarea v-model="form.description" required placeholder="Opis kursu..." rows="4"></textarea>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Krótki opis</label>
            <input v-model="form.shortDescription" placeholder="Maks. 500 znaków" />
          </div>
          <div class="form-group">
            <label>Cena (zł)</label>
            <input v-model.number="form.price" type="number" required min="0" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Poziom</label>
            <select v-model="form.level">
              <option :value="CourseLevel.Beginner">Początkujący</option>
              <option :value="CourseLevel.Intermediate">Średni</option>
              <option :value="CourseLevel.Advanced">Zaawansowany</option>
            </select>
          </div>
          <div class="form-group">
            <label>Status</label>
            <select v-model="form.status">
              <option :value="CourseStatus.Draft">Szkic</option>
              <option :value="CourseStatus.Published">Opublikowany</option>
              <option :value="CourseStatus.Hidden">Ukryty</option>
            </select>
          </div>
        </div>
        <div class="form-group">
          <label>URL miniaturki</label>
          <input v-model="form.thumbnailUrl" placeholder="https://..." />
        </div>
        <button type="submit" class="btn btn-primary" :disabled="isSubmitting">
          {{ isNew ? 'Utwórz kurs' : 'Zapisz zmiany' }}
        </button>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useCreateCourse, useUpdateCourse } from '@/features/courses/composables/useCourses'
import { CourseLevel, CourseStatus } from '@/features/courses/types/course.types'

const props = defineProps<{
  id?: string
  isNew?: boolean
}>()

const isNew = computed(() => props.isNew)

const createMutation = useCreateCourse()
const updateMutation = useUpdateCourse()

const isSubmitting = computed(() => createMutation.isPending.value || updateMutation.isPending.value)

const form = ref({
  title: '',
  description: '',
  shortDescription: '',
  price: 0,
  level: CourseLevel.Beginner,
  thumbnailUrl: '',
  status: CourseStatus.Draft,
  language: 'English',
  categoryIds: [] as string[],
  technologyIds: [] as string[]
})

function submit() {
  if (props.isNew) {
    createMutation.mutate({
      title: form.value.title,
      description: form.value.description,
      shortDescription: form.value.shortDescription,
      price: form.value.price,
      level: form.value.level,
      thumbnailUrl: form.value.thumbnailUrl,
      language: form.value.language,
      categoryIds: form.value.categoryIds,
      technologyIds: form.value.technologyIds
    })
  } else if (props.id) {
    updateMutation.mutate({
      id: props.id,
      ...form.value
    })
  }
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.course-editor {
  padding: calc($header-height + 40px) 0 80px;
}

.editor-form {
  --lg-r: 28px;
  --lg-blur: 0px;
  max-width: 700px;
  padding: 40px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 24px;

  label {
    font-size: 0.88rem;
    font-weight: 600;
    color: $color-muted;
  }

  input, textarea, select {
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

    &::placeholder {
      color: $color-faint;
    }

    &:focus {
      background: rgba(255, 255, 255, 0.07);
      box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.35), inset 0 0 0 1px rgba(167, 139, 250, 0.5), 0 0 0 4px rgba(139, 92, 246, 0.18);
    }
  }

  textarea {
    height: auto;
    padding: 14px 18px;
    resize: vertical;
  }
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;

  @media (max-width: 560px) {
    grid-template-columns: 1fr;
  }
}

.btn {
  height: 48px;
  margin-top: 8px;
}
</style>
