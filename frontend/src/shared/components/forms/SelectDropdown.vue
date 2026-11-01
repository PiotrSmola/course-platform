<template>
  <div class="select-dropdown" ref="boxRef">
    <button
      type="button"
      class="select-trigger"
      :class="{ open: isOpen }"
      @click="isOpen = !isOpen"
    >
      <span v-if="label" class="select-label">{{ label }}</span>
      <span class="select-value">{{ currentLabel }}</span>
      <svg
        class="select-arrow"
        :class="{ flipped: isOpen }"
        width="14"
        height="14"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        stroke-width="2.4"
        stroke-linecap="round"
        stroke-linejoin="round"
      >
        <polyline points="6 9 12 15 18 9" />
      </svg>
    </button>
    <ul v-if="isOpen" class="select-menu" @pointerleave="hideBlob">
      <span ref="blobRef" class="select-blob" aria-hidden="true"></span>
      <li
        v-for="opt in options"
        :key="String(opt.value)"
        class="select-option"
        :class="{ active: modelValue === opt.value }"
        @click="select(opt.value)"
        @pointerenter="moveBlob($event)"
      >
        <span>{{ opt.label }}</span>
        <svg
          v-if="modelValue === opt.value"
          width="14"
          height="14"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2.6"
          stroke-linecap="round"
          stroke-linejoin="round"
        >
          <polyline points="20 6 9 17 4 12" />
        </svg>
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'

export type SelectOptionValue = string | number | undefined

const props = defineProps<{
  modelValue: SelectOptionValue
  options: { value: SelectOptionValue; label: string }[]
  label?: string
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: SelectOptionValue): void
}>()

const isOpen = ref(false)
const boxRef = ref<HTMLElement | null>(null)
const blobRef = ref<HTMLElement | null>(null)

const currentLabel = computed(() => props.options.find((o) => o.value === props.modelValue)?.label ?? '')

function select(value: SelectOptionValue) {
  emit('update:modelValue', value)
  isOpen.value = false
}

function moveBlob(e: PointerEvent) {
  if (!blobRef.value) return
  const target = e.currentTarget as HTMLElement
  const li = target.closest('li')
  if (!li) return
  const ul = li.parentElement
  if (!ul) return
  const ulRect = ul.getBoundingClientRect()
  const liRect = li.getBoundingClientRect()
  blobRef.value.style.left = `${liRect.left - ulRect.left}px`
  blobRef.value.style.width = `${liRect.width}px`
  blobRef.value.style.top = `${liRect.top - ulRect.top}px`
  blobRef.value.style.height = `${liRect.height}px`
  blobRef.value.style.opacity = '1'
}

function hideBlob() {
  if (!blobRef.value) return
  blobRef.value.style.opacity = '0'
}

function handleClickOutside(event: MouseEvent) {
  if (isOpen.value && boxRef.value && !boxRef.value.contains(event.target as Node)) {
    isOpen.value = false
  }
}

onMounted(() => document.addEventListener('mousedown', handleClickOutside))
onBeforeUnmount(() => document.removeEventListener('mousedown', handleClickOutside))
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.select-dropdown {
  position: relative;
}

.select-trigger {
  @include liquid-glass;
  --lg-r: #{$radius-md};
  --lg-blur: 0px;
  --lg-tint: rgba(13, 15, 19, 0.45);
  --lg-glow: 0.06;
  appearance: none;
  background: none;
  border: none;
  font: inherit;
  font-size: 0.9rem;
  color: $color-ink;
  padding: 12px 14px 12px 16px;
  cursor: pointer;
  outline: none;
  width: 100%;
  min-width: 180px;
  display: inline-flex;
  align-items: center;
  gap: 10px;
  text-align: left;
  box-shadow:
    0 8px 22px -10px rgba(0, 0, 0, 0.45),
    inset 0 0 0 1px $color-hairline;
  transition: --lg-tint 0.3s, box-shadow 0.2s;

  &:hover {
    --lg-tint: rgba(13, 15, 19, 0.3);
    box-shadow:
      0 8px 22px -10px rgba(0, 0, 0, 0.45),
      inset 0 0 0 1px rgba($color-ink, 0.2);
  }

  &.open,
  &:focus-visible {
    outline: none;
    box-shadow:
      0 8px 22px -10px rgba(0, 0, 0, 0.45),
      inset 0 0 0 1px $color-hairline,
      0 0 0 2px rgba($color-accent, 0.6);
  }
}

.select-trigger::before {
  inset: 0;
  clip-path: none;
}

.select-label {
  @include mono-label;
  font-size: 0.62rem;
  color: $color-faint;
}

.select-value {
  flex: 1;
  color: $color-ink;
  font-weight: 500;
}

.select-arrow {
  color: $color-muted;
  transition: transform 0.25s;
  flex-shrink: 0;

  &.flipped {
    transform: rotate(180deg);
  }
}

.select-trigger.open .select-arrow {
  color: $color-ink;
}

.select-menu {
  @include liquid-glass;
  --lg-r: #{$radius-md};
  --lg-blur: 14px;
  --lg-sat: 140%;
  --lg-tint: rgba(13, 15, 19, 0.66);
  --lg-glow: 0.05;
  position: absolute;
  top: calc(100% + 8px);
  left: 0;
  right: 0;
  min-width: 180px;
  margin: 0;
  padding: 6px;
  list-style: none;
  z-index: 30;
  box-shadow:
    0 24px 50px -16px rgba(0, 0, 0, 0.65),
    0 4px 12px rgba(0, 0, 0, 0.3);
}

.select-blob {
  position: absolute;
  top: 0;
  left: 0;
  border-radius: 10px;
  background: rgba($color-ink, 0.08);
  box-shadow: inset 0 0 0 1px rgba($color-ink, 0.08);
  opacity: 0;
  pointer-events: none;
  transition:
    left 0.45s $ease-jelly,
    width 0.45s $ease-jelly,
    top 0.45s $ease-jelly,
    height 0.45s $ease-jelly,
    opacity 0.2s;
}

.select-option {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 12px;
  font-size: 0.9rem;
  color: $color-muted;
  cursor: pointer;
  transition: color 0.18s;

  & + & {
    box-shadow: 0 -1px 0 rgba($color-ink, 0.06);
  }

  &:hover {
    color: $color-ink;
  }

  svg {
    color: $color-accent;
    flex-shrink: 0;
  }

  &.active {
    color: $color-ink;
    font-weight: 500;
  }

  &:focus-visible {
    outline: none;
  }
}
</style>
