<template>
  <div class="contact-page">
    <section class="contact-hero">
      <div class="container">
        <header class="section-head contact-head">
          <span class="eyebrow">Kontakt</span>
          <h1>Jak możemy <em>pomóc?</em></h1>
          <p>Opisz swój problem lub pytanie. Wiadomość trafi bezpośrednio do zespołu wsparcia CoursePlatform.</p>
        </header>
      </div>
    </section>

    <section class="contact-content">
      <div class="container contact-grid">
        <form class="contact-form glass-panel" @submit="onSubmit">
          <div class="form-row">
            <div class="form-group">
              <label for="support-name">Imię i nazwisko</label>
              <input id="support-name" v-model="name" type="text" autocomplete="name" placeholder="Jan Kowalski" />
              <span v-if="errors.name" class="error">{{ errors.name }}</span>
            </div>
            <div class="form-group">
              <label for="support-email">Adres e-mail</label>
              <input id="support-email" v-model="email" type="email" autocomplete="email" placeholder="jan@example.com" />
              <span v-if="errors.email" class="error">{{ errors.email }}</span>
            </div>
          </div>

          <div class="form-group">
            <label for="support-subject">Temat</label>
            <input id="support-subject" v-model="subject" type="text" placeholder="Np. problem z dostępem do kursu" />
            <span v-if="errors.subject" class="error">{{ errors.subject }}</span>
          </div>

          <div class="form-group">
            <label for="support-message">Wiadomość</label>
            <textarea id="support-message" v-model="message" rows="7" placeholder="Opisz, czego potrzebujesz…" />
            <span v-if="errors.message" class="error">{{ errors.message }}</span>
          </div>

          <button type="submit" class="btn btn-primary" :disabled="!meta.valid || supportMutation.isPending.value">
            {{ supportMutation.isPending.value ? 'Wysyłamy…' : 'Wyślij wiadomość' }}
          </button>
        </form>

        <aside class="contact-tips paper-card">
          <span class="contact-tips__label">Zanim napiszesz</span>
          <h2>Kilka rzeczy, które <em>przyspieszą</em> odpowiedź</h2>
          <ol class="contact-tips__list">
            <li>Sprawdź, czy odpowiedzi nie ma w FAQ — opisujemy tam trial, płatności, kody i certyfikaty.</li>
            <li>Podaj adres e-mail powiązany z kontem na platformie.</li>
            <li>Napisz, którego kursu lub lekcji dotyczy sprawa.</li>
            <li>Nigdy nie wysyłaj w wiadomości danych karty ani hasła.</li>
          </ol>
          <div class="contact-tips__actions">
            <router-link class="btn btn-ghost btn-pill" :to="{ name: 'Faq' }">Przejdź do FAQ</router-link>
            <router-link class="btn btn-ghost btn-pill" :to="{ name: 'Help' }">Centrum pomocy</router-link>
          </div>
        </aside>
      </div>

      <div class="container">
        <nav class="contact-shortcuts" aria-label="Sprawy, które załatwisz sam">
          <router-link :to="{ name: 'ForgotPassword' }">
            <span class="contact-shortcuts__label">Konto</span>
            <span class="contact-shortcuts__title">Nie pamiętasz hasła?</span>
            <span class="contact-shortcuts__cta">Zresetuj hasło →</span>
          </router-link>
          <router-link :to="{ name: 'GiftRedeem' }">
            <span class="contact-shortcuts__label">Prezenty</span>
            <span class="contact-shortcuts__title">Masz kod prezentowy?</span>
            <span class="contact-shortcuts__cta">Zrealizuj kod →</span>
          </router-link>
          <router-link :to="{ name: 'Business' }">
            <span class="contact-shortcuts__label">Zespoły</span>
            <span class="contact-shortcuts__title">Szukasz planu dla firmy?</span>
            <span class="contact-shortcuts__cta">Oferta dla firm →</span>
          </router-link>
        </nav>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useSendSupportMessage } from '@/features/support/composables/useSupport'
import { supportMessageSchema, type SupportMessageForm } from '@/features/support/schemas/support.schema'

const supportMutation = useSendSupportMessage()

const { handleSubmit, defineField, errors, meta, resetForm } = useForm<SupportMessageForm>({
  validationSchema: toTypedSchema(supportMessageSchema)
})

