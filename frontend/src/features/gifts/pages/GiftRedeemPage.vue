<template>
  <div class="gift-redeem-page">
    <section class="gift-redeem-card glass-card">
      <template v-if="!authStore.isAuthenticated">
        <span class="eyebrow">Prezent</span>
        <h1>Zrealizuj kod prezentowy</h1>
        <p>Zaloguj sie lub utworz konto, aby przypisac kurs do swojej biblioteki.</p>
        <div class="gift-redeem-actions">
          <router-link class="btn btn-primary" :to="{ name: 'Login', query: { redirect: route.fullPath } }">
            Zaloguj sie
          </router-link>
          <router-link class="btn btn-ghost" :to="{ name: 'Register', query: { redirect: route.fullPath } }">
            Utworz konto
          </router-link>
        </div>
      </template>

      <template v-else>
        <span class="eyebrow">Prezent</span>
        <h1>Zrealizuj kod prezentowy</h1>
        <p>Wpisz kod z wiadomosci e-mail. Kurs pojawi sie od razu w Twoich kursach.</p>
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

.gift-redeem-page {
  display: grid;
  min-height: 58vh;
  place-items: center;
  padding: 56px 24px 88px;
}

.gift-redeem-card {
  --lg-r: 28px;
  --lg-blur: 2px;
  width: min(520px, 100%);
  padding: 42px 36px;
  text-align: center;

  h1 {
    margin: 14px 0 10px;
    font-family: $font-display;
    font-size: clamp(1.55rem, 5vw, 2rem);
    letter-spacing: -0.03em;
  }

  > p {
    color: $color-muted;
    line-height: 1.6;
  }

  @media (max-width: 520px) {
    padding: 34px 22px;
  }
}

.gift-redeem-actions {
  display: flex;
  justify-content: center;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: 28px;
}

.gift-redeem-form {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-top: 28px;
  text-align: left;

  label {
    color: $color-muted;
    font-size: 0.86rem;
    font-weight: 600;
  }

  input {
    width: 100%;
    min-width: 0;
    height: 50px;
    border: 1px solid rgba(255, 255, 255, 0.14);
    border-radius: 14px;
    padding: 0 16px;
    background: rgba(255, 255, 255, 0.05);
    color: $color-ink;
    font: inherit;
    font-weight: 700;
    letter-spacing: 0.04em;
    outline: 0;

    &:focus {
      border-color: rgba(34, 211, 238, 0.58);
      box-shadow: 0 0 0 4px rgba(34, 211, 238, 0.14);
    }
  }

  .btn {
    width: 100%;
    margin-top: 6px;
  }
}
</style>
