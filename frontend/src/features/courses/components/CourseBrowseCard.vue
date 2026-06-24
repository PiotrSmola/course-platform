<template>
  <article class="course-card glass-card">
    <div class="course-thumb" :style="{ backgroundImage: `url(${course.thumbnailUrl})` }">
      <span v-if="levelLabel" class="course-badge">{{ levelLabel }}</span>
    </div>
    <div class="course-body">
      <h3 class="course-title">{{ course.title }}</h3>
      <p class="course-instructor">{{ course.instructorName }}</p>
      <div class="course-meta">
        <div class="course-rating">
          <span class="rating-value">{{ course.averageRating.toFixed(1) }}</span>
          <div class="stars">
            <span
              v-for="i in 5"
              :key="i"
              class="star"
              :class="{ filled: i <= Math.round(course.averageRating) }"
            >★</span>
          </div>
          <span class="rating-count">({{ course.reviewCount }})</span>
        </div>
        <span class="course-lessons">{{ course.lessonCount }} lekcji</span>
      </div>
      <div class="course-footer">
        <span class="course-price"><strong>{{ course.price }} zł</strong></span>
        <router-link
          class="btn btn-primary course-btn"
          :to="{ name: 'CourseDetails', params: { id: course.id } }"
        >
          Zobacz
        </router-link>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { CourseListDto } from '@/features/courses/types/course.types'
import { CourseLevel } from '@/features/courses/types/course.types'

interface Props {
  course: CourseListDto
}

const props = defineProps<Props>()

const levelLabel = computed(() => {
  switch (props.course.level) {
    case CourseLevel.Beginner:
      return 'Początkujący'
    case CourseLevel.Intermediate:
      return 'Średni'
    case CourseLevel.Advanced:
      return 'Zaawansowany'
    default:
      return ''
  }
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.course-card {
  --lg-r: 20px;
  --lg-blur: 0px;
  overflow: hidden;
  transition: transform 0.35s, box-shadow 0.35s;
  display: flex;
  flex-direction: column;
  height: 100%;

  &:hover {
    --lg-tint: rgba(255, 255, 255, 0.06);
    transform: translateY(-6px);
    box-shadow: 0 24px 56px rgba(3, 6, 24, 0.55), 0 4px 12px rgba(3, 6, 24, 0.3);
  }
}

.course-thumb {
  position: relative;
  width: 100%;
  aspect-ratio: 16 / 9;
  background-size: cover;
  background-position: center;
  border-radius: 20px;
  margin: 20px 20px 0;
  width: calc(100% - 40px);
  box-shadow:
    0 12px 28px rgba(3, 6, 24, 0.4),
    inset 0 1px 1px rgba(255, 255, 255, 0.35),
    inset 0 0 0 1px rgba(255, 255, 255, 0.12);
}

.course-badge {
  position: absolute;
  top: 10px;
  left: 10px;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 0.72rem;
  font-weight: 600;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: $color-ink;
  background: rgba(10, 14, 23, 0.75);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.12);
}

.course-body {
  padding: 18px 20px 20px;
  display: flex;
  flex-direction: column;
  flex: 1;
}

.course-title {
  font-family: $font-display;
  font-size: 1.05rem;
  font-weight: 600;
  line-height: 1.35;
  letter-spacing: -0.01em;
  margin-bottom: 6px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.course-instructor {
  font-size: 0.82rem;
  color: $color-faint;
  margin-bottom: 10px;
}

.course-meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-bottom: 14px;
  margin-top: auto;
}

.course-rating {
  display: flex;
  align-items: center;
  gap: 6px;
}

.rating-value {
  font-size: 0.88rem;
  font-weight: 700;
  color: $color-gold;
}

.stars {
  display: flex;
  gap: 1px;
}

.star {
  font-size: 0.82rem;
  color: rgba(255, 255, 255, 0.18);
  line-height: 1;

  &.filled {
    color: $color-gold;
  }
}

.rating-count {
  font-size: 0.78rem;
  color: $color-faint;
}

.course-lessons {
  font-size: 0.78rem;
  color: $color-faint;
  white-space: nowrap;
}

.course-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding-top: 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
}

.course-price strong {
  font-size: 1.15rem;
  font-weight: 700;
}

.course-btn {
  padding: 10px 18px;
  font-size: 0.86rem;
}
</style>
