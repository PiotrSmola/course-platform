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
