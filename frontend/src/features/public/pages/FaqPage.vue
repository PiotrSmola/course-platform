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
        <div class="faq-contact glass">
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

.faq-layout > * {
  max-width: 920px;
}

.faq-contact {
  --lg-r: 18px;
  --lg-blur: 10px;
  --lg-tint: rgba(13, 15, 19, 0.3);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
  margin-top: 48px;
  padding: 20px 20px 20px 26px;

  h2 {
    margin-bottom: 4px;
    font-size: 1.3rem;
    font-weight: 600;
  }

  p {
    color: $color-muted;
    font-size: 0.92rem;
  }

  @media (max-width: 620px) {
    flex-direction: column;
    align-items: flex-start;
    padding: 22px 20px;

    .btn {
      width: 100%;
    }
  }
}
</style>
