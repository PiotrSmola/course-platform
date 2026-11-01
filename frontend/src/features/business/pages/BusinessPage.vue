<template>
  <div class="business-page">
    <div class="business-hero">
      <div class="container">
        <header class="section-head business-head">
          <span class="eyebrow">Dla firm</span>
          <h1 class="business-title">Szkolenia dla zespołów, które chcą <em>rosnąć</em></h1>
          <p class="business-subtitle">
            Wybierz plan dopasowany do skali Twojej organizacji. Daj zespołowi dostęp do pełnej biblioteki kursów, panel postępów i wsparcie, które rośnie razem z firmą.
          </p>
        </header>
      </div>
    </div>

    <div class="container business-body">
      <div v-if="isLoading" class="business-loading">
        <div class="spinner" />
        <p>Ładowanie planów...</p>
      </div>

      <div v-else-if="isError" class="business-error">
        <p>Nie udało się pobrać planów. Spróbuj ponownie.</p>
      </div>

      <template v-else>
        <div class="plans-grid">
          <PlanCard
            v-for="(plan, index) in items"
            :key="plan.id"
            :plan="plan"
            :index="index"
            :variant="variants[index]"
          />
        </div>
        <p class="plans-note">
          Żaden plan nie pasuje do Waszej skali?
          <router-link :to="{ name: 'Contact' }">Napisz do nas</router-link>
          — dobierzemy zakres do zespołu.
        </p>
      </template>
    </div>

    <section class="container business-section">
      <header class="section-head">
        <span class="eyebrow">Zakres</span>
        <h2>Co dostaje <em>Twój zespół</em></h2>
        <p>Zespół korzysta z tych samych narzędzi co każdy kursant: kursów z lekcjami wideo, quizów, materiałów i certyfikatów.</p>
      </header>

      <div class="offer">
        <article class="offer-main glass-panel">
          <span class="offer-no">01</span>
          <h3 class="offer-title">Dostęp do biblioteki <em>kursów</em></h3>
          <p class="offer-text">
            Kursy podzielone na moduły i lekcje wideo — od podstaw po tematy zaawansowane. Uczestnicy przeglądają katalog według kategorii i technologii, a każdy kurs ma ten sam zestaw narzędzi do nauki.
          </p>
          <ul class="offer-strip">
            <li v-for="item in courseParts" :key="item">{{ item }}</li>
          </ul>
          <div class="offer-links">
            <router-link class="btn btn-ghost" :to="{ name: 'Courses' }">Katalog kursów</router-link>
            <router-link class="btn btn-ghost btn-pill" :to="{ name: 'CategoriesList' }">Kategorie</router-link>
          </div>
        </article>

        <ol class="offer-list">
          <li v-for="(item, index) in offerItems" :key="item.title" class="offer-row">
            <div class="offer-row-head">
              <span class="offer-row-no">{{ pad(index + 2) }}</span>
              <h3 class="offer-row-title">{{ item.title }}</h3>
            </div>
            <div class="offer-row-body">
              <p>{{ item.text }}</p>
              <router-link v-if="item.to" class="offer-row-link" :to="item.to">
                {{ item.linkText }}
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6" /></svg>
              </router-link>
            </div>
          </li>
        </ol>
      </div>
    </section>

    <section class="container business-section rollout">
      <header class="rollout-head">
        <span class="eyebrow">Wdrożenie</span>
        <h2>Jak wygląda <em>wdrożenie</em></h2>
        <p>Cztery kroki od pierwszej wiadomości do zespołu, który uczy się na platformie.</p>
        <router-link class="btn btn-primary" :to="{ name: 'Contact' }">Zacznij od kontaktu</router-link>
      </header>

      <ol class="rollout-steps">
        <li v-for="(step, index) in rolloutSteps" :key="step.title" class="rollout-step">
          <span class="rollout-num">{{ pad(index + 1) }}</span>
          <div class="rollout-body">
            <h3>{{ step.title }}</h3>
            <p>{{ step.text }}</p>
          </div>
        </li>
      </ol>
    </section>

    <section class="container business-section faq">
      <header class="section-head">
        <span class="eyebrow">Pytania firm</span>
        <h2>Zanim <em>napiszesz</em></h2>
        <p>Najczęstsze pytania od zespołów. Pozostałe odpowiedzi znajdziesz w FAQ albo w rozmowie z nami.</p>
      </header>

      <ul class="faq-list">
        <li v-for="(item, index) in faqItems" :key="item.question" class="faq-item" :class="{ 'is-open': openFaq === index }">
          <button
            :id="`biz-faq-q-${index}`"
            type="button"
            class="faq-question"
            :aria-expanded="openFaq === index"
            :aria-controls="`biz-faq-a-${index}`"
            @click="toggleFaq(index)"
          >
            <span class="faq-num">{{ pad(index + 1) }}</span>
            <span class="faq-text">{{ item.question }}</span>
            <span class="faq-icon" aria-hidden="true" />
          </button>
          <div
            v-show="openFaq === index"
            :id="`biz-faq-a-${index}`"
            class="faq-answer"
            role="region"
            :aria-labelledby="`biz-faq-q-${index}`"
          >
            <p>{{ item.answer }}</p>
          </div>
        </li>
      </ul>
    </section>

    <section class="container business-section">
      <div class="final-cta glass-panel">
        <div class="final-text">
          <span class="eyebrow">Kontakt</span>
          <h2 class="final-title">Porozmawiajmy o <em>Twoim zespole</em></h2>
          <p>Napisz, ilu osób dotyczy szkolenie i jakie tematy są dla Was ważne. Odpowiemy z propozycją planu i ścieżek nauki.</p>
        </div>
        <div class="final-actions">
          <router-link class="btn btn-primary" :to="{ name: 'Contact' }">Formularz kontaktowy</router-link>
          <router-link class="btn btn-ghost" :to="{ name: 'Faq' }">Najczęstsze pytania</router-link>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import type { RouteLocationRaw } from 'vue-router'
