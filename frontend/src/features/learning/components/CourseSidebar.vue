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
      <span aria-hidden="true">{{ isOpen ? '-' : '+' }}</span>
    </button>
    <div id="course-program" class="course-sidebar__panel glass">
      <div class="course-sidebar__scroll">
        <h2>Program kursu</h2>
        <div v-for="(module, mIdx) in modules" :key="module.id" class="sidebar-module">
          <h3>{{ mIdx + 1 }}. {{ module.title }}</h3>
          <ul>
            <li v-for="lesson in module.lessons" :key="lesson.id">
              <router-link
                v-if="!lesson.isLocked"
                :to="{ name: 'Learning', params: { courseId, lessonId: lesson.id } }"
                :class="{ active: lesson.id === currentLessonId, completed: lesson.isCompleted }"
                @click="isOpen = false"
              >
                <span class="icon">{{ lesson.isCompleted ? '✓' : '▶' }}</span>
                <span class="title">{{ lesson.title }}</span>
                <span class="duration">{{ lesson.duration }}m</span>
              </router-link>
              <div
                v-else
                class="locked"
                :title="lesson.lockReason || 'Lekcja zablokowana'"
              >
                <span class="icon">🔒</span>
                <span class="title">{{ lesson.title }}</span>
                <span class="duration">{{ lesson.duration }}m</span>
              </div>
            </li>
          </ul>
        </div>
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
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.course-sidebar {
  align-self: start;
  position: sticky;
  top: calc(#{$header-height} + 40px);
  max-height: calc(100vh - #{$header-height} - 80px);
  width: 100%;
}

.course-sidebar__panel {
  --lg-r: 20px;
  --lg-blur: 0px;
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
  padding: 20px;

  h2 {
    font-size: 1rem;
    margin-bottom: 16px;
  }
}

.sidebar-module {
  margin-bottom: 16px;

  h3 {
    font-size: 0.85rem;
    color: $color-muted;
    margin-bottom: 8px;
  }

  ul {
    list-style: none;
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  a,
  .locked {
    display: grid;
    grid-template-columns: auto 1fr auto;
    gap: 8px;
    align-items: center;
    padding: 8px 10px;
    border-radius: 10px;
    text-decoration: none;
    color: $color-muted;
    font-size: 0.85rem;
    transition: background 0.2s, color 0.2s;
  }

  a {
    &:hover,
    &.active {
      background: rgba(255, 255, 255, 0.06);
      color: $color-ink;
    }

    &.completed .icon {
      color: #4ade80;
    }
  }

  .locked {
    opacity: 0.55;
    cursor: not-allowed;
  }

  .icon {
    color: $color-gold;
    font-size: 0.75rem;
  }

  .duration {
    color: $color-faint;
    font-size: 0.75rem;
  }
}
.course-sidebar {
  width: 320px;
}

.course-sidebar__panel {
  --lg-blur: 12px;
}

.course-sidebar__scroll h2 {
  font-size: 1.125rem;
}

.sidebar-module h3 {
  font-size: 0.975rem;
}

.sidebar-module a,
.sidebar-module .locked {
  background: rgba(255, 255, 255, 0.035);
  -webkit-backdrop-filter: blur(8px) saturate(150%);
  backdrop-filter: blur(8px) saturate(150%);
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
    --lg-r: 16px;
    --lg-blur: 12px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 100%;
    min-height: 52px;
    padding: 0 16px;
    border: 0;
    color: $color-ink;
    background: transparent;
    font: inherit;
    font-weight: 700;
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
