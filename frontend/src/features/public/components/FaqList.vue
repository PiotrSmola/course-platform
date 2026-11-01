<template>
  <div class="faq-list">
    <details v-for="(item, index) in visibleItems" :key="item.question" class="faq-item">
      <summary>
        <span class="faq-num">{{ String(index + 1).padStart(2, '0') }}</span>
        <span class="faq-question">{{ item.question }}</span>
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" aria-hidden="true">
          <path d="M12 5v14M5 12h14" />
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
@use "@/assets/styles/abstracts/mixins" as *;

.faq-list {
  border-top: 1px solid $color-ink;
}

.faq-item {
  border-bottom: 1px solid $color-hairline;

  summary {
    display: grid;
    grid-template-columns: 48px minmax(0, 1fr) auto;
    align-items: baseline;
    gap: 12px;
    padding: 20px 4px;
    color: $color-ink;
    cursor: pointer;
    list-style: none;
    transition: color 0.2s;

    &::-webkit-details-marker {
      display: none;
    }

    &:hover {
      color: $color-accent;
    }

    svg {
      width: 18px;
      height: 18px;
      align-self: center;
      color: $color-muted;
      transition: transform 0.3s $ease-out, color 0.2s;
    }
  }

  &[open] summary svg {
    transform: rotate(45deg);
    color: $color-accent;
  }
}

.faq-num {
  font-family: $font-mono;
  font-size: 0.72rem;
  color: $color-faint;
}

.faq-question {
  font-family: $font-display;
  font-size: 1.15rem;
  font-weight: 500;
  line-height: 1.3;
}

.faq-answer {
  padding: 0 40px 22px 64px;

  p {
    max-width: 68ch;
    color: $color-muted;
    line-height: 1.65;
  }
}

@media (max-width: 560px) {
  .faq-item summary {
    grid-template-columns: 34px minmax(0, 1fr) auto;
    padding: 18px 0;
  }

  .faq-question {
    font-size: 1rem;
  }

  .faq-answer {
    padding: 0 0 18px 46px;
  }
}
</style>