import { useBusinessPlans } from '@/features/business/composables/useBusiness'
import PlanCard from '@/features/business/components/PlanCard.vue'

const { isLoading, isError, data } = useBusinessPlans()
const items = computed(() => data.value?.items ?? [])

const variants = computed(() => {
  let regular = 0
  return items.value.map((plan) => {
    if (plan.isFeatured) return 'paper' as const
    return regular++ % 2 === 0 ? ('glass' as const) : ('outline' as const)
  })
})

function pad(n: number): string {
  return String(n).padStart(2, '0')
}

const courseParts = ['Moduły', 'Lekcje wideo', 'Quizy', 'Materiały', 'Pytania i odpowiedzi', 'Certyfikat']

const offerItems: { title: string; text: string; to?: RouteLocationRaw; linkText?: string }[] = [
  {
    title: 'Ścieżki nauki dla ról w zespole',
    text: 'Gotowe zestawy kursów w ustalonej kolejności. Każda osoba może iść ścieżką dopasowaną do swojej roli, zamiast wybierać kursy na wyczucie.',
    to: { name: 'PathsList' },
    linkText: 'Zobacz ścieżki'
  },
  {
    title: 'Postęp i certyfikaty uczestników',
    text: 'Postęp liczony lekcja po lekcji, quizy do zaliczenia i certyfikat po każdym kursie. Numer certyfikatu można sprawdzić na publicznej stronie weryfikacji.'
  },
  {
    title: 'Pytania do instruktorów pod lekcjami',
    text: 'Pod każdą lekcją jest sekcja pytań i odpowiedzi. Uczestnik pyta w kontekście konkretnego materiału, a odpowiada instruktor kursu.'
  },
  {
    title: 'Materiały do pobrania',
    text: 'Pliki dołączone przez instruktora do lekcji są dostępne do pobrania bezpośrednio z widoku nauki.'
  }
]

const rolloutSteps = [
  {
    title: 'Kontakt',
    text: 'Piszesz przez formularz kontaktowy: ilu osób dotyczy szkolenie i jakich tematów potrzebujecie.'
  },
  {
    title: 'Dobór planu i ścieżek',
    text: 'Wspólnie wybieramy plan oraz ścieżki nauki i kursy, które pasują do ról w zespole.'
  },
  {
    title: 'Zaproszenie zespołu',
    text: 'Uczestnicy zakładają konta na platformie i dostają dostęp do kursów objętych planem.'
  },
  {
    title: 'Nauka i postępy',
    text: 'Zespół uczy się we własnym tempie. Postęp, zaliczone quizy i certyfikaty widać przy każdym kursie.'
  }
]

