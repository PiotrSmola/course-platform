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
            <SelectDropdown v-model="level" :options="levelOptions" />
            <span v-if="errors.level" class="error">{{ errors.level }}</span>
          </div>
          <div v-if="!isNew" class="form-group">
            <label>Status</label>
            <SelectDropdown v-model="status" :options="statusOptions" />
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
          <label>Miniaturka kursu</label>
          <input
            ref="thumbnailInputRef"
            type="file"
            accept="image/*"
            class="sr-only"
            @change="onThumbnailInputChange"
          />
          <button
            type="button"
            class="file-upload-btn"
            @click="thumbnailInputRef?.click()"
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
              <polyline points="17 8 12 3 7 8" />
              <line x1="12" y1="3" x2="12" y2="15" />
            </svg>
            <span>Wybierz plik</span>
          </button>
          <div v-if="currentThumbnailUrl || newThumbnailPreviewUrl" class="thumbnail-preview-row">
            <div v-if="currentThumbnailUrl" class="thumb-preview">
              <img :src="currentThumbnailUrl" alt="Obecna miniaturka" />
              <span>Obecna</span>
            </div>
            <svg
              v-if="currentThumbnailUrl && newThumbnailPreviewUrl"
              class="thumb-arrow"
              width="24"
              height="24"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
              stroke-linecap="round"
              stroke-linejoin="round"
            >
              <line x1="5" y1="12" x2="19" y2="12" />
              <polyline points="12 5 19 12 12 19" />
            </svg>
            <div v-if="newThumbnailPreviewUrl" class="thumb-preview">
              <img :src="newThumbnailPreviewUrl" alt="Nowa miniaturka" />
              <span>Nowa</span>
            </div>
          </div>
          <span v-if="selectedThumbnailFile" class="file-name">{{ selectedThumbnailFile.name }}</span>
          <span v-if="thumbnailStatus" class="helper">{{ thumbnailStatus }}</span>
          <span v-if="errors.thumbnailObjectKey" class="error">{{ errors.thumbnailObjectKey }}</span>
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
import { useCourseThumbnailUrl } from '@/features/courses/composables/useCourseAssets'
import { getCategories, getTechnologies, presignCourseThumbnailUpload, confirmCourseThumbnailUpload } from '@/features/courses/api/courses.api'
import { CourseLevel, CourseStatus } from '@/features/courses/types/course.types'
import { createCourseSchema, updateCourseSchema } from '@/features/courses/schemas/course.schema'
import CourseStructureEditor from '@/features/instructor/components/CourseStructureEditor.vue'
import SelectDropdown from '@/shared/components/forms/SelectDropdown.vue'

const levelOptions = [
  { value: CourseLevel.Beginner, label: 'Początkujący' },
  { value: CourseLevel.Intermediate, label: 'Średni' },
  { value: CourseLevel.Advanced, label: 'Zaawansowany' }
]

const statusOptions = [
  { value: CourseStatus.Draft, label: 'Szkic' },
  { value: CourseStatus.Published, label: 'Opublikowany' },
  { value: CourseStatus.Hidden, label: 'Ukryty' }
]

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
  validationSchema: computed(() => toTypedSchema(schema.value)),
  initialValues: {
    title: '',
    description: '',
    shortDescription: '',
    price: 0,
    level: CourseLevel.Beginner,
    status: CourseStatus.Draft,
    thumbnailObjectKey: '',
    language: 'English',
    categoryIds: [] as string[],
    technologyIds: [] as string[]
  }
})

const [title] = defineField('title')
const [description] = defineField('description')
const [shortDescription] = defineField('shortDescription')
const [price] = defineField('price')
const [level] = defineField('level')
const [status] = defineField('status')
const [thumbnailObjectKey] = defineField('thumbnailObjectKey')
const [language] = defineField('language')
const [categoryIds] = defineField('categoryIds')
const [technologyIds] = defineField('technologyIds')

const thumbnailStatus = ref<string>('')
const thumbnailInputRef = ref<HTMLInputElement | null>(null)
const selectedThumbnailFile = ref<File | null>(null)
const newThumbnailPreviewUrl = ref<string>('')

const thumbnailUrlQuery = useCourseThumbnailUrl(computed(() => props.id ?? ''))
const currentThumbnailUrl = computed(() => thumbnailUrlQuery.data.value ?? '')

watch(selectedThumbnailFile, (file) => {
  if (newThumbnailPreviewUrl.value) {
    URL.revokeObjectURL(newThumbnailPreviewUrl.value)
  }
  newThumbnailPreviewUrl.value = file ? URL.createObjectURL(file) : ''
})

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

const courseQuery = useCourseDetails(() => props.id ?? '', () => !isNew.value)

const course = computed(() => courseQuery.data.value)

watch(
  () => [courseQuery.data.value, categories.value, technologies.value] as const,
  ([data, cats, techs]) => {
    if (!data) return
    if (cats.length === 0 || techs.length === 0) return
    if (meta.value.dirty) return
    resetForm({
      values: {
        title: data.title,
        description: data.description,
        shortDescription: data.shortDescription,
        price: data.price,
        level: data.level,
        status: data.status,
        thumbnailObjectKey: data.thumbnailObjectKey,
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
    thumbnailObjectKey: values.thumbnailObjectKey ?? '',
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

function onThumbnailInputChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  selectedThumbnailFile.value = file
  void uploadThumbnail(file)
}

async function uploadThumbnail(file: File) {
  if (!props.id) return

  thumbnailStatus.value = 'Generowanie URL do uploadu...'
  const presign = await presignCourseThumbnailUpload(props.id, file.type || 'application/octet-stream')

  thumbnailStatus.value = 'Upload miniaturki...'
  const res = await fetch(presign.url, { method: 'PUT', body: file, headers: { 'Content-Type': file.type } })
  if (!res.ok) throw new Error(`Thumbnail upload failed: ${res.status}`)

  thumbnailStatus.value = 'Zapis miniaturki...'
  await confirmCourseThumbnailUpload(props.id, presign.objectKey)
  thumbnailObjectKey.value = presign.objectKey
  thumbnailStatus.value = 'Gotowe'
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
  max-width: 960px;
  margin-inline: auto;
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

  .helper {
    color: $color-faint;
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

.thumbnail-preview-row {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-top: 8px;
}

.thumb-preview {
  display: flex;
  flex-direction: column;
  gap: 6px;
  width: 120px;

  img {
    width: 100%;
    aspect-ratio: 16 / 10;
    object-fit: cover;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(3, 6, 24, 0.35);
  }

  span {
    font-size: 0.75rem;
    color: $color-faint;
    text-align: center;
  }
}

.thumb-arrow {
  color: $color-gold;
  flex-shrink: 0;
}

.file-name {
  color: $color-muted;
  font-size: 0.82rem;
  font-weight: 500;
}
</style>
