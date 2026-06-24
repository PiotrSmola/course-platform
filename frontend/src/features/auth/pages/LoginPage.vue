<template>
  <div class="login-page">
    <h2>Logowanie</h2>
    <LoginForm />
    <p class="auth-switch">
      Nie masz konta? <router-link :to="{ name: 'Register' }">Zarejestruj się</router-link>
    </p>
  </div>
</template>

<script setup lang="ts">
import LoginForm from '@/features/auth/components/LoginForm.vue'
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.login-page {
  h2 {
    font-size: 1.25rem;
    margin-bottom: 24px;
    text-align: center;
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