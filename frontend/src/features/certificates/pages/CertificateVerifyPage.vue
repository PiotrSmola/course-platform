<template>
  <div class="verify-page">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Weryfikacja certyfikatu</span>
        <h2>Sprawdź <em>autentyczność</em></h2>
      </div>

      <div class="verify-card glass-card">
        <div v-if="verification.isLoading.value" class="state">
          <div class="spinner" />
          <p>Weryfikujemy certyfikat...</p>
        </div>

        <template v-else-if="verification.data.value">
          <div class="result-status success"><span>Status</span><b>✓ Zweryfikowany</b></div>
          <p class="verdict">Certyfikat jest autentyczny</p>
          <h3 class="holder">{{ verification.data.value.holderName }}</h3>
          <p class="course">ukończył(a) kurs <strong>{{ verification.data.value.courseTitle }}</strong></p>
          <dl class="details">
            <div>
              <dt>Numer certyfikatu</dt>
              <dd>{{ verification.data.value.number }}</dd>
            </div>
            <div>
              <dt>Data wystawienia</dt>
              <dd>{{ new Date(verification.data.value.issuedAt).toLocaleDateString('pl-PL') }}</dd>
            </div>
            <div>
              <dt>Instruktor</dt>
              <dd>{{ verification.data.value.instructorName }}</dd>
            </div>
          </dl>
        </template>

        <template v-else>
          <div class="result-status error"><span>Status</span><b>✕ Brak w rejestrze</b></div>
          <p class="verdict">Nie znaleziono certyfikatu</p>
          <p class="hint">Certyfikat o numerze „{{ number }}” nie istnieje lub numer jest błędny.</p>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useCertificateVerification } from '@/features/certificates/composables/useCertificates'

const route = useRoute()
const number = computed(() => (route.params.number as string) ?? '')
const verification = useCertificateVerification(() => number.value)
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.verify-page {
  padding: 32px 0 96px;
}

.verify-card {
  --lg-blur: 10px;
  max-width: 640px;
  padding: 28px 32px 32px;

  &:hover {
    transform: none;
  }

  @media (max-width: 560px) {
    padding: 24px 20px;
  }
}

.state {
  display: flex;
  align-items: center;
  gap: 14px;

  p {
    @include mono-label;
    color: $color-faint;
  }
}

.spinner {
  width: 22px;
  height: 22px;
  border: 2px solid rgba($color-ink, 0.1);
  border-top-color: $color-accent;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.result-status {
  @include mono-label;
  display: flex;
  justify-content: space-between;
  gap: 12px;
  padding-bottom: 14px;
  margin-bottom: 22px;
  font-size: 0.64rem;
  color: $color-faint;
  border-bottom: 1px solid $color-hairline;

  b {
    font-weight: 500;
  }

  &.success b {
    color: $color-success;
  }

  &.error b {
    color: $color-danger;
  }
}

.verdict {
  @include mono-label;
  color: $color-muted;
  margin-bottom: 12px;
}

.holder {
  font-size: clamp(2rem, 5vw, 2.8rem);
  font-weight: 600;
  margin-bottom: 8px;
}

.course {
  color: $color-muted;
  margin-bottom: 28px;

  strong {
    color: $color-ink;
    font-weight: 600;
  }
}

.details {
  margin: 0;
  border-top: 1px solid $color-ink;

  > div {
    display: grid;
    grid-template-columns: minmax(0, 1fr) auto;
    gap: 12px;
    padding: 12px 0;
    border-bottom: 1px solid $color-hairline;
  }

  dt {
    @include mono-label;
    font-size: 0.62rem;
    color: $color-faint;
    align-self: center;
  }

  dd {
    margin: 0;
    font-family: $font-mono;
    font-size: 0.84rem;
    text-align: right;
    font-variant-numeric: tabular-nums;
  }
}

.hint {
  color: $color-muted;
  font-size: 0.92rem;
}
</style>
