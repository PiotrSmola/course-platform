<template>
  <div class="wishlist-page">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Lista życzeń</span>
        <h2>Ulubione <em>kursy</em></h2>
      </div>

      <div v-if="!authStore.token" class="state-wrap">
        <EmptyState
          title="Zaloguj się, aby zobaczyć listę życzeń"
          description="Zapisuj kursy sercem i wracaj do nich później."
          action-label="Zaloguj się"
          action-to="Login"
        />
      </div>
      <div v-else-if="isLoading" class="state-wrap skeletons">
        <SkeletonBlock v-for="n in 3" :key="n" height="280px" />
      </div>
      <div v-else-if="isError" class="state-wrap">
        <ErrorState
          title="Nie udało się załadować listy życzeń"
          description="Sprawdź połączenie i spróbuj ponownie za chwilę."
          action-label="Przeglądaj katalog"
          action-to="Courses"
        />
      </div>
      <div v-else-if="!items.length" class="state-wrap">
        <EmptyState
          title="Lista życzeń jest pusta"
          description="Dodaj kursy sercem na stronie szczegółów kursu."
          action-label="Przeglądaj katalog"
          action-to="Courses"
        />
      </div>
      <div v-else class="courses-grid">
        <article v-for="item in items" :key="item.id" class="course-card glass-card">
          <CourseThumbnail class="course-thumb" :url="item.courseThumbnailUrl" :title="item.courseTitle" :seed="item.courseId" />
          <div class="course-info">
            <div class="card-meta">
              <span class="level">{{ levelLabel(item.courseLevel) }}</span>
              <span class="price">{{ item.price }} zł</span>
            </div>
            <h3>{{ item.courseTitle }}</h3>
            <div class="card-actions">
              <router-link class="btn btn-primary btn-pill" :to="{ name: 'CourseDetails', params: { id: item.courseId } }">
                Zobacz kurs
              </router-link>
              <button
                type="button"
                class="btn btn-ghost btn-pill"
                :disabled="removeMutation.isPending.value"
                @click="remove(item.courseId)"
              >
                Usuń
              </button>
            </div>
          </div>
        </article>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useWishlist, useRemoveFromWishlist } from '@/features/wishlist/composables/useWishlist'
import { CourseLevel } from '@/features/courses/types/course.types'
import CourseThumbnail from '@/shared/components/media/CourseThumbnail.vue'
import EmptyState from '@/shared/components/ui/EmptyState.vue'
import ErrorState from '@/shared/components/ui/ErrorState.vue'
import SkeletonBlock from '@/shared/components/ui/SkeletonBlock.vue'

const authStore = useAuthStore()
const { data, isLoading, isError } = useWishlist()
const removeMutation = useRemoveFromWishlist()
const items = computed(() => data.value ?? [])

function levelLabel(level: CourseLevel) {
  switch (level) {
    case CourseLevel.Beginner: return 'Początkujący'
    case CourseLevel.Intermediate: return 'Średni'
    case CourseLevel.Advanced: return 'Zaawansowany'
    default: return ''
  }
}

function remove(courseId: string) {
  removeMutation.mutate(courseId)
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.wishlist-page {
  padding: 32px 0 96px;
}

.section-head {
  margin-bottom: 40px;

  h2 {
    font-size: clamp(2.2rem, 4.4vw, 3.4rem);
    margin-top: 18px;
  }
}

.state-wrap {
  padding: 24px 0;
}

.skeletons {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 24px;
}

.courses-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 24px;
}

.course-card {
  --lg-blur: 10px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.course-thumb {
  position: relative;
  aspect-ratio: 16 / 9;
  margin: 12px 12px 0;
  border-radius: 14px 14px 14px 4px;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.08);
}

.course-info {
  padding: 16px 20px 20px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  flex: 1;

  h3 {
    font-size: 1.28rem;
    font-weight: 600;
    line-height: 1.2;
  }
}

.card-meta {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 12px;

  .level {
    @include mono-label;
    font-size: 0.64rem;
    color: $color-faint;
  }

  .price {
    font-family: $font-display;
    font-size: 1.35rem;
    font-weight: 600;
    line-height: 1;
  }
}

.card-actions {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
  margin-top: auto;
  padding-top: 16px;
  border-top: 1px dashed $color-hairline;

  .btn {
    padding: 10px 18px;
    font-size: 0.86rem;
  }
}
</style>
