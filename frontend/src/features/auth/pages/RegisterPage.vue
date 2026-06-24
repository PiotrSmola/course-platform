<template>
  <div class="auth-page">
    <h1>Register</h1>
    <RegisterForm />
  </div>
</template>

<script setup lang="ts">
import RegisterForm from '@/features/auth/components/RegisterForm.vue'
</script>

<style lang="scss" scoped>
.auth-page {
  max-width: 400px;
  margin: 0 auto;
  padding: 2rem;
}
</style>
