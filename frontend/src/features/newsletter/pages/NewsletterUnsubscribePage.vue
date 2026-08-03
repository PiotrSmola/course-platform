<template>
  <div class="newsletter-page">
    <section class="newsletter-card glass-card">
      <div v-if="state === 'success'" class="newsletter-state">
        <span class="state-icon success" aria-hidden="true">✓</span>
        <h1>Wypisano z newslettera</h1>
        <p>Nie będziemy już wysyłać wiadomości promocyjnych na ten adres.</p>
        <router-link class="btn btn-ghost" :to="{ name: 'Home' }">Wróć na stronę główną</router-link>
      </div>

      <div v-else-if="state === 'error'" class="newsletter-state">
        <span class="state-icon error" aria-hidden="true">!</span>
        <h1>Nie udało się wypisać</h1>
        <p>{{ errorMessage }}</p>
        <router-link class="btn btn-ghost" :to="{ name: 'Home' }">Wróć na stronę główną</router-link>
      </div>

      <div v-else class="newsletter-state">
        <span class="state-icon warning" aria-hidden="true">?</span>
        <h1>Potwierdź wypisanie</h1>
        <p>Po potwierdzeniu przestaniemy wysyłać newsletter na ten adres.</p>
        <div class="newsletter-actions">
          <button type="button" class="btn btn-primary" :disabled="isSubmitting" @click="unsubscribe">Wypisz mnie</button>
          <router-link class="btn btn-ghost" :to="{ name: 'Home' }">Zostaję</router-link>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'
import { unsubscribeNewsletter } from '@/features/newsletter/api/newsletter.api'
import { getApiErrorMessage } from '@/shared/api/apiError'

const route = useRoute()
const token = computed(() => typeof route.query.token === 'string' ? route.query.token : '')
const state = ref<'confirm' | 'success' | 'error'>(token.value ? 'confirm' : 'error')
const isSubmitting = ref(false)
const errorMessage = ref(token.value ? 'Spróbuj ponownie za chwilę.' : 'Link wypisujący jest nieprawidłowy lub wygasł.')

async function unsubscribe() {
  if (!token.value) return

  isSubmitting.value = true
  try {
    const response = await unsubscribeNewsletter({ token: token.value })
    if (response.message.startsWith('Link ')) {
      errorMessage.value = response.message
      state.value = 'error'
      return
    }
    state.value = 'success'
  } catch (error) {
    errorMessage.value = getApiErrorMessage(error) || errorMessage.value
    state.value = 'error'
  } finally {
    isSubmitting.value = false
  }
}
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

  > .btn {
    margin-top: 28px;
  }
}

.state-icon {
  display: grid;
  width: 46px;
  height: 46px;
  place-items: center;
  border-radius: 50%;
  font-size: 1.45rem;
  font-weight: 800;

  &.success {
    color: #bbf7d0;
    background: rgba(34, 197, 94, 0.16);
    box-shadow: inset 0 0 0 1px rgba(34, 197, 94, 0.38);
  }

  &.warning {
    color: #fde68a;
    background: rgba(245, 158, 11, 0.14);
    box-shadow: inset 0 0 0 1px rgba(245, 158, 11, 0.36);
  }

  &.error {
    color: #fecaca;
    background: rgba(239, 68, 68, 0.14);
    box-shadow: inset 0 0 0 1px rgba(239, 68, 68, 0.36);
  }
}

.newsletter-actions {
  display: flex;
  justify-content: center;
  gap: 10px;
  margin-top: 28px;

  @media (max-width: 460px) {
    width: 100%;
    flex-direction: column;

    .btn {
      width: 100%;
    }
  }
}
</style>
