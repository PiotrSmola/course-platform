<template>
  <form @submit="onSubmit" class="forgot-password-form">
    <div class="form-group">
      <label for="email-input">Email</label>
      <input
        id="email-input"
        v-model="email"
        type="email"
        autocomplete="email"
        placeholder="twoj@email.com"
      />
      <span v-if="errors.email" class="error">{{ errors.email }}</span>
    </div>
    <button type="submit" class="btn btn-primary" :disabled="!meta.valid || isLoading">
      {{ isLoading ? 'Wysyłanie...' : 'Wyślij link resetujący' }}
    </button>
  </form>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useAuth } from '@/features/auth/composables/useAuth'
import { forgotPasswordSchema } from '@/features/auth/schemas/auth.schema'

const { forgotPassword } = useAuth()

const { handleSubmit, defineField, errors, meta } = useForm({
  validationSchema: toTypedSchema(forgotPasswordSchema)
})

const [email] = defineField('email')
const isLoading = ref(false)

const onSubmit = handleSubmit(async (values) => {
  isLoading.value = true
  try {
    await forgotPassword.mutateAsync(values)
  } finally {
    isLoading.value = false
  }
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.forgot-password-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
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

  .error {
    color: #f87171;
    font-size: 0.82rem;
    font-weight: 500;
  }
}

.btn {
  height: 48px;
  margin-top: 8px;
}
</style>
