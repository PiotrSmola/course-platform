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

    <section class="pricing-plans">
      <div class="container">
        <div class="plans-layout">
          <article class="plan-featured paper-card">
            <div class="plan-featured__top">
              <span class="plan-featured__num">No. 02</span>
              <span class="plan-featured__name">All-access</span>
              <span class="tag">Polecany</span>
            </div>
            <h2>Cała biblioteka w jednej <em>subskrypcji</em></h2>
            <p class="plan-featured__price">
              <strong>{{ monthlyPrice }}</strong>
              <span>zł / mies.</span>
            </p>
            <p class="plan-featured__lead">Ucz się w dowolnym tempie i korzystaj ze wszystkich opublikowanych kursów.</p>
            <ul class="plan-featured__list">
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

          <article class="plan-single glass-card is-static">
            <span class="plan-single__num">01</span>
            <div class="plan-single__body">
              <span class="plan-label">Pojedynczy kurs</span>
              <h2>Kupujesz tylko to, czego potrzebujesz</h2>
              <p>Jednorazowa płatność daje stały dostęp do kupionego kursu z Twojego konta.</p>
              <div class="plan-single__tags">
                <span class="tag">Bez terminu ważności</span>
                <span class="tag">Postęp na koncie</span>
                <span class="tag">Certyfikat</span>
              </div>
              <router-link class="btn btn-ghost" :to="{ name: 'Courses' }">Przeglądaj kursy</router-link>
            </div>
          </article>

          <article class="plan-business outline-card">
            <div class="plan-business__head">
              <span class="plan-label">No. 03 · Dla firm</span>
              <h2>Rozwój całego zespołu</h2>
            </div>
            <dl class="plan-business__spec">
              <div>
                <dt>Plany</dt>
                <dd>Dopasowane do wielkości zespołu</dd>
              </div>
              <div>
                <dt>Rozliczenie</dt>
                <dd>Wspólne dla całej firmy</dd>
              </div>
              <div>
                <dt>Postępy</dt>
                <dd>Wgląd w naukę zespołu</dd>
              </div>
              <div>
                <dt>Start</dt>
                <dd>Wsparcie przy wdrożeniu</dd>
              </div>
            </dl>
            <router-link class="plan-business__link" :to="{ name: 'Business' }">Poznaj ofertę dla firm →</router-link>
          </article>
        </div>
      </div>
    </section>

    <section class="pricing-compare">
      <div class="container">
        <header class="section-head">
          <span class="eyebrow">Porównanie</span>
          <h2>Co dokładnie <em>dostajesz</em></h2>
          <p>Wszystkie formy dostępu dają te same lekcje, materiały i quizy. Różni je zakres biblioteki i sposób rozliczenia.</p>
        </header>

        <div class="compare-panel glass-panel">
          <table class="compare-table">
            <thead>
              <tr>
                <th scope="col">Funkcja</th>
                <th v-for="(plan, index) in comparePlans" :key="plan" scope="col" :class="{ 'is-featured': index === 1 }">
                  {{ plan }}
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in compareRows" :key="row.feature">
                <th scope="row">{{ row.feature }}</th>
                <td
                  v-for="(value, index) in row.values"
                  :key="comparePlans[index]"
                  :data-plan="comparePlans[index]"
                  :class="{ 'is-featured': index === 1 }"
                >
                  <svg v-if="value === true" class="compare-yes" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-label="Tak" role="img">
                    <path d="M5 12.5l4.5 4.5L19 7.5" />
                  </svg>
                  <span v-else-if="value === false" class="compare-no" aria-label="Nie">—</span>
                  <span v-else class="compare-text">{{ value }}</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </section>

    <section class="pricing-payments">
      <div class="container">
        <header class="section-head">
          <span class="eyebrow">Płatności</span>
          <h2>Jak <em>płacisz</em> i zarządzasz dostępem</h2>
        </header>

        <div class="payments-layout">
          <dl class="payments-rows">
            <div class="payments-row">
              <dt>Karta · Stripe</dt>
              <dd>
                <p>Płacisz kartą w bezpiecznym Stripe Checkout. Po opłaceniu zamówienia wracasz na platformę, a dostęp pojawia się na Twoim koncie po potwierdzeniu płatności.</p>
              </dd>
            </div>
            <div class="payments-row">
              <dt>Kody rabatowe</dt>
              <dd>
                <p>Kod rabatowy wpisujesz przy zakupie pojedynczego kursu. Obniżoną cenę widzisz jeszcze przed przejściem do płatności.</p>
              </dd>
            </div>
            <div class="payments-row">
              <dt>Subskrypcja</dt>
              <dd>
                <p>Subskrypcją All-access zarządzasz w panelu rozliczeń Stripe, do którego przejdziesz ze swojego profilu.</p>
              </dd>
            </div>
            <div class="payments-row">
              <dt>Prezent</dt>
              <dd>
                <p>Kurs możesz kupić komuś w prezencie. Odbiorca otrzyma jednorazowy kod, który doda kurs do jego konta.</p>
                <router-link class="payments-link" :to="{ name: 'GiftRedeem' }">Masz kod prezentowy? →</router-link>
              </dd>
            </div>
          </dl>

          <aside class="trial-ticket ticket-card">
            <span class="tag">Na start</span>
            <h3>Bezpłatny trial wybranego kursu</h3>
            <p>Wybierz jeden kurs i sprawdź jego pierwsze lekcje, zanim zdecydujesz o zakupie lub subskrypcji.</p>
            <router-link class="btn btn-primary" :to="trialDestination">Wypróbuj za darmo</router-link>
          </aside>
        </div>
      </div>
    </section>

    <section class="pricing-faq">
      <div class="container pricing-faq__layout">
        <div class="pricing-faq__copy">
          <span class="eyebrow">Pytania</span>
          <h2>Zanim <em>kupisz</em></h2>
          <p>Krótkie odpowiedzi o trialu, zakupie kursu i subskrypcji.</p>
          <router-link class="btn btn-ghost btn-pill" :to="{ name: 'Faq' }">Wszystkie pytania</router-link>
        </div>
        <FaqList :compact="true" />
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useCreateSubscriptionCheckout, useSubscriptionOffer } from '@/features/payments/composables/usePayments'
import FaqList from '@/features/public/components/FaqList.vue'

