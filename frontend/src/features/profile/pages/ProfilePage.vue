<template>
  <div class="profile-page">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Moje konto</span>
        <h2>Profil <em>użytkownika</em></h2>
      </div>

      <div v-if="profile.isLoading.value" class="loading">Ładowanie...</div>
      <div v-else-if="profile.error.value" class="error">Nie udało się załadować profilu</div>
      <div v-else-if="profile.data.value" class="profile-grid">
        <div class="profile-card glass-card">
          <div class="profile-header">
            <div class="avatar">
              <span>{{ profile.data.value.firstName.charAt(0) }}</span>
            </div>
            <div class="profile-meta">
              <h3>{{ profile.data.value.firstName }} {{ profile.data.value.lastName }}</h3>
              <p class="email">{{ profile.data.value.email }}</p>
              <p class="joined">
                Dołączono {{ new Date(profile.data.value.createdAt).toLocaleDateString('pl-PL') }}
              </p>
            </div>
          </div>

          <form @submit="onSubmit" class="profile-form">
            <div class="form-row">
              <div class="form-group">
                <label for="firstName">Imię</label>
                <input
                  id="firstName"
                  v-model="firstName"
                  type="text"
                  class="form-input"
                  :class="{ error: errors.firstName }"
                />
                <span v-if="errors.firstName" class="error-text">{{ errors.firstName }}</span>
              </div>
              <div class="form-group">
                <label for="lastName">Nazwisko</label>
                <input
                  id="lastName"
                  v-model="lastName"
                  type="text"
                  class="form-input"
                  :class="{ error: errors.lastName }"
                />
                <span v-if="errors.lastName" class="error-text">{{ errors.lastName }}</span>
              </div>
            </div>
            <button
              type="submit"
              class="btn btn-primary"
              :disabled="!meta.valid || update.isPending.value"
            >
              {{ update.isPending.value ? 'Zapisywanie...' : 'Zapisz zmiany' }}
            </button>
          </form>
        </div>

        <div class="stats-card glass-card">
          <h3 class="card-label">Statystyki nauki</h3>
          <div class="stats-grid">
            <div class="stat-item">
              <span class="stat-value">{{ stats.totalEnrollments }}</span>
              <span class="stat-label">Zapisanych kursów</span>
            </div>
            <div class="stat-item">
              <span class="stat-value">{{ stats.completedCourses }}</span>
              <span class="stat-label">Ukończonych kursów</span>
            </div>
            <div class="stat-item">
              <span class="stat-value">{{ stats.totalLessonsCompleted }}</span>
              <span class="stat-label">Ukończonych lekcji</span>
            </div>
            <div class="stat-item">
              <span class="stat-value">{{ Math.round(stats.averageProgressPercentage) }}%</span>
              <span class="stat-label">Średni postęp</span>
            </div>
            <div class="stat-item">
              <span class="stat-value">{{ formatTime(stats.totalLearningTimeSeconds) }}</span>
              <span class="stat-label">Czas nauki</span>
            </div>
            <div class="stat-item">
              <span class="stat-value">{{ stats.certificatesEarned }}</span>
              <span class="stat-label">Certyfikatów</span>
            </div>
          </div>
        </div>

        <div class="purchases-card glass-card">
          <h3 class="card-label">Ostatnie zakupy</h3>
          <div v-if="purchases.isLoading.value" class="purchases-empty">Ładowanie...</div>
          <div v-else-if="!purchases.data.value?.length" class="purchases-empty">
            Nie masz jeszcze żadnych zakupionych kursów.
          </div>
          <div v-else class="purchases-list">
            <component :is="purchase.courseId ? 'router-link' : 'div'"
              v-for="purchase in purchases.data.value"
              :key="purchase.kind + purchase.id"
              class="purchase-item"
              :to="purchase.courseId ? { name: 'CourseDetails', params: { id: purchase.courseId } } : undefined"
            >
              <div class="purchase-info">
                <span class="purchase-kind">{{ purchaseKindLabel(purchase.kind) }}</span>
                <span class="purchase-title">{{ purchase.courseTitle }}</span>
                <span class="purchase-date">
                  {{ new Date(purchase.completedAt).toLocaleDateString('pl-PL') }}
                </span>
                <span v-if="purchase.recipientEmail" class="purchase-date">Dla: {{ purchase.recipientEmail }}</span>
                <span v-if="purchase.giftCode" class="purchase-date">Kod: {{ purchase.giftCode }}</span>
              </div>
              <span class="purchase-amount">{{ formatPrice(purchase.amount, purchase.currency) }}</span>
            </component>
          </div>
        </div>

        <div class="certificates-card glass-card">
          <h3 class="card-label">Certyfikaty</h3>
          <div v-if="certificates.isLoading.value" class="purchases-empty">Ładowanie...</div>
          <div v-else-if="!certificates.data.value?.length" class="purchases-empty">
            Ukończ kurs w 100%, aby otrzymać certyfikat.
          </div>
          <div v-else class="purchases-list">
            <div v-for="cert in certificates.data.value" :key="cert.id" class="certificate-item">
              <div class="purchase-info">
                <span class="purchase-title">{{ cert.courseTitle }}</span>
                <span class="purchase-date">
                  {{ cert.number }} · {{ new Date(cert.issuedAt).toLocaleDateString('pl-PL') }}
                </span>
              </div>
              <div class="certificate-actions">
                <router-link
                  class="btn btn-ghost btn-sm"
                  :to="{ name: 'CertificateVerify', params: { number: cert.number } }"
                >
                  Weryfikuj
                </router-link>
                <button
                  type="button"
                  class="btn btn-primary btn-sm"
                  :disabled="downloadCertificate.isPending.value"
                  @click="downloadCertificate.mutate(cert.id)"
                >
                  Pobierz PDF
                </button>
              </div>
            </div>
          </div>
        </div>

        <div class="danger-card glass-card">
          <h3 class="card-label">Strefa niebezpieczna</h3>
          <p>Usunięcie konta jest nieodwracalne. Wszystkie Twoje dane, postępy i recenzje zostaną trwale usunięte.</p>
          <button class="btn btn-danger" @click="showDeleteModal = true">
            Usuń konto
          </button>
        </div>
      </div>
    </div>

    <div v-if="showDeleteModal" class="modal-overlay" @click.self="showDeleteModal = false">
      <div class="modal glass-card">
        <h3>Czy na pewno chcesz usunąć konto?</h3>
        <p>Tej operacji nie można cofnąć. Wszystkie dane zostaną trwale usunięte.</p>
        <div class="modal-actions">
          <button class="btn btn-ghost" @click="showDeleteModal = false">Anuluj</button>
          <button
            class="btn btn-danger"
            :disabled="deleteAccount.isPending.value"
            @click="confirmDelete"
          >
            {{ deleteAccount.isPending.value ? 'Usuwanie...' : 'Tak, usuń konto' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useProfile } from '@/features/profile/composables/useProfile'
import { useMyPaymentHistory } from '@/features/payments/composables/usePayments'
import { useMyCertificates, useDownloadCertificate } from '@/features/certificates/composables/useCertificates'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { updateProfileSchema } from '@/features/profile/schemas/profile.schema'

const { profile, update, deleteAccount } = useProfile()
const purchases = useMyPaymentHistory()
const certificates = useMyCertificates()
const downloadCertificate = useDownloadCertificate()
const authStore = useAuthStore()
const router = useRouter()
const showDeleteModal = ref(false)

const { handleSubmit, defineField, errors, meta, resetForm } = useForm({
  validationSchema: toTypedSchema(updateProfileSchema)
})

const [firstName] = defineField('firstName')
const [lastName] = defineField('lastName')

watch(
  () => profile.data.value,
  (data) => {
    if (data) {
      resetForm({
        values: {
          firstName: data.firstName,
          lastName: data.lastName
        }
      })
      authStore.setUser({
        id: data.id,
        email: data.email,
        firstName: data.firstName,
        lastName: data.lastName,
        roles: authStore.user?.roles ?? []
      })
    }
  },
  { immediate: true }
)

const stats = computed(() => {
  return profile.data.value?.statistics ?? {
    totalEnrollments: 0,
    completedCourses: 0,
    totalLessonsCompleted: 0,
    totalLessonsAvailable: 0,
    averageProgressPercentage: 0,
    totalLearningTimeSeconds: 0,
    certificatesEarned: 0,
    lastActivityAt: null
  }
})

function formatPrice(amount: number, currency: string): string {
  return new Intl.NumberFormat('pl-PL', { style: 'currency', currency: currency.toUpperCase() }).format(amount)
}

function purchaseKindLabel(kind: 'course' | 'subscription' | 'gift'): string {
  switch (kind) {
    case 'subscription': return 'Subskrypcja All-access'
    case 'gift': return 'Prezent'
    default: return 'Kurs'
  }
}

function formatTime(seconds: number): string {
  const hours = Math.floor(seconds / 3600)
  const mins = Math.floor((seconds % 3600) / 60)
  if (hours > 0) return `${hours}h ${mins}m`
  return `${mins}m`
}

const onSubmit = handleSubmit((values) => {
  update.mutate(values)
})

function confirmDelete() {
  deleteAccount.mutate(undefined, {
    onSuccess: () => {
      authStore.logout()
      router.push({ name: 'Home' })
    }
  })
}
</script>

<style lang="scss" scoped>
.profile-page {
  padding: 24px 0 80px;
}

.profile-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 24px;

  > .profile-card,
  > .stats-card,
  > .danger-card {
    grid-column: 1 / -1;
  }

  @media (max-width: 860px) {
    grid-template-columns: minmax(0, 1fr);
  }
}

