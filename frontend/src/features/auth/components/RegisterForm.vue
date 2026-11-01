<template>
  <form @submit="onSubmit" class="register-form">
    <div class="form-row">
      <div class="form-group">
        <label>Imię</label>
        <input v-model="firstName" type="text" autocomplete="given-name" placeholder="Jan" />
        <span v-if="errors.firstName" class="error">{{ errors.firstName }}</span>
      </div>
      <div class="form-group">
        <label>Nazwisko</label>
        <input v-model="lastName" type="text" autocomplete="family-name" placeholder="Kowalski" />
        <span v-if="errors.lastName" class="error">{{ errors.lastName }}</span>
      </div>
    </div>
    <div class="form-group">
      <label>Email</label>
      <input v-model="email" type="email" autocomplete="email" placeholder="jan@email.com" />
      <span v-if="errors.email" class="error">{{ errors.email }}</span>
    </div>
    <div class="form-group">
      <label>Hasło</label>
      <input v-model="password" type="password" autocomplete="new-password" placeholder="Min. 6 znaków" />
      <span v-if="errors.password" class="error">{{ errors.password }}</span>
    </div>
    <button type="submit" class="btn btn-primary" :disabled="!meta.valid || isLoading">
      {{ isLoading ? 'Tworzenie konta...' : 'Utwórz konto' }}
    </button>
  </form>
</template>

<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useAuth } from '@/features/auth/composables/useAuth'
import { registerSchema } from '@/features/auth/schemas/auth.schema'
import { ref } from 'vue'

const { register } = useAuth()

const { handleSubmit, defineField, errors, meta } = useForm({
  validationSchema: toTypedSchema(registerSchema)
})

const [firstName] = defineField('firstName')
const [lastName] = defineField('lastName')
const [email] = defineField('email')
const [password] = defineField('password')

const isLoading = ref(false)

const onSubmit = handleSubmit(async (values) => {
  isLoading.value = true
  try {
    await register.mutateAsync(values)
  } finally {
    isLoading.value = false
  }
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.register-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;

  > .form-group {
    min-width: 0;
  }

  @media (max-width: 480px) {
    grid-template-columns: 1fr;
    gap: 20px;
  }
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
