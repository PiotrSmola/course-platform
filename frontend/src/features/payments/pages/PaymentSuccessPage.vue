<template>
  <div class="payment-result">
    <div class="result-card glass-card">
      <template v-if="!sessionId">
        <h1>Brak sesji płatności</h1>
        <p>Nie znaleziono identyfikatora płatności w adresie.</p>
        <router-link class="btn btn-primary" :to="{ name: 'Courses' }">Przeglądaj kursy</router-link>
      </template>

      <template v-else-if="isSubscriptionFlow && isSubscriptionChecking">
        <div class="spinner" />
        <h1>Aktywujemy subskrypcję...</h1>
        <p>Stripe potwierdza płatność i synchronizuje dostęp do wszystkich kursów.</p>
      </template>

      <template v-else-if="isSubscriptionFlow && subscriptionQuery.isError.value">
        <div class="result-icon error">✕ Błąd</div>
        <h1>Nie udało się potwierdzić subskrypcji</h1>
        <p>Spróbuj ponownie za chwilę. Jeśli płatność została pobrana, dostęp powinien pojawić się po odświeżeniu.</p>
        <div class="result-actions">
          <button class="btn btn-primary" @click="subscriptionQuery.refetch()">Spróbuj ponownie</button>
          <router-link class="btn btn-ghost" :to="{ name: 'Courses' }">Wróć do kursów</router-link>
        </div>
      </template>

      <template v-else-if="isSubscriptionFlow && subscriptionPollingTimeout">
        <div class="result-icon error">✕ Błąd</div>
        <h1>Subskrypcja jeszcze się synchronizuje</h1>
        <p>Webhook Stripe potrzebuje więcej czasu. Odśwież stronę za chwilę lub przejdź do katalogu kursów.</p>
        <div class="result-actions">
          <button class="btn btn-primary" @click="subscriptionQuery.refetch()">Odśwież status</button>
          <router-link class="btn btn-ghost" :to="{ name: 'Courses' }">Przejdź do kursów</router-link>
        </div>
      </template>

      <template v-else-if="isSubscriptionFlow && isSubscriptionCompleted">
        <div class="result-icon success">✓ Gotowe</div>
        <h1>All-access aktywny</h1>
        <p>Masz już dostęp do wszystkich opublikowanych kursów w ramach miesięcznej subskrypcji.</p>
        <div class="result-actions">
          <router-link class="btn btn-primary" :to="{ name: 'Courses' }">Przeglądaj kursy</router-link>
          <router-link class="btn btn-ghost" :to="{ name: 'Home' }">Wróć na start</router-link>
        </div>
      </template>

      <template v-else-if="isSubscriptionFlow">
        <div class="result-icon error">✕ Błąd</div>
        <h1>Subskrypcja nie jest aktywna</h1>
        <p>{{ subscriptionFailureMessage }}</p>
        <div class="result-actions">
          <router-link class="btn btn-primary" :to="{ name: 'Courses' }">Przeglądaj kursy</router-link>
        </div>
      </template>
      <template v-else-if="isGiftFlow">
        <div class="result-icon success">✓ Gotowe</div>
        <h1>Prezent został kupiony</h1>
        <p>Po potwierdzeniu płatności kod zostanie wysłany na adres odbiorcy.</p>
        <div class="result-actions">
          <router-link class="btn btn-primary" :to="{ name: 'Profile' }">Moje zakupy</router-link>
          <router-link class="btn btn-ghost" :to="{ name: 'Courses' }">Wróć do kursów</router-link>
        </div>
      </template>


      <template v-else-if="isChecking">
        <div class="spinner" />
        <h1>Przetwarzamy płatność...</h1>
        <p>To potrwa tylko chwilę. Nie zamykaj tej strony.</p>
      </template>

      <template v-else-if="statusQuery.isError.value">
        <div class="result-icon error">✕ Błąd</div>
        <h1>Wystąpił błąd</h1>
        <p>Nie udało się sprawdzić statusu płatności. Spróbuj ponownie później lub skontaktuj się z nami.</p>
        <div class="result-actions">
          <button class="btn btn-primary" @click="statusQuery.refetch()">Spróbuj ponownie</button>
          <router-link class="btn btn-ghost" :to="{ name: 'Courses' }">Wróć do kursów</router-link>
        </div>
      </template>

      <template v-else-if="isPollingTimeout">
        <div class="result-icon error">✕ Błąd</div>
        <h1>Przekroczono czas oczekiwania</h1>
        <p>Nie udało się potwierdzić statusu płatności w czasie oczekiwania. Spróbuj ponownie lub skontaktuj się z nami.</p>
        <div class="result-actions">
          <button class="btn btn-primary" @click="statusQuery.refetch()">Spróbuj ponownie</button>
          <router-link class="btn btn-ghost" :to="{ name: 'Courses' }">Wróć do kursów</router-link>
        </div>
      </template>

      <template v-else-if="isCompleted">
        <div class="result-icon success">✓ Gotowe</div>
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
        <div class="result-icon error">✕ Błąd</div>
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
import { useMySubscription, usePaymentStatus } from '@/features/payments/composables/usePayments'
import { PaymentStatus, SubscriptionStatus } from '@/features/payments/types/payment.types'
import { queryKeys } from '@/shared/queryKeys'

