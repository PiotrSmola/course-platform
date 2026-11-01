<template>
  <div class="newsletter-page">
    <section class="newsletter-card glass-card">
      <div v-if="state === 'success'" class="newsletter-state">
        <span class="state-tag success">Newsletter <b>✓ Gotowe</b></span>
        <h1>Wypisano z newslettera</h1>
        <p>Nie będziemy już wysyłać wiadomości promocyjnych na ten adres.</p>
        <router-link class="btn btn-ghost" :to="{ name: 'Home' }">Wróć na stronę główną</router-link>
      </div>

      <div v-else-if="state === 'error'" class="newsletter-state">
        <span class="state-tag error">Newsletter <b>Błąd</b></span>
        <h1>Nie udało się wypisać</h1>
        <p>{{ errorMessage }}</p>
        <router-link class="btn btn-ghost" :to="{ name: 'Home' }">Wróć na stronę główną</router-link>
      </div>

      <div v-else class="newsletter-state">
        <span class="state-tag warning">Newsletter <b>Potwierdzenie</b></span>
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
@use "@/assets/styles/abstracts/mixins" as *;

.newsletter-page {
  display: grid;
  min-height: 58vh;
  place-items: center;
  padding: 60px 24px;
}

.newsletter-card {
  --lg-blur: 10px;
  width: min(540px, 100%);
  padding: 30px 32px 32px;

  &:hover {
    transform: none;
  }

  @media (max-width: 560px) {
    padding: 26px 22px;
  }
}

.newsletter-state {
  display: flex;
  align-items: flex-start;
  flex-direction: column;

  h1 {
    margin: 22px 0 10px;
    font-size: clamp(1.7rem, 4vw, 2.1rem);
    font-weight: 600;
  }

  p {
    color: $color-muted;
    line-height: 1.6;
  }

  > .btn {
    margin-top: 28px;
  }
}

.state-tag {
  @include tag;
  align-self: stretch;
  justify-content: space-between;
  padding: 0 0 14px;
  background: none;
  box-shadow: none;
  border-bottom: 1px solid $color-hairline;
  border-radius: 0;
  font-size: 0.66rem;
  color: $color-faint;

  b {
    font-weight: 500;
  }

  &.success b {
    color: $color-success;
  }

  &.warning b {
    color: $color-accent;
  }

  &.error b {
    color: $color-danger;
  }

  &.loading b {
    color: $color-accent;
  }
}

.spinner {
  display: inline-block;
  width: 12px;
  height: 12px;
  border: 2px solid rgba($color-ink, 0.12);
  border-top-color: $color-accent;
  border-radius: 50%;
  animation: newsletter-spin 0.8s linear infinite;
}

.newsletter-actions {
  display: flex;
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

@keyframes newsletter-spin {
  to { transform: rotate(360deg); }
}
</style>
