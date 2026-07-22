<template>
  <div class="wishlist-page">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Lista życzeń</span>
        <h2>Ulubione kursy</h2>
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
          <CourseThumbnail class="course-thumb" :url="item.courseThumbnailUrl" />
          <div class="course-info">
            <div class="card-meta">
              <span class="level">{{ levelLabel(item.courseLevel) }}</span>
              <span class="price">{{ item.price }} zł</span>
            </div>
            <h3>{{ item.courseTitle }}</h3>
            <div class="card-actions">
              <router-link class="btn btn-primary" :to="{ name: 'CourseDetails', params: { id: item.courseId } }">
                Zobacz kurs
              </router-link>
              <button
                type="button"
                class="btn btn-ghost"
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

.wishlist-page {
  padding: calc($header-height + 40px) 0 80px;
}

.section-head {
  margin-bottom: 40px;

  h2 {
    font-size: clamp(1.8rem, 3vw, 2.4rem);
    margin-top: 8px;
  }
}

.state-wrap {
  padding: 40px 0;
}

.skeletons {
  display: grid;
  gap: 24px;
}

.courses-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 24px;
}

.course-card {
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.course-thumb {
  width: 100%;
  aspect-ratio: 16/10;
}

.course-info {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  flex: 1;
}

.card-meta {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  font-size: 0.85rem;
  color: $color-muted;
}

.card-actions {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  margin-top: auto;
}
</style>
