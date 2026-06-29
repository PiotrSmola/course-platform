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
          <label>Kategorie</label>
          <select v-model="categoryIds" multiple class="multi-select">
            <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
          </select>
          <span v-if="errors.categoryIds" class="error">{{ errors.categoryIds }}</span>
        </div>
        <div class="form-group">
          <label>Technologie</label>
          <select v-model="technologyIds" multiple class="multi-select">
            <option v-for="tech in technologies" :key="tech.id" :value="tech.id">{{ tech.name }}</option>
          </select>
          <span v-if="errors.technologyIds" class="error">{{ errors.technologyIds }}</span>
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

      <CourseStructureEditor
        v-if="!isNew && props.id && course"
        :course-id="props.id"
        :course-modules="course.modules"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useQuery } from '@tanstack/vue-query'
import { useCreateCourse, useUpdateCourse, useCourseDetails } from '@/features/courses/composables/useCourses'
import { getCategories, getTechnologies } from '@/features/courses/api/courses.api'
import { CourseLevel, CourseStatus } from '@/features/courses/types/course.types'
import { createCourseSchema, updateCourseSchema } from '@/features/courses/schemas/course.schema'
import CourseStructureEditor from '@/features/instructor/components/CourseStructureEditor.vue'

const props = defineProps<{
  id?: string
  isNew?: boolean
}>()

const isNew = computed(() => props.isNew)

const createMutation = useCreateCourse()
const updateMutation = useUpdateCourse()

const isSubmitting = computed(() => createMutation.isPending.value || updateMutation.isPending.value)

const schema = computed(() => (isNew.value ? createCourseSchema : updateCourseSchema))

const { handleSubmit, defineField, errors, meta, resetForm } = useForm({
  validationSchema: computed(() => toTypedSchema(schema.value))
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

const categoriesQuery = useQuery({
  queryKey: ['categories'],
  queryFn: getCategories
})

const technologiesQuery = useQuery({
  queryKey: ['technologies'],
  queryFn: getTechnologies
})

const categories = computed(() => categoriesQuery.data.value ?? [])
const technologies = computed(() => technologiesQuery.data.value ?? [])

const courseQuery = useCourseDetails(props.id ?? '', !isNew.value)

const course = computed(() => courseQuery.data.value)

watch(
  () => [courseQuery.data.value, categories.value, technologies.value] as const,
  ([data, cats, techs]) => {
    if (!data) return
    if (cats.length === 0 || techs.length === 0) return
    resetForm({
      values: {
        title: data.title,
        description: data.description,
        shortDescription: data.shortDescription,
        price: data.price,
        level: data.level,
        status: data.status,
        thumbnailUrl: data.thumbnailUrl,
        language: data.language,
        categoryIds: data.categoryNames.map(name => {
          const cat = cats.find(c => c.name === name)
          return cat?.id ?? ''
        }).filter(Boolean),
        technologyIds: data.technologyNames.map(name => {
          const tech = techs.find(t => t.name === name)
          return tech?.id ?? ''
        }).filter(Boolean)
      }
    })
  },
  { immediate: true }
)

const onSubmit = handleSubmit(async (values) => {
  const basePayload = {
    title: values.title,
    description: values.description,
    shortDescription: values.shortDescription ?? '',
    price: values.price,
    level: values.level,
    thumbnailUrl: values.thumbnailUrl ?? '',
    language: values.language,
    categoryIds: values.categoryIds ?? [],
    technologyIds: values.technologyIds ?? []
  }

  if (isNew.value) {
    createMutation.mutate(basePayload)
    return
  }

  if (!props.id) return

  if (values.status === undefined) return

  updateMutation.mutate({
    id: props.id,
    status: values.status,
    ...basePayload
  })
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

.multi-select {
  height: auto;
  min-height: 120px;
  padding: 8px 12px;

  option {
    padding: 6px 8px;
    border-radius: 8px;
  }
}

.btn {
  height: 48px;
  margin-top: 8px;
}
</style>
