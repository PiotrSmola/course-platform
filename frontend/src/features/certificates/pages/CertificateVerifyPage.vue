<template>
  <div class="verify-page">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow glass no-warp">Weryfikacja certyfikatu</span>
        <h2>Sprawdź autentyczność</h2>
      </div>

      <div class="verify-card glass-card">
        <div v-if="verification.isLoading.value" class="state">
          <div class="spinner" />
          <p>Weryfikujemy certyfikat...</p>
        </div>

        <template v-else-if="verification.data.value">
          <div class="result-icon success">✓</div>
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
          <div class="result-icon error">✕</div>
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

.verify-page {
  padding: calc($header-height + 40px) 0 80px;
}

.verify-card {
  max-width: 560px;
  margin: 0 auto;
  padding: 44px 40px;
  text-align: center;
}

.state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  color: $color-muted;
}

.spinner {
  width: 42px;
  height: 42px;
  border: 3px solid rgba(255, 255, 255, 0.1);
  border-top-color: $color-gold;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.result-icon {
  width: 64px;
  height: 64px;
  margin: 0 auto 18px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  font-size: 1.8rem;
  font-weight: 700;

  &.success {
    background: rgba(34, 197, 94, 0.18);
    color: #4ade80;
    box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.25), 0 0 0 1px rgba(74, 222, 128, 0.3);
  }

  &.error {
    background: rgba(239, 68, 68, 0.18);
    color: #f87171;
    box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.25), 0 0 0 1px rgba(248, 113, 113, 0.3);
  }
}

.verdict {
  font-size: 0.9rem;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: $color-muted;
  margin-bottom: 14px;
}

.holder {
  font-size: 1.8rem;
  margin-bottom: 6px;
}

.course {
  color: $color-muted;
  margin-bottom: 28px;
}

.details {
  display: grid;
  gap: 8px;
  margin: 0;
  text-align: left;

  > div {
    display: grid;
    grid-template-columns: 1fr auto;
    gap: 12px;
    padding: 10px 14px;
    border-radius: 12px;
    background: rgba(255, 255, 255, 0.04);
    box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.14), inset 0 0 0 1px rgba(255, 255, 255, 0.05);
    font-size: 0.9rem;
  }

  dt {
    color: $color-muted;
  }

  dd {
    margin: 0;
    font-weight: 600;
    font-variant-numeric: tabular-nums;
  }
}

.hint {
  color: $color-muted;
  font-size: 0.92rem;
}
</style>
