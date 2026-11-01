<template>
  <div class="faq-page">
    <section class="faq-hero">
      <div class="container">
        <header class="section-head faq-head">
          <span class="eyebrow">FAQ</span>
          <h1>Najczęściej zadawane <em>pytania</em></h1>
          <p>Krótko i konkretnie: wszystko o dostępie do kursów, płatnościach, trialu oraz certyfikatach.</p>
        </header>
      </div>
    </section>

    <section class="faq-content">
      <div class="container faq-layout">
        <FaqList />

        <aside class="faq-aside">
          <div class="faq-contact paper-card">
            <span class="faq-contact__label">Nie ma tu Twojego pytania?</span>
            <h2>Napisz do <em>zespołu wsparcia</em></h2>
            <p>Opisz problem i podaj adres e-mail powiązany z kontem. Wiadomość trafi bezpośrednio do osób, które mogą pomóc.</p>
            <router-link class="btn btn-primary" :to="{ name: 'Contact' }">Skontaktuj się z nami</router-link>
          </div>

          <nav class="faq-shortcuts" aria-label="Na skróty">
            <span class="faq-shortcuts__label">Na skróty</span>
            <router-link :to="{ name: 'Help' }">Centrum pomocy</router-link>
            <router-link :to="{ name: 'Pricing' }">Cennik i porównanie planów</router-link>
            <router-link :to="trialDestination">Bezpłatny trial</router-link>
            <router-link :to="{ name: 'GiftRedeem' }">Zrealizuj kod prezentowy</router-link>
          </nav>
        </aside>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import FaqList from '@/features/public/components/FaqList.vue'

const authStore = useAuthStore()

const trialDestination = computed(() =>
  authStore.isAuthenticated
    ? { name: 'Trial' }
    : { name: 'Register', query: { redirect: '/trial' } }
)
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.faq-hero {
  padding: 32px 0 0;

  h1 {
    font-size: clamp(2.4rem, 4.6vw, 3.8rem);
    margin-top: 18px;
  }
}

.faq-head {
  margin-bottom: 40px;
}

.faq-content {
  padding: 0 0 96px;
}

.faq-layout {
  display: grid;
  grid-template-columns: minmax(0, 1.65fr) minmax(0, 0.85fr);
  gap: 56px;
  align-items: start;

  @media (max-width: 920px) {
    grid-template-columns: 1fr;
    gap: 48px;
  }
}

.faq-aside {
  position: sticky;
  top: 110px;
  display: grid;
  gap: 36px;

  @media (max-width: 920px) {
    position: static;
    max-width: 560px;
  }
}

.faq-contact {
  display: flex;
  flex-direction: column;
  padding: 24px 26px 26px;
  transform: rotate(1deg);

  h2 {
    margin: 16px 0 12px;
    font-size: 1.55rem;
    font-weight: 600;
    line-height: 1.15;
    color: #15171b;
  }

  p {
    font-size: 0.94rem;
    line-height: 1.6;
  }

  .btn {
    margin-top: 22px;
  }

  @media (max-width: 920px) {
    transform: rotate(-0.5deg);
  }
}

.faq-contact__label {
  @include mono-label;
  padding-bottom: 12px;
  color: rgba(21, 23, 27, 0.6);
  border-bottom: 1px solid #15171b;
}

.faq-shortcuts {
  display: flex;
  flex-direction: column;

  a {
    display: flex;
    justify-content: space-between;
    gap: 12px;
    padding: 13px 0;
    color: $color-ink;
    text-decoration: none;
    border-bottom: 1px dashed $color-hairline;
    transition: color 0.2s;

    &::after {
      content: "→";
      font-family: $font-mono;
      color: $color-faint;
      transition: color 0.2s, transform 0.3s $ease-out;
    }

    &:hover {
      color: $color-accent;

      &::after {
        color: $color-accent;
        transform: translateX(3px);
      }
    }
  }
}

.faq-shortcuts__label {
  @include mono-label;
  padding-bottom: 12px;
  color: $color-accent;
  border-bottom: 1px solid $color-hairline;
}
</style>
