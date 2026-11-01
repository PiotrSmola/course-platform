<template>
  <form class="newsletter-form" @submit.prevent="subscribe">
    <label class="sr-only" for="footer-newsletter-email">Adres e-mail</label>
    <input
      id="footer-newsletter-email"
      v-model="email"
      type="email"
      autocomplete="email"
      placeholder="Twój e-mail"
      required
      :disabled="newsletterMutation.isPending.value"
    />
    <button type="submit" class="btn btn-primary" :disabled="newsletterMutation.isPending.value">
      {{ newsletterMutation.isPending.value ? 'Zapisujemy…' : 'Zapisz się' }}
    </button>
    <p v-if="message" class="newsletter-message" aria-live="polite">{{ message }}</p>
  </form>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useNewsletterSubscribe } from '@/features/newsletter/composables/useNewsletter'

const email = ref('')
const message = ref('')
const newsletterMutation = useNewsletterSubscribe()

async function subscribe() {
  message.value = ''

  try {
    const response = await newsletterMutation.mutateAsync({ email: email.value.trim() })
    message.value = response.message || 'Sprawdź skrzynkę e-mail, aby potwierdzić zapis.'
    email.value = ''
  } catch {
    message.value = 'Nie udało się zapisać. Spróbuj ponownie za chwilę.'
  }
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.newsletter-form {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 10px;
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

input {
  width: 100%;
  min-width: 0;
  height: 46px;
  padding: 0 16px;
  border: 0;
  border-radius: $radius-md;
  outline: 0;
  background: rgba(13, 15, 19, 0.45);
  box-shadow: inset 0 0 0 1px $color-hairline;
  color: $color-ink;
  font: inherit;
  font-size: 0.92rem;
  transition: box-shadow 160ms ease;

  &::placeholder {
    color: $color-faint;
  }

  &:focus {
    box-shadow: inset 0 0 0 1px $color-hairline, 0 0 0 2px rgba($color-accent, 0.6);
  }
}

.btn {
  height: 46px;
  padding-inline: 20px;
  font-size: 0.9rem;
}

.newsletter-message {
  grid-column: 1 / -1;
  color: $color-success;
  font-family: $font-mono;
  font-size: 0.72rem;
  line-height: 1.45;
}

@media (max-width: 560px) {
  .newsletter-form {
    grid-template-columns: 1fr;
  }

  .btn {
    width: 100%;
  }
}
</style>
