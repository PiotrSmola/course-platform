<template>
  <aside class="course-sidebar" :class="{ 'is-open': isOpen }">
    <button
      type="button"
      class="course-sidebar__toggle"
      :aria-expanded="isOpen"
      aria-controls="course-program"
      aria-label="Pokaz lub ukryj program kursu"
      @click="isOpen = !isOpen"
    >
      <span>Program kursu</span>
      <span aria-hidden="true">{{ isOpen ? '−' : '+' }}</span>
    </button>
    <div id="course-program" class="course-sidebar__panel glass">
      <div class="course-sidebar__scroll">
        <h2>Program kursu</h2>
        <ol class="toc">
          <li v-for="(module, mIdx) in modules" :key="module.id" class="sidebar-module">
            <div class="module-head">
              <span class="module-no">{{ String(mIdx + 1).padStart(2, '0') }}</span>
              <h3>{{ module.title }}</h3>
            </div>
            <ol class="lessons">
              <li v-for="(lesson, lIdx) in module.lessons" :key="lesson.id">
                <router-link
                  v-if="!lesson.isLocked"
                  :to="{ name: 'Learning', params: { courseId, lessonId: lesson.id } }"
                  class="lesson-row"
                  :class="{ active: lesson.id === currentLessonId, completed: lesson.isCompleted }"
                  :aria-current="lesson.id === currentLessonId ? 'page' : undefined"
                  @click="isOpen = false"
                >
                  <span class="mark">
                    <svg v-if="lesson.isCompleted" width="12" height="12" viewBox="0 0 12 12" aria-label="Ukończona">
                      <polyline points="2 6.5 5 9.5 10 3" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" />
                    </svg>
                    <template v-else>{{ mIdx + 1 }}.{{ lIdx + 1 }}</template>
                  </span>
                  <span class="title">{{ lesson.title }}</span>
                  <span class="duration">{{ lesson.duration }}m</span>
                </router-link>
                <div
                  v-else
                  class="lesson-row locked"
                  :title="lesson.lockReason || 'Lekcja zablokowana'"
                >
                  <span class="mark">
                    <svg width="11" height="12" viewBox="0 0 11 12" aria-label="Zablokowana">
                      <rect x="1" y="5.5" width="9" height="6" rx="1.2" fill="none" stroke="currentColor" stroke-width="1.2" />
                      <path d="M3 5.5V3.8a2.5 2.5 0 0 1 5 0v1.7" fill="none" stroke="currentColor" stroke-width="1.2" />
                    </svg>
                  </span>
                  <span class="title">{{ lesson.title }}</span>
                  <span class="duration">{{ lesson.duration }}m</span>
                </div>
              </li>
            </ol>
          </li>
        </ol>
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { ModuleDto } from '@/features/courses/types/course.types'

defineProps<{
  courseId: string
  currentLessonId: string
  modules: ModuleDto[]
}>()

const isOpen = ref(false)
</script>

<style lang="scss" scoped>
.course-sidebar {
  align-self: start;
  position: sticky;
  top: calc(#{$header-height} + 40px);
  width: 100%;
  max-height: calc(100vh - #{$header-height} - 80px);
}

.course-sidebar__panel {
  --lg-r: #{$radius-card};
  --lg-blur: 12px;
  --lg-tint: rgba(13, 15, 19, 0.28);
  display: flex;
  flex-direction: column;
  max-height: calc(100vh - #{$header-height} - 80px);
  overflow: hidden;
}

.course-sidebar__scroll {
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 22px 18px 18px;

  h2 {
    @include mono-label;
    display: flex;
    align-items: center;
    gap: 12px;
    margin: 0 4px 6px;
    color: $color-accent;

    &::before {
      content: "";
      width: 28px;
      height: 1px;
      background: currentColor;
      opacity: 0.7;
    }
  }
}

.toc,
.lessons {
  list-style: none;
  margin: 0;
  padding: 0;
}

.sidebar-module {
  padding: 16px 0 10px;

  & + & {
    border-top: 1px solid $color-hairline;
  }
}

.module-head {
  display: grid;
  grid-template-columns: 34px minmax(0, 1fr);
  align-items: baseline;
  gap: 4px;
  margin: 0 4px 8px;

  h3 {
    margin: 0;
    font-family: $font-family;
    font-size: 0.86rem;
    font-weight: 600;
    line-height: 1.35;
    letter-spacing: 0;
    color: $color-ink;
  }
}

.module-no {
  font-family: $font-display;
  font-size: 1.15rem;
  font-weight: 600;
  line-height: 1;
  font-variant-numeric: lining-nums;
  color: $color-muted;
}

.lessons {
  display: flex;
  flex-direction: column;
}

.lesson-row {
  position: relative;
  display: grid;
  grid-template-columns: 34px minmax(0, 1fr) auto;
  gap: 4px;
  align-items: baseline;
  padding: 7px 8px 7px 4px;
  border-radius: 2px $radius-xs $radius-xs 2px;
  text-decoration: none;
  color: $color-muted;
  font-size: 0.84rem;
  line-height: 1.4;
  transition: background 0.2s, color 0.2s;

  .mark {
    font-family: $font-mono;
    font-size: 0.66rem;
    color: $color-faint;

    svg {
      display: block;
      transform: translateY(1px);
      color: $color-accent;
    }
  }

  .duration {
    font-family: $font-mono;
    font-size: 0.68rem;
    font-variant-numeric: tabular-nums;
    color: $color-faint;
  }
}

a.lesson-row {
  &:hover {
    color: $color-ink;
    background: rgba($color-ink, 0.04);
  }

  &.active {
    color: $color-ink;
    background: rgba($color-ink, 0.06);
    box-shadow: inset 2px 0 0 $color-accent;

    .mark {
      color: $color-accent;
    }
  }
}

.lesson-row.locked {
  cursor: not-allowed;
  color: $color-faint;

  .mark svg {
    color: $color-faint;
  }
}

.course-sidebar__toggle {
  display: none;
}

@media (max-width: 880px) {
  .course-sidebar {
    position: static;
    top: auto;
    width: 100%;
    max-height: none;
  }

  .course-sidebar__toggle {
    @include liquid-glass;
    @include mono-label;
    --lg-r: 999px;
    --lg-blur: 12px;
    --lg-tint: rgba(13, 15, 19, 0.28);
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 100%;
    min-height: 48px;
    padding: 0 20px;
    border: 0;
    color: $color-ink;
    background: transparent;
    cursor: pointer;
  }

  .course-sidebar__panel {
    display: none;
    max-height: none;
    margin-top: 10px;
  }

  .course-sidebar.is-open .course-sidebar__panel {
    display: flex;
    max-height: min(60vh, 520px);
  }
}
</style>
