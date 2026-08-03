<template>
  <div class="pricing-page">
    <section class="pricing-hero">
      <div class="container">
        <span class="eyebrow">Cennik</span>
        <h1>Wybierz dostęp, który pasuje do Twojego sposobu nauki</h1>
        <p>Kup pojedynczy kurs, ucz się bez limitu z All-access lub wyposaż w wiedzę cały zespół.</p>
      </div>
    </section>

    <section class="pricing-content">
      <div class="container">
        <div class="pricing-grid">
          <article class="pricing-card glass-card">
            <span class="pricing-card__eyebrow">Pojedynczy kurs</span>
            <h2>Kupujesz tylko to, czego potrzebujesz</h2>
            <p>Jednorazowa płatność daje stały dostęp do kupionego kursu z Twojego konta.</p>
            <ul>
              <li>Dostęp bez terminu ważności</li>
              <li>Materiały i postęp zapisane na koncie</li>
              <li>Certyfikat po ukończeniu</li>
            </ul>
            <router-link class="btn btn-ghost" :to="{ name: 'Courses' }">Przeglądaj kursy</router-link>
          </article>

          <article class="pricing-card pricing-card--featured glass-card">
            <span class="pricing-card__eyebrow">All-access</span>
            <h2>Cała biblioteka w jednej subskrypcji</h2>
            <p class="price"><strong>{{ monthlyPrice }} zł</strong><span>/ mies.</span></p>
            <p>Ucz się w dowolnym tempie i korzystaj ze wszystkich opublikowanych kursów.</p>
            <ul>
              <li>Dostęp do całej biblioteki</li>
              <li>Elastyczne zarządzanie planem</li>
              <li>Nowe kursy dostępne od razu</li>
            </ul>
            <button
              v-if="authStore.isAuthenticated"
              type="button"
              class="btn btn-primary"
              :disabled="subscriptionMutation.isPending.value"
              @click="subscriptionMutation.mutate()"
            >
              {{ subscriptionMutation.isPending.value ? 'Przekierowujemy…' : 'Aktywuj All-access' }}
            </button>
            <router-link v-else class="btn btn-primary" :to="{ name: 'Register', query: { redirect: '/pricing' } }">
              Załóż konto i subskrybuj
            </router-link>
          </article>

          <article class="pricing-card glass-card">
            <span class="pricing-card__eyebrow">Dla firm</span>
            <h2>Rozwój całego zespołu</h2>
            <p>Plany dla firm ułatwiają zakup dostępu dla wielu osób i dają wgląd w postępy nauki.</p>
            <ul>
              <li>Plany dopasowane do wielkości zespołu</li>
              <li>Wspólne rozliczenie</li>
              <li>Wsparcie przy wdrożeniu</li>
            </ul>
            <router-link class="btn btn-ghost" :to="{ name: 'Business' }">Poznaj ofertę dla firm</router-link>
          </article>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useCreateSubscriptionCheckout, useSubscriptionOffer } from '@/features/payments/composables/usePayments'

const authStore = useAuthStore()
const subscriptionOfferQuery = useSubscriptionOffer()
const subscriptionMutation = useCreateSubscriptionCheckout()
const monthlyPrice = computed(() => subscriptionOfferQuery.data.value?.monthlyPricePln ?? 399)
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.pricing-hero {
  padding: 44px 0 38px;
  text-align: center;
  border-bottom: 1px solid $color-hairline;

  .eyebrow {
    margin-bottom: 18px;
  }

  h1 {
    max-width: 800px;
    margin: 0 auto 14px;
    font-family: $font-display;
    font-size: $font-size-xl;
    line-height: 1.08;
    letter-spacing: -0.03em;
  }

  p {
    max-width: 660px;
    margin: 0 auto;
    color: $color-muted;
    line-height: 1.65;
  }
}

.pricing-content {
  padding: 56px 0 96px;
}

.pricing-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 20px;
  align-items: stretch;

  @media (max-width: 920px) {
    grid-template-columns: 1fr;
    max-width: 520px;
    margin: 0 auto;
  }
}

.pricing-card {
  --lg-r: 24px;
  --lg-blur: 2px;
  display: flex;
  flex-direction: column;
  padding: 30px 28px;

  h2 {
    margin: 14px 0 12px;
    font-family: $font-display;
    font-size: 1.28rem;
    line-height: 1.25;
  }

  > p:not(.price) {
    color: $color-muted;
    line-height: 1.6;
  }

  ul {
    display: grid;
    gap: 11px;
    margin: 26px 0 30px;
    color: $color-muted;
    font-size: 0.92rem;
  }

  li {
    display: flex;
    gap: 9px;
    align-items: flex-start;

    &::before {
      content: '✓';
      color: $color-gold;
      font-weight: 800;
    }
  }

  .btn {
    width: 100%;
    margin-top: auto;
  }
}

.pricing-card--featured {
  --lg-tint: rgba(245, 158, 11, 0.1);
  box-shadow: 0 20px 52px rgba(245, 158, 11, 0.16), 0 10px 30px rgba(3, 6, 24, 0.35);
}

.pricing-card__eyebrow {
  color: $color-gold;
  font-size: 0.76rem;
  font-weight: 700;
  letter-spacing: 0.11em;
  text-transform: uppercase;
}

.price {
  display: flex;
  align-items: baseline;
  gap: 7px;
  margin: 0 0 12px;

  strong {
    font-family: $font-display;
    font-size: 2.2rem;
    letter-spacing: -0.04em;
  }

  span {
    color: $color-faint;
  }
}
</style>
