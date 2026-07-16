<template>
  <div class="admin-dashboard">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Panel administratora</span>
        <h2>Zarządzanie platformą</h2>
      </div>

      <div class="tabs">
        <button type="button" class="glass no-warp" :class="{ active: tab === 'users' }" @click="tab = 'users'">Użytkownicy</button>
        <button type="button" class="glass no-warp" :class="{ active: tab === 'courses' }" @click="tab = 'courses'">Kursy</button>
        <button type="button" class="glass no-warp" :class="{ active: tab === 'reviews' }" @click="tab = 'reviews'">Recenzje</button>
        <button type="button" class="glass no-warp" :class="{ active: tab === 'audit' }" @click="tab = 'audit'">Audit log</button>
        <button type="button" class="glass no-warp" :class="{ active: tab === 'search' }" @click="tab = 'search'">Wyszukiwarka</button>
      </div>

      <section v-if="tab === 'users'" class="panel glass-card">
        <div v-if="usersLoading" class="loading">Ładowanie...</div>
        <div v-else class="table-scroll">
        <table class="data-table">
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
        </div>
      </section>

      <section v-else-if="tab === 'courses'" class="panel glass-card">
        <div v-if="coursesLoading" class="loading">Ładowanie...</div>
        <div v-else class="table-scroll">
        <table class="data-table">
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
        </div>
      </section>

      <section v-else-if="tab === 'reviews'" class="panel glass-card">
        <div v-if="reviewsLoading" class="loading">Ładowanie...</div>
        <div v-else class="table-scroll">
          <table class="data-table">
            <thead>
              <tr>
                <th>Kurs</th>
                <th>Autor</th>
                <th>Ocena</th>
                <th>Komentarz</th>
                <th>Data</th>
                <th>Akcje</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="review in adminReviews" :key="review.id">
                <td>{{ review.courseTitle }}</td>
                <td>{{ review.authorName }}</td>
                <td>{{ review.rating }}/5</td>
                <td class="review-comment">{{ review.comment }}</td>
                <td>{{ formatDate(review.createdAt) }}</td>
                <td>
                  <button
                    type="button"
                    class="btn btn-ghost danger"
                    @click="removeReview(review.id)"
                  >
                    Usuń
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <section v-else-if="tab === 'audit'" class="panel glass-card">
        <div v-if="auditLogsLoading" class="loading">Ładowanie...</div>
        <div v-else>
          <div class="table-scroll">
            <table class="data-table">
              <thead>
                <tr>
                  <th>Data</th>
                  <th>Akcja</th>
                  <th>Encja</th>
                  <th>Admin</th>
                  <th>Szczegóły</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="log in auditLogs" :key="log.id">
                  <td>{{ formatDate(log.createdAt) }}</td>
                  <td>{{ log.action }}</td>
                  <td>{{ log.entityType }} ({{ log.entityId.slice(0, 8) }})</td>
                  <td>{{ log.adminEmail ?? '—' }}</td>
                  <td class="audit-details">{{ log.details }}</td>
                </tr>
              </tbody>
            </table>
          </div>
          <div class="pagination">
            <button
              type="button"
              class="btn btn-ghost"
              :disabled="auditPageNumber <= 1"
              @click="auditPageNumber--"
            >
              ← Poprzednia
            </button>
            <span>Strona {{ auditPageNumber }}</span>
            <button
              type="button"
              class="btn btn-ghost"
              :disabled="!auditLogsData || auditLogs.length < auditPageSize"
              @click="auditPageNumber++"
            >
              Następna →
            </button>
          </div>
        </div>
      </section>

      <section v-else class="panel search-panel">
        <SearchAdminPanel />
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useAdminUsers, useAdminCourses, useAdminReviews, useAdminAuditLogs, useAdminMutations } from '@/features/admin/composables/useAdmin'
import { CourseStatus } from '@/features/courses/types/course.types'
import SearchAdminPanel from '@/features/admin/components/SearchAdminPanel.vue'

type AdminTab = 'users' | 'courses' | 'reviews' | 'audit' | 'search'
const tab = ref<AdminTab>('users')

const { data: usersData, isLoading: usersLoading } = useAdminUsers()
const { data: coursesData, isLoading: coursesLoading } = useAdminCourses()
const { data: reviewsData, isLoading: reviewsLoading } = useAdminReviews()
const auditPageNumber = ref(1)
const auditPageSize = 50
const { data: auditLogsData, isLoading: auditLogsLoading } = useAdminAuditLogs(auditPageNumber, auditPageSize)
const { assignRole, setCourseStatus, removeReview: removeReviewMutation } = useAdminMutations()

const users = computed(() => usersData.value ?? [])
const adminCourses = computed(() => coursesData.value?.items ?? [])
const adminReviews = computed(() => reviewsData.value ?? [])
const auditLogs = computed(() => auditLogsData.value?.items ?? [])

function formatDate(value: string): string {
  return new Date(value).toLocaleString('pl-PL')
}

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

function removeReview(reviewId: string) {
  removeReviewMutation.mutate(reviewId)
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.admin-dashboard {
  padding: calc($header-height + 40px) 0 80px;
}

.tabs {
  display: flex;
  gap: 10px;
  margin-bottom: 24px;

  button {
    --lg-r: 999px;
    --lg-blur: 0px;
    padding: 10px 20px;
    border: none;
    background: none;
    color: $color-muted;
    cursor: pointer;
    font: inherit;
    font-weight: 500;
    transition: color 0.25s, transform 0.2s;

    &:hover {
      color: $color-ink;
      transform: translateY(-1px);
    }

    &.active {
      --lg-tint: rgba(245, 158, 11, 0.14);
      color: $color-ink;
      font-weight: 600;

      &::after {
        box-shadow:
          inset 2px 2px 1px -1px rgba(255, 255, 255, 0.6),
          inset -2px -2px 1px -1px rgba(255, 255, 255, 0.22),
          inset 0 0 0 1px rgba(251, 191, 36, 0.4),
          inset 0 -10px 18px -12px rgba(245, 158, 11, 0.5);
      }
    }
  }
}

.panel {
  padding: 24px;
}

.table-scroll {
  overflow-x: auto;
}

.search-panel {
  padding: 0;
  background: transparent;
}

.data-table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
  font-size: 0.9rem;

  th, td {
    text-align: left;
    padding: 13px 14px;
  }

  th {
    color: $color-muted;
    font-weight: 600;
    font-size: 0.8rem;
    letter-spacing: 0.05em;
    text-transform: uppercase;
    background: rgba(255, 255, 255, 0.05);
    box-shadow:
      inset 0 1px 1px rgba(255, 255, 255, 0.22),
      inset 0 -1px 0 rgba(255, 255, 255, 0.08);

    &:first-child { border-radius: 14px 0 0 14px; }
    &:last-child { border-radius: 0 14px 14px 0; }
  }

  td {
    box-shadow: inset 0 -1px 0 rgba(255, 255, 255, 0.05);
    transition: background 0.2s;
  }

  tbody tr:last-child td {
    box-shadow: none;
  }

  tbody tr:hover td {
    background: rgba(255, 255, 255, 0.045);
  }

  tbody td:first-child { border-radius: 12px 0 0 12px; }
  tbody td:last-child { border-radius: 0 12px 12px 0; }
}

.actions-cell {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.review-comment {
  max-width: 320px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.audit-details {
  max-width: 360px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;
  margin-top: 20px;

  span {
    color: $color-muted;
    font-size: 0.9rem;
  }
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