<template>
  <div class="faq-list">
    <details v-for="item in visibleItems" :key="item.question" class="faq-item glass-card">
      <summary>
        <span>{{ item.question }}</span>
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
          <path d="m6 9 6 6 6-6" />
        </svg>
      </summary>
      <div class="faq-answer">
        <p>{{ item.answer }}</p>
      </div>
    </details>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  compact?: boolean
}>(), {
  compact: false
})

const items = [
  {
    question: 'Jak działa bezpłatny trial?',
    answer: 'Po założeniu konta wybierasz jeden kurs i otrzymujesz dostęp do jego dwóch pierwszych lekcji. Trial jest jednorazowy i pozostaje przypisany do wybranego kursu.'
  },
  {
    question: 'Czy mogę kupić kurs bez subskrypcji?',
    answer: 'Tak. Każdy płatny kurs możesz kupić osobno i zachować do niego dostęp na swoim koncie. W trakcie zakupu możesz też zastosować kod rabatowy.'
  },
  {
    question: 'Co daje All-access?',
    answer: 'Aktywna subskrypcja All-access odblokowuje wszystkie opublikowane kursy. Planem i płatnościami zarządzasz bezpośrednio w bezpiecznym panelu rozliczeń.'
  },
  {
    question: 'Kiedy otrzymam certyfikat?',
    answer: 'Certyfikat jest dostępny po ukończeniu wszystkich wymaganych lekcji kursu. Ma własny numer, który można zweryfikować na stronie platformy.'
  },
  {
    question: 'Czy mogę uczyć się na telefonie?',
    answer: 'Tak. Kursy, materiały i odtwarzacz są przygotowane do wygodnej nauki na małych ekranach, a postęp pozostaje zapisany na Twoim koncie.'
  },
  {
    question: 'Czy kurs mogę kupić komuś w prezencie?',
    answer: 'Tak. Wybierz opcję zakupu prezentu przy kursie, podaj adres odbiorcy, a po opłaceniu zakupu otrzyma on jednorazowy kod dostępu.'
  },
  {
    question: 'Czym różni się kod rabatowy od kodu prezentowego?',
    answer: 'Kod rabatowy obniża cenę w checkoutcie. Kod prezentowy jest jednorazowym kluczem, który dodaje konkretny kurs do konta odbiorcy.'
  },
  {
    question: 'Gdzie uzyskam pomoc?',
    answer: 'Napisz do nas przez formularz kontaktowy. Opisz problem i podaj adres e-mail powiązany z kontem, a zespół wsparcia odpowie najszybciej jak to możliwe.'
  }
]

const visibleItems = computed(() => props.compact ? items.slice(0, 4) : items)
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.faq-list {
  display: grid;
  gap: 12px;
}

.faq-item {
  --lg-r: 18px;
  --lg-blur: 2px;
  overflow: hidden;

  summary,
  .faq-answer {
    position: relative;
    z-index: 1;
  }

  summary {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 18px;
    padding: 20px 22px;
    color: $color-ink;
    cursor: pointer;
    font-family: $font-display;
    font-size: 1rem;
    font-weight: 600;
    list-style: none;

    &::-webkit-details-marker {
      display: none;
    }

    svg {
      width: 19px;
      height: 19px;
      flex-shrink: 0;
      color: $color-gold;
      transition: transform 160ms ease;
    }
  }

  &[open] summary svg {
    transform: rotate(180deg);
  }
}

.faq-answer {
  padding: 0 22px 21px;

  p {
    max-width: 900px;
    color: $color-muted;
    line-height: 1.65;
  }
}

@media (max-width: 560px) {
  .faq-item summary {
    padding: 18px;
    font-size: 0.94rem;
  }

  .faq-answer {
    padding: 0 18px 18px;
  }
}
</style>
