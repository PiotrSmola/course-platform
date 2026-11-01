<template>
  <form @submit="onSubmit" class="login-form">
    <div class="form-group">
      <label for="email-input">Email</label>
      <input id="email-input" v-model="email" type="email" autocomplete="email" placeholder="twoj@email.com" />
      <span v-if="errors.email" class="error">{{ errors.email }}</span>
    </div>
    <div class="form-group">
      <label for="password-input">Hasło</label>
      <input id="password-input" v-model="password" type="password" autocomplete="current-password" placeholder="••••••" />
      <span v-if="errors.password" class="error">{{ errors.password }}</span>
    </div>
    <button type="submit" class="btn btn-primary" :disabled="!meta.valid || isLoading">
      {{ isLoading ? 'Logowanie...' : 'Zaloguj się' }}
    </button>
  </form>
</template>

<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useAuth } from '@/features/auth/composables/useAuth'
import { loginSchema } from '@/features/auth/schemas/auth.schema'
import { ref } from 'vue'

const { login } = useAuth()

const { handleSubmit, defineField, errors, meta } = useForm({
  validationSchema: toTypedSchema(loginSchema)
})

const [email] = defineField('email')
const [password] = defineField('password')

const isLoading = ref(false)

const onSubmit = handleSubmit(async (values) => {
  isLoading.value = true
  try {
    await login.mutateAsync(values)
  } finally {
    isLoading.value = false
  }
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.login-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;

  label {
    @include mono-label;
    font-size: 0.64rem;
    color: $color-faint;
  }

  input {
    box-sizing: border-box;
    width: 100%;
    min-width: 0;
    height: 48px;
    padding: 0 16px;
    border-radius: $radius-md;
    border: none;
    background: rgba(13, 15, 19, 0.45);
    box-shadow: inset 0 0 0 1px $color-hairline;
    color: $color-ink;
    font: inherit;
    font-size: 0.94rem;
    outline: none;
    transition: box-shadow 0.2s;

    &::placeholder {
      color: $color-faint;
    }

    &:focus,
    &:focus-visible {
      outline: none;
      box-shadow: inset 0 0 0 1px $color-hairline, 0 0 0 2px rgba($color-accent, 0.6);
    }
  }

  .error {
    color: $color-danger;
    font-size: 0.8rem;
    font-weight: 500;
  }
}

.btn {
  height: 48px;
  margin-top: 8px;
}
</style>
