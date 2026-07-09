<template>
  <div class="search-admin">
    <div class="status-banner" :class="bannerClass">
      <div class="banner-icon" aria-hidden="true">
        <span v-if="isRunning" class="spinner"></span>
        <span v-else-if="job?.status === ReindexStatus.Succeeded">✓</span>
        <span v-else-if="job?.status === ReindexStatus.Failed">!</span>
        <span v-else-if="job?.status === ReindexStatus.Cancelled">⊘</span>
        <span v-else>•</span>
      </div>
      <div class="banner-text">
        <strong>{{ statusLabel }}</strong>
        <span v-if="isRunning">{{ phaseLabel }}</span>
        <span v-else-if="job?.status === ReindexStatus.Succeeded">
          Indeks zawiera {{ stats?.documentCount ?? 0 }} dokument(ów).
        </span>
        <span v-else-if="job?.status === ReindexStatus.Failed && job.errorMessage">
          {{ job.errorMessage }}
        </span>
        <span v-else-if="statsError">
          Nie udało się pobrać stanu wyszukiwarki — sprawdź, czy API i Elasticsearch działają.
        </span>
        <span v-else-if="!stats">Ładowanie stanu wyszukiwarki...</span>
        <span v-else-if="!stats.enabled">
          Elasticsearch jest wyłączony w konfiguracji — reindex niedostępny.
        </span>
        <span v-else-if="!stats.exists">
          Indeks nie został jeszcze utworzony. Uruchom reindeksowanie.
        </span>
        <span v-else>Gotowy do reindeksowania.</span>
      </div>
    </div>

    <div class="grid">
      <article class="card glass-card">
        <header class="card-head">
          <h3>Indeks wyszukiwarki</h3>
          <button
            type="button"
            class="btn btn-primary"
            :disabled="!canStart"
            @click="onStart"
          >
            <span v-if="isRunning" class="spinner small"></span>
            {{ isRunning ? 'Reindeksowanie…' : 'Reindeksuj teraz' }}
          </button>
        </header>

        <dl class="kv">
          <div><dt>Alias</dt><dd>{{ stats?.aliasName ?? '—' }}</dd></div>
          <div><dt>Konkretny indeks</dt><dd>{{ stats?.concreteIndexName ?? '—' }}</dd></div>
          <div><dt>Dokumenty</dt><dd>{{ formatNumber(stats?.documentCount ?? 0) }}</dd></div>
          <div><dt>Rozmiar</dt><dd>{{ formatSize(stats?.sizeBytes) }}</dd></div>
          <div><dt>Status ES</dt><dd>{{ stats?.health ?? '—' }}</dd></div>
        </dl>
      </article>

      <article class="card glass-card progress-card">
        <header class="card-head">
          <h3>Postęp</h3>
          <span v-if="job" class="muted">{{ job.progress.percent.toFixed(1) }}%</span>
        </header>

        <div v-if="job" class="progress-wrap">
          <div class="progress-track">
            <div class="progress-bar" :style="{ width: progressWidth + '%' }"></div>
          </div>

          <div class="progress-meta">
            <span>
              {{ job.progress.processed }} / {{ job.progress.total }}
              <template v-if="job.progress.failed > 0">
                <span class="failed"> ({{ job.progress.failed }} błędów)</span>
              </template>
            </span>
            <span v-if="job.progress.batchesTotal > 0">
              Batch {{ job.progress.batchesCompleted }} / {{ job.progress.batchesTotal }}
            </span>
          </div>

          <div v-if="job.startedAt" class="timing">
            <span>Start: {{ formatTime(job.startedAt) }}</span>
            <span v-if="job.finishedAt">
              Koniec: {{ formatTime(job.finishedAt) }} ({{ durationLabel }})
            </span>
            <span v-else-if="isRunning">Trwa: {{ durationLabel }}</span>
          </div>
        </div>
        <div v-else class="muted empty">Brak aktywnych ani ostatnich zadań.</div>
      </article>

      <article class="card glass-card logs-card">
        <header class="card-head">
          <h3>Logi</h3>
          <span class="muted">{{ job?.logs.length ?? 0 }} wpis(ów)</span>
        </header>

        <div ref="logBox" class="logs">
          <div v-if="!job?.logs.length" class="muted empty">
            Logi pojawią się po uruchomieniu reindeksowania.
          </div>
          <div
            v-for="(entry, idx) in (job?.logs ?? [])"
            :key="idx"
            class="log-line"
            :class="REINDEX_LOG_LEVEL_CLASS[entry.level]"
          >
            <span class="log-time">{{ formatTime(entry.timestamp) }}</span>
            <span class="log-level">{{ REINDEX_LOG_LEVEL_LABEL[entry.level] }}</span>
            <span class="log-msg">{{ entry.message }}</span>
          </div>
        </div>
      </article>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import { useReindexJob, useSearchStats } from '@/features/admin/composables/useReindexJob'
