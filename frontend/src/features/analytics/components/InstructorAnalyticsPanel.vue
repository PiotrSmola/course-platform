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
        <div class="stat-card glass-card">
          <span class="stat-value">{{ formatCurrency(displayStats.totalRevenue) }}</span>
          <span class="stat-label">Przychód</span>
        </div>
        <div class="stat-card glass-card">
          <span class="stat-value">{{ displayStats.averageCompletionRate }}%</span>
          <span class="stat-label">Ukończenie kursu</span>
        </div>
      </div>

      <div v-for="course in visibleCourses" :key="course.courseId" class="course-block glass-card">
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
                <td>{{ row.orderIndex }}</td>
                <td>
                  <strong>{{ row.lessonTitle }}</strong>
                  <div class="muted">{{ row.moduleTitle }}</div>
                </td>
                <td>{{ row.reachedCount }}</td>
                <td>{{ row.completedCount }}</td>
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
@use "@/assets/styles/abstracts/variables" as *;

.analytics-panel {
  display: flex;
  flex-direction: column;
  gap: 20px;
  margin-top: 24px;
}

.analytics-head {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;

  h3 {
    margin: 0;
  }

  select {
    border-radius: 12px;
    border: 1px solid rgba(255, 255, 255, 0.12);
    background: rgba(255, 255, 255, 0.04);
    color: inherit;
    padding: 10px 12px;
  }
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 12px;
}

.stat-card {
  padding: 18px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.stat-value {
  font-size: 1.4rem;
  font-weight: 700;
}

.stat-label {
  color: $color-muted;
  font-size: 0.85rem;
}

.course-block {
  padding: 18px;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.course-block__head {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;

  h4 {
    margin: 0;
  }
}

.meta {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  color: $color-muted;
  font-size: 0.85rem;
}

.table-scroll {
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;

  th,
  td {
    text-align: left;
    padding: 10px 8px;
    border-bottom: 1px solid rgba(255, 255, 255, 0.08);
    vertical-align: top;
  }
}

.muted {
  color: $color-muted;
  font-size: 0.8rem;
}

.dropoff {
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 120px;
}

.bar {
  height: 6px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.08);
  overflow: hidden;
}

.fill {
  height: 100%;
  background: linear-gradient(90deg, #f87171, #fbbf24);
}

.state {
  color: $color-muted;
}
</style>
