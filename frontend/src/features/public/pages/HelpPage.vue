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

    <section class="help-hub">
      <div class="container">
        <div class="hub-grid">
          <article class="hub-contact paper-card">
            <span class="hub-contact__num">No. 04</span>
            <h2>Nie znalazłeś odpowiedzi? <em>Napisz do nas</em></h2>
            <p>Wiadomość z formularza trafia bezpośrednio do zespołu wsparcia. Ułatwisz nam pomoc, jeśli podasz:</p>
            <ol class="hub-contact__list">
              <li>adres e-mail powiązany z kontem,</li>
              <li>nazwę kursu lub lekcji, której dotyczy pytanie,</li>
              <li>krótki opis tego, co się stało.</li>
            </ol>
            <router-link class="btn btn-primary" :to="{ name: 'Contact' }">Kontakt z pomocą</router-link>
          </article>

          <article class="hub-start glass-card is-static">
            <div class="hub-start__head">
              <span class="hub-label">No. 01 · Jak zacząć naukę</span>
              <h2>Trzy drogi do pierwszej lekcji</h2>
            </div>
            <nav class="hub-start__links" aria-label="Jak zacząć naukę">
              <router-link :to="{ name: 'Courses' }">
                <span>Katalog kursów</span>
                <small>Wybierz pojedynczy kurs</small>
              </router-link>
              <router-link :to="{ name: 'PathsList' }">
                <span>Ścieżki rozwoju</span>
                <small>Kursy ułożone pod jeden cel</small>
              </router-link>
              <router-link :to="trialDestination">
                <span>Bezpłatny trial</span>
                <small>Sprawdź wybrany kurs</small>
              </router-link>
            </nav>
          </article>

          <router-link class="hub-topic outline-card" :to="{ name: 'Pricing' }">
            <span class="hub-label">No. 02</span>
            <h2>Cennik i płatności</h2>
            <p>Pojedynczy kurs, All-access i oferta dla zespołów w jednym porównaniu.</p>
            <span class="hub-topic__link">Zobacz cennik →</span>
          </router-link>

          <router-link class="hub-topic outline-card" :to="{ name: 'Faq' }">
            <span class="hub-label">No. 03</span>
            <h2>Najczęstsze pytania</h2>
            <p>Triale, certyfikaty, kody rabatowe, prezenty i dostęp All-access.</p>
            <span class="hub-topic__link">Otwórz FAQ →</span>
          </router-link>
        </div>
      </div>
    </section>

    <section class="help-steps">
      <div class="container">
        <header class="section-head">
          <span class="eyebrow">Pierwsze kroki</span>
          <h2>Od założenia konta do <em>certyfikatu</em></h2>
          <p>Tak wygląda droga przez kurs na CoursePlatform, niezależnie od tego, czy kupujesz kurs, czy korzystasz z All-access.</p>
        </header>

        <ol class="steps">
          <li v-for="(step, index) in steps" :key="step.title" class="step">
            <span class="step__num">{{ String(index + 1).padStart(2, '0') }}</span>
            <h3>{{ step.title }}</h3>
            <p>{{ step.text }}</p>
          </li>
        </ol>
      </div>
    </section>

    <section class="help-topics">
      <div class="container">
        <header class="section-head">
          <span class="eyebrow">Tematy pomocy</span>
          <h2>Szukasz czegoś <em>konkretnego?</em></h2>
        </header>

        <dl class="topics">
          <div class="topic">
            <dt>Konto i logowanie</dt>
            <dd>
              <p>Nie pamiętasz hasła? Poproś o link do ustawienia nowego — wyślemy go na adres e-mail przypisany do konta. Po rejestracji potwierdź swój adres e-mail linkiem z wiadomości powitalnej.</p>
              <div class="topic__links">
                <router-link :to="{ name: 'ForgotPassword' }">Zresetuj hasło</router-link>
                <router-link :to="{ name: 'Contact' }">Problem z kontem</router-link>
              </div>
            </dd>
          </div>
          <div class="topic">
            <dt>Płatności i faktury</dt>
            <dd>
              <p>Płatności kartą obsługuje Stripe Checkout. Historię zakupów znajdziesz w profilu, a rozliczeniami subskrypcji All-access zarządzasz w panelu rozliczeń Stripe.</p>
              <div class="topic__links">
                <router-link :to="{ name: 'Pricing' }">Cennik</router-link>
                <router-link :to="{ name: 'Faq' }">FAQ o płatnościach</router-link>
              </div>
            </dd>
          </div>
          <div class="topic">
            <dt>Nauka i postęp</dt>
            <dd>
              <p>Postęp zapisuje się na Twoim koncie. Kolejne lekcje odblokowują się po kolei, a quiz trzeba zaliczyć, żeby przejść dalej. Przy lekcji znajdziesz materiały do pobrania i możesz zadać pytanie instruktorowi.</p>
              <div class="topic__links">
                <router-link :to="{ name: 'Courses' }">Katalog kursów</router-link>
                <router-link :to="{ name: 'PathsList' }">Ścieżki rozwoju</router-link>
              </div>
            </dd>
          </div>
          <div class="topic">
            <dt>Certyfikaty</dt>
            <dd>
              <p>Certyfikat otrzymasz po ukończeniu wszystkich wymaganych lekcji kursu. Każdy ma własny numer — wpisz go poniżej, żeby sprawdzić, czy jest prawdziwy.</p>
              <form class="verify-form" @submit.prevent="verifyCertificate">
                <label class="sr-only" for="help-certificate-number">Numer certyfikatu</label>
                <input
                  id="help-certificate-number"
                  v-model="certificateNumber"
                  type="text"
                  autocomplete="off"
                  spellcheck="false"
                  placeholder="CERT-2026-…"
                />
                <button type="submit" class="btn btn-ghost btn-pill" :disabled="!certificateNumber.trim()">Zweryfikuj</button>
              </form>
            </dd>
          </div>
          <div class="topic">
            <dt>Prezenty i kody</dt>
            <dd>
              <p>Kod rabatowy obniża cenę przy zakupie kursu. Kod prezentowy to jednorazowy klucz, który dodaje konkretny kurs do konta odbiorcy.</p>
              <div class="topic__links">
                <router-link :to="{ name: 'GiftRedeem' }">Zrealizuj kod prezentowy</router-link>
                <router-link :to="{ name: 'Faq' }">Kody w FAQ</router-link>
              </div>
            </dd>
          </div>
        </dl>
      </div>
    </section>

    <section class="help-faq-section">
      <div class="container help-faq">
        <div class="section-copy">
          <span class="eyebrow">Szybkie odpowiedzi</span>
          <h2>Najczęściej potrzebne <em>informacje</em></h2>
          <router-link class="btn btn-ghost btn-pill" :to="{ name: 'Faq' }">Pełne FAQ</router-link>
        </div>
        <FaqList :compact="true" />
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import FaqList from '@/features/public/components/FaqList.vue'

