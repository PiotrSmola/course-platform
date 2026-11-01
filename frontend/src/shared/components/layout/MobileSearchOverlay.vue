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
@use "@/assets/styles/abstracts/mixins" as *;

.mobile-search {
  position: fixed;
  inset: 0;
  z-index: 230;
  display: flex;
  flex-direction: column;
  padding: 18px 20px 20px;
  overflow-y: auto;
  background: $color-bg;
}

.mobile-search__top {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 22px;
  padding-bottom: 16px;
  border-bottom: 1px solid $color-hairline;
  color: $color-ink;
  font-family: $font-display;
  font-size: 1.35rem;
  font-weight: 600;
  letter-spacing: -0.01em;
}

.mobile-search__close {
  display: grid;
  width: 40px;
  height: 40px;
  place-items: center;
  flex-shrink: 0;
  border: 0;
  border-radius: 50%;
  background: rgba($color-ink, 0.06);
  box-shadow: inset 0 0 0 1px $color-hairline;
  color: $color-ink;
  cursor: pointer;

  svg {
    width: 20px;
    height: 20px;
  }
}

.mobile-search__input-wrap {
  --lg-r: 999px;
  --lg-blur: 4px;
  --lg-tint: rgba(13, 15, 19, 0.45);
  --lg-glow: 0.06;
  display: flex;
  align-items: center;
  gap: 11px;
  padding: 0 10px 0 18px;

  &:focus-within {
    box-shadow:
      0 12px 32px -8px rgba(0, 0, 0, 0.45),
      0 0 0 2px rgba($color-accent, 0.6);
  }

  > svg {
    width: 19px;
    height: 19px;
    flex-shrink: 0;
    color: $color-muted;
  }

  input {
    width: 100%;
    height: 54px;
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

    &:focus-visible {
      outline: none;
    }
  }
}

.mobile-search__clear {
  display: grid;
  width: 32px;
  height: 32px;
  flex-shrink: 0;
  place-items: center;
  border: 0;
  border-radius: 50%;
  background: rgba($color-ink, 0.1);
  color: $color-ink;
  font-size: 1.25rem;
  line-height: 1;
  cursor: pointer;
}

.mobile-search__results {
  flex: 1;
  padding: 18px 0;
}

.mobile-search__placeholder,
.mobile-search__state {
  margin: 18px 4px;
  color: $color-muted;
  line-height: 1.6;
}

.mobile-search__state {
  display: flex;
  gap: 10px;
  align-items: center;
}

.spinner {
  width: 17px;
  height: 17px;
  border: 2px solid rgba($color-ink, 0.12);
  border-top-color: $color-accent;
  border-radius: 50%;
  animation: mobile-search-spin 0.8s linear infinite;
}

.mobile-search__list {
  display: grid;
}

.mobile-search__result {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 16px 4px;
  border-bottom: 1px solid $color-hairline;
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
    font-size: 1rem;
    font-weight: 500;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  small {
    font-family: $font-mono;
    font-size: 0.7rem;
    color: $color-faint;
  }

  > svg {
    width: 18px;
    height: 18px;
    flex-shrink: 0;
    color: $color-faint;
  }
}

.mobile-search-enter-active,
.mobile-search-leave-active {
  transition: transform 260ms $ease-out;
}

.mobile-search-enter-from,
.mobile-search-leave-to {
  transform: translateX(100%);
}

@keyframes mobile-search-spin {
  to { transform: rotate(360deg); }
}
</style>
