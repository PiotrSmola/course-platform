<template>
  <form @submit="onSubmit" class="reset-password-form">
    <div class="form-group">
      <label for="new-password-input">Nowe hasło</label>
      <input
        id="new-password-input"
        v-model="newPassword"
        type="password"
        autocomplete="new-password"
        placeholder="Min. 6 znaków"
      />
      <span v-if="errors.newPassword" class="error">{{ errors.newPassword }}</span>
    </div>
    <div class="form-group">
      <label for="confirm-password-input">Potwierdź hasło</label>
      <input
        id="confirm-password-input"
        v-model="confirmPassword"
        type="password"
        autocomplete="new-password"
        placeholder="Powtórz hasło"
      />
      <span v-if="errors.confirmPassword" class="error">{{ errors.confirmPassword }}</span>
    </div>
    <button type="submit" class="btn btn-primary" :disabled="!meta.valid || isLoading || !hasToken">
      {{ isLoading ? 'Zapisywanie...' : 'Ustaw nowe hasło' }}
    </button>
    <p v-if="!hasToken" class="token-error">Link resetujący jest nieprawidłowy lub wygasł.</p>
  </form>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useAuth } from '@/features/auth/composables/useAuth'
import { resetPasswordSchema } from '@/features/auth/schemas/auth.schema'

const route = useRoute()
const { resetPassword } = useAuth()

const email = computed(() => (route.query.email as string) ?? '')
const token = computed(() => (route.query.token as string) ?? '')
const hasToken = computed(() => Boolean(email.value && token.value))

const { handleSubmit, defineField, errors, meta } = useForm({
  validationSchema: toTypedSchema(resetPasswordSchema)
})

const [newPassword] = defineField('newPassword')
const [confirmPassword] = defineField('confirmPassword')
const isLoading = ref(false)

const onSubmit = handleSubmit(async (values) => {
  if (!hasToken.value) return
  isLoading.value = true
  try {
    await resetPassword.mutateAsync({
      email: email.value,
      token: token.value,
      newPassword: values.newPassword
    })
  } finally {
    isLoading.value = false
  }
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.reset-password-form {
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

.token-error {
  color: #f87171;
  font-size: 0.88rem;
  text-align: center;
}

.btn {
  height: 48px;
  margin-top: 8px;
}
</style>