const router = useRouter()
const authStore = useAuthStore()
const certificateNumber = ref('')

const trialDestination = computed(() =>
  authStore.isAuthenticated
    ? { name: 'Trial' }
    : { name: 'Register', query: { redirect: '/trial' } }
)

const steps = [
  {
    title: 'Załóż konto',
    text: 'Zarejestruj się i potwierdź adres e-mail. Na koncie zapisują się Twoje kursy, postęp i certyfikaty.'
  },
  {
    title: 'Wybierz, jak się uczysz',
    text: 'Kup pojedynczy kurs, wybierz ścieżkę rozwoju, aktywuj All-access albo uruchom bezpłatny trial wybranego kursu.'
  },
  {
    title: 'Ucz się krok po kroku',
    text: 'Lekcje odblokowują się po kolei, a quizy trzeba zaliczyć, żeby przejść dalej. Pytania zadasz instruktorowi przy lekcji.'
  },
  {
    title: 'Odbierz certyfikat',
    text: 'Po ukończeniu kursu dostajesz certyfikat z unikalnym numerem, który każdy może zweryfikować na platformie.'
  }
]

function verifyCertificate() {
  const number = certificateNumber.value.trim()
  if (!number) return
  router.push({ name: 'CertificateVerify', params: { number } })
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

$paper-ink: #15171b;

.page-hero {
  padding: 32px 0 0;

  h1 {
    font-size: clamp(2.4rem, 4.6vw, 3.8rem);
    margin-top: 18px;
  }
}

.help-head {
  margin-bottom: 48px;
}

.help-hub,
.help-steps,
.help-topics {
  padding-bottom: 112px;
}

.help-faq-section {
  padding-bottom: 96px;
}

.help-steps h2,
.help-topics h2 {
  font-size: clamp(1.9rem, 3.4vw, 2.6rem);
}

.hub-label {
  @include mono-label;
  color: $color-accent;
}

.hub-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.05fr) repeat(2, minmax(0, 1fr));
  grid-template-rows: auto auto;
  gap: 22px;

  @media (max-width: 1020px) {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  @media (max-width: 640px) {
    grid-template-columns: 1fr;
  }
}

