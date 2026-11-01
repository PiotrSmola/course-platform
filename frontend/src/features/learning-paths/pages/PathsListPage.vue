<template>
  <div class="paths-page">
    <div class="paths-hero">
      <div class="container">
        <header class="section-head paths-head">
          <span class="eyebrow">Ścieżki nauki</span>
          <h1 class="paths-title">Zaplanowane ścieżki <em>rozwoju</em></h1>
          <p class="paths-subtitle">
            Kuratorowane zestawy kursów, które prowadzą od podstaw do zaawansowanych tematów. Wybierz ścieżkę dopasowaną do Twojego celu — frontend, backend albo pełen stos.
          </p>
        </header>
      </div>
    </div>

    <div id="sciezki" class="container paths-body">
      <div v-if="isLoading" class="paths-loading">
        <div class="spinner" />
        <p>Ładowanie ścieżek...</p>
      </div>

      <div v-else-if="isError" class="paths-empty">
        <p>Nie udało się pobrać ścieżek. Spróbuj ponownie.</p>
      </div>

      <div v-else-if="items.length === 0" class="paths-empty">
        <p>Wkrótce pojawią się tutaj nowe ścieżki.</p>
      </div>

      <div v-else class="paths-grid">
        <PathCard
          v-for="(path, index) in items"
          :key="path.id"
          class="path-cell"
          :path="path"
          :index="index"
          :variant="variantFor(index)"
        />
      </div>
    </div>

    <section class="container paths-section">
      <header class="section-head">
        <span class="eyebrow">Jak to działa</span>
        <h2>Jak działa <em>ścieżka</em></h2>
        <p>Ścieżka to nie playlista. Kursy mają ustaloną kolejność, a lekcje otwierają się w miarę postępów — dzięki temu zawsze wiesz, co jest następne.</p>
      </header>

      <ol class="how-steps">
        <li v-for="(step, index) in steps" :key="step.title" class="how-step" :class="{ 'is-last': index === steps.length - 1 }">
          <span class="how-dot" aria-hidden="true" />
          <span class="how-label">Krok {{ pad(index + 1) }}</span>
          <span class="how-num">{{ pad(index + 1) }}</span>
          <h3 class="how-title">{{ step.title }}</h3>
          <p class="how-text">{{ step.text }}</p>
        </li>
      </ol>
    </section>

    <section class="container paths-section">
      <header class="section-head">
        <span class="eyebrow">Porównanie</span>
        <h2>Ścieżka czy <em>pojedynczy kurs?</em></h2>
        <p>Oba sposoby korzystają z tych samych kursów, lekcji i certyfikatów. Różnią się tym, kto układa plan nauki.</p>
      </header>

      <div class="compare">
        <article class="compare-card outline-card">
          <div class="compare-top">
            <span class="compare-label">Opcja A</span>
            <h3 class="compare-title">Pojedynczy kurs</h3>
          </div>
          <dl class="compare-rows">
            <div v-for="row in compareRows" :key="row.label" class="compare-row">
              <dt>{{ row.label }}</dt>
              <dd>{{ row.course }}</dd>
            </div>
          </dl>
          <router-link class="btn btn-ghost compare-cta" :to="{ name: 'Courses' }">Przeglądaj katalog</router-link>
        </article>

        <article class="compare-card paper-card is-path">
          <div class="compare-top">
            <span class="compare-label">Opcja B</span>
            <h3 class="compare-title">Ścieżka <em>nauki</em></h3>
          </div>
          <dl class="compare-rows">
            <div v-for="row in compareRows" :key="row.label" class="compare-row">
              <dt>{{ row.label }}</dt>
              <dd>{{ row.path }}</dd>
            </div>
          </dl>
          <a class="btn btn-primary compare-cta" href="#sciezki">Wybierz ścieżkę</a>
        </article>
      </div>
    </section>

    <section class="container paths-section paths-cta-wrap">
      <div class="paths-cta ticket-card">
        <div class="cta-text">
          <span class="cta-label">Wskazówka</span>
          <h2 class="cta-title">Nie wiesz, od czego zacząć?</h2>
          <p>Przejrzyj katalog kursów i wybierz jeden temat na start albo sprawdź odpowiedzi na najczęstsze pytania o dostęp, płatności, trial i certyfikaty.</p>
        </div>
        <div class="cta-actions">
          <router-link class="btn btn-primary" :to="{ name: 'Courses' }">Zobacz katalog</router-link>
          <router-link class="btn btn-ghost" :to="{ name: 'Faq' }">Najczęstsze pytania</router-link>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useLearningPaths } from '@/features/learning-paths/composables/useLearningPaths'