const route = useRoute()
const queryClient = useQueryClient()

const sessionId = computed(() => (route.query.session_id as string) ?? '')
const isSubscriptionFlow = computed(() => route.query.type === 'subscription')
const isGiftFlow = computed(() => route.query.type === 'gift')
const statusQuery = usePaymentStatus(() => (isSubscriptionFlow.value || isGiftFlow.value ? '' : sessionId.value))
const subscriptionQuery = useMySubscription(() => isSubscriptionFlow.value, true)

const isChecking = computed(() =>
  !isSubscriptionFlow.value && !isGiftFlow.value && (
  statusQuery.isLoading.value ||
  (!statusQuery.isPollingTimeout.value && statusQuery.data.value?.status === PaymentStatus.Pending)
))
const isCompleted = computed(() => statusQuery.data.value?.status === PaymentStatus.Completed)
const isPollingTimeout = computed(() => statusQuery.isPollingTimeout.value)
const isSubscriptionChecking = computed(() =>
  isSubscriptionFlow.value && (
    subscriptionQuery.isLoading.value ||
    (!subscriptionQuery.isPollingTimeout.value && !subscriptionQuery.data.value?.hasActiveAccess)
  )
)
const isSubscriptionCompleted = computed(() => subscriptionQuery.data.value?.hasActiveAccess ?? false)
const subscriptionPollingTimeout = computed(() => subscriptionQuery.isPollingTimeout.value)

const failureMessage = computed(() => {
  if (statusQuery.data.value?.status === PaymentStatus.Expired) return 'Sesja płatności wygasła. Spróbuj ponownie.'
  return 'Płatność została odrzucona. Spróbuj ponownie lub skontaktuj się z nami.'
})

const subscriptionFailureMessage = computed(() => {
  if (subscriptionQuery.data.value?.status === SubscriptionStatus.Canceled) {
    return 'Subskrypcja została anulowana lub nie została dokończona.'
  }
  if (subscriptionQuery.data.value?.status === SubscriptionStatus.PastDue) {
    return 'Subskrypcja jest w stanie PastDue. Dostęp może wygasnąć po końcu bieżącego okresu.'
  }
  return 'Checkout zakończył się, ale dostęp nie został jeszcze aktywowany.'
})

watch(isCompleted, (completed) => {
  if (completed && statusQuery.data.value) {
    queryClient.invalidateQueries({ queryKey: queryKeys.enrollments() })
    queryClient.invalidateQueries({ queryKey: queryKeys.course(statusQuery.data.value.courseId) })
    queryClient.invalidateQueries({ queryKey: queryKeys.myPurchases() })
  }
})

watch(isSubscriptionCompleted, (completed) => {
  if (completed) {
    queryClient.invalidateQueries({ queryKey: queryKeys.mySubscription() })
  }
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.payment-result {
  min-height: 70vh;
  display: grid;
  place-items: center;
  padding: 40px 20px 80px;
}

.result-card {
  --lg-blur: 10px;
  max-width: 540px;
  width: 100%;
  padding: 30px 32px 32px;

  &:hover {
    transform: none;
  }

  h1 {
    font-size: clamp(1.7rem, 4vw, 2.2rem);
    font-weight: 600;
    margin-bottom: 12px;
  }

  p {
    color: $color-muted;
    margin-bottom: 28px;
    line-height: 1.6;

    strong {
      color: $color-ink;
      font-weight: 600;
    }
  }

  @media (max-width: 520px) {
    padding: 26px 22px;
  }
}

.result-icon {
  @include tag;
  margin-bottom: 22px;
  color: $color-accent;
  box-shadow: inset 0 0 0 1px rgba($color-accent, 0.4);

  &.success {
    color: $color-success;
    box-shadow: inset 0 0 0 1px rgba($color-success, 0.4);
  }

  &.error {
    color: $color-danger;
    box-shadow: inset 0 0 0 1px rgba($color-danger, 0.4);
  }
}

.result-actions {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  padding-top: 20px;
  border-top: 1px dashed $color-hairline;
}

.spinner {
  width: 22px;
  height: 22px;
  margin-bottom: 22px;
  border: 2px solid rgba($color-ink, 0.1);
  border-top-color: $color-accent;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
