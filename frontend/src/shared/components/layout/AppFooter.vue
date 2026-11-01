<template>
  <footer class="site-footer">
    <div class="container">
      <div class="footer-masthead">
        <router-link class="footer-wordmark" :to="{ name: 'Home' }" aria-label="CoursePlatform — strona główna">
          Course<em>Platform</em>
        </router-link>
        <p>Nowoczesna platforma e-learningowa. Ucz się w dowolnym miejscu i czasie, zdobywaj certyfikaty i rozwijaj karierę.</p>
      </div>

      <div class="footer-grid">
        <nav class="footer-col" aria-label="Platforma">
          <h4>Platforma</h4>
          <ul>
            <li><router-link :to="{ name: 'Business' }">Dla firm</router-link></li>
            <li><router-link :to="{ name: 'Courses' }">Katalog kursów</router-link></li>
            <li><router-link v-if="authStore.isAuthenticated" :to="{ name: 'MyCourses' }">Moje kursy</router-link></li>
            <li><router-link v-if="authStore.isInstructor" :to="{ name: 'InstructorDashboard' }">Panel instruktora</router-link></li>
            <li><router-link v-if="authStore.isAdmin" :to="{ name: 'AdminDashboard' }">Panel administratora</router-link></li>
          </ul>
        </nav>

        <nav class="footer-col" aria-label="Pomoc">
          <h4>Pomoc</h4>
          <ul>
            <li><router-link :to="{ name: 'Help' }">Jak zacz&#261;&#263;</router-link></li>
            <li><router-link :to="{ name: 'Pricing' }">Cennik</router-link></li>
            <li><router-link :to="{ name: 'Faq' }">FAQ</router-link></li>
            <li><router-link :to="{ name: 'Contact' }">Kontakt</router-link></li>
          </ul>
        </nav>

        <div class="footer-col">
          <h4>Obserwuj</h4>
          <FooterSocialLinks />
        </div>

        <div class="footer-col footer-news">
          <h4>Newsletter</h4>
          <p>Zapisz się i otrzymuj powiadomienia o nowych kursach i promocjach.</p>
          <FooterNewsletterForm />
        </div>
      </div>

      <div class="footer-bottom">
        <FooterLocale />
        <p>© {{ year }} CoursePlatform. Wszystkie prawa zastrzeżone.</p>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/features/auth/stores/auth.store'
import FooterLocale from '@/shared/components/layout/FooterLocale.vue'
import FooterNewsletterForm from '@/shared/components/layout/FooterNewsletterForm.vue'
import FooterSocialLinks from '@/shared/components/layout/FooterSocialLinks.vue'

const authStore = useAuthStore()
const year = new Date().getFullYear()

</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.site-footer {
  position: relative;
  margin-top: 40px;
  padding: 72px 0 28px;
  background: rgba($color-bg, 0.6);
  border-top: 1px solid $color-hairline;
}

.footer-masthead {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 360px);
  align-items: end;
  gap: 24px 48px;
  padding-bottom: 40px;
  border-bottom: 1px solid $color-hairline;

  p {
    font-size: 0.94rem;
    line-height: 1.6;
    color: $color-muted;
  }

  @media (max-width: 860px) {
    grid-template-columns: 1fr;
  }
}

.footer-wordmark {
  font-family: $font-display;
  font-size: clamp(2.8rem, 8vw, 6.4rem);
  font-weight: 600;
  line-height: 0.95;
  letter-spacing: -0.015em;
  color: $color-ink;
  text-decoration: none;

  em {
    @include highlight;
  }
}

.footer-grid {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr 1.6fr;
  gap: 40px;
  padding: 40px 0 48px;

  @media (max-width: 880px) {
    grid-template-columns: 1fr 1fr;
  }

  @media (max-width: 560px) {
    grid-template-columns: 1fr;
    gap: 0;
    padding-top: 8px;
  }
}

.footer-col {
  h4 {
    @include mono-label;
    display: flex;
    align-items: center;
    gap: 10px;
    margin-bottom: 18px;
    padding-bottom: 12px;
    border-bottom: 1px dashed $color-hairline;
    font-size: 0.66rem;
    color: $color-faint;
  }

  ul {
    display: flex;
    flex-direction: column;
    gap: 11px;
  }

  li:empty {
    display: none;
  }

  a {
    font-size: 0.93rem;
    color: $color-muted;
    text-decoration: none;
    transition: color 0.25s;

    &:hover {
      color: $color-ink;
    }
  }

  @media (max-width: 560px) {
    padding: 24px 0;

    & + & {
      border-top: 1px solid $color-hairline;
    }
  }
}

.footer-news {
  > p {
    font-size: 0.92rem;
    line-height: 1.55;
    color: $color-muted;
    margin-bottom: 16px;
  }
}

.footer-bottom {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  gap: 10px;
  padding-top: 22px;
  border-top: 1px solid $color-hairline;
  font-family: $font-mono;
  font-size: 0.7rem;
  letter-spacing: 0.04em;
  color: $color-faint;

  @media (max-width: 560px) {
    flex-direction: column;
  }
}
</style>