import {
  REINDEX_LOG_LEVEL_CLASS,
  REINDEX_LOG_LEVEL_LABEL,
  REINDEX_PHASE_LABEL,
  REINDEX_STATUS_LABEL,
  ReindexStatus,
  isTerminalStatus
} from '@/features/admin/types/search.types'
import type { ReindexPhase } from '@/features/admin/types/search.types'

const { data: stats, isError: statsError } = useSearchStats()
const { job, isRunning, start, newLogCount } = useReindexJob()

const logBox = ref<HTMLElement | null>(null)
const now = ref(Date.now())

let nowTimer: number | undefined

function startNowTimer() {
  if (nowTimer !== undefined) return
  nowTimer = window.setInterval(() => {
    now.value = Date.now()
  }, 1000)
}

function stopNowTimer() {
  if (nowTimer !== undefined) {
    window.clearInterval(nowTimer)
    nowTimer = undefined
  }
}

watch(isRunning, (running) => {
  if (running) startNowTimer()
  else stopNowTimer()
}, { immediate: true })

watch(newLogCount, async () => {
  if (!newLogCount.value) return
  await nextTick()
  if (logBox.value) {
    logBox.value.scrollTop = logBox.value.scrollHeight
  }
})

const canStart = computed(() => {
  if (!stats.value) return false
  if (!stats.value.enabled) return false
  if (isRunning.value) return false
  return true
})

const statusLabel = computed(() => {
  const status = job.value?.status ?? ReindexStatus.Idle
  return REINDEX_STATUS_LABEL[status]
})

const phaseLabel = computed(() => {
  const phase: ReindexPhase | undefined = job.value?.phase
  return phase !== undefined ? REINDEX_PHASE_LABEL[phase] : ''
})

const progressWidth = computed(() => {
  if (!job.value) return 0
  if (isTerminalStatus(job.value.status) && job.value.progress.total === 0) return 100
  return Math.max(0, Math.min(100, job.value.progress.percent))
})

const bannerClass = computed(() => {
  if (isRunning.value) return 'banner-running'
  switch (job.value?.status) {
    case ReindexStatus.Succeeded: return 'banner-success'
    case ReindexStatus.Failed: return 'banner-error'
    case ReindexStatus.Cancelled: return 'banner-warning'
    default: return 'banner-neutral'
  }
})

const durationLabel = computed(() => {
  const start = job.value?.startedAt
  if (!start) return ''
  const end = job.value?.finishedAt ? new Date(job.value.finishedAt).getTime() : now.value
  return formatDuration(new Date(start).getTime(), end)
})

function onStart() {
  start.mutate(undefined)
}

function formatNumber(n: number): string {
  return new Intl.NumberFormat('pl-PL').format(n)
}

function formatSize(bytes: number | null | undefined): string {
  if (bytes == null) return '—'
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
  return `${(bytes / (1024 * 1024 * 1024)).toFixed(2)} GB`
}

function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString('pl-PL', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
}

