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
    font-size: 2.4rem;
    line-height: 1.05;
    margin-bottom: 12px;
  }

  .hint {
    font-size: 0.92rem;
    color: $color-muted;
    margin-bottom: 26px;
    line-height: 1.55;
  }

  .auth-switch {
    margin-top: 28px;
    padding-top: 20px;
    border-top: 1px dashed $color-hairline;
    font-size: 0.9rem;
    color: $color-muted;

    a {
      color: $color-ink;
      font-weight: 500;
      text-decoration: underline;
      text-decoration-color: rgba($color-accent, 0.6);
      text-underline-offset: 4px;
      transition: text-decoration-color 0.2s;

      &:hover {
        text-decoration-color: $color-accent;
      }
    }
  }
}
</style>
