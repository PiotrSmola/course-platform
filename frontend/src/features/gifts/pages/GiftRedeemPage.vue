<template>
  <div class="gift-redeem-page">
    <section class="gift-redeem-card glass-card">
      <template v-if="!authStore.isAuthenticated">
        <span class="eyebrow">Prezent</span>
        <h1>Zrealizuj kod <em>prezentowy</em></h1>
        <p>Zaloguj się lub utwórz konto, aby przypisać kurs do swojej biblioteki.</p>
        <div class="gift-redeem-actions">
          <router-link class="btn btn-primary" :to="{ name: 'Login', query: { redirect: route.fullPath } }">
            Zaloguj się
          </router-link>
          <router-link class="btn btn-ghost" :to="{ name: 'Register', query: { redirect: route.fullPath } }">
            Utwórz konto
          </router-link>
        </div>
      </template>

      <template v-else>
        <span class="eyebrow">Prezent</span>
        <h1>Zrealizuj kod <em>prezentowy</em></h1>
        <p>Wpisz kod z wiadomości e-mail. Kurs pojawi się od razu w Twoich kursach.</p>
        <form class="gift-redeem-form" @submit.prevent="redeem">
          <label for="gift-code">Kod prezentowy</label>
          <input
            id="gift-code"
            v-model.trim="code"
            type="text"
            autocomplete="off"
            autocapitalize="characters"
            spellcheck="false"
            placeholder="GIFT-..."
            :disabled="redeemMutation.isPending.value"
          />
          <button type="submit" class="btn btn-primary" :disabled="!code || redeemMutation.isPending.value">
            {{ redeemMutation.isPending.value ? 'Realizujemy...' : 'Dodaj kurs do konta' }}
          </button>
        </form>
      </template>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useRedeemGiftCode } from '@/features/gifts/composables/useGifts'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const code = ref('')
const redeemMutation = useRedeemGiftCode()

async function redeem() {
  try {
    const result = await redeemMutation.mutateAsync(code.value.trim())
    await router.push({ name: 'CourseDetails', params: { id: result.courseId } })
  } catch {
    return
  }
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.gift-redeem-page {
  display: grid;
  min-height: 58vh;
  place-items: center;
  padding: 56px 24px 88px;
}

.gift-redeem-card {
  --lg-blur: 10px;
  width: min(540px, 100%);
  padding: 30px 32px 32px;

  &:hover {
    transform: none;
  }

  h1 {
    margin: 18px 0 10px;
    font-size: clamp(1.8rem, 5vw, 2.3rem);
    font-weight: 600;
  }

  > p {
    color: $color-muted;
    line-height: 1.6;
  }

  @media (max-width: 520px) {
    padding: 26px 22px;
  }
}

.gift-redeem-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: 28px;
}

.gift-redeem-form {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-top: 24px;
  padding-top: 20px;
  border-top: 1px dashed $color-hairline;

  label {
    @include mono-label;
    font-size: 0.62rem;
    color: $color-faint;
  }

  input {
    width: 100%;
    min-width: 0;
    height: 50px;
    border: none;
    border-radius: $radius-md;
    padding: 0 16px;
    background: rgba(13, 15, 19, 0.45);
    box-shadow: inset 0 0 0 1px $color-hairline;
    color: $color-ink;
    font-family: $font-mono;
    font-size: 0.95rem;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    outline: 0;
    transition: box-shadow 0.2s;

    &::placeholder {
      color: $color-faint;
    }

    &:focus {
      box-shadow:
        inset 0 0 0 1px $color-hairline,
        0 0 0 2px rgba($color-accent, 0.6);
    }
  }

  .btn {
    width: 100%;
    margin-top: 6px;
  }
}
</style>
