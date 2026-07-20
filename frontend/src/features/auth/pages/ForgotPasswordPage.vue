<template>
  <div class="forgot-password-page">
    <h2>Przypomnienie hasła</h2>
    <p class="hint">Podaj adres email powiązany z kontem. Wyślemy link do resetu hasła.</p>
    <ForgotPasswordForm />
    <p class="auth-switch">
      <router-link :to="{ name: 'Login' }">Wróć do logowania</router-link>
    </p>
  </div>
</template>

<script setup lang="ts">
import ForgotPasswordForm from '@/features/auth/components/ForgotPasswordForm.vue'
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.forgot-password-page {
  h2 {
    font-size: 1.25rem;
    margin-bottom: 12px;
    text-align: center;
  }

  .hint {
    text-align: center;
    font-size: 0.9rem;
    color: $color-muted;
    margin-bottom: 24px;
    line-height: 1.5;
  }

  .auth-switch {
    text-align: center;
    margin-top: 24px;
    font-size: 0.9rem;
    color: $color-muted;

    a {
      color: $color-cyan;
      font-weight: 600;

      &:hover {
        text-decoration: underline;
      }
    }
  }
}
</style>
