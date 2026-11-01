<template>
  <article class="course-card glass-card" :class="{ 'is-featured': featured }">
    <div class="course-thumb-wrap">
      <CourseThumbnail
        class="course-thumb"
        :url="course.thumbnailUrl"
        :title="course.title"
        :label="course.technologyNames?.[0] ?? course.categoryNames?.[0]"
        :seed="course.id"
      />
      <span v-if="isComingSoon" class="course-badge">Wkrótce</span>
    </div>
    <div class="course-body">
      <p class="course-kicker">
        <span v-if="levelLabel">{{ levelLabel }}</span>
        <span>{{ course.lessonCount }} lekcji</span>
      </p>
      <h3 class="course-title">{{ course.title }}</h3>
      <p class="course-instructor">{{ course.instructorName }}</p>
      <p v-if="featured && course.shortDescription" class="course-desc">{{ course.shortDescription }}</p>
      <div class="course-tags" v-if="course.categoryNames.length || course.technologyNames.length">
        <span v-for="cat in course.categoryNames" :key="cat" class="tag">{{ cat }}</span>
        <span v-for="tech in course.technologyNames" :key="tech" class="tag tech">{{ tech }}</span>
      </div>
      <div class="course-footer">
        <div class="course-numbers">
          <span class="course-price">{{ isComingSoon ? 'Wkrótce' : `${course.price} zł` }}</span>
          <span class="course-rating" :title="`Średnia ocen ${course.averageRating.toFixed(1)} z ${course.reviewCount} opinii`">
            <span class="star" aria-hidden="true">★</span>
            {{ course.averageRating.toFixed(1) }}
            <span class="rating-count">/ {{ course.reviewCount }} opinii</span>
          </span>
        </div>
        <router-link
          class="btn btn-ghost btn-pill course-btn"
          :to="{ name: 'CourseDetails', params: { id: course.id } }"
        >
          {{ isComingSoon ? 'Lista oczekujących' : 'Zobacz' }}
        </router-link>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { CourseListDto } from '@/features/courses/types/course.types'
import { CourseLevel, CourseStatus } from '@/features/courses/types/course.types'
import CourseThumbnail from '@/shared/components/media/CourseThumbnail.vue'

interface Props {
  course: CourseListDto
  featured?: boolean
}

const props = defineProps<Props>()

const isComingSoon = computed(() => props.course.status === CourseStatus.Hidden)

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
.course-card {
  --lg-blur: 10px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  height: 100%;
}

.course-thumb-wrap {
  position: relative;
  margin: 12px 12px 0;
}

.course-thumb {
  position: relative;
  width: 100%;
  aspect-ratio: 16 / 9;
  border-radius: 14px 14px 14px 4px;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.08);
}

.course-badge {
  @include tag;
  position: absolute;
  top: 10px;
  right: 10px;
  color: $color-on-accent;
  background: $color-accent;
  box-shadow: none;
}

.course-body {
  padding: 16px 20px 20px;
  display: flex;
  flex-direction: column;
  flex: 1;
}

.course-kicker {
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

.course-title {
  font-family: $font-display;
  font-size: 1.08rem;
  font-weight: 600;
  line-height: 1.25;
  letter-spacing: -0.005em;
  margin-bottom: 6px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.course-instructor {
  font-size: 0.84rem;
  color: $color-muted;
  margin-bottom: 14px;
}

.course-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-bottom: 18px;
}

.tag.tech {
  color: $color-accent;
  box-shadow: inset 0 0 0 1px rgba($color-accent, 0.35);
}

.course-footer {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 12px;
  margin-top: auto;
  padding-top: 16px;
  border-top: 1px dashed $color-hairline;
}

.course-numbers {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.course-price {
  font-family: $font-display;
  font-size: 1.35rem;
  font-weight: 600;
  line-height: 1;
}

.course-rating {
  font-family: $font-mono;
  font-size: 0.72rem;
  color: $color-ink;

  .star {
    color: $color-accent;
  }
}

.rating-count {
  color: $color-faint;
}

.course-btn {
  padding: 10px 18px;
  font-size: 0.86rem;
}

.course-desc {
  margin: -4px 0 16px;
  color: $color-muted;
  font-size: 0.95rem;
  line-height: 1.6;
}

.course-card.is-featured {
  --lg-r: 30px 30px 30px 6px;

  @media (min-width: 640px) {
    grid-column: span 2;
    display: grid;
    grid-template-columns: minmax(0, 1.05fr) minmax(0, 1fr);

    .course-thumb-wrap {
      margin: 14px 0 14px 14px;
    }

    .course-thumb {
      height: 100%;
      aspect-ratio: auto;
      min-height: 280px;
      border-radius: 20px 20px 20px 4px;
    }

    .course-body {
      padding: 28px 30px 26px;
    }

    .course-title {
      font-size: 1.6rem;
      line-height: 1.15;
      -webkit-line-clamp: 3;
    }

    .course-price {
      font-size: 1.9rem;
    }
  }
}
</style>
