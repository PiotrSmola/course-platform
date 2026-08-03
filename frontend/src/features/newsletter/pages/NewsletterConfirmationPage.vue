<template>
  <div class="newsletter-page">
    <section class="newsletter-card glass-card">
      <div v-if="state === 'loading'" class="newsletter-state">
        <span class="spinner" aria-hidden="true" />
        <h1>Potwierdzamy Twój adres e-mail</h1>
        <p>To potrwa tylko chwilę.</p>
      </div>

      <div v-else-if="state === 'success'" class="newsletter-state">
        <span class="state-icon success" aria-hidden="true">✓</span>
        <h1>Twój zapis jest potwierdzony</h1>
        <p>Od teraz będziemy wysyłać Ci informacje o nowych kursach i promocjach.</p>
        <router-link class="btn btn-primary" :to="{ name: 'Home' }">Wróć na stronę główną</router-link>
      </div>

      <div v-else class="newsletter-state">
        <span class="state-icon error" aria-hidden="true">!</span>
        <h1>Nie udało się potwierdzić zapisu</h1>
        <p>{{ errorMessage }}</p>
        <router-link class="btn btn-ghost" :to="{ name: 'Home' }">Wróć na stronę główną</router-link>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { confirmNewsletterSubscription } from '@/features/newsletter/api/newsletter.api'
import { getApiErrorMessage } from '@/shared/api/apiError'

const route = useRoute()
const state = ref<'loading' | 'success' | 'error'>('loading')
const errorMessage = ref('Link potwierdzający jest nieprawidłowy lub wygasł.')

onMounted(async () => {
  const token = typeof route.query.token === 'string' ? route.query.token : ''
  if (!token) {
    state.value = 'error'
    return
  }

  try {
    const response = await confirmNewsletterSubscription({ token })
    if (response.message.startsWith('Link ')) {
      errorMessage.value = response.message
      state.value = 'error'
      return
    }
    state.value = 'success'
  } catch (error) {
    errorMessage.value = getApiErrorMessage(error) || errorMessage.value
    state.value = 'error'
  }
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.newsletter-page {
  display: grid;
  min-height: 58vh;
  place-items: center;
  padding: 60px 24px;
}

.newsletter-card {
  --lg-r: 28px;
  --lg-blur: 2px;
  width: min(520px, 100%);
  padding: 44px 36px;
  text-align: center;

  @media (max-width: 560px) {
    padding: 36px 24px;
  }
}

.newsletter-state {
  display: flex;
  align-items: center;
  flex-direction: column;

  h1 {
    margin: 18px 0 10px;
    font-family: $font-display;
    font-size: 1.55rem;
    letter-spacing: -0.025em;
  }

  p {
    color: $color-muted;
    line-height: 1.6;
  }

  .btn {
    margin-top: 28px;
  }
}

.spinner,
.state-icon {
  display: grid;
  width: 46px;
  height: 46px;
  place-items: center;
  border-radius: 50%;
}

.spinner {
  border: 3px solid rgba(255, 255, 255, 0.12);
  border-top-color: $color-gold;
  animation: newsletter-spin 0.8s linear infinite;
}

.state-icon {
  font-size: 1.45rem;
  font-weight: 800;

  &.success {
    color: #bbf7d0;
    background: rgba(34, 197, 94, 0.16);
    box-shadow: inset 0 0 0 1px rgba(34, 197, 94, 0.38);
  }

  &.error {
    color: #fecaca;
    background: rgba(239, 68, 68, 0.14);
    box-shadow: inset 0 0 0 1px rgba(239, 68, 68, 0.36);
  }
}

@keyframes newsletter-spin {
  to { transform: rotate(360deg); }
}
</style>
