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
        <aside class="contact-info">
          <span class="contact-info__eyebrow">Pomoc na każdym etapie</span>
          <h2>Najpierw sprawdź szybkie odpowiedzi</h2>
          <p>W FAQ opisujemy zasady triala, płatności, kodów rabatowych, prezentów i dostępu All-access.</p>
          <router-link class="btn btn-ghost" :to="{ name: 'Faq' }">Przejdź do FAQ</router-link>
          <div class="contact-info__note">
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
  grid-template-columns: minmax(240px, 0.7fr) minmax(0, 1.3fr);
  gap: 48px;
  align-items: start;

  @media (max-width: 840px) {
    grid-template-columns: 1fr;
    gap: 32px;
  }
}

.contact-info {
  padding-top: 4px;

  h2 {
    margin: 14px 0 12px;
    font-size: 1.6rem;
    font-weight: 600;
    line-height: 1.2;
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
  @include mono-label;
  color: $color-accent;
}

.contact-info__note {
  margin-top: 36px;
  padding-top: 16px;
  border-top: 1px solid $color-hairline;
  font-family: $font-mono;
  font-size: 0.74rem;
  line-height: 1.6;
  color: $color-faint;
}

.contact-form {
  --lg-blur: 10px;
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 30px;

  &:hover {
    transform: none;
  }

  @media (max-width: 560px) {
    padding: 24px 20px;
  }
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