.hub-contact {
  grid-row: 1 / span 2;
  display: flex;
  flex-direction: column;
  padding: 28px 30px 30px;
  transform: rotate(-0.8deg);

  h2 {
    margin: 22px 0 16px;
    font-size: clamp(1.6rem, 2.4vw, 2rem);
    font-weight: 600;
    line-height: 1.15;
    color: $paper-ink;
  }

  p {
    line-height: 1.6;
  }

  .btn {
    align-self: flex-start;
    margin-top: auto;
  }

  @media (max-width: 1020px) {
    grid-row: auto;
    grid-column: 1 / -1;
    transform: rotate(-0.4deg);
  }

  @media (max-width: 640px) {
    padding: 24px 22px 26px;

    .btn {
      width: 100%;
    }
  }
}

.hub-contact__num {
  font-family: $font-mono;
  font-size: 0.72rem;
  padding-bottom: 14px;
  color: rgba(21, 23, 27, 0.55);
  border-bottom: 1px solid $paper-ink;
}

.hub-contact__list {
  margin: 18px 0 30px;
  padding: 0;
  list-style: none;
  counter-reset: tip;

  li {
    display: flex;
    gap: 14px;
    padding: 9px 0;
    border-bottom: 1px dashed rgba(21, 23, 27, 0.18);
    counter-increment: tip;

    &::before {
      content: counter(tip, decimal-leading-zero);
      font-family: $font-mono;
      font-size: 0.74rem;
      padding-top: 2px;
      color: $paper-ink;
    }
  }
}

.hub-start {
  --lg-blur: 10px;
  grid-column: 2 / span 2;
  padding: 24px 26px 18px;

  h2 {
    margin: 10px 0 0;
    font-size: 1.45rem;
    font-weight: 600;
  }

  @media (max-width: 1020px) {
    grid-column: 1 / -1;
  }

  @media (max-width: 640px) {
    padding: 22px 20px 14px;
  }
}

.hub-start__head {
  padding-bottom: 18px;
  border-bottom: 1px solid $color-hairline;
}

.hub-start__links {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));

  a {
    display: flex;
    flex-direction: column;
    gap: 6px;
    padding: 18px 16px 10px 0;
    color: $color-ink;
    text-decoration: none;
    transition: color 0.2s;

    & + a {
      padding-left: 16px;
      border-left: 1px dashed $color-hairline;
    }

    span {
      font-family: $font-display;
      font-size: 1.1rem;
      font-weight: 500;

      &::after {
        content: " →";
        color: $color-faint;
        transition: color 0.2s;
      }
    }

    small {
      font-family: $font-mono;
      font-size: 0.7rem;
      color: $color-faint;
    }

    &:hover span,
    &:hover span::after {
      color: $color-accent;
    }
  }

  @media (max-width: 640px) {
    grid-template-columns: 1fr;

    a {
      padding: 14px 0;

      & + a {
        padding-left: 0;
        border-left: 0;
        border-top: 1px dashed $color-hairline;
      }
    }
  }
}

.hub-topic {
  display: flex;
  flex-direction: column;
  padding: 22px 24px;
  color: inherit;
  text-decoration: none;

  h2 {
    margin: 14px 0 8px;
    font-size: 1.25rem;
    font-weight: 600;
  }

  p {
    color: $color-muted;
    font-size: 0.92rem;
    line-height: 1.6;
  }
}

.hub-topic__link {
  @include mono-label;
  margin-top: auto;
  padding-top: 18px;
  font-size: 0.64rem;
  color: $color-muted;
  transition: color 0.2s;
}