type CompareValue = boolean | string

const authStore = useAuthStore()
const subscriptionOfferQuery = useSubscriptionOffer()
const subscriptionMutation = useCreateSubscriptionCheckout()
const monthlyPrice = computed(() => subscriptionOfferQuery.data.value?.monthlyPricePln ?? 399)

const trialDestination = computed(() =>
  authStore.isAuthenticated
    ? { name: 'Trial' }
    : { name: 'Register', query: { redirect: '/trial' } }
)

const comparePlans = ['Pojedynczy kurs', 'All-access', 'Dla firm']

const compareRows: { feature: string; values: [CompareValue, CompareValue, CompareValue] }[] = [
  { feature: 'Dostęp do kursu', values: ['Bez terminu ważności', 'Z aktywną subskrypcją', 'W ramach planu'] },
  { feature: 'Wszystkie opublikowane kursy', values: [false, true, true] },
  { feature: 'Nowe kursy od razu', values: [false, true, true] },
  { feature: 'Certyfikat po ukończeniu', values: [true, true, true] },
  { feature: 'Pytania do instruktorów', values: [true, true, true] },
  { feature: 'Materiały do pobrania', values: [true, true, true] },
  { feature: 'Panel postępów zespołu', values: [false, false, true] },
  { feature: 'Rozliczenie', values: ['Jednorazowe', 'Miesięczne', 'Wspólne dla firmy'] }
]
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

$paper-ink: #15171b;
$paper-line: rgba(21, 23, 27, 0.16);

.pricing-hero {
  padding: 32px 0 0;

  h1 {
    font-size: clamp(2.4rem, 4.6vw, 3.8rem);
    margin-top: 18px;
  }
}

.pricing-head {
  margin-bottom: 56px;
}

.pricing-plans,
.pricing-compare,
.pricing-payments {
  padding-bottom: 112px;
}

.pricing-faq {
  padding-bottom: 96px;
}

.plan-label {
  @include mono-label;
  color: $color-accent;
}

.plans-layout {
  display: grid;
  grid-template-columns: minmax(0, 1.1fr) minmax(0, 1fr);
  grid-template-rows: auto auto;
  gap: 24px 36px;
  align-items: start;

  @media (max-width: 920px) {
    grid-template-columns: 1fr;
    max-width: 600px;
  }
}

.plan-featured {
  grid-row: 1 / span 2;
  display: flex;
  flex-direction: column;
  padding: 30px 34px 34px;
  transform: rotate(-1deg);

  h2 {
    margin: 0 0 24px;
    font-size: clamp(1.7rem, 2.6vw, 2.2rem);
    font-weight: 600;
    line-height: 1.15;
    color: $paper-ink;
  }

  .btn {
    align-self: flex-start;
    min-width: 240px;
  }

  @media (max-width: 920px) {
    grid-row: auto;
    transform: rotate(-0.6deg);
  }

  @media (max-width: 560px) {
    padding: 24px 22px 26px;

    .btn {
      width: 100%;
      min-width: 0;
    }
  }
}

