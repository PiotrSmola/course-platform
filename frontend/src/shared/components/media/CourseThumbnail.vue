<template>
  <div class="thumb" :style="style">
    <div v-if="!url" class="cover" :style="coverStyle" aria-hidden="true">
      <div class="cover-band">
        <span class="cover-kicker">{{ label || 'CoursePlatform' }}</span>
        <span class="cover-number">No. {{ number }}</span>
      </div>
      <span class="cover-title">{{ shortTitle }}</span>
      <span v-if="label" class="cover-mark">CoursePlatform</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  url?: string | null
  title?: string
  label?: string | null
  seed?: string
}>()

const COVERS = [
  { bg: '#1b1840', band: '#4a34d6' },
  { bg: '#0e2f31', band: '#1f8f8a' },
  { bg: '#331710', band: '#d0582d' },
  { bg: '#22260f', band: '#a9c93a' },
  { bg: '#2e1026', band: '#c2457f' },
  { bg: '#121d2b', band: '#5b8bd6' },
  { bg: '#2a2410', band: '#e0b53c' },
  { bg: '#1c1c1f', band: '#d9d4c7' }
]

const hash = computed(() => {
  const source = props.seed || props.title || ''
  let h = 0x811c9dc5
  for (let i = 0; i < source.length; i++) {
    h ^= source.charCodeAt(i)
    h = Math.imul(h, 0x01000193) >>> 0
  }
  return h
})

const number = computed(() => String((hash.value % 97) + 1).padStart(2, '0'))

const shortTitle = computed(() => {
  const title = props.title ?? ''
  return title.split(/\s[—–-]\s|:\s/)[0]
})

const style = computed(() => {
  return props.url ? { backgroundImage: `url(${props.url})` } : undefined
})

const coverStyle = computed(() => {
  const cover = COVERS[hash.value % COVERS.length]
  return { '--cover-bg': cover.bg, '--cover-band': cover.band }
})
</script>

<style lang="scss" scoped>
.thumb {
  container-type: inline-size;
  overflow: hidden;
  background-color: $color-surface;
  background-size: cover;
  background-position: center;
}

.cover {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  padding: 0 7cqi 6cqi;
  background:
    linear-gradient(rgba(242, 239, 230, 0.05) 1px, transparent 1px) 0 0 / 100% 7cqi,
    var(--cover-bg);
  color: $color-ink;
}

.cover-band {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 8px;
  margin: 0 -7cqi;
  padding: 0 7cqi 3cqi;
  height: 34%;
  background: var(--cover-band);
  color: rgba(13, 15, 19, 0.82);
}

.cover-kicker,
.cover-number,
.cover-mark {
  font-family: $font-mono;
  font-size: clamp(0.56rem, 3.4cqi, 0.8rem);
  font-weight: 500;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  white-space: nowrap;
}

.cover-kicker {
  overflow: hidden;
  text-overflow: ellipsis;
}

.cover-title {
  margin-top: 5cqi;
  font-family: $font-display;
  font-size: clamp(0.9rem, 6.6cqi, 2.6rem);
  font-weight: 600;
  line-height: 1.08;
  letter-spacing: -0.01em;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.cover-mark {
  position: absolute;
  left: 7cqi;
  bottom: 5cqi;
  color: rgba(242, 239, 230, 0.4);
}
</style>
