<template>
  <div class="faq-page">
    <section class="faq-hero">
      <div class="container">
        <span class="eyebrow">FAQ</span>
        <h1>Najczęściej zadawane pytania</h1>
        <p>Krótko i konkretnie: wszystko o dostępie do kursów, płatnościach, trialu oraz certyfikatach.</p>
      </div>
    </section>

    <section class="faq-content">
      <div class="container">
        <FaqList />
        <div class="faq-contact glass-card">
          <div>
            <h2>Nadal potrzebujesz pomocy?</h2>
            <p>Napisz do nas, jeśli Twoje pytanie nie znalazło odpowiedzi w FAQ.</p>
          </div>
          <router-link class="btn btn-primary" :to="{ name: 'Contact' }">Skontaktuj się z nami</router-link>
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

.faq-hero {
  padding: 44px 0 38px;
  text-align: center;
  border-bottom: 1px solid $color-hairline;

  .eyebrow {
    margin-bottom: 18px;
  }

  h1 {
    margin-bottom: 14px;
    font-family: $font-display;
    font-size: $font-size-xl;
    letter-spacing: -0.03em;
  }

  p {
    max-width: 650px;
    margin: 0 auto;
    color: $color-muted;
    line-height: 1.65;
  }
}

.faq-content {
  width: min(900px, 100% - 48px);
  margin: 0 auto;
  padding: 54px 0 96px;
}

.faq-contact {
  --lg-r: 22px;
  --lg-blur: 2px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
  margin-top: 42px;
  padding: 26px 28px;

  h2 {
    margin-bottom: 7px;
    font-family: $font-display;
    font-size: 1.2rem;
  }

  p {
    color: $color-muted;
  }

  @media (max-width: 620px) {
    flex-direction: column;
    align-items: flex-start;
    padding: 24px 20px;

    .btn {
      width: 100%;
    }
  }
}
</style>
