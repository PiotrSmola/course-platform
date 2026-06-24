<template>
  <form @submit.prevent="handleSubmit" class="register-form">
    <div class="form-group">
      <label>First Name</label>
      <input v-model="firstName" type="text" required />
    </div>
    <div class="form-group">
      <label>Last Name</label>
      <input v-model="lastName" type="text" required />
    </div>
    <div class="form-group">
      <label>Email</label>
      <input v-model="email" type="email" required />
    </div>
    <div class="form-group">
      <label>Password</label>
      <input v-model="password" type="password" required />
    </div>
    <button type="submit" :disabled="isLoading">Register</button>
  </form>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useAuth } from '@/features/auth/composables/useAuth'

const { register } = useAuth()
const firstName = ref('')
const lastName = ref('')
const email = ref('')
const password = ref('')
const isLoading = ref(false)

async function handleSubmit() {
  isLoading.value = true
  await register.mutateAsync({
    firstName: firstName.value,
    lastName: lastName.value,
    email: email.value,
    password: password.value
  })
  isLoading.value = false
}
</script>

<style lang="scss" scoped>
.register-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;

  .form-group {
    display: flex;
    flex-direction: column;
  }

  input {
    padding: 0.5rem;
  }

  button {
    padding: 0.75rem;
    cursor: pointer;
  }
}
</style>
