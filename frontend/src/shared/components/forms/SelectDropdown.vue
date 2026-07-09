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
  --lg-r: 14px;
  --lg-blur: 0px;
  --lg-tint: rgba(255, 255, 255, 0.04);
  appearance: none;
  background: none;
  border: none;
  font: inherit;
  font-size: 0.9rem;
  color: $color-ink;
  padding: 13px 16px;
  cursor: pointer;
  outline: none;
  width: 100%;
  min-width: 180px;
  display: inline-flex;
  align-items: center;
  gap: 10px;
  text-align: left;
  box-shadow:
    0 10px 30px rgba(3, 6, 24, 0.35),
    0 2px 8px rgba(3, 6, 24, 0.22),
    0 18px 30px -22px rgba(170, 200, 255, 0.35),
    inset 0 1px 1px rgba(255, 255, 255, 0.3),
    inset 0 0 0 1px rgba(255, 255, 255, 0.1);
  transition: --lg-tint 0.35s, box-shadow 0.35s, transform 0.2s;

  &:hover {
    --lg-tint: rgba(245, 158, 11, 0.08);
    box-shadow:
      0 10px 30px rgba(3, 6, 24, 0.4),
      0 2px 8px rgba(3, 6, 24, 0.25),
      0 18px 30px -22px rgba(245, 158, 11, 0.4),
      inset 0 1px 1px rgba(255, 255, 255, 0.35),
      inset 0 0 0 1px rgba(245, 158, 11, 0.3);
  }

  &.open {
    --lg-tint: rgba(245, 158, 11, 0.1);
  }

  &:focus-visible {
    outline: none;
  }
}

.select-trigger::before {
  inset: 0;
  clip-path: none;
}

.select-label {
  color: $color-faint;
  font-size: 0.78rem;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.06em;
}

.select-value {
  flex: 1;
  color: $color-ink;
  font-weight: 600;
}

.select-arrow {
  color: $color-gold;
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
  --lg-r: 16px;
  --lg-blur: 2px;
  --lg-tint: rgba(17, 24, 39, 0.55);
  position: absolute;
  top: calc(100% + 8px);
  left: 0;
  right: 0;
  min-width: 180px;
  margin: 0;
  padding: 8px;
  list-style: none;
  z-index: 30;
  box-shadow:
    0 24px 60px rgba(3, 6, 24, 0.55),
    0 4px 14px rgba(3, 6, 24, 0.35),
    inset 0 1px 1px rgba(255, 255, 255, 0.18),
    inset 0 0 0 1px rgba(255, 255, 255, 0.1);
  transition: --lg-tint 0.35s, box-shadow 0.35s;
}

.select-menu::before {
  content: "";
  position: absolute;
  inset: 0;
  border-radius: inherit;
  pointer-events: none;
  background:
    radial-gradient(180px 100px at 18% 0%, rgba(255, 255, 255, 0.07), transparent 65%),
    radial-gradient(120px 80px at 100% 100%, rgba(245, 158, 11, 0.05), transparent 70%);
}

.select-blob {
  position: absolute;
  top: 0;
  left: 0;
  border-radius: 12px;
  background: linear-gradient(135deg, rgba(245, 158, 11, 0.16), rgba(245, 158, 11, 0.08));
  box-shadow:
    inset 1.5px 1.5px 2px -1px rgba(255, 255, 255, 0.35),
    inset -1.5px -1.5px 2px -1px rgba(255, 255, 255, 0.08),
    inset 0 -8px 16px -10px rgba(245, 158, 11, 0.25);
  opacity: 0;
  pointer-events: none;
  transition:
    left 0.45s cubic-bezier(0.3, 1.55, 0.35, 1),
    width 0.45s cubic-bezier(0.3, 1.55, 0.35, 1),
    top 0.45s cubic-bezier(0.3, 1.55, 0.35, 1),
    height 0.45s cubic-bezier(0.3, 1.55, 0.35, 1),
    opacity 0.2s;
}

.select-option {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px 14px;
  font-size: 0.9rem;
  color: $color-ink;
  cursor: pointer;
  transition: color 0.18s;

  svg {
    color: $color-gold;
    flex-shrink: 0;
  }

  &.active {
    color: $color-ink;
    font-weight: 600;
  }

  &:focus-visible {
    outline: none;
  }
}
</style>
