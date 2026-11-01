<template>
  <article class="product-card glass-card" ref="cardRef">
    <div class="p-visual">
      <CourseThumbnail
        class="p-image"
        :url="course.thumbnailUrl"
        :title="course.title"
        :label="course.technologyNames?.[0] ?? course.categoryNames?.[0]"
        :seed="course.id"
      />
    </div>
    <div class="p-info">
      <p class="p-specs">
        <span>{{ course.moduleCount }} modułów</span>
        <span>{{ course.lessonCount }} lekcji</span>
      </p>
      <h3 class="p-name">{{ course.title }}</h3>
      <span class="p-brand">{{ course.instructorName }}</span>
      <div class="p-row">
        <p class="p-price">{{ course.price }} zł</p>
        <router-link class="btn btn-ghost btn-pill add-btn" :to="{ name: 'CourseDetails', params: { id: course.id } }">
          Zobacz
        </router-link>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import type { CourseListDto } from '@/features/courses/types/course.types'
import CourseThumbnail from '@/shared/components/media/CourseThumbnail.vue'

interface Props {
  course: CourseListDto
}

defineProps<Props>()

const cardRef = ref<HTMLElement>()

onMounted(() => {
  if (!cardRef.value) return
  const observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.classList.add('visible')
          observer.unobserve(entry.target)
        }
      })
    },
    { threshold: 0.12 }
  )
  observer.observe(cardRef.value)
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.product-card {
  --lg-blur: 10px;
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden;
}

.p-visual {
  margin: 12px 12px 0;
}

.p-image {
  position: relative;
  width: 100%;
  aspect-ratio: 16 / 9;
  border-radius: 14px 14px 14px 4px;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.08);
}

.p-info {
  display: flex;
  flex-direction: column;
  flex: 1;
  padding: 16px 20px 20px;
}

.p-specs {
  @include mono-label;
  display: flex;
  gap: 10px;
  margin-bottom: 10px;
  font-size: 0.64rem;
  color: $color-faint;

  span + span::before {
    content: "/";
    margin-right: 10px;
    color: $color-hairline;
  }
}

.p-name {
  font-size: 1.22rem;
  font-weight: 500;
  line-height: 1.18;
  margin-bottom: 6px;
}

.p-brand {
  font-size: 0.84rem;
  color: $color-muted;
  margin-bottom: 16px;
}

.p-row {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 12px;
  margin-top: auto;
  padding-top: 16px;
  border-top: 1px dashed $color-hairline;
}

.p-price {
  font-family: $font-display;
  font-weight: 600;
  font-size: 1.45rem;
  line-height: 1;
}

.add-btn {
  padding: 10px 18px;
  font-size: 0.86rem;
}
</style>