import PathCard from '@/features/learning-paths/components/PathCard.vue'

const { isLoading, isError, data } = useLearningPaths()
const items = computed(() => data.value?.items ?? [])

function variantFor(index: number): 'featured' | 'tall' | 'outline' | 'row' {
  if (index === 0) return 'featured'
  if (index === 1) return 'tall'
  return index % 2 === 0 ? 'outline' : 'row'
}

function pad(n: number): string {
  return String(n).padStart(2, '0')
}

const steps = [
  {
    title: 'Wybierasz ścieżkę',
    text: 'Dopasowaną do celu i poziomu. Przy każdej widzisz liczbę kursów i szacowany czas nauki.'
  },
  {
    title: 'Kursy w ustalonej kolejności',
    text: 'Plan prowadzi od podstaw do tematów zaawansowanych. Kursy oznaczone jako opcjonalne możesz pominąć.'
  },
  {
    title: 'Postęp i quizy otwierają kolejne lekcje',
    text: 'Następna lekcja odblokowuje się po ukończeniu poprzedniej i zaliczeniu quizu, więc nie przeskoczysz fundamentów.'
  },
  {
    title: 'Certyfikat po każdym kursie',
    text: 'Każdy certyfikat ma własny numer i publiczną stronę weryfikacji, którą możesz podlinkować w CV.'
  }
]

const compareRows = [
  {
    label: 'Dla kogo',
    course: 'Gdy potrzebujesz jednej konkretnej umiejętności lub technologii.',
    path: 'Gdy celujesz w rolę zawodową, a nie w pojedynczy temat.'
  },
  {
    label: 'Zakres',
    course: 'Moduły i lekcje wideo wokół jednego tematu.',
    path: 'Kilka kursów ułożonych od podstaw do tematów zaawansowanych.'
  },
  {
    label: 'Kolejność',
    course: 'Sam decydujesz, który kurs będzie następny.',
    path: 'Ustalona z góry — plan mówi, który kurs jest teraz.'
  },
  {
    label: 'Dostęp',
    course: 'Jednorazowy zakup, bezpłatny trial wybranego kursu albo miesięczna subskrypcja.',
    path: 'Każdy kurs ze ścieżki kupujesz osobno albo otwierasz w ramach miesięcznej subskrypcji.'
  },
  {
    label: 'Certyfikat',
    course: 'Po ukończeniu kursu.',
    path: 'Po każdym ukończonym kursie ze ścieżki.'
  }
]
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

$paper-ink: #15171b;
$paper-line: rgba(21, 23, 27, 0.16);

.paths-page {
  min-height: 100vh;
  padding-bottom: 96px;
}

.paths-hero {
  padding: 32px 0 0;
}

.paths-head {
  margin-bottom: 40px;
}

.paths-title {
  font-size: $font-size-2xl;
  margin-top: 18px;
}

.paths-subtitle {
  font-size: 0.98rem;
  line-height: 1.6;
}