.glass-card {
  --lg-blur: 10px;

  &:hover {
    transform: none;
  }
}

.profile-card,
.stats-card,
.purchases-card,
.certificates-card,
.danger-card {
  padding: 28px;

  @media (max-width: 560px) {
    padding: 22px 18px;
  }
}

.card-label {
  @include mono-label;
  display: flex;
  align-items: center;
  gap: 12px;
  margin: 0 0 20px;
  color: $color-faint;

  &::before {
    content: "";
    width: 28px;
    height: 1px;
    background: currentColor;
    opacity: 0.7;
  }
}

.profile-header {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 28px;
  padding-bottom: 24px;
  border-bottom: 1px solid $color-hairline;

  .avatar {
    width: 72px;
    height: 72px;
    border-radius: 50%;
    background: $color-accent;
    display: flex;
    align-items: center;
    justify-content: center;
    color: $color-on-accent;
    font-family: $font-display;
    font-size: 1.9rem;
    font-weight: 600;
    flex-shrink: 0;
  }

  .profile-meta {
    min-width: 0;

    h3 {
      font-size: 1.5rem;
      font-weight: 600;
      margin-bottom: 6px;
    }

    .email {
      font-size: 0.92rem;
      color: $color-muted;
      margin-bottom: 4px;
      overflow-wrap: anywhere;
    }

    .joined {
      @include mono-label;
      font-size: 0.62rem;
      color: $color-faint;
    }
  }
}

