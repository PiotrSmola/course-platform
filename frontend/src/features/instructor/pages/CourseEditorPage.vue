<template>
  <div class="course-editor">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">{{ isNew ? 'Nowy kurs' : 'Edycja kursu' }}</span>
        <h2>{{ isNew ? 'Utwórz kurs' : 'Edytuj kurs' }}</h2>
      </div>

      <form @submit="onSubmit" class="editor-form glass">
        <div class="form-group">
          <label>Tytuł</label>
          <input v-model="title" placeholder="Np. Vue 3 Mastery" />
          <span v-if="errors.title" class="error">{{ errors.title }}</span>
        </div>
        <div class="form-group">
          <label>Opis</label>
          <textarea v-model="description" placeholder="Opis kursu..." rows="4"></textarea>
          <span v-if="errors.description" class="error">{{ errors.description }}</span>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Krótki opis</label>
            <input v-model="shortDescription" placeholder="Maks. 500 znaków" />
            <span v-if="errors.shortDescription" class="error">{{ errors.shortDescription }}</span>
          </div>
          <div class="form-group">
            <label>Cena (zł)</label>
            <input v-model.number="price" type="number" min="0" />
            <span v-if="errors.price" class="error">{{ errors.price }}</span>
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Poziom</label>
            <select v-model="level">
              <option :value="CourseLevel.Beginner">Początkujący</option>
              <option :value="CourseLevel.Intermediate">Średni</option>
              <option :value="CourseLevel.Advanced">Zaawansowany</option>
            </select>
            <span v-if="errors.level" class="error">{{ errors.level }}</span>
          </div>
          <div v-if="!isNew" class="form-group">
            <label>Status</label>
            <select v-model="status">
              <option :value="CourseStatus.Draft">Szkic</option>
              <option :value="CourseStatus.Published">Opublikowany</option>
              <option :value="CourseStatus.Hidden">Ukryty</option>
            </select>
            <span v-if="errors.status" class="error">{{ errors.status }}</span>
          </div>
        </div>
        <div class="form-group">
          <label>URL miniaturki</label>
          <input v-model="thumbnailUrl" placeholder="https://..." />
          <span v-if="errors.thumbnailUrl" class="error">{{ errors.thumbnailUrl }}</span>
        </div>
        <div class="form-group">
          <label>Język</label>
          <input v-model="language" placeholder="Np. Polski" />
          <span v-if="errors.language" class="error">{{ errors.language }}</span>
        </div>
        <button type="submit" class="btn btn-primary" :disabled="!meta.valid || isSubmitting">
          {{ isNew ? 'Utwórz kurs' : 'Zapisz zmiany' }}
        </button>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useCreateCourse, useUpdateCourse } from '@/features/courses/composables/useCourses'
import { CourseLevel, CourseStatus } from '@/features/courses/types/course.types'
import { createCourseSchema } from '@/features/courses/schemas/course.schema'

const props = defineProps<{
  id?: string
  isNew?: boolean
}>()

const isNew = computed(() => props.isNew)

const createMutation = useCreateCourse()
const updateMutation = useUpdateCourse()

const isSubmitting = computed(() => createMutation.isPending.value || updateMutation.isPending.value)

const { handleSubmit, defineField, errors, meta } = useForm({
  validationSchema: toTypedSchema(createCourseSchema)
})

const [title] = defineField('title')
const [description] = defineField('description')
const [shortDescription] = defineField('shortDescription')
const [price] = defineField('price')
const [level] = defineField('level')
const [status] = defineField('status')
const [thumbnailUrl] = defineField('thumbnailUrl')
const [language] = defineField('language')
const [categoryIds] = defineField('categoryIds')
const [technologyIds] = defineField('technologyIds')

title.value = ''
description.value = ''
shortDescription.value = ''
price.value = 0
level.value = CourseLevel.Beginner
status.value = CourseStatus.Draft
thumbnailUrl.value = ''
language.value = 'English'
categoryIds.value = []
technologyIds.value = []

const onSubmit = handleSubmit(async (values) => {
  if (isNew.value) {
    const { status: _status, ...createData } = values as any
    createMutation.mutate({
      title: createData.title,
      description: createData.description,
      shortDescription: createData.shortDescription || '',
      price: createData.price,
      level: createData.level,
      thumbnailUrl: createData.thumbnailUrl || '',
      language: createData.language,
      categoryIds: createData.categoryIds || [],
      technologyIds: createData.technologyIds || []
    })
  } else if (props.id) {
    updateMutation.mutate({
      id: props.id,
      ...values
    } as any)
  }
})
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

  .error {
    color: #f87171;
    font-size: 0.82rem;
    font-weight: 500;
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
