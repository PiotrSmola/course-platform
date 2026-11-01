<template>
  <div class="confirm-email-page">
    <div v-if="status === 'loading'" class="state">
      <div class="spinner" />
      <p>Potwierdzanie adresu email...</p>
    </div>
    <ErrorState
      v-else-if="status === 'error'"
      title="Nie udało się potwierdzić emaila"
      :description="errorMessage"
      action-label="Przejdź do logowania"
      action-to="Login"
    />
    <div v-else class="state success">
      <h2>Email potwierdzony</h2>
      <p>Twój adres email został pomyślnie zweryfikowany. Możesz się teraz zalogować.</p>
      <router-link class="btn btn-primary" :to="{ name: 'Login' }">Zaloguj się</router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { confirmEmail } from '@/features/auth/api/auth.api'
import { getApiErrorMessage } from '@/shared/api/apiError'
import ErrorState from '@/shared/components/ui/ErrorState.vue'

const route = useRoute()
const status = ref<'loading' | 'success' | 'error'>('loading')
const errorMessage = ref('Link potwierdzający jest nieprawidłowy lub wygasł.')

onMounted(async () => {
  const email = route.query.email as string | undefined
  const token = route.query.token as string | undefined

  if (!email || !token) {
    status.value = 'error'
    errorMessage.value = 'Brak wymaganych parametrów w linku potwierdzającym.'
    return
  }

  try {
    await confirmEmail({ email, token })
    status.value = 'success'
  } catch (error) {
    status.value = 'error'
    errorMessage.value = getApiErrorMessage(error) || errorMessage.value
  }
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.state {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 16px;

  p {
    color: $color-muted;
    font-size: 0.94rem;
    line-height: 1.6;
  }

  &.success h2 {
    font-size: 2.4rem;
    line-height: 1.05;
  }

  .btn {
    margin-top: 6px;
  }
}

.spinner {
  width: 28px;
  height: 28px;
  border: 2px solid rgba($color-ink, 0.12);
  border-top-color: $color-accent;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