function formatDuration(startMs: number, endMs: number): string {
  const sec = Math.max(0, Math.floor((endMs - startMs) / 1000))
  if (sec < 60) return `${sec} s`
  const m = Math.floor(sec / 60)
  const s = sec % 60
  if (m < 60) return `${m} min ${s} s`
  const h = Math.floor(m / 60)
  return `${h} h ${m % 60} min`
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.search-admin {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.status-banner {
  @include liquid-glass;
  --lg-r: 18px;
  --lg-blur: 0px;
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 14px 18px;

  strong {
    margin-right: 8px;
    color: $color-ink;
  }

  .banner-text {
    color: $color-muted;
    font-size: 0.92rem;
    line-height: 1.4;
  }

  .banner-icon {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    display: grid;
    place-items: center;
    background: rgba(255, 255, 255, 0.06);
    box-shadow:
      inset 0 1px 1px rgba(255, 255, 255, 0.3),
      inset 0 0 0 1px rgba(255, 255, 255, 0.08);
    font-weight: 700;
    color: $color-ink;
    flex-shrink: 0;
  }

  &.banner-running {
    --lg-tint: rgba(34, 211, 238, 0.1);

    &::after {
      box-shadow:
        inset 2px 2px 1px -1px rgba(255, 255, 255, 0.55),
        inset -2px -2px 1px -1px rgba(255, 255, 255, 0.2),
        inset 0 0 0 1px rgba(34, 211, 238, 0.35),
        inset 0 -12px 24px -14px rgba(34, 211, 238, 0.45);
    }

    .banner-icon {
      background: rgba(34, 211, 238, 0.2);
      color: $color-cyan;
    }
  }

  &.banner-success {
    --lg-tint: rgba(34, 197, 94, 0.1);

    &::after {
      box-shadow:
        inset 2px 2px 1px -1px rgba(255, 255, 255, 0.55),
        inset -2px -2px 1px -1px rgba(255, 255, 255, 0.2),
        inset 0 0 0 1px rgba(34, 197, 94, 0.35),
        inset 0 -12px 24px -14px rgba(34, 197, 94, 0.45);
    }

    .banner-icon {
      background: rgba(34, 197, 94, 0.2);
      color: #4ade80;
    }
  }

  &.banner-error {
    --lg-tint: rgba(244, 63, 94, 0.1);

    &::after {
      box-shadow:
        inset 2px 2px 1px -1px rgba(255, 255, 255, 0.55),
        inset -2px -2px 1px -1px rgba(255, 255, 255, 0.2),
        inset 0 0 0 1px rgba(244, 63, 94, 0.35),
        inset 0 -12px 24px -14px rgba(244, 63, 94, 0.45);
    }

    .banner-icon {
      background: rgba(244, 63, 94, 0.2);
      color: $color-rose;
    }
  }

  &.banner-warning {
    --lg-tint: rgba(245, 158, 11, 0.1);

    &::after {
      box-shadow:
        inset 2px 2px 1px -1px rgba(255, 255, 255, 0.55),
        inset -2px -2px 1px -1px rgba(255, 255, 255, 0.2),
        inset 0 0 0 1px rgba(245, 158, 11, 0.35),
        inset 0 -12px 24px -14px rgba(245, 158, 11, 0.45);
    }

    .banner-icon {
      background: rgba(245, 158, 11, 0.2);
      color: $color-gold;
    }
  }
}

.grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
  grid-template-areas:
    "stats progress"
    "logs logs";
  gap: 16px;
}

@media (max-width: 880px) {
  .grid {
    grid-template-columns: 1fr;
    grid-template-areas:
      "stats"
      "progress"
      "logs";
  }
}

.card {
  padding: 18px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  min-width: 0;

  &:nth-child(1) { grid-area: stats; }
  &:nth-child(2) { grid-area: progress; }
  &:nth-child(3) { grid-area: logs; }
}

.card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;

  h3 {
    margin: 0;
    font-size: 1rem;
    color: $color-ink;
    font-weight: 600;
  }
}

