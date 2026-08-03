<template>
  <div class="public-page help-page">
    <section class="page-hero">
      <div class="container">
        <span class="eyebrow">Centrum pomocy</span>
        <h1>Wszystko, czego potrzebujesz, żeby zacząć</h1>
        <p>Znajdź odpowiedź, wybierz odpowiedni sposób nauki lub skontaktuj się z zespołem CoursePlatform.</p>
      </div>
    </section>

    <section class="help-content">
      <div class="container">
        <div class="help-grid">
          <router-link class="help-card glass-card" :to="{ name: 'Courses' }">
            <span class="help-card__number">01</span>
            <h2>Jak zacząć naukę</h2>
            <p>Przeglądaj kursy, uruchom bezpłatny trial lub wybierz ścieżkę rozwoju dopasowaną do celu.</p>
            <span class="help-card__link">Przejdź do katalogu →</span>
          </router-link>
          <router-link class="help-card glass-card" :to="{ name: 'Pricing' }">
            <span class="help-card__number">02</span>
            <h2>Cennik i płatności</h2>
            <p>Porównaj zakup pojedynczego kursu, All-access oraz ofertę przygotowaną dla całych zespołów.</p>
            <span class="help-card__link">Zobacz cennik →</span>
          </router-link>
          <router-link class="help-card glass-card" :to="{ name: 'Faq' }">
            <span class="help-card__number">03</span>
            <h2>Najczęstsze pytania</h2>
            <p>Dowiedz się, jak działają triale, certyfikaty, kody rabatowe, prezenty i dostęp All-access.</p>
            <span class="help-card__link">Otwórz FAQ →</span>
          </router-link>
          <router-link class="help-card glass-card" :to="{ name: 'Contact' }">
            <span class="help-card__number">04</span>
            <h2>Kontakt z pomocą</h2>
            <p>Nie znalazłeś odpowiedzi? Wyślij wiadomość — przekażemy ją bezpośrednio do zespołu wsparcia.</p>
            <span class="help-card__link">Napisz do nas →</span>
          </router-link>
        </div>

        <div class="help-faq">
          <div class="section-copy">
            <span class="eyebrow">Szybkie odpowiedzi</span>
            <h2>Najczęściej potrzebne informacje</h2>
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

.page-hero {
  padding: 44px 0 38px;
  text-align: center;
  border-bottom: 1px solid $color-hairline;

  .eyebrow {
    margin-bottom: 18px;
  }

  h1 {
    max-width: 760px;
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

.help-content {
  padding: 52px 0 96px;
}

.help-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 18px;

  @media (max-width: 680px) {
    grid-template-columns: 1fr;
  }
}

.help-card {
  --lg-r: 22px;
  --lg-blur: 2px;
  display: flex;
  min-height: 230px;
  flex-direction: column;
  padding: 28px;
  color: inherit;
  text-decoration: none;
  transition: transform 180ms ease, box-shadow 180ms ease;

  &:hover {
    transform: translateY(-4px);
    box-shadow: 0 22px 48px rgba(3, 6, 24, 0.44);
  }

  h2 {
    margin: 16px 0 10px;
    font-family: $font-display;
    font-size: 1.2rem;
  }

  p {
    color: $color-muted;
    line-height: 1.6;
  }
}

.help-card__number {
  color: $color-gold;
  font-family: $font-display;
  font-size: 0.76rem;
  font-weight: 700;
  letter-spacing: 0.14em;
}

.help-card__link {
  margin-top: auto;
  padding-top: 22px;
  color: $color-ink;
  font-size: 0.9rem;
  font-weight: 700;
}

.help-faq {
  max-width: 900px;
  margin: 80px auto 0;
}

.section-copy {
  margin-bottom: 24px;
  text-align: center;

  .eyebrow {
    margin-bottom: 15px;
  }

  h2 {
    font-family: $font-display;
    font-size: clamp(1.5rem, 3vw, 2.1rem);
    letter-spacing: -0.025em;
  }
}
</style>