.plan-featured__top {
  display: flex;
  align-items: center;
  gap: 14px;
  padding-bottom: 16px;
  margin-bottom: 26px;
  border-bottom: 1px solid $paper-ink;

  .tag {
    margin-left: auto;
  }
}

.plan-featured__num {
  font-family: $font-mono;
  font-size: 0.72rem;
  color: rgba(21, 23, 27, 0.55);
}

.plan-featured__name {
  @include mono-label;
  color: $paper-ink;
}

.plan-featured__price {
  display: flex;
  align-items: baseline;
  flex-wrap: wrap;
  gap: 4px 14px;
  margin: 0 0 18px;
  padding: 18px 0;
  border-top: 1px dashed $paper-line;
  border-bottom: 1px dashed $paper-line;

  strong {
    font-family: $font-display;
    font-size: clamp(4.6rem, 9vw, 7rem);
    font-weight: 600;
    line-height: 0.9;
    letter-spacing: -0.015em;
    color: $paper-ink;
  }

  span {
    font-family: $font-mono;
    font-size: 0.86rem;
    letter-spacing: 0.06em;
    text-transform: uppercase;
    color: rgba(21, 23, 27, 0.6);
  }
}

.plan-featured__lead {
  max-width: 44ch;
  line-height: 1.6;
}

.plan-featured__list {
  display: grid;
  margin: 24px 0 32px;
  padding: 0;
  list-style: none;

  li {
    display: flex;
    gap: 14px;
    align-items: baseline;
    padding: 10px 0;
    font-weight: 500;
    color: $paper-ink;
    border-bottom: 1px solid $paper-line;

    &::before {
      content: "✓";
      font-family: $font-mono;
      font-weight: 700;
      color: $paper-ink;
    }
  }
}

.plan-single {
  --lg-blur: 10px;
  display: grid;
  grid-template-columns: auto minmax(0, 1fr);
  gap: 24px;
  padding: 26px 28px 28px;

  h2 {
    margin: 10px 0 10px;
    font-size: 1.4rem;
    font-weight: 600;
    line-height: 1.2;
  }

  p {
    color: $color-muted;
    line-height: 1.6;
  }

  .btn {
    margin-top: 22px;
  }

  @media (max-width: 560px) {
    grid-template-columns: 1fr;
    gap: 6px;
    padding: 22px 20px 24px;

    .btn {
      width: 100%;
    }
  }
}

.plan-single__num {
  font-family: $font-display;
  font-size: 3.6rem;
  font-weight: 500;
  line-height: 0.9;
  color: rgba($color-ink, 0.22);

  @media (max-width: 560px) {
    font-size: 2.4rem;
  }
}

.plan-single__tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 18px;
}

.plan-business {
  padding: 24px 28px 22px;

  h2 {
    margin: 10px 0 0;
    font-size: 1.4rem;
    font-weight: 600;
  }

  @media (max-width: 560px) {
    padding: 22px 20px 20px;
  }
}

.plan-business__head {
  padding-bottom: 18px;
}

.plan-business__spec {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  margin: 0;
  border-top: 1px dashed rgba($color-ink, 0.22);

  > div {
    padding: 14px 16px 14px 0;
    border-bottom: 1px dashed rgba($color-ink, 0.22);
  }

  > div:nth-child(even) {
    padding-left: 16px;
    border-left: 1px dashed rgba($color-ink, 0.22);
  }

  dt {
    @include mono-label;
    font-size: 0.6rem;
    color: $color-faint;
  }

  dd {
    margin: 6px 0 0;
    font-size: 0.92rem;
    color: $color-ink;
  }

  @media (max-width: 420px) {
    grid-template-columns: 1fr;

    > div:nth-child(even) {
      padding-left: 0;
      border-left: 0;
    }
  }
}

.plan-business__link {
  @include mono-label;
  display: inline-block;
  margin-top: 18px;
  color: $color-muted;
  text-decoration: none;
  transition: color 0.2s;

  &:hover {
    color: $color-accent;
  }
}

.pricing-compare h2,
.pricing-payments h2 {
  font-size: clamp(1.9rem, 3.4vw, 2.6rem);
}

.compare-panel {
  padding: 12px 28px 16px;

  @media (max-width: 640px) {
    padding: 8px 18px 12px;
  }
}