.kv {
  display: grid;
  grid-template-columns: 1fr;
  gap: 8px;
  margin: 0;

  > div {
    display: grid;
    grid-template-columns: 1fr auto;
    gap: 12px;
    padding: 7px 10px;
    border-radius: 10px;
    box-shadow: inset 0 -1px 0 rgba(255, 255, 255, 0.05);
    font-size: 0.88rem;
    transition: background 0.2s;

    &:hover { background: rgba(255, 255, 255, 0.04); }
    &:last-child { box-shadow: none; }
  }

  dt {
    color: $color-muted;
    margin: 0;
  }

  dd {
    margin: 0;
    color: $color-ink;
    font-variant-numeric: tabular-nums;
    text-align: right;
    word-break: break-all;
  }
}

.progress-wrap {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.progress-track {
  position: relative;
  height: 10px;
  background: rgba(255, 255, 255, 0.06);
  box-shadow:
    inset 0 1px 2px rgba(3, 6, 24, 0.4),
    inset 0 0 0 1px rgba(255, 255, 255, 0.08);
  border-radius: $radius-full;
  overflow: hidden;
}

.progress-bar {
  height: 100%;
  background: $color-grad;
  background-size: 200% 100%;
  transition: width 0.3s ease;
  animation: shift 4s linear infinite;
  border-radius: $radius-full;
  box-shadow:
    inset 0 1px 0 rgba(255, 255, 255, 0.35),
    0 0 12px rgba(245, 158, 11, 0.4);
}

@keyframes shift {
  from { background-position: 0% 0; }
  to { background-position: 200% 0; }
}

.progress-meta {
  display: flex;
  justify-content: space-between;
  font-size: 0.85rem;
  color: $color-muted;

  .failed {
    color: $color-rose;
  }
}

.timing {
  display: flex;
  justify-content: space-between;
  font-size: 0.8rem;
  color: $color-faint;
}

.logs {
  max-height: 320px;
  overflow-y: auto;
  background: rgba(6, 10, 26, 0.35);
  box-shadow:
    inset 0 2px 6px rgba(3, 6, 24, 0.45),
    inset 0 0 0 1px rgba(255, 255, 255, 0.07),
    inset 0 -12px 24px -16px rgba(255, 255, 255, 0.14);
  border-radius: 14px;
  padding: 12px 14px;
  font-family: ui-monospace, "SFMono-Regular", "Menlo", monospace;
  font-size: 0.78rem;
  display: flex;
  flex-direction: column;
  gap: 4px;

  &::-webkit-scrollbar { width: 8px; }
  &::-webkit-scrollbar-thumb { background: rgba(255, 255, 255, 0.1); border-radius: 4px; }
}

.log-line {
  display: grid;
  grid-template-columns: 80px 70px 1fr;
  gap: 8px;
  align-items: baseline;
  line-height: 1.4;

  .log-time {
    color: $color-faint;
  }

  .log-level {
    text-transform: uppercase;
    font-size: 0.7rem;
    letter-spacing: 0.04em;
    color: $color-muted;
  }

  .log-msg {
    color: $color-ink;
    word-break: break-word;
  }

  &.level-warning .log-level { color: $color-gold; }
  &.level-error {
    .log-level { color: $color-rose; }
    .log-msg { color: #fda4af; }
  }
}

.muted {
  color: $color-muted;
  font-size: 0.85rem;
}

.empty {
  text-align: center;
  padding: 24px 12px;
  color: $color-faint;
}

.btn {
  padding: 10px 20px;
  font-size: 0.9rem;

  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
    animation: none;
    transform: none;
  }
}

.spinner {
  width: 14px;
  height: 14px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: currentColor;
  border-radius: 50%;
  display: inline-block;
  animation: spin 0.8s linear infinite;

  &.small {
    width: 12px;
    height: 12px;
    border-width: 2px;
    margin-right: 6px;
  }
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>