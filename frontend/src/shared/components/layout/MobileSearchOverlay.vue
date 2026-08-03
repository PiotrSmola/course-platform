<template>
  <Transition name="mobile-search">
    <div v-if="modelValue" class="mobile-search" role="dialog" aria-modal="true" aria-label="Wyszukiwanie kursów" @keydown.escape="close">
      <div class="mobile-search__top">
        <button type="button" class="mobile-search__close" aria-label="Zamknij wyszukiwanie" @click="close">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
            <path d="m15 18-6-6 6-6" />
          </svg>
        </button>
        <span>Wyszukiwanie</span>
      </div>

      <div class="mobile-search__input-wrap glass">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
          <circle cx="11" cy="11" r="7" />
          <path d="m20 20-4.2-4.2" />
        </svg>
        <input
          ref="inputRef"
          v-model="searchQuery"
          type="search"
          placeholder="Szukaj kursów, kategorii, technologii…"
          autocomplete="off"
        />
        <button v-if="searchQuery" type="button" class="mobile-search__clear" aria-label="Wyczyść wyszukiwanie" @click="searchQuery = ''">×</button>
      </div>

      <div class="mobile-search__results">
        <p v-if="!debouncedQuery" class="mobile-search__placeholder">Wpisz to, czego chcesz się nauczyć.</p>
        <div v-else-if="isLoading" class="mobile-search__state"><span class="spinner" aria-hidden="true" />Szukamy kursów…</div>
        <p v-else-if="!results?.items.length" class="mobile-search__placeholder">Nie znaleźliśmy wyników dla „{{ debouncedQuery }}”.</p>
        <div v-else class="mobile-search__list">
          <router-link
            v-for="course in results.items"
            :key="course.id"
            :to="{ name: 'CourseDetails', params: { id: course.id } }"
            class="mobile-search__result"
            @click="close"
          >
            <span>
              <strong>{{ course.title }}</strong>
              <small>{{ course.instructorName }} · {{ course.averageRating.toFixed(1) }} ★</small>
            </span>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><path d="m9 18 6-6-6-6" /></svg>
          </router-link>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { nextTick, onBeforeUnmount, ref, watch } from 'vue'
import { useCourseSearch } from '@/features/courses/composables/useCourseMeta'

const props = defineProps<{
  modelValue: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

const inputRef = ref<HTMLInputElement | null>(null)
const searchQuery = ref('')
const debouncedQuery = ref('')
let searchTimer: ReturnType<typeof setTimeout> | null = null

const { data: results, isLoading } = useCourseSearch(debouncedQuery)

function close() {
  emit('update:modelValue', false)
  searchQuery.value = ''
  debouncedQuery.value = ''
}

watch(searchQuery, (value) => {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    debouncedQuery.value = value.trim()
  }, 250)
})

watch(() => props.modelValue, async (isOpen) => {
  document.body.classList.toggle('is-overlay-open', isOpen)
  if (isOpen) {
    await nextTick()
    inputRef.value?.focus()
  }
})

onBeforeUnmount(() => {
  if (searchTimer) clearTimeout(searchTimer)
  document.body.classList.remove('is-overlay-open')
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.mobile-search {
  position: fixed;
  inset: 0;
  z-index: 230;
  display: flex;
  flex-direction: column;
  padding: 20px;
  overflow-y: auto;
  background:
    radial-gradient(600px 400px at 100% 0%, rgba(245, 158, 11, 0.2), transparent 70%),
    $color-bg;
}

.mobile-search__top {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 26px;
  color: $color-ink;
  font-family: $font-display;
  font-size: 1.05rem;
  font-weight: 700;
}

.mobile-search__close {
  display: grid;
  width: 42px;
  height: 42px;
  place-items: center;
  border: 1px solid rgba(255, 255, 255, 0.12);
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.06);
  color: $color-ink;
  cursor: pointer;

  svg {
    width: 21px;
    height: 21px;
  }
}

.mobile-search__input-wrap {
  --lg-r: 18px;
  --lg-blur: 2px;
  display: flex;
  align-items: center;
  gap: 11px;
  padding: 0 14px;

  > svg {
    width: 21px;
    height: 21px;
    flex-shrink: 0;
    color: $color-gold;
  }

  input {
    width: 100%;
    height: 58px;
    min-width: 0;
    border: 0;
    outline: 0;
    background: transparent;
    color: $color-ink;
    font: inherit;
    font-size: 1rem;

    &::placeholder {
      color: $color-faint;
    }
  }
}

.mobile-search__clear {
  display: grid;
  width: 30px;
  height: 30px;
  place-items: center;
  border: 0;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.1);
  color: $color-ink;
  font-size: 1.35rem;
  line-height: 1;
  cursor: pointer;
}

.mobile-search__results {
  flex: 1;
  padding: 22px 0;
}

.mobile-search__placeholder,
.mobile-search__state {
  margin: 28px auto;
  color: $color-muted;
  line-height: 1.6;
  text-align: center;
}

.mobile-search__state {
  display: flex;
  justify-content: center;
  gap: 10px;
  align-items: center;
}

.spinner {
  width: 19px;
  height: 19px;
  border: 2px solid rgba(255, 255, 255, 0.15);
  border-top-color: $color-gold;
  border-radius: 50%;
  animation: mobile-search-spin 0.8s linear infinite;
}

.mobile-search__list {
  display: grid;
  gap: 8px;
}

.mobile-search__result {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 16px;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.05);
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.09);
  color: $color-ink;
  text-decoration: none;

  > span {
    display: grid;
    min-width: 0;
    gap: 5px;
  }

  strong {
    overflow: hidden;
    font-family: $font-display;
    font-size: 0.98rem;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  small {
    color: $color-muted;
    font-size: 0.8rem;
  }

  > svg {
    width: 19px;
    height: 19px;
    flex-shrink: 0;
    color: $color-gold;
  }
}

.mobile-search-enter-active,
.mobile-search-leave-active {
  transition: opacity 160ms ease;
}

.mobile-search-enter-from,
.mobile-search-leave-to {
  opacity: 0;
}

@keyframes mobile-search-spin {
  to { transform: rotate(360deg); }
}
</style>