.profile-form {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 20px;

  .form-row {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 16px;
    width: 100%;

    @media (max-width: 560px) {
      grid-template-columns: 1fr;
    }
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: 8px;

    label {
      @include mono-label;
      font-size: 0.64rem;
      color: $color-muted;
    }
  }

  .form-input {
    height: 46px;
    padding: 0 16px;
    border: none;
    border-radius: $radius-md;
    background: rgba(13, 15, 19, 0.45);
    box-shadow: inset 0 0 0 1px $color-hairline;
    color: $color-ink;
    font: inherit;
    font-size: 0.94rem;
    outline: none;
    transition: box-shadow 0.2s;

    &:focus {
      box-shadow: inset 0 0 0 1px $color-hairline, 0 0 0 2px rgba($color-accent, 0.6);
    }

    &.error {
      box-shadow: inset 0 0 0 1px rgba($color-danger, 0.7);
    }
  }

  .error-text {
    font-size: 0.8rem;
    color: $color-danger;
  }
}

.stats-grid {
  display: flex;
  flex-wrap: wrap;
  overflow: hidden;
  border-top: 1px solid $color-hairline;
  border-bottom: 1px solid $color-hairline;
}

.stat-item {
  flex: 1 1 150px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin: -1px 0 0 -1px;
  padding: 22px 20px 20px;
  border-top: 1px solid $color-hairline;
  border-left: 1px solid $color-hairline;

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
}

.purchases-empty {
  font-size: 0.9rem;
  color: $color-muted;
}

.purchases-list {
  display: flex;
  flex-direction: column;
}

.purchase-item,
.certificate-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 14px 0;
  border-bottom: 1px solid $color-hairline;
  text-decoration: none;
  color: inherit;

  &:first-child {
    padding-top: 0;
  }

  &:last-child {
    border-bottom: none;
    padding-bottom: 0;
  }
}

.purchase-item {
  transition: color 0.2s;

  &:hover .purchase-title {
    color: $color-accent;
  }
}

.purchase-info {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 5px;
  min-width: 0;
}

.purchase-title {
  font-size: 0.96rem;
  font-weight: 500;
  color: $color-ink;
  transition: color 0.2s;
}

.purchase-kind {
  @include tag;
  color: $color-sky;
  box-shadow: inset 0 0 0 1px rgba($color-sky, 0.35);
}

.purchase-date {
  font-family: $font-mono;
  font-size: 0.7rem;
  color: $color-faint;
  overflow-wrap: anywhere;
}

.purchase-amount {
  flex-shrink: 0;
  font-family: $font-display;
  font-size: 1.2rem;
  font-weight: 600;
  line-height: 1;
  color: $color-ink;
}

.certificate-actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 8px;
  flex-shrink: 0;
}

.btn-sm {
  padding: 8px 14px;
  font-size: 0.8rem;
}

.danger-card {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 4px 24px;

  .card-label {
    flex-basis: 100%;
    margin-bottom: 8px;
    color: $color-danger;
  }

  p {
    flex: 1 1 320px;
    max-width: 62ch;
    font-size: 0.9rem;
    color: $color-muted;
  }
}

.btn-danger {
  --lg-tint: #{rgba($color-danger, 0.22)};

  &:hover {
    --lg-tint: #{rgba($color-danger, 0.32)};
  }
}

.danger-card .btn,
.modal-actions .btn {
  padding: 11px 22px;
  font-size: 0.88rem;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 200;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(8, 9, 12, 0.78);
}

.modal {
  --lg-blur: 18px;
  --lg-tint: rgba(13, 15, 19, 0.6);
  max-width: 440px;
  width: 100%;
  margin: 20px;
  padding: 28px;

  h3 {
    font-size: 1.3rem;
    font-weight: 600;
    margin-bottom: 10px;
  }

  p {
    font-size: 0.9rem;
    color: $color-muted;
    margin-bottom: 24px;
  }
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.loading,
div.error {
  @include mono-label;
  text-align: center;
  padding: 60px;
  color: $color-muted;
}
</style>
