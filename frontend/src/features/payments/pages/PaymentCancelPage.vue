<template>
  <div class="payment-result">
    <div class="result-card glass-card">
      <div class="result-icon">!</div>
      <h1>Płatność anulowana</h1>
      <p>Nie obciążyliśmy Twojej karty. Możesz wrócić do kursu i spróbować ponownie.</p>
      <div class="result-actions">
        <router-link
          v-if="courseId"
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
  background: rgba(245, 158, 11, 0.18);
  color: $color-gold;
  box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.25), 0 0 0 1px rgba(245, 158, 11, 0.3);
}

.result-actions {
  display: flex;
  justify-content: center;
  gap: 12px;
  flex-wrap: wrap;
}
</style>
