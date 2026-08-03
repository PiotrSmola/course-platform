<template>
  <footer class="site-footer">
    <div class="container">
      <div class="footer-grid">
        <div class="footer-about">
          <router-link class="logo" :to="{ name: 'Home' }">
            <svg class="logo-mark" viewBox="0 0 64 64" aria-hidden="true">
              <path d="M12 12h40v4H12zm0 8h28v4H12zm0 8h36v4H12zm0 12h24v4H12zm0 8h32v4H12z" fill="url(#logo-g)"/>
              <circle cx="50" cy="20" r="8" fill="url(#logo-g)" opacity="0.4"/>
              <defs>
                <linearGradient id="logo-g" x1="0" y1="0" x2="1" y2="1">
                  <stop offset="0" stop-color="#f59e0b"/>
                  <stop offset="1" stop-color="#ec4899"/>
                </linearGradient>
              </defs>
            </svg>
            <span>CoursePlatform</span>
          </router-link>
          <p>Nowoczesna platforma e-learningowa. Ucz się w dowolnym miejscu i czasie, zdobywaj certyfikaty i rozwijaj karierę.</p>
          <FooterSocialLinks />
        </div>

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
  background: rgba(10, 14, 23, 0.18);
  -webkit-backdrop-filter: blur(8px) saturate(180%);
  backdrop-filter: blur(8px) saturate(180%);
  box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.16);
  padding: 64px 0 28px;
}

.footer-grid {
  display: grid;
  grid-template-columns: 2.1fr 1fr 1fr 1.7fr;
  gap: 40px;
  padding-bottom: 44px;

  @media (max-width: 880px) {
    grid-template-columns: 1fr 1fr;
  }

  @media (max-width: 560px) {
    grid-template-columns: 1fr;
    gap: 32px;
  }
}

.footer-about {
  .logo {
    display: flex;
    align-items: center;
    gap: 10px;
    font-size: 1.25rem;
    font-weight: 700;

    .logo-mark {
      width: 28px;
      height: 28px;
    }
  }

  p {
    margin: 16px 0 22px;
    font-size: 0.92rem;
    color: $color-muted;
    max-width: 300px;
  }
}

.socials {
  display: flex;
  gap: 10px;

  a {
    width: 40px;
    height: 40px;
    display: grid;
    place-items: center;
    font-size: 0.82rem;
    font-weight: 700;
    color: $color-muted;
    box-shadow: 0 6px 16px rgba(3, 6, 24, 0.3);
    transition: color 0.25s, transform 0.2s;

    &:hover {
      color: $color-ink;
      transform: translateY(-3px);
    }
  }
}

.footer-col {
  h4 {
    font-size: 0.82rem;
    font-weight: 700;
    letter-spacing: 0.1em;
    text-transform: uppercase;
    color: $color-faint;
    margin-bottom: 18px;
  }

  ul {
    display: flex;
    flex-direction: column;
    gap: 11px;
  }

  a {
    font-size: 0.93rem;
    color: $color-muted;
    transition: color 0.25s;

    &:hover {
      color: $color-ink;
    }
  }
}

.footer-news {
  > p {
    font-size: 0.92rem;
    color: $color-muted;
    margin-bottom: 16px;
  }
}

.newsletter-form {
  display: flex;
  gap: 10px;

  @media (max-width: 560px) {
    flex-direction: column;
  }

  input {
    flex: 1;
    min-width: 0;
    height: 48px;
    padding: 0 18px;
    border-radius: 999px;
    border: none;
    background: rgba(255, 255, 255, 0.04);
    box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.3), inset 0 0 0 1px rgba(255, 255, 255, 0.1);
    color: $color-ink;
    font: inherit;
    font-size: 0.92rem;
    outline: none;
    transition: background 0.3s, box-shadow 0.3s;

    &::placeholder {
      color: $color-faint;
    }

    &:focus {
      background: rgba(255, 255, 255, 0.07);
      box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.35), inset 0 0 0 1px rgba(245, 158, 11, 0.5), 0 0 0 4px rgba(245, 158, 11, 0.18);
    }
  }

  .btn {
    padding: 0 22px;
    height: 48px;
    font-size: 0.9rem;
  }
}

.footer-bottom {
  box-shadow: inset 0 1px 0 $color-hairline;
  padding-top: 26px;
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  gap: 10px;
  font-size: 0.84rem;
  color: $color-faint;

  @media (max-width: 560px) {
    justify-content: center;
    text-align: center;
  }
}
</style>
