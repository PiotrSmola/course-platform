<template>
  <form @submit.prevent="handleSubmit" class="register-form">
    <div class="form-row">
      <div class="form-group">
        <label>Imię</label>
        <input v-model="firstName" type="text" required placeholder="Jan" />
      </div>
      <div class="form-group">
        <label>Nazwisko</label>
        <input v-model="lastName" type="text" required placeholder="Kowalski" />
      </div>
    </div>
    <div class="form-group">
      <label>Email</label>
      <input v-model="email" type="email" required placeholder="jan@email.com" />
    </div>
    <div class="form-group">
      <label>Hasło</label>
      <input v-model="password" type="password" required placeholder="Min. 6 znaków" />
    </div>
    <button type="submit" class="btn btn-primary" :disabled="isLoading">
      {{ isLoading ? 'Tworzenie konta...' : 'Utwórz konto' }}
    </button>
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
@use "@/assets/styles/abstracts/variables" as *;

.register-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;

  @media (max-width: 480px) {
    grid-template-columns: 1fr;
  }
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;

  label {
    font-size: 0.88rem;
    font-weight: 600;
    color: $color-muted;
  }

  input {
    height: 48px;
    padding: 0 18px;
    border-radius: 16px;
    border: none;
    background: rgba(255, 255, 255, 0.04);
    box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.3), inset 0 0 0 1px rgba(255, 255, 255, 0.1);
    color: $color-ink;
    font: inherit;
    font-size: 0.94rem;
    outline: none;
    transition: background 0.3s, box-shadow 0.3s;

    &::placeholder {
      color: $color-faint;
    }

    &:focus {
      background: rgba(255, 255, 255, 0.07);
      box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.35), inset 0 0 0 1px rgba(167, 139, 250, 0.5), 0 0 0 4px rgba(139, 92, 246, 0.18);
    }
  }
}

.btn {
  height: 48px;
  margin-top: 8px;
}
</style>
