<template>
  <section class="analytics-panel">
    <div class="analytics-head">
      <h3>Analytics</h3>
      <select v-model="selectedCourseId">
        <option value="">Wszystkie kursy</option>
        <option v-for="course in courseOptions" :key="course.courseId" :value="course.courseId">
          {{ course.title }}
        </option>
      </select>
    </div>

    <div v-if="isLoading" class="state">Ładowanie analytics...</div>
    <div v-else-if="isError" class="state">Nie udało się załadować analytics.</div>
    <template v-else-if="analytics">
      <div class="stats-grid">
        <div class="stat-card">
          <span class="stat-value">{{ formatCurrency(displayStats.totalRevenue) }}</span>
          <span class="stat-label">Przychód</span>
        </div>
        <div class="stat-card">
          <span class="stat-value">{{ displayStats.averageCompletionRate }}%</span>
          <span class="stat-label">Ukończenie kursu</span>
        </div>
      </div>

      <div v-for="course in visibleCourses" :key="course.courseId" class="course-block glass">
        <div class="course-block__head">
          <h4>{{ course.title }}</h4>
          <div class="meta">
            <span>{{ course.enrollmentCount }} enrolled</span>
            <span>{{ formatCurrency(course.revenue) }}</span>
            <span>{{ course.completionRate }}% ukończeń</span>
          </div>
        </div>

        <div class="table-scroll">
          <table class="data-table">
            <thead>
              <tr>
                <th>#</th>
                <th>Lekcja</th>
                <th>Dotarło</th>
                <th>Ukończyło</th>
                <th>Drop-off</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in course.dropOff" :key="row.lessonId">
                <td class="order">{{ String(row.orderIndex).padStart(2, '0') }}</td>
                <td>
                  <div class="lesson-name">{{ row.lessonTitle }}</div>
                  <div class="muted">{{ row.moduleTitle }}</div>
                </td>
                <td class="num">{{ row.reachedCount }}</td>
                <td class="num">{{ row.completedCount }}</td>
                <td>
                  <div class="dropoff">
                    <span>{{ row.dropOffPercent }}%</span>
                    <div class="bar">
                      <div class="fill" :style="{ width: `${Math.min(row.dropOffPercent, 100)}%` }"></div>
                    </div>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>
  </section>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useInstructorAnalytics } from '@/features/analytics/composables/useAnalytics'

const selectedCourseId = ref('')
const { data, isLoading, isError } = useInstructorAnalytics()

const analytics = computed(() => data.value)
const courseOptions = computed(() => analytics.value?.courses ?? [])
const visibleCourses = computed(() => {
  if (!analytics.value) return []
  if (!selectedCourseId.value) return analytics.value.courses
  return analytics.value.courses.filter((c) => c.courseId === selectedCourseId.value)
})

const displayStats = computed(() => {
  const courses = visibleCourses.value
  if (courses.length === 0) {
    return { totalRevenue: 0, averageCompletionRate: 0 }
  }

  return {
    totalRevenue: courses.reduce((sum, course) => sum + course.revenue, 0),
    averageCompletionRate: Math.round(
      (courses.reduce((sum, course) => sum + course.completionRate, 0) / courses.length) * 100
    ) / 100
  }
})

function formatCurrency(value: number) {
  return new Intl.NumberFormat('pl-PL', {
    style: 'currency',
    currency: 'PLN',
    maximumFractionDigits: 0
  }).format(value)
}
</script>

<style lang="scss" scoped>
.analytics-panel {
  display: flex;
  flex-direction: column;
  gap: 28px;
}

.analytics-head {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;

  h3 {
    margin: 0;
    font-size: 1.35rem;
    font-weight: 600;
  }

  select {
    min-width: 240px;
    max-width: 100%;
    height: 42px;
    padding: 0 14px;
    border: none;
    border-radius: $radius-md;
    background: rgba(13, 15, 19, 0.45);
    box-shadow: inset 0 0 0 1px $color-hairline;
    color: $color-ink;
    font: inherit;
    font-size: 0.9rem;
    transition: box-shadow 0.2s;

    &::placeholder {
      color: $color-faint;
    }

    &:focus {
      outline: none;
      box-shadow: inset 0 0 0 1px $color-hairline, 0 0 0 2px rgba($color-accent, 0.6);
    }
  }
}

.stats-grid {
  display: flex;
  flex-wrap: wrap;
  overflow: hidden;
  border-top: 1px solid $color-hairline;
  border-bottom: 1px solid $color-hairline;
}

.stat-card {
  flex: 1 1 200px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin: -1px 0 0 -1px;
  padding: 24px 22px 20px;
  border-top: 1px solid $color-hairline;
  border-left: 1px solid $color-hairline;
}

.stat-value {
  font-family: $font-display;
  font-size: clamp(1.9rem, 3vw, 2.5rem);
  font-weight: 600;
  line-height: 1;
  letter-spacing: -0.015em;
  font-variant-numeric: lining-nums;
  color: $color-ink;
  white-space: nowrap;
}

.stat-label {
  @include mono-label;
  font-size: 0.62rem;
  color: $color-faint;
}

.course-block {
  --lg-r: #{$radius-card};
  --lg-blur: 12px;
  --lg-tint: rgba(13, 15, 19, 0.28);
  padding: 22px 24px 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;

  @media (max-width: 560px) {
    padding: 18px 14px 8px;
  }
}

.course-block__head {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: 8px 16px;
  flex-wrap: wrap;
  padding-bottom: 10px;

  h4 {
    margin: 0;
    font-size: 1.15rem;
    font-weight: 600;
  }
}

.meta {
  @include mono-label;
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  font-size: 0.62rem;
  color: $color-faint;

  span + span::before {
    content: "/";
    margin-right: 10px;
    color: $color-hairline;
  }
}

.table-scroll {
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;

  th,
  td {
    text-align: left;
    padding: 13px 14px;
    border-bottom: 1px solid $color-hairline;
    vertical-align: middle;

    &:first-child {
      padding-left: 4px;
    }
  }

  th {
    @include mono-label;
    padding-top: 14px;
    padding-bottom: 12px;
    font-size: 0.62rem;
    color: $color-faint;
    white-space: nowrap;
  }

  tbody tr:last-child td {
    border-bottom: none;
  }

  tbody tr {
    transition: background 0.2s;

    &:hover {
      background: rgba($color-ink, 0.03);
    }
  }
}

.data-table td {
  vertical-align: top;
}

.order {
  font-family: $font-display;
  font-size: 1rem;
  font-weight: 600;
  color: $color-faint;
}

.num {
  font-family: $font-mono;
  font-size: 0.82rem;
  font-variant-numeric: tabular-nums;
}

.lesson-name {
  font-weight: 500;
}

.muted {
  margin-top: 2px;
  font-family: $font-mono;
  font-size: 0.68rem;
  color: $color-faint;
}

.dropoff {
  display: grid;
  grid-template-columns: minmax(80px, 1fr) 44px;
  align-items: center;
  gap: 12px;
  min-width: 140px;

  span {
    order: 2;
    font-family: $font-mono;
    font-size: 0.78rem;
    font-variant-numeric: tabular-nums;
    text-align: right;
  }
}

.bar {
  height: 4px;
  border-radius: 2px;
  background: rgba($color-ink, 0.1);
  overflow: hidden;
}

.fill {
  height: 100%;
  border-radius: 2px;
  background: $color-accent;
}

.state {
  @include mono-label;
  color: $color-muted;
}
</style>
