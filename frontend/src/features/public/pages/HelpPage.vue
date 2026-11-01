<template>
  <div class="public-page help-page">
    <section class="page-hero">
      <div class="container">
        <header class="section-head help-head">
          <span class="eyebrow">Centrum pomocy</span>
          <h1>Wszystko, czego potrzebujesz, żeby <em>zacząć</em></h1>
          <p>Znajdź odpowiedź, wybierz odpowiedni sposób nauki lub skontaktuj się z zespołem CoursePlatform.</p>
        </header>
      </div>
    </section>

    <section class="help-content">
      <div class="container">
        <div class="help-grid">
          <router-link class="help-card glass-card" :to="{ name: 'Courses' }">
            <span class="help-card__number">No. 01</span>
            <h2>Jak zacząć naukę</h2>
            <p>Przeglądaj kursy, uruchom bezpłatny trial lub wybierz ścieżkę rozwoju dopasowaną do celu.</p>
            <span class="help-card__link">Przejdź do katalogu →</span>
          </router-link>
          <router-link class="help-card glass-card" :to="{ name: 'Pricing' }">
            <span class="help-card__number">No. 02</span>
            <h2>Cennik i płatności</h2>
            <p>Porównaj zakup pojedynczego kursu, All-access oraz ofertę przygotowaną dla całych zespołów.</p>
            <span class="help-card__link">Zobacz cennik →</span>
          </router-link>
          <router-link class="help-card glass-card" :to="{ name: 'Faq' }">
            <span class="help-card__number">No. 03</span>
            <h2>Najczęstsze pytania</h2>
            <p>Dowiedz się, jak działają triale, certyfikaty, kody rabatowe, prezenty i dostęp All-access.</p>
            <span class="help-card__link">Otwórz FAQ →</span>
          </router-link>
          <router-link class="help-card glass-card" :to="{ name: 'Contact' }">
            <span class="help-card__number">No. 04</span>
            <h2>Kontakt z pomocą</h2>
            <p>Nie znalazłeś odpowiedzi? Wyślij wiadomość — przekażemy ją bezpośrednio do zespołu wsparcia.</p>
            <span class="help-card__link">Napisz do nas →</span>
          </router-link>
        </div>

        <div class="help-faq">
          <div class="section-copy">
            <span class="eyebrow">Szybkie odpowiedzi</span>
            <h2>Najczęściej potrzebne <em>informacje</em></h2>
          </div>
          <FaqList :compact="true" />
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import FaqList from '@/features/public/components/FaqList.vue'
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.page-hero {
  padding: 32px 0 0;

  h1 {
    font-size: clamp(2.4rem, 4.6vw, 3.8rem);
    margin-top: 18px;
  }
}

.help-head {
  margin-bottom: 40px;
}

.help-content {
  padding: 0 0 96px;
}

.help-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 18px;

  @media (max-width: 1100px) {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  @media (max-width: 620px) {
    grid-template-columns: 1fr;
  }
}

.help-card {
  --lg-blur: 10px;
  display: flex;
  min-height: 250px;
  flex-direction: column;
  padding: 22px 24px 22px;
  color: inherit;
  text-decoration: none;

  h2 {
    margin: 0 0 10px;
    font-size: 1.35rem;
    font-weight: 600;
  }

  p {
    color: $color-muted;
    font-size: 0.92rem;
    line-height: 1.6;
  }
}

.help-card__number {
  @include mono-label;
  padding-bottom: 14px;
  margin-bottom: 18px;
  color: $color-accent;
  border-bottom: 1px solid $color-hairline;
}

.help-card__link {
  @include mono-label;
  margin-top: auto;
  padding-top: 16px;
  font-size: 0.64rem;
  color: $color-muted;
  border-top: 1px dashed $color-hairline;
  transition: color 0.2s;
}

.help-card:hover .help-card__link {
  color: $color-accent;
}

.help-faq {
  display: grid;
  grid-template-columns: minmax(0, 0.8fr) minmax(0, 1.6fr);
  gap: 48px;
  margin-top: 96px;

  @media (max-width: 860px) {
    grid-template-columns: 1fr;
    gap: 28px;
  }
}

.section-copy h2 {
  margin-top: 14px;
  font-size: clamp(1.8rem, 3vw, 2.4rem);
}
</style>