.paths-body {
  scroll-margin-top: calc(#{$header-height} + 24px);
}

.paths-loading,
.paths-empty {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 48px 0;
  color: $color-muted;
}

.paths-loading p {
  @include mono-label;
  color: $color-faint;
}

.paths-grid {
  display: grid;
  grid-template-columns: repeat(12, minmax(0, 1fr));
  gap: 28px 24px;
}

.path-cell {
  grid-column: span 6;

  &:nth-child(1) {
    grid-column: span 8;
  }

  &:nth-child(2) {
    grid-column: span 4;
  }

  &:nth-child(1):last-child,
  &:nth-child(n + 3):nth-child(odd):last-child {
    grid-column: 1 / -1;
  }

  @media (max-width: 1100px) {
    &:nth-child(n) {
      grid-column: span 6;
    }

    &:nth-child(1),
    &:nth-child(n + 2):nth-child(even):last-child {
      grid-column: 1 / -1;
    }
  }

  @media (max-width: 720px) {
    &:nth-child(n) {
      grid-column: 1 / -1;
    }
  }
}

.paths-section {
  padding-top: 110px;

  @media (max-width: 720px) {
    padding-top: 80px;
  }
}

.how-steps {
  position: relative;
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 32px;
  list-style: none;
  padding: 0;
  margin: 0;

  &::before {
    content: "";
    position: absolute;
    top: 6px;
    left: 0;
    right: 0;
    height: 1px;
    background: rgba($color-ink, 0.18);
  }

  @media (max-width: 960px) {
    grid-template-columns: 1fr;
    gap: 36px;
    padding-left: 34px;

    &::before {
      top: 6px;
      bottom: 6px;
      left: 6px;
      right: auto;
      width: 1px;
      height: auto;
      background: rgba($color-ink, 0.18);
    }
  }
}

.how-step {
  position: relative;
  min-width: 0;
  padding-top: 36px;

  @media (max-width: 960px) {
    padding-top: 0;
  }
}

.how-dot {
  position: absolute;
  top: 0;
  left: 0;
  width: 13px;
  height: 13px;
  border-radius: 2px;
  background: $color-bg;
  box-shadow: inset 0 0 0 1px $color-accent;

  .is-last & {
    background: $color-accent;
    box-shadow: 0 0 0 5px rgba($color-accent, 0.16);
  }

  @media (max-width: 960px) {
    left: -34px;
    top: 0;
  }
}

.how-label {
  @include mono-label;
  display: block;
  font-size: 0.62rem;
  color: $color-faint;
  margin-bottom: 14px;
}

.how-num {
  display: block;
  font-family: $font-display;
  font-size: 4.2rem;
  font-weight: 600;
  line-height: 0.85;
  letter-spacing: -0.015em;
  color: transparent;
  -webkit-text-stroke: 1px rgba($color-ink, 0.4);
  margin-bottom: 18px;

  .is-last & {
    color: $color-accent;
    -webkit-text-stroke-color: $color-accent;
  }

  @media (max-width: 960px) {
    font-size: 3rem;
  }
}

.how-title {
  font-size: 1.28rem;
  line-height: 1.15;
  margin-bottom: 10px;
}

.how-text {
  max-width: 34ch;
  font-size: 0.92rem;
  line-height: 1.6;
  color: $color-muted;
}

.compare {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 32px;
  align-items: stretch;

  @media (max-width: 860px) {
    grid-template-columns: 1fr;
  }
}

.compare-card {
  display: flex;
  flex-direction: column;
  padding: 30px 32px 32px;
  min-width: 0;

  @media (max-width: 560px) {
    padding: 24px 20px 24px;
  }

  &.is-path {
    transform: rotate(0.8deg) translateY(-8px);

    @media (max-width: 860px) {
      transform: rotate(0.6deg);
    }
  }
}

.compare-top {
  padding-bottom: 20px;
  margin-bottom: 4px;
  border-bottom: 1px solid $color-hairline;

  .is-path & {
    border-bottom-color: $paper-ink;
  }
}

.compare-label {
  @include mono-label;
  display: block;
  color: $color-faint;
  margin-bottom: 12px;

  .is-path & {
    color: rgba($paper-ink, 0.55);
  }
}

.compare-title {
  font-size: clamp(1.6rem, 2.4vw, 2rem);
  font-weight: 600;

  .is-path & {
    color: $paper-ink;
  }
}

.compare-rows {
  margin: 0 0 28px;
  flex: 1;
}

.compare-row {
  display: grid;
  grid-template-columns: 110px minmax(0, 1fr);
  gap: 18px;
  padding: 14px 0;
  border-bottom: 1px dashed $color-hairline;

  .is-path & {
    border-bottom-color: $paper-line;
  }

  dt {
    @include mono-label;
    font-size: 0.62rem;
    padding-top: 3px;
    color: $color-faint;

    .is-path & {
      color: rgba($paper-ink, 0.55);
    }
  }

  dd {
    margin: 0;
    font-size: 0.94rem;
    line-height: 1.5;
    color: $color-muted;

    .is-path & {
      color: $paper-ink;
    }
  }

  @media (max-width: 560px) {
    grid-template-columns: 1fr;
    gap: 6px;
  }
}

.compare-cta {
  align-self: flex-start;
}

.paths-cta {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  align-items: end;
  gap: 32px;
  padding: 40px 44px;

  @media (max-width: 860px) {
    grid-template-columns: 1fr;
    align-items: start;
    padding: 30px 24px;
  }
}

.cta-label {
  @include mono-label;
  display: block;
  margin-bottom: 14px;
  color: rgba(17, 20, 10, 0.6);
}

.cta-title {
  font-size: clamp(1.9rem, 3.4vw, 2.8rem);
  font-weight: 600;
  color: $color-on-accent;
  margin-bottom: 12px;
}

.cta-text p {
  max-width: 60ch;
  font-size: 1rem;
  line-height: 1.6;
}

.cta-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
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
  .paths-hero {
    padding-top: 40px;
  }
}
</style>