const faqItems = [
  {
    question: 'Czy możemy najpierw sprawdzić platformę?',
    answer: 'Tak. Każdy może uruchomić bezpłatny trial wybranego kursu i zobaczyć lekcje, quizy oraz materiały, zanim zespół zdecyduje się na plan.'
  },
  {
    question: 'Jak potwierdzić, że pracownik ukończył kurs?',
    answer: 'Każdy certyfikat ma unikalny numer i publiczną stronę weryfikacji. Wystarczy link do niej, żeby potwierdzić ukończenie kursu.'
  },
  {
    question: 'Czy uczestnicy mogą pytać o treść lekcji?',
    answer: 'Tak. Pod każdą lekcją jest sekcja pytań i odpowiedzi, w której odpowiada instruktor kursu.'
  },
  {
    question: 'Czym plan dla firm różni się od zakupu pojedynczych kursów?',
    answer: 'Pojedynczy kurs kupuje się jednorazowo, a miesięczna subskrypcja otwiera bibliotekę dla jednej osoby. Plany dla firm obejmują cały zespół — zakres każdego z nich widzisz w kartach planów powyżej.'
  }
]

const openFaq = ref<number | null>(0)

function toggleFaq(index: number) {
  openFaq.value = openFaq.value === index ? null : index
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.business-page {
  min-height: 100vh;
  padding-bottom: 96px;
}

.business-hero {
  padding: 32px 0 0;
}

.business-head {
  margin-bottom: 64px;
}

.business-title {
  font-size: clamp(2.4rem, 4.6vw, 3.8rem);
  margin-top: 18px;
}

.business-subtitle {
  font-size: 0.98rem;
  line-height: 1.6;
}

.plans-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 28px;
  align-items: stretch;

  @media (max-width: 960px) {
    grid-template-columns: 1fr;
    max-width: 560px;
    gap: 36px;
  }
}

.plans-note {
  margin-top: 36px;
  font-size: 0.92rem;
  color: $color-muted;

  a {
    color: $color-accent;
    text-decoration: underline;
    text-underline-offset: 3px;
  }
}

.business-section {
  padding-top: 120px;

  @media (max-width: 720px) {
    padding-top: 84px;
  }
}

.offer {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 1.05fr);
  gap: 48px;
  align-items: start;

  @media (max-width: 1000px) {
    grid-template-columns: 1fr;
  }
}

.offer-main {
  padding: 36px 36px 34px;
  min-width: 0;

  @media (max-width: 560px) {
    --lg-r: 24px;
    padding: 26px 20px 24px;
  }
}

.offer-no {
  display: block;
  font-family: $font-display;
  font-size: clamp(5rem, 9vw, 7.5rem);
  font-weight: 600;
  line-height: 0.8;
  letter-spacing: -0.015em;
  color: $color-accent;
  margin-bottom: 28px;
}

.offer-title {
  font-size: clamp(1.7rem, 2.6vw, 2.3rem);
  font-weight: 600;
  margin-bottom: 16px;
}

.offer-text {
  max-width: 54ch;
  font-size: 0.98rem;
  line-height: 1.65;
  color: $color-muted;
  margin-bottom: 26px;
}

.offer-strip {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  list-style: none;
  padding: 18px 0;
  margin: 0 0 26px;
  border-top: 1px dashed $color-hairline;
  border-bottom: 1px dashed $color-hairline;

  li {
    @include tag;
  }
}

