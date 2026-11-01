<template>
  <div class="register-page">
    <h2>Rejestracja</h2>
    <RegisterForm />
    <p class="auth-switch">
      Masz już konto? <router-link :to="{ name: 'Login', query: route.query }">Zaloguj się</router-link>
    </p>
  </div>
</template>

<script setup lang="ts">
import RegisterForm from '@/features/auth/components/RegisterForm.vue'
import { useRoute } from 'vue-router'

const route = useRoute()
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.register-page {
  h2 {
    font-size: 2.4rem;
    line-height: 1.05;
    margin-bottom: 26px;
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