.compare-table {
  width: 100%;
  border-collapse: collapse;
  table-layout: fixed;

  th,
  td {
    padding: 16px 12px;
    text-align: center;
    vertical-align: middle;
    border-bottom: 1px solid $color-hairline;
  }

  thead th {
    @include mono-label;
    padding-top: 20px;
    padding-bottom: 18px;
    color: $color-faint;
    border-bottom-color: rgba($color-ink, 0.4);

    &.is-featured {
      color: $color-accent;
    }
  }

  th:first-child {
    width: 34%;
    padding-left: 0;
    text-align: left;
  }

  tbody th {
    font-weight: 500;
    color: $color-ink;
  }

  tbody tr:last-child > * {
    border-bottom: 0;
  }

  td.is-featured {
    background: rgba($color-accent, 0.06);
  }
}

.compare-yes {
  display: inline-block;
  width: 20px;
  height: 20px;
  color: $color-accent;
  vertical-align: middle;
}

.compare-no {
  font-family: $font-mono;
  color: $color-faint;
}

.compare-text {
  font-family: $font-mono;
  font-size: 0.78rem;
  line-height: 1.4;
  color: $color-muted;
}

@media (max-width: 640px) {
  .compare-table {
    table-layout: auto;

    thead {
      position: absolute;
      width: 1px;
      height: 1px;
      overflow: hidden;
      clip-path: inset(50%);
    }

    tbody tr {
      display: grid;
      grid-template-columns: repeat(3, minmax(0, 1fr));
      padding: 16px 0;
      border-bottom: 1px solid $color-hairline;
    }

    tbody tr:last-child {
      border-bottom: 0;
    }

    tbody th,
    td {
      padding: 0;
      border-bottom: 0;
    }

    tbody th:first-child {
      grid-column: 1 / -1;
      width: auto;
      margin-bottom: 12px;
    }

    td {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      gap: 6px;
      padding: 8px 8px 8px 0;
      text-align: left;

      &::before {
        content: attr(data-plan);
        @include mono-label;
        font-size: 0.56rem;
        letter-spacing: 0.08em;
        color: $color-faint;
      }
    }

    td + td {
      padding-left: 8px;
    }

    td.is-featured {
      &::before {
        color: $color-accent;
      }
    }
  }
}

.payments-layout {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 340px);
  gap: 56px;
  align-items: start;

  @media (max-width: 920px) {
    grid-template-columns: 1fr;
    gap: 40px;
  }
}

.payments-rows {
  margin: 0;
  border-top: 1px solid $color-ink;
}

.payments-row {
  display: grid;
  grid-template-columns: minmax(0, 200px) minmax(0, 1fr);
  gap: 32px;
  padding: 24px 0;
  border-bottom: 1px solid $color-hairline;

  dt {
    @include mono-label;
    padding-top: 4px;
    color: $color-ink;
  }

  dd {
    margin: 0;

    p {
      max-width: 62ch;
      color: $color-muted;
      line-height: 1.65;
    }
  }

  @media (max-width: 620px) {
    grid-template-columns: 1fr;
    gap: 10px;
    padding: 20px 0;
  }
}

.payments-link {
  @include mono-label;
  display: inline-block;
  margin-top: 12px;
  color: $color-accent;
  text-decoration: none;

  &:hover {
    text-decoration: underline;
    text-underline-offset: 4px;
  }
}

.trial-ticket {
  padding: 24px 26px 26px;
  transform: rotate(1.2deg);

  h3 {
    margin: 18px 0 10px;
    font-size: 1.5rem;
    font-weight: 600;
    line-height: 1.15;
    color: $color-on-accent;
  }

  p {
    line-height: 1.55;
  }

  .btn {
    width: 100%;
    margin-top: 22px;
  }

  @media (max-width: 920px) {
    max-width: 420px;
    transform: none;
  }
}

.pricing-faq__layout {
  display: grid;
  grid-template-columns: minmax(0, 0.8fr) minmax(0, 1.6fr);
  gap: 48px;
  padding-top: 56px;
  border-top: 1px solid $color-hairline;

  @media (max-width: 860px) {
    grid-template-columns: 1fr;
    gap: 28px;
  }
}

.pricing-faq__copy {
  h2 {
    margin: 14px 0 12px;
    font-size: clamp(1.8rem, 3vw, 2.4rem);
  }

  p {
    color: $color-muted;
    line-height: 1.6;
  }

  .btn {
    margin-top: 22px;
    padding: 10px 20px;
    font-size: 0.86rem;
  }
}
</style>