.hub-topic:hover .hub-topic__link {
  color: $color-accent;
}

.steps {
  position: relative;
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 28px;
  margin: 0;
  padding: 0;
  list-style: none;

  &::before {
    content: "";
    position: absolute;
    top: 62px;
    left: 0;
    right: 0;
    height: 1px;
    background: rgba($color-ink, 0.28);
  }

  @media (max-width: 900px) {
    grid-template-columns: 1fr;
    gap: 0;
    padding-left: 32px;

    &::before {
      top: 14px;
      bottom: 14px;
      left: 5px;
      right: auto;
      width: 1px;
      height: auto;
    }
  }
}

.step {
  position: relative;

  &::before {
    content: "";
    position: absolute;
    top: 57px;
    left: 0;
    width: 11px;
    height: 11px;
    border-radius: 50%;
    background: $color-bg;
    box-shadow: inset 0 0 0 2px $color-accent;
  }

  &:last-child::before {
    background: $color-accent;
  }

  h3 {
    margin: 50px 0 10px;
    font-size: 1.25rem;
    font-weight: 600;
  }

  p {
    max-width: 34ch;
    color: $color-muted;
    font-size: 0.94rem;
    line-height: 1.6;
  }

  @media (max-width: 900px) {
    padding-bottom: 32px;

    &::before {
      top: 9px;
      left: -32px;
    }

    h3 {
      margin-top: 8px;
    }

    p {
      max-width: 60ch;
    }

    &:last-child {
      padding-bottom: 0;
    }
  }
}

.step__num {
  display: block;
  font-family: $font-display;
  font-size: 2.4rem;
  font-weight: 500;
  line-height: 1;
  letter-spacing: -0.015em;
  color: $color-ink;

  @media (max-width: 900px) {
    font-family: $font-mono;
    font-size: 0.78rem;
    letter-spacing: 0.1em;
    color: $color-accent;
  }
}

.topics {
  margin: 0;
  border-top: 1px solid $color-ink;
}

.topic {
  display: grid;
  grid-template-columns: minmax(0, 280px) minmax(0, 1fr);
  gap: 40px;
  padding: 26px 0;
  border-bottom: 1px solid $color-hairline;

  dt {
    font-family: $font-display;
    font-size: 1.3rem;
    font-weight: 500;
    color: $color-ink;
  }

  dd {
    margin: 0;

    p {
      max-width: 68ch;
      color: $color-muted;
      line-height: 1.65;
    }
  }

  @media (max-width: 720px) {
    grid-template-columns: 1fr;
    gap: 10px;
    padding: 22px 0;

    dt {
      font-size: 1.15rem;
    }
  }
}

.topic__links {
  display: flex;
  flex-wrap: wrap;
  gap: 8px 24px;
  margin-top: 14px;

  a {
    @include mono-label;
    color: $color-accent;
    text-decoration: none;

    &::after {
      content: " →";
    }

    &:hover {
      text-decoration: underline;
      text-underline-offset: 4px;
    }
  }
}

.verify-form {
  display: flex;
  gap: 10px;
  max-width: 520px;
  margin-top: 16px;

  input {
    flex: 1;
    min-width: 0;
    height: 44px;
    padding: 0 16px;
    border: 0;
    border-radius: $radius-md;
    outline: 0;
    background: rgba(13, 15, 19, 0.45);
    box-shadow: inset 0 0 0 1px $color-hairline;
    color: $color-ink;
    font-family: $font-mono;
    font-size: 0.86rem;
    transition: box-shadow 160ms ease;

    &::placeholder {
      color: $color-faint;
    }

    &:focus {
      box-shadow:
        inset 0 0 0 1px $color-hairline,
        0 0 0 2px rgba($color-accent, 0.6);
    }
  }

  .btn {
    padding: 10px 20px;
    font-size: 0.86rem;
  }

  @media (max-width: 420px) {
    flex-direction: column;
  }
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip-path: inset(50%);
  white-space: nowrap;
}

.help-faq {
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

.section-copy {
  h2 {
    margin-top: 14px;
    font-size: clamp(1.8rem, 3vw, 2.4rem);
  }

  .btn {
    margin-top: 22px;
    padding: 10px 20px;
    font-size: 0.86rem;
  }
}
</style>
