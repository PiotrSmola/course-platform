<template>
  <div class="course-editor">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">{{ isNew ? 'Nowy kurs' : 'Edycja kursu' }}</span>
        <h2>{{ isNew ? 'Utwórz' : 'Edytuj' }} <em>kurs</em></h2>
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
            accept="image/jpeg,image/png,image/webp"
            class="sr-only"
            @change="onThumbnailInputChange"
          />
          <button
            type="button"
            class="btn btn-ghost file-upload-btn"
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
import { useCourseThumbnailUpload } from '@/features/instructor/composables/useInstructor'
import { getCategories, getTechnologies } from '@/features/courses/api/courses.api'
import { CourseLevel } from '@/features/courses/types/course.types'
import { createCourseSchema, updateCourseSchema } from '@/features/courses/schemas/course.schema'
import CourseStructureEditor from '@/features/instructor/components/CourseStructureEditor.vue'
import SelectDropdown from '@/shared/components/forms/SelectDropdown.vue'
import { queryKeys } from '@/shared/queryKeys'

const levelOptions = [
  { value: CourseLevel.Beginner, label: 'Początkujący' },
  { value: CourseLevel.Intermediate, label: 'Średni' },
  { value: CourseLevel.Advanced, label: 'Zaawansowany' }
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
const [language] = defineField('language')
const [categoryIds] = defineField('categoryIds')
const [technologyIds] = defineField('technologyIds')

const thumbnailStatus = ref<string>('')
const thumbnailInputRef = ref<HTMLInputElement | null>(null)
const selectedThumbnailFile = ref<File | null>(null)
const newThumbnailPreviewUrl = ref<string>('')

watch(selectedThumbnailFile, (file) => {
  if (newThumbnailPreviewUrl.value) {
    URL.revokeObjectURL(newThumbnailPreviewUrl.value)
  }
  newThumbnailPreviewUrl.value = file ? URL.createObjectURL(file) : ''
})

const categoriesQuery = useQuery({
  queryKey: queryKeys.categories(),
  queryFn: getCategories
})

const technologiesQuery = useQuery({
  queryKey: queryKeys.technologies(),
  queryFn: getTechnologies
})

const categories = computed(() => categoriesQuery.data.value ?? [])
const technologies = computed(() => technologiesQuery.data.value ?? [])

const courseQuery = useCourseDetails(() => props.id ?? '', () => !isNew.value)

const course = computed(() => courseQuery.data.value)
const currentThumbnailUrl = computed(() => course.value?.thumbnailUrl ?? '')

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
    language: values.language,
    categoryIds: values.categoryIds ?? [],
    technologyIds: values.technologyIds ?? []
  }

  if (isNew.value) {
    createMutation.mutate(basePayload)
    return
  }

  if (!props.id) return

  updateMutation.mutate({
    id: props.id,
    ...basePayload
  })
})

const thumbnailUpload = useCourseThumbnailUpload(props.id ?? '')

function onThumbnailInputChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  input.value = ''
  if (!file || !props.id) return
  selectedThumbnailFile.value = file
  thumbnailUpload.mutate(
    { file, onStatus: (status) => { thumbnailStatus.value = status } },
    {
      onSuccess: () => {
        thumbnailStatus.value = 'Gotowe'
        selectedThumbnailFile.value = null
      },
      onError: () => {
        thumbnailStatus.value = ''
        selectedThumbnailFile.value = null
      }
    }
  )
}
</script>

<style lang="scss" scoped>
.course-editor {
  padding: 24px 0 80px;
}

.editor-form {
  --lg-r: #{$radius-card};
  --lg-blur: 12px;
  --lg-tint: rgba(13, 15, 19, 0.28);
  max-width: 960px;
  margin-inline: auto;
  padding: 36px 40px 40px;

  @media (max-width: 560px) {
    padding: 22px 16px 24px;
  }
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 22px;

  label {
    @include mono-label;
    font-size: 0.64rem;
    color: $color-muted;
  }

  input,
  textarea,
  select {
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

  textarea {
    height: auto;
    padding: 14px 16px;
    line-height: 1.55;
    resize: vertical;
  }

  .error {
    color: $color-danger;
    font-size: 0.8rem;
  }

  .helper {
    font-family: $font-mono;
    font-size: 0.72rem;
    color: $color-faint;
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
  height: auto !important;
  min-height: 132px;
  padding: 8px !important;

  option {
    padding: 7px 10px;
    border-radius: $radius-xs;
    color: $color-muted;

    &:checked {
      color: $color-ink;
      background: rgba($color-accent, 0.22);
    }
  }
}

.editor-form > .btn {
  margin-top: 10px;
  padding: 13px 28px;
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
  padding: 11px 20px;
  font-size: 0.86rem;

  svg {
    color: $color-accent;
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
  width: 140px;

  img {
    width: 100%;
    aspect-ratio: 16 / 10;
    object-fit: cover;
    border-radius: 12px 12px 12px 4px;
    box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.08), 0 4px 12px rgba(0, 0, 0, 0.35);
  }

  span {
    @include mono-label;
    font-size: 0.6rem;
    color: $color-faint;
  }
}

.thumb-arrow {
  color: $color-accent;
  flex-shrink: 0;
}

.file-name {
  font-family: $font-mono;
  font-size: 0.74rem;
  color: $color-muted;
}
</style>
