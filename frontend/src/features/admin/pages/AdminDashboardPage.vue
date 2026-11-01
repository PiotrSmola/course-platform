<template>
  <div class="admin-dashboard">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Panel administratora</span>
        <h2>Zarządzanie <em>platformą</em></h2>
      </div>

      <div class="tabs-scroll">
        <div class="tabs glass">
          <button type="button" :class="{ active: tab === 'users' }" @click="tab = 'users'">Użytkownicy</button>
          <button type="button" :class="{ active: tab === 'courses' }" @click="tab = 'courses'">Kursy</button>
          <button type="button" :class="{ active: tab === 'reviews' }" @click="tab = 'reviews'">Recenzje</button>
          <button type="button" :class="{ active: tab === 'audit' }" @click="tab = 'audit'">Audit log</button>
          <button type="button" :class="{ active: tab === 'coupons' }" @click="tab = 'coupons'">Kupony</button>
          <button type="button" :class="{ active: tab === 'revenue' }" @click="tab = 'revenue'">Przychody</button>
          <button type="button" :class="{ active: tab === 'qa' }" @click="tab = 'qa'">Q&A</button>
          <button type="button" :class="{ active: tab === 'search' }" @click="tab = 'search'">Wyszukiwarka</button>
        </div>
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
              <td class="mono">{{ user.email }}</td>
              <td>{{ user.firstName }} {{ user.lastName }}</td>
              <td>
                <span v-if="!user.roles.length" class="muted">—</span>
                <span class="chips">
                  <span v-for="role in user.roles" :key="role" class="chip" :class="`role-${role.toLowerCase()}`">{{ role }}</span>
                </span>
              </td>
              <td>
                <button
                  v-if="!user.roles.includes('Instructor')"
                  type="button"
                  class="btn btn-ghost btn-sm"
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
              <td>
                <span
                  class="chip"
                  :class="{
                    'status-published': course.status === CourseStatus.Published,
                    'status-hidden': course.status === CourseStatus.Hidden
                  }"
                >{{ statusLabel(course.status) }}</span>
              </td>
              <td>{{ course.instructorName }}</td>
              <td class="actions-cell">
                <button
                  v-if="course.status !== CourseStatus.Published"
                  type="button"
                  class="btn btn-ghost btn-sm"
                  @click="publish(course.id)"
                >
                  Opublikuj
                </button>
                <button
                  v-if="course.status !== CourseStatus.Hidden"
                  type="button"
                  class="btn btn-sm danger"
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
                <td class="rating">{{ review.rating }}<span>/5</span></td>
                <td class="review-comment">{{ review.comment }}</td>
                <td class="mono">{{ formatDate(review.createdAt) }}</td>
                <td>
                  <button
                    type="button"
                    class="btn btn-sm danger"
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
                  <td class="mono">{{ formatDate(log.createdAt) }}</td>
                  <td><span class="chip">{{ log.action }}</span></td>
                  <td>{{ log.entityType }} <span class="mono muted">{{ log.entityId.slice(0, 8) }}</span></td>
                  <td class="mono">{{ log.adminEmail ?? '—' }}</td>
                  <td class="audit-details">{{ log.details }}</td>
                </tr>
              </tbody>
            </table>
          </div>
          <div class="pagination">
            <button
              type="button"
              class="btn btn-ghost btn-sm"
              :disabled="auditPageNumber <= 1"
              @click="auditPageNumber--"
            >
              ← Poprzednia
            </button>
            <span>Strona {{ auditPageNumber }}</span>
            <button
              type="button"
              class="btn btn-ghost btn-sm"
              :disabled="!auditLogsData || auditLogs.length < auditPageSize"
              @click="auditPageNumber++"
            >
              Następna →
            </button>
          </div>
        </div>
      </section>

      <section v-else-if="tab === 'coupons'" class="panel glass-card">
        <CouponsAdminPanel />
      </section>

      <section v-else-if="tab === 'revenue'" class="panel glass-card">
        <AdminRevenuePanel />
      </section>

      <section v-else-if="tab === 'qa'" class="panel glass-card">
        <QaModerationPanel :course-options="adminCourseOptions" />
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
import CouponsAdminPanel from '@/features/admin/components/CouponsAdminPanel.vue'
import AdminRevenuePanel from '@/features/analytics/components/AdminRevenuePanel.vue'
import QaModerationPanel from '@/features/lesson-discussion/components/QaModerationPanel.vue'

