<template>
  <div class="profile-page">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Moje konto</span>
        <h2>Profil użytkownika</h2>
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
          <h3>Statystyki nauki</h3>
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
          <h3>Ostatnie zakupy</h3>
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
          <h3>Certyfikaty</h3>
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
          <h3>Strefa niebezpieczna</h3>
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
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.profile-page {
  padding: calc($header-height + 40px) 0 80px;
}

.profile-grid {
  display: grid;
  gap: 24px;
}

.profile-card,
.stats-card,
.danger-card {
  padding: 28px;
}

.profile-header {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 28px;

  .avatar {
    width: 72px;
    height: 72px;
    border-radius: 50%;
    background: $color-grad;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #0a0e17;
    font-size: 1.6rem;
    font-weight: 700;
    flex-shrink: 0;
  }

  .profile-meta {
    h3 {
      font-size: 1.25rem;
      margin-bottom: 4px;
    }

    .email {
      font-size: 0.9rem;
      color: $color-muted;
      margin-bottom: 4px;
    }

    .joined {
      font-size: 0.82rem;
      color: $color-faint;
    }
  }
}

.profile-form {
  display: flex;
  flex-direction: column;
  gap: 20px;

  .form-row {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 16px;

    @media (max-width: 560px) {
      grid-template-columns: 1fr;
    }
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: 6px;

    label {
      font-size: 0.85rem;
      font-weight: 500;
      color: $color-muted;
    }
  }

  .form-input {
    height: 44px;
    padding: 0 14px;
    border-radius: 10px;
    border: 1px solid rgba(255, 255, 255, 0.1);
    background: rgba(255, 255, 255, 0.04);
    color: $color-ink;
    font: inherit;
    font-size: 0.94rem;
    outline: none;
    transition: border-color 0.2s, box-shadow 0.2s;

    &:focus {
      border-color: rgba(167, 139, 250, 0.5);
      box-shadow: 0 0 0 3px rgba(139, 92, 246, 0.15);
    }

    &.error {
      border-color: #ef4444;
    }
  }

  .error-text {
    font-size: 0.82rem;
    color: #ef4444;
  }
}

.stats-card {
  h3 {
    font-size: 1.1rem;
    margin-bottom: 20px;
  }
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;

  @media (max-width: 720px) {
    grid-template-columns: repeat(2, 1fr);
  }
}

.stat-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 16px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.04);

  .stat-value {
    font-size: 1.5rem;
    font-weight: 700;
    background: linear-gradient(90deg, #a78bfa, #22d3ee);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
  }

  .stat-label {
    font-size: 0.82rem;
    color: $color-muted;
  }
}

.purchases-card,
.certificates-card {
  padding: 28px;

  h3 {
    font-size: 1.1rem;
    margin-bottom: 20px;
  }
}

.certificate-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 14px 16px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.04);
  box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.14), inset 0 0 0 1px rgba(255, 255, 255, 0.05);
}

.certificate-actions {
  display: flex;
  gap: 8px;
  flex-shrink: 0;
}

.btn-sm {
  padding: 8px 14px;
  font-size: 0.82rem;
}

.purchases-empty {
  font-size: 0.9rem;
  color: $color-muted;
}

.purchases-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.purchase-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 14px 16px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.04);
  text-decoration: none;
  color: inherit;
  transition: background 0.2s;

  &:hover {
    background: rgba(255, 255, 255, 0.08);
  }
}

.purchase-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.purchase-title {
  font-size: 0.94rem;
  font-weight: 600;
  color: $color-ink;
}
.purchase-kind {
  color: $color-cyan;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
}


.purchase-date {
  font-size: 0.8rem;
  color: $color-faint;
}

.purchase-amount {
  flex-shrink: 0;
  font-size: 0.94rem;
  font-weight: 700;
  color: $color-gold;
}

.danger-card {
  h3 {
    font-size: 1.1rem;
    margin-bottom: 10px;
    color: #ef4444;
  }

  p {
    font-size: 0.9rem;
    color: $color-muted;
    margin-bottom: 20px;
  }

  .btn-danger {
    background: rgba(239, 68, 68, 0.15);
    color: #ef4444;
    border: 1px solid rgba(239, 68, 68, 0.3);

    &:hover {
      background: rgba(239, 68, 68, 0.25);
    }
  }
}

.btn {
  padding: 10px 20px;
  font-size: 0.9rem;
  border-radius: 10px;
  cursor: pointer;
  transition: background 0.2s, transform 0.15s;
  border: none;
  font-weight: 500;

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
}

.btn-primary {
  background: linear-gradient(90deg, #f59e0b, #ec4899);
  color: #fff;

  &:hover:not(:disabled) {
    transform: translateY(-1px);
    filter: brightness(1.1);
  }
}

.btn-ghost {
  background: transparent;
  border: 1px solid rgba(255, 255, 255, 0.15);
  color: $color-muted;

  &:hover {
    color: $color-ink;
    border-color: rgba(255, 255, 255, 0.3);
  }
}

.modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 200;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(3, 6, 24, 0.7);
  backdrop-filter: blur(6px);
}

.modal {
  max-width: 440px;
  width: 100%;
  margin: 20px;
  padding: 28px;

  h3 {
    font-size: 1.15rem;
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
.error {
  text-align: center;
  padding: 60px;
  color: $color-muted;
}
</style>
