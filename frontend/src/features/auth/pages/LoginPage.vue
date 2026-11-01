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
    font-size: 2.4rem;
    line-height: 1.05;
    margin-bottom: 26px;
  }

  .auth-forgot {
    margin-top: 14px;
    text-align: right;
    font-size: 0.84rem;

    a {
      color: $color-muted;
      transition: color 0.2s;

      &:hover {
        color: $color-ink;
      }
    }
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