const [name] = defineField('name')
const [email] = defineField('email')
const [subject] = defineField('subject')
const [message] = defineField('message')

const onSubmit = handleSubmit(async (values) => {
  await supportMutation.mutateAsync(values)
  resetForm()
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.contact-hero {
  padding: 32px 0 0;

  h1 {
    font-size: clamp(2.4rem, 4.6vw, 3.8rem);
    margin-top: 18px;
  }
}

.contact-head {
  margin-bottom: 40px;
}

.contact-content {
  padding: 0 0 96px;
}

.contact-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.4fr) minmax(260px, 0.75fr);
  gap: 48px;
  align-items: start;

  @media (max-width: 900px) {
    grid-template-columns: 1fr;
    gap: 40px;
  }
}

.contact-tips {
  padding: 26px 28px 28px;
  transform: rotate(1deg);

  h2 {
    margin: 18px 0 8px;
    font-size: 1.5rem;
    font-weight: 600;
    line-height: 1.15;
    color: #15171b;
  }

  @media (max-width: 900px) {
    max-width: 560px;
    transform: rotate(-0.5deg);
  }

  @media (max-width: 560px) {
    padding: 22px 20px 24px;
  }
}

.contact-tips__label {
  @include mono-label;
  display: block;
  padding-bottom: 12px;
  color: rgba(21, 23, 27, 0.6);
  border-bottom: 1px solid #15171b;
}

.contact-tips__list {
  margin: 14px 0 24px;
  padding: 0;
  list-style: none;
  counter-reset: tip;

  li {
    display: grid;
    grid-template-columns: 28px minmax(0, 1fr);
    padding: 10px 0;
    font-size: 0.92rem;
    line-height: 1.55;
    border-bottom: 1px dashed rgba(21, 23, 27, 0.18);
    counter-increment: tip;

    &::before {
      content: counter(tip, decimal-leading-zero);
      font-family: $font-mono;
      font-size: 0.72rem;
      padding-top: 3px;
      color: #15171b;
    }
  }
}

.contact-tips__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;

  .btn {
    padding: 10px 18px;
    font-size: 0.86rem;
  }
}

.contact-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 34px;

  @media (max-width: 560px) {
    --lg-r: 22px;
    padding: 24px 20px;
  }
}

.contact-shortcuts {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  margin-top: 80px;
  border-top: 1px solid $color-ink;

  a {
    display: flex;
    flex-direction: column;
    gap: 8px;
    padding: 22px 24px 20px 0;
    color: $color-ink;
    text-decoration: none;

    & + a {
      padding-left: 24px;
      border-left: 1px dashed $color-hairline;
    }

    &:hover .contact-shortcuts__cta {
      color: $color-accent;
    }
  }

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
    margin-top: 64px;

    a,
    a + a {
      padding: 18px 0;
      border-left: 0;
      border-bottom: 1px solid $color-hairline;
    }
  }
}

.contact-shortcuts__label {
  @include mono-label;
  font-size: 0.62rem;
  color: $color-faint;
}

.contact-shortcuts__title {
  font-family: $font-display;
  font-size: 1.2rem;
  font-weight: 500;
}

.contact-shortcuts__cta {
  @include mono-label;
  color: $color-muted;
  transition: color 0.2s;
}

.form-row {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;

  @media (max-width: 560px) {
    grid-template-columns: 1fr;
  }
}

.form-group {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 8px;

  label {
    @include mono-label;
    font-size: 0.62rem;
    color: $color-faint;
  }

  input,
  textarea {
    width: 100%;
    min-width: 0;
    border: 0;
    border-radius: $radius-md;
    outline: 0;
    background: rgba(13, 15, 19, 0.45);
    box-shadow: inset 0 0 0 1px $color-hairline;
    color: $color-ink;
    font: inherit;
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

  input {
    height: 48px;
    padding: 0 16px;
  }

  textarea {
    min-height: 160px;
    padding: 14px 16px;
    resize: vertical;
    line-height: 1.55;
  }
}

.error {
  color: $color-danger;
  font-size: 0.8rem;
  font-weight: 500;
}

.contact-form > .btn {
  align-self: flex-start;

  @media (max-width: 560px) {
    width: 100%;
  }
}
</style>
