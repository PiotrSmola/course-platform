<template>
  <div class="payment-result">
    <div class="result-card glass-card">
      <div class="result-icon">Anulowano</div>
      <h1>{{ isSubscriptionFlow ? 'Subskrypcja anulowana' : 'Płatność anulowana' }}</h1>
      <p>
        {{
          isSubscriptionFlow
            ? 'Nie aktywowaliśmy planu All-access. Możesz wrócić do katalogu i spróbować ponownie.'
            : 'Nie obciążyliśmy Twojej karty. Możesz wrócić do kursu i spróbować ponownie.'
        }}
      </p>
      <div class="result-actions">
        <router-link
          v-if="courseId && !isSubscriptionFlow"
          class="btn btn-primary"
          :to="{ name: 'CourseDetails', params: { id: courseId } }"
        >
          Wróć do kursu
        </router-link>
        <router-link v-else class="btn btn-primary" :to="{ name: 'Courses' }">Przeglądaj kursy</router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()
const courseId = computed(() => (route.query.courseId as string) ?? '')
const isSubscriptionFlow = computed(() => route.query.type === 'subscription')
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
