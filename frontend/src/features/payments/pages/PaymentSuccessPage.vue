<template>
  <div class="payment-result">
    <div class="result-card glass-card">
      <template v-if="!sessionId">
        <h1>Brak sesji płatności</h1>
        <p>Nie znaleziono identyfikatora płatności w adresie.</p>
        <router-link class="btn btn-primary" :to="{ name: 'Courses' }">Przeglądaj kursy</router-link>
      </template>

      <template v-else-if="isChecking">
        <div class="spinner" />
        <h1>Przetwarzamy płatność...</h1>
        <p>To potrwa tylko chwilę. Nie zamykaj tej strony.</p>
      </template>

      <template v-else-if="statusQuery.isError.value">
        <div class="result-icon error">✕</div>
        <h1>Wystąpił błąd</h1>
        <p>Nie udało się sprawdzić statusu płatności. Spróbuj ponownie później lub skontaktuj się z nami.</p>
        <div class="result-actions">
          <button class="btn btn-primary" @click="statusQuery.refetch()">Spróbuj ponownie</button>
          <router-link class="btn btn-ghost" :to="{ name: 'Courses' }">Wróć do kursów</router-link>
        </div>
      </template>

      <template v-else-if="isPollingTimeout">
        <div class="result-icon error">✕</div>
        <h1>Przekroczono czas oczekiwania</h1>
        <p>Nie udało się potwierdzić statusu płatności w czasie oczekiwania. Spróbuj ponownie lub skontaktuj się z nami.</p>
        <div class="result-actions">
          <button class="btn btn-primary" @click="statusQuery.refetch()">Spróbuj ponownie</button>
          <router-link class="btn btn-ghost" :to="{ name: 'Courses' }">Wróć do kursów</router-link>
        </div>
      </template>

      <template v-else-if="isCompleted">
        <div class="result-icon success">✓</div>
        <h1>Płatność zakończona</h1>
        <p>
          Masz już dostęp do kursu
          <strong v-if="statusQuery.data.value">{{ statusQuery.data.value.courseTitle }}</strong>.
        </p>
        <div class="result-actions">
          <router-link
            v-if="statusQuery.data.value"
            class="btn btn-primary"
            :to="{ name: 'CourseDetails', params: { id: statusQuery.data.value.courseId } }"
          >
            Przejdź do kursu
          </router-link>
          <router-link class="btn btn-ghost" :to="{ name: 'MyCourses' }">Moje kursy</router-link>
        </div>
      </template>

      <template v-else>
        <div class="result-icon error">✕</div>
        <h1>Płatność nie powiodła się</h1>
        <p>{{ failureMessage }}</p>
        <div class="result-actions">
          <router-link
            v-if="statusQuery.data.value"
            class="btn btn-primary"
            :to="{ name: 'CourseDetails', params: { id: statusQuery.data.value.courseId } }"
          >
            Wróć do kursu
          </router-link>
          <router-link v-else class="btn btn-primary" :to="{ name: 'Courses' }">Przeglądaj kursy</router-link>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useQueryClient } from '@tanstack/vue-query'
import { usePaymentStatus } from '@/features/payments/composables/usePayments'
import { PaymentStatus } from '@/features/payments/types/payment.types'
import { queryKeys } from '@/shared/queryKeys'

const route = useRoute()
const queryClient = useQueryClient()

const sessionId = computed(() => (route.query.session_id as string) ?? '')
const statusQuery = usePaymentStatus(() => sessionId.value)

const isChecking = computed(() =>
  statusQuery.isLoading.value ||
  (!statusQuery.isPollingTimeout.value && statusQuery.data.value?.status === PaymentStatus.Pending)
)
const isCompleted = computed(() => statusQuery.data.value?.status === PaymentStatus.Completed)
const isPollingTimeout = computed(() => statusQuery.isPollingTimeout.value)

const failureMessage = computed(() => {
  if (statusQuery.data.value?.status === PaymentStatus.Expired) return 'Sesja płatności wygasła. Spróbuj ponownie.'
  return 'Płatność została odrzucona. Spróbuj ponownie lub skontaktuj się z nami.'
})

watch(isCompleted, (completed) => {
  if (completed && statusQuery.data.value) {
    queryClient.invalidateQueries({ queryKey: queryKeys.enrollments() })
    queryClient.invalidateQueries({ queryKey: queryKeys.course(statusQuery.data.value.courseId) })
    queryClient.invalidateQueries({ queryKey: queryKeys.myPurchases() })
  }
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.payment-result {
  min-height: 70vh;
  display: grid;
  place-items: center;
  padding: calc($header-height + 40px) 20px 80px;
}

.result-card {
  max-width: 480px;
  width: 100%;
  padding: 48px 40px;
  text-align: center;

  h1 {
    font-size: 1.6rem;
    margin-bottom: 12px;
  }

  p {
    color: $color-muted;
    margin-bottom: 28px;
    line-height: 1.6;
  }
}

.result-icon {
  width: 64px;
  height: 64px;
  margin: 0 auto 24px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  font-size: 1.8rem;
  font-weight: 700;

  &.success {
    background: rgba(34, 197, 94, 0.18);
    color: #4ade80;
    box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.25), 0 0 0 1px rgba(74, 222, 128, 0.3);
  }

  &.error {
    background: rgba(239, 68, 68, 0.18);
    color: #f87171;
    box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.25), 0 0 0 1px rgba(248, 113, 113, 0.3);
  }
}

.result-actions {
  display: flex;
  justify-content: center;
  gap: 12px;
  flex-wrap: wrap;
}

.spinner {
  width: 48px;
  height: 48px;
  margin: 0 auto 24px;
  border: 3px solid rgba(255, 255, 255, 0.1);
  border-top-color: $color-gold;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
