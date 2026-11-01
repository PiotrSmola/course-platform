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
@use "@/assets/styles/abstracts/mixins" as *;

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

.token-error {
  padding: 10px 12px;
  border-radius: $radius-xs;
  box-shadow: inset 3px 0 0 0 $color-danger, inset 0 0 0 1px $color-hairline;
  color: $color-danger;
  font-size: 0.86rem;
}
</style>