.offer-links {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.offer-list {
  list-style: none;
  padding: 0;
  margin: 0;
  border-top: 1px solid $color-ink;
}

.offer-row {
  display: grid;
  grid-template-columns: minmax(0, 0.9fr) minmax(0, 1.1fr);
  gap: 24px;
  padding: 24px 0;
  border-bottom: 1px solid $color-hairline;

  @media (max-width: 560px) {
    grid-template-columns: 1fr;
    gap: 10px;
  }
}

.offer-row-head {
  display: flex;
  gap: 14px;
  align-items: baseline;
  min-width: 0;
}

.offer-row-no {
  @include mono-label;
  color: $color-accent;
}

.offer-row-title {
  font-size: 1.15rem;
  line-height: 1.2;
}

.offer-row-body {
  min-width: 0;

  p {
    font-size: 0.92rem;
    line-height: 1.6;
    color: $color-muted;
  }
}

.offer-row-link {
  @include mono-label;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  margin-top: 12px;
  font-size: 0.64rem;
  color: $color-accent;
  text-decoration: none;
  transition: gap 0.2s;

  &:hover {
    gap: 10px;
  }
}

.rollout {
  display: grid;
  grid-template-columns: minmax(0, 0.85fr) minmax(0, 1.15fr);
  gap: 64px;
  align-items: start;

  @media (max-width: 960px) {
    grid-template-columns: 1fr;
    gap: 40px;
  }
}

.rollout-head {
  position: sticky;
  top: calc(#{$header-height} + 24px);

  h2 {
    margin: 16px 0 16px;
  }

  p {
    max-width: 40ch;
    color: $color-muted;
    margin-bottom: 28px;
  }

  @media (max-width: 960px) {
    position: static;
  }
}

.rollout-steps {
  position: relative;
  list-style: none;
  padding: 0;
  margin: 0;

  &::before {
    content: "";
    position: absolute;
    top: 30px;
    bottom: 30px;
    left: 29px;
    width: 1px;
    background: rgba($color-ink, 0.2);
  }
}

.rollout-step {
  position: relative;
  display: grid;
  grid-template-columns: 60px minmax(0, 1fr);
  gap: 28px;
  padding-bottom: 44px;

  &:last-child {
    padding-bottom: 0;
  }

  @media (max-width: 560px) {
    gap: 18px;
  }
}

.rollout-num {
  display: grid;
  place-items: center;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: $color-bg;
  box-shadow: inset 0 0 0 1px rgba($color-ink, 0.28);
  font-family: $font-display;
  font-size: 1.4rem;
  font-weight: 600;
  color: $color-ink;

  .rollout-step:last-child & {
    color: $color-on-accent;
    background: $color-accent;
    box-shadow: 0 0 0 6px rgba($color-accent, 0.14);
  }
}

.rollout-body {
  padding-top: 14px;
  min-width: 0;

  h3 {
    font-size: 1.4rem;
    margin-bottom: 8px;
  }

  p {
    max-width: 52ch;
    font-size: 0.95rem;
    line-height: 1.6;
    color: $color-muted;
  }
}

.faq-list {
  max-width: 920px;
  list-style: none;
  padding: 0;
  margin: 0;
  border-top: 1px solid $color-hairline;
}

.faq-item {
  border-bottom: 1px solid $color-hairline;
}

.faq-question {
  display: grid;
  grid-template-columns: 40px minmax(0, 1fr) 20px;
  align-items: center;
  gap: 16px;
  width: 100%;
  padding: 22px 0;
  border: none;
  background: none;
  font: inherit;
  text-align: left;
  color: $color-ink;
  cursor: pointer;

  &:hover .faq-text {
    color: $color-accent;
  }
}

.faq-num {
  @include mono-label;
  color: $color-faint;

  .is-open & {
    color: $color-accent;
  }
}

.faq-text {
  font-family: $font-display;
  font-size: 1.18rem;
  font-weight: 500;
  line-height: 1.3;
  transition: color 0.2s;
}

.faq-icon {
  position: relative;
  width: 14px;
  height: 14px;
  justify-self: end;

  &::before,
  &::after {
    content: "";
    position: absolute;
    top: 50%;
    left: 0;
    width: 14px;
    height: 1.5px;
    margin-top: -0.75px;
    background: currentColor;
    transition: transform 0.3s $ease-out;
  }

  &::after {
    transform: rotate(90deg);
  }

  .is-open &::after {
    transform: rotate(0deg);
  }
}

.faq-answer {
  padding: 0 36px 24px 56px;

  p {
    max-width: 68ch;
    font-size: 0.95rem;
    line-height: 1.65;
    color: $color-muted;
  }

  @media (max-width: 560px) {
    padding: 0 0 22px 56px;
  }
}

.final-cta {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  align-items: end;
  gap: 40px;
  padding: 48px 48px;

  @media (max-width: 860px) {
    grid-template-columns: 1fr;
    align-items: start;
    gap: 28px;
    padding: 32px 24px;
    --lg-r: 24px;
  }
}

.final-title {
  margin: 16px 0 14px;
}

.final-text p {
  max-width: 58ch;
  color: $color-muted;
  line-height: 1.6;
}

.final-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.business-loading,
.business-error {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 48px 0;
  color: $color-muted;
}

.business-loading p {
  @include mono-label;
  color: $color-faint;
}

.spinner {
  width: 22px;
  height: 22px;
  border: 2px solid rgba($color-ink, 0.1);
  border-top-color: $color-accent;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 560px) {
  .business-hero {
    padding-top: 40px;
  }
}
</style>
