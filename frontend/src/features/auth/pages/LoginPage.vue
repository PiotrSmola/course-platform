<template>
  <div class="login-page">
    <h2>Logowanie</h2>
    <LoginForm />
    <p class="auth-forgot">
      <router-link :to="{ name: 'ForgotPassword' }">Nie pamiętasz hasła?</router-link>
    </p>
    <p class="auth-switch">
      Nie masz konta? <router-link :to="{ name: 'Register', query: route.query }">Zarejestruj się</router-link>
    </p>
  </div>
</template>

<script setup lang="ts">
import LoginForm from '@/features/auth/components/LoginForm.vue'
import { useRoute } from 'vue-router'

const route = useRoute()
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.login-page {
  h2 {
    font-size: 1.25rem;
    margin-bottom: 24px;
    text-align: center;
  }

  .auth-forgot {
    text-align: center;
    margin-top: 16px;
    font-size: 0.88rem;

    a {
      color: $color-muted;
      font-weight: 500;

      &:hover {
        color: $color-cyan;
        text-decoration: underline;
      }
    }
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