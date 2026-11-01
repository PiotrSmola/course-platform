<template>
  <div class="pricing-page">
    <section class="pricing-hero">
      <div class="container">
        <header class="section-head pricing-head">
          <span class="eyebrow">Cennik</span>
          <h1>Wybierz dostęp, który pasuje do Twojego <em>sposobu nauki</em></h1>
          <p>Kup pojedynczy kurs, ucz się bez limitu z All-access lub wyposaż w wiedzę cały zespół.</p>
        </header>
      </div>
    </section>

    <section class="pricing-content">
      <div class="container">
        <div class="pricing-grid">
          <article class="pricing-card glass-card">
            <div class="pricing-card__top">
              <span class="pricing-card__num">01</span>
              <span class="pricing-card__eyebrow">Pojedynczy kurs</span>
            </div>
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
            <div class="pricing-card__top">
              <span class="pricing-card__num">02</span>
              <span class="pricing-card__eyebrow">All-access</span>
              <span class="pricing-card__flag">Polecany</span>
            </div>
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
            <div class="pricing-card__top">
              <span class="pricing-card__num">03</span>
              <span class="pricing-card__eyebrow">Dla firm</span>
            </div>
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
@use "@/assets/styles/abstracts/mixins" as *;

.pricing-hero {
  padding: 32px 0 0;

  h1 {
    font-size: clamp(2.4rem, 4.6vw, 3.8rem);
    margin-top: 18px;
  }
}

.pricing-head {
  margin-bottom: 48px;
}

.pricing-content {
  padding: 0 0 96px;
}

.pricing-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 20px;
  align-items: stretch;

  @media (max-width: 920px) {
    grid-template-columns: 1fr;
    max-width: 560px;
  }
}

.pricing-card {
  --lg-blur: 10px;
  display: flex;
  flex-direction: column;
  padding: 24px 26px 26px;

  h2 {
    margin: 0 0 14px;
    font-size: 1.45rem;
    font-weight: 600;
    line-height: 1.2;
  }

  > p:not(.price) {
    color: $color-muted;
    line-height: 1.6;
  }

  ul {
    display: grid;
    margin: 24px 0 30px;
    padding: 0;
    list-style: none;
    font-size: 0.92rem;
    color: $color-ink;
    border-top: 1px dashed $color-hairline;
  }

  li {
    display: flex;
    gap: 12px;
    align-items: baseline;
    padding: 11px 0;
    border-bottom: 1px dashed $color-hairline;

    &::before {
      content: "—";
      font-family: $font-mono;
      color: $color-accent;
    }
  }

  .btn {
    width: 100%;
    margin-top: auto;
  }
}

.pricing-card__top {
  display: flex;
  align-items: center;
  gap: 12px;
  padding-bottom: 14px;
  margin-bottom: 20px;
  border-bottom: 1px solid $color-hairline;
}

.pricing-card__num {
  font-family: $font-mono;
  font-size: 0.72rem;
  color: $color-faint;
}

.pricing-card__eyebrow {
  @include mono-label;
  color: $color-ink;
}

.pricing-card__flag {
  @include tag;
  margin-left: auto;
  color: $color-on-accent;
  background: $color-accent;
  box-shadow: none;
}

.pricing-card--featured {
  --lg-tint: rgba(13, 15, 19, 0.4);
  box-shadow:
    0 0 0 1px rgba($color-accent, 0.45),
    0 28px 60px -24px rgba($color-accent, 0.28),
    0 12px 32px -8px rgba(0, 0, 0, 0.45);

  &:hover {
    box-shadow:
      0 0 0 1px rgba($color-accent, 0.6),
      0 32px 64px -24px rgba($color-accent, 0.32),
      0 12px 32px -8px rgba(0, 0, 0, 0.5);
  }

  .pricing-card__eyebrow {
    color: $color-accent;
  }
}

.price {
  display: flex;
  align-items: baseline;
  gap: 10px;
  margin: 4px 0 14px;

  strong {
    font-family: $font-display;
    font-size: 3.4rem;
    font-weight: 600;
    line-height: 1;
    letter-spacing: -0.015em;
  }

  span {
    font-family: $font-mono;
    font-size: 0.8rem;
    color: $color-faint;
  }
}
</style>
