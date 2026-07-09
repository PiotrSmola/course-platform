<template>
  <div class="admin-dashboard">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Panel administratora</span>
        <h2>Zarządzanie platformą</h2>
      </div>

      <div class="tabs">
        <button type="button" :class="{ active: tab === 'users' }" @click="tab = 'users'">Użytkownicy</button>
        <button type="button" :class="{ active: tab === 'courses' }" @click="tab = 'courses'">Kursy</button>
        <button type="button" :class="{ active: tab === 'search' }" @click="tab = 'search'">Wyszukiwarka</button>
      </div>

      <section v-if="tab === 'users'" class="panel glass-card">
        <div v-if="usersLoading" class="loading">Ładowanie...</div>
        <table v-else class="data-table">
          <thead>
            <tr>
              <th>Email</th>
              <th>Imię i nazwisko</th>
              <th>Role</th>
              <th>Akcje</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in users" :key="user.id">
              <td>{{ user.email }}</td>
              <td>{{ user.firstName }} {{ user.lastName }}</td>
              <td>{{ user.roles.join(', ') || '—' }}</td>
              <td>
                <button
                  v-if="!user.roles.includes('Instructor')"
                  type="button"
                  class="btn btn-ghost"
                  @click="assignInstructor(user.id)"
                >
                  Nadaj Instructor
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <section v-else-if="tab === 'courses'" class="panel glass-card">
        <div v-if="coursesLoading" class="loading">Ładowanie...</div>
        <table v-else class="data-table">
          <thead>
            <tr>
              <th>Tytuł</th>
              <th>Status</th>
              <th>Instruktor</th>
              <th>Akcje</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="course in adminCourses" :key="course.id">
              <td>{{ course.title }}</td>
              <td>{{ statusLabel(course.status) }}</td>
              <td>{{ course.instructorName }}</td>
              <td class="actions-cell">
                <button
                  v-if="course.status !== CourseStatus.Published"
                  type="button"
                  class="btn btn-ghost"
                  @click="publish(course.id)"
                >
                  Opublikuj
                </button>
                <button
                  v-if="course.status !== CourseStatus.Hidden"
                  type="button"
                  class="btn btn-ghost danger"
                  @click="hide(course.id)"
                >
                  Ukryj
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <section v-else class="panel search-panel">
        <SearchAdminPanel />
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useAdminUsers, useAdminCourses, useAdminMutations } from '@/features/admin/composables/useAdmin'
import { CourseStatus } from '@/features/courses/types/course.types'
import SearchAdminPanel from '@/features/admin/components/SearchAdminPanel.vue'

type AdminTab = 'users' | 'courses' | 'search'
const tab = ref<AdminTab>('users')

const { data: usersData, isLoading: usersLoading } = useAdminUsers()
const { data: coursesData, isLoading: coursesLoading } = useAdminCourses()
const { assignRole, setCourseStatus } = useAdminMutations()

const users = computed(() => usersData.value ?? [])
const adminCourses = computed(() => coursesData.value?.items ?? [])

function statusLabel(status: CourseStatus) {
  switch (status) {
    case CourseStatus.Published: return 'Opublikowany'
    case CourseStatus.Hidden: return 'Ukryty'
    default: return 'Szkic'
  }
}

function assignInstructor(userId: string) {
  assignRole.mutate({ userId, role: 'Instructor' })
}

function publish(courseId: string) {
  setCourseStatus.mutate({ courseId, status: CourseStatus.Published })
}

function hide(courseId: string) {
  setCourseStatus.mutate({ courseId, status: CourseStatus.Hidden })
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.admin-dashboard {
  padding: calc($header-height + 40px) 0 80px;
}

.tabs {
  display: flex;
  gap: 8px;
  margin-bottom: 24px;

  button {
    padding: 10px 18px;
    border-radius: 999px;
    border: none;
    background: rgba(255, 255, 255, 0.04);
    color: $color-muted;
    cursor: pointer;
    font: inherit;

    &.active {
      background: rgba(245, 158, 11, 0.15);
      color: $color-ink;
    }
  }
}

.panel {
  padding: 24px;
  overflow-x: auto;
}

.search-panel {
  padding: 0;
  background: transparent;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;

  th, td {
    text-align: left;
    padding: 12px;
    border-bottom: 1px solid rgba(255, 255, 255, 0.06);
  }

  th {
    color: $color-muted;
    font-weight: 600;
  }
}

.actions-cell {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.danger {
  color: #f87171;
}

.loading {
  text-align: center;
  padding: 32px;
  color: $color-muted;
}
</style>