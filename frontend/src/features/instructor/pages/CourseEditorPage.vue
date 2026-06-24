<template>
  <div class="course-editor">
    <h1>{{ isNew ? 'Create Course' : 'Edit Course' }}</h1>
    <form @submit.prevent="submit">
      <div class="form-group">
        <label>Title</label>
        <input v-model="form.title" required />
      </div>
      <div class="form-group">
        <label>Description</label>
        <textarea v-model="form.description" required></textarea>
      </div>
      <div class="form-group">
        <label>Short Description</label>
        <input v-model="form.shortDescription" />
      </div>
      <div class="form-group">
        <label>Price</label>
        <input v-model.number="form.price" type="number" required />
      </div>
      <div class="form-group">
        <label>Thumbnail URL</label>
        <input v-model="form.thumbnailUrl" />
      </div>
      <button type="submit">Save</button>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useCreateCourse, useUpdateCourse } from '@/features/courses/composables/useCourses'
import { CourseLevel, CourseStatus } from '@/features/courses/types/course.types'

const props = defineProps<{
  id?: string
  isNew?: boolean
}>()

const createMutation = useCreateCourse()
const updateMutation = useUpdateCourse()

const form = ref({
  title: '',
  description: '',
  shortDescription: '',
  price: 0,
  level: CourseLevel.Beginner,
  thumbnailUrl: '',
  status: CourseStatus.Draft
})

function submit() {
  if (props.isNew) {
    createMutation.mutate({
      title: form.value.title,
      description: form.value.description,
      shortDescription: form.value.shortDescription,
      price: form.value.price,
      level: form.value.level,
      thumbnailUrl: form.value.thumbnailUrl
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
.course-editor {
  padding: 2rem;
  max-width: 700px;
}

.form-group {
  display: flex;
  flex-direction: column;
  margin-bottom: 1rem;
}

input, textarea {
  padding: 0.5rem;
}

button {
  padding: 0.75rem 1.5rem;
  cursor: pointer;
}
</style>
