<template>
  <article class="product-card glass-card" ref="cardRef">
    <div class="p-visual">
      <div class="p-image" :style="thumbnailStyle"></div>
    </div>
    <div class="p-info">
      <span class="p-brand">{{ course.instructorName }}</span>
      <h3 class="p-name">{{ course.title }}</h3>
      <p class="p-specs">{{ course.moduleCount }} modułów · {{ course.lessonCount }} lekcji</p>
      <div class="p-row">
        <p class="p-price">
          <strong>{{ course.price }} zł</strong>
        </p>
        <router-link class="btn btn-primary add-btn" :to="{ name: 'CourseDetails', params: { id: course.id } }">
          Zobacz
        </router-link>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import type { CourseListDto } from '@/features/courses/types/course.types'

interface Props {
  course: CourseListDto
}

const props = defineProps<Props>()

const cardRef = ref<HTMLElement>()

const thumbnailStyle = computed(() => {
  const url = props.course.thumbnailUrl
  return url ? { backgroundImage: `url(${url})` } : {}
})

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
  --lg-r: 28px;
  --lg-blur: 0px;
  position: relative;
  padding: 26px 24px 24px;
  transition: transform 0.35s, box-shadow 0.35s;

  &:hover {
    --lg-tint: rgba(255, 255, 255, 0.06);
    transform: translateY(-8px);
    box-shadow: 0 28px 64px rgba(3, 6, 24, 0.55), 0 4px 12px rgba(3, 6, 24, 0.3);
  }

  &.visible {
    transform: none;
  }
}

.p-visual {
  display: flex;
  justify-content: center;
  padding: 14px 0 22px;
}

.p-image {
  width: 100%;
  height: 160px;
  border-radius: 20px;
  background-size: cover;
  background-position: center;
  box-shadow:
    0 18px 40px rgba(3, 6, 24, 0.5),
    inset 0 1px 1px rgba(255, 255, 255, 0.45),
    inset 0 0 0 1px rgba(255, 255, 255, 0.18);
  position: relative;
  transition: transform 0.4s;
}

.product-card:hover .p-image {
  transform: translateY(-6px);
}

.p-brand {
  font-size: 0.78rem;
  font-weight: 600;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: $color-faint;
}

.p-name {
  font-size: 1.25rem;
  margin: 4px 0 6px;
  letter-spacing: -0.01em;
}

.p-specs {
  font-size: 0.88rem;
  color: $color-muted;
  margin-bottom: 18px;
}

.p-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.p-price strong {
  font-size: 1.22rem;
}

.add-btn {
  padding: 11px 18px;
  font-size: 0.88rem;
}
</style>
