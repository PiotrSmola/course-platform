<template>
  <div class="contact-page">
    <section class="contact-hero">
      <div class="container">
        <span class="eyebrow">Kontakt</span>
        <h1>Jak możemy pomóc?</h1>
        <p>Opisz swój problem lub pytanie. Wiadomość trafi bezpośrednio do zespołu wsparcia CoursePlatform.</p>
      </div>
    </section>

    <section class="contact-content">
      <div class="container contact-grid">
        <aside class="contact-info glass-card">
          <span class="contact-info__eyebrow">Pomoc na każdym etapie</span>
          <h2>Najpierw sprawdź szybkie odpowiedzi</h2>
          <p>W FAQ opisujemy zasady triala, płatności, kodów rabatowych, prezentów i dostępu All-access.</p>
          <router-link class="btn btn-ghost" :to="{ name: 'Faq' }">Przejdź do FAQ</router-link>
          <div class="contact-info__note">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <circle cx="12" cy="12" r="9" />
              <path d="M12 8v4l2.5 2" />
            </svg>
            <span>Odpowiadamy na zgłoszenia tak szybko, jak to możliwe.</span>
          </div>
        </aside>

        <form class="contact-form glass-card" @submit="onSubmit">
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

.contact-hero {
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
    max-width: 660px;
    margin: 0 auto;
    color: $color-muted;
    line-height: 1.65;
  }
}

.contact-content {
  padding: 54px 0 96px;
}

.contact-grid {
  display: grid;
  grid-template-columns: minmax(260px, 0.7fr) minmax(0, 1.3fr);
  gap: 24px;
  align-items: start;

  @media (max-width: 840px) {
    grid-template-columns: 1fr;
  }
}

.contact-info,
.contact-form {
  --lg-r: 24px;
  --lg-blur: 2px;
  padding: 30px;

  @media (max-width: 560px) {
    padding: 24px 20px;
  }
}

.contact-info {
  h2 {
    margin: 13px 0 12px;
    font-family: $font-display;
    font-size: 1.35rem;
    line-height: 1.25;
  }

  > p {
    color: $color-muted;
    line-height: 1.65;
  }

  .btn {
    margin-top: 24px;
  }
}

.contact-info__eyebrow {
  color: $color-gold;
  font-size: 0.76rem;
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
}

.contact-info__note {
  display: flex;
  gap: 10px;
  align-items: flex-start;
  margin-top: 36px;
  padding-top: 20px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  color: $color-faint;
  font-size: 0.87rem;
  line-height: 1.5;

  svg {
    width: 20px;
    height: 20px;
    flex-shrink: 0;
    color: $color-gold;
  }
}

.contact-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
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
    color: $color-muted;
    font-size: 0.88rem;
    font-weight: 600;
  }

  input,
  textarea {
    width: 100%;
    min-width: 0;
    border: 0;
    border-radius: 14px;
    outline: 0;
    background: rgba(255, 255, 255, 0.05);
    box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.28), inset 0 0 0 1px rgba(255, 255, 255, 0.1);
    color: $color-ink;
    font: inherit;
    transition: background 160ms ease, box-shadow 160ms ease;

    &::placeholder {
      color: $color-faint;
    }

    &:focus {
      background: rgba(255, 255, 255, 0.08);
      box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.35), inset 0 0 0 1px rgba(34, 211, 238, 0.56), 0 0 0 4px rgba(34, 211, 238, 0.14);
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
  color: #fca5a5;
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