type AdminTab = 'users' | 'courses' | 'reviews' | 'audit' | 'coupons' | 'revenue' | 'qa' | 'search'
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
const adminCourseOptions = computed(() =>
  adminCourses.value.map((course) => ({ id: course.id, title: course.title }))
)

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
.admin-dashboard {
  padding: 24px 0 80px;
}

.tabs-scroll {
  margin: -8px -8px 22px;
  padding: 8px 8px 14px;
  overflow-x: auto;
  scrollbar-width: none;

  &::-webkit-scrollbar {
    display: none;
  }
}

.tabs {
  --lg-r: 999px;
  --lg-blur: 12px;
  --lg-tint: rgba(13, 15, 19, 0.3);
  display: inline-flex;
  gap: 2px;
  padding: 5px;

  button {
    padding: 9px 18px;
    border: none;
    border-radius: 999px;
    background: transparent;
    color: $color-muted;
    cursor: pointer;
    font: inherit;
    font-size: 0.88rem;
    font-weight: 500;
    white-space: nowrap;
    transition: background 0.25s, color 0.25s;

    &:hover {
      color: $color-ink;
    }

    &.active {
      color: $color-ink;
      background: rgba($color-ink, 0.12);
      box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.12);
    }
  }
}

.panel {
  --lg-blur: 12px;
  padding: 12px 24px 20px;

  &.glass-card:hover {
    transform: none;
  }

  @media (max-width: 560px) {
    padding: 8px 14px 16px;
  }
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
  border-collapse: collapse;
  font-size: 0.9rem;

  th,
  td {
    text-align: left;
    padding: 14px 14px;
    border-bottom: 1px solid $color-hairline;
    vertical-align: middle;

    &:first-child {
      padding-left: 4px;
    }
  }

  th {
    @include mono-label;
    padding-top: 16px;
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

.mono {
  font-family: $font-mono;
  font-size: 0.78rem;
  color: $color-muted;
}

.muted {
  color: $color-faint;
}

.chips {
  display: inline-flex;
  flex-wrap: wrap;
  gap: 6px;
}

.chip {
  @include tag;
  white-space: nowrap;

  &.role-admin {
    color: $color-coral;
    box-shadow: inset 0 0 0 1px rgba($color-coral, 0.4);
  }

  &.role-instructor,
  &.status-published {
    color: $color-accent;
    box-shadow: inset 0 0 0 1px rgba($color-accent, 0.35);
  }

  &.status-hidden {
    color: $color-coral;
    box-shadow: inset 0 0 0 1px rgba($color-coral, 0.4);
  }
}

.rating {
  font-family: $font-display;
  font-size: 1.05rem;
  font-weight: 600;

  span {
    font-family: $font-mono;
    font-size: 0.72rem;
    color: $color-faint;
  }
}

.actions-cell {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.btn-sm {
  padding: 7px 14px;
  font-size: 0.8rem;
  white-space: nowrap;
}

.btn.danger {
  --lg-tint: #{rgba($color-danger, 0.22)};

  &:hover {
    --lg-tint: #{rgba($color-danger, 0.32)};
  }
}

.review-comment {
  max-width: 320px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  color: $color-muted;
}

.audit-details {
  max-width: 360px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  font-family: $font-mono;
  font-size: 0.74rem;
  color: $color-muted;
}

.pagination {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-top: 8px;
  padding-top: 16px;
  border-top: 1px solid $color-hairline;

  span {
    @include mono-label;
    font-size: 0.64rem;
    color: $color-muted;
  }
}

.loading {
  @include mono-label;
  text-align: center;
  padding: 32px;
  color: $color-muted;
}
</style>
