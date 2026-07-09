export type ReindexStatus = 'Idle' | 'Running' | 'Succeeded' | 'Failed' | 'Cancelled'

export type ReindexPhase =
  | 'Counting'
  | 'PreparingIndex'
  | 'Indexing'
  | 'SwitchingAlias'
  | 'CleaningUp'
  | 'Done'

export type ReindexLogLevel = 'Info' | 'Warning' | 'Error'

export interface ReindexLogEntry {
  timestamp: string
  level: ReindexLogLevel
  message: string
}

export interface ReindexProgress {
  total: number
  processed: number
  failed: number
  batchSize: number
  batchesCompleted: number
  batchesTotal: number
  percent: number
}

export interface ReindexJobState {
  jobId: string
  status: ReindexStatus
  phase: ReindexPhase
  progress: ReindexProgress
  logs: ReindexLogEntry[]
  startedAt: string | null
  finishedAt: string | null
  errorMessage: string | null
}

export interface CourseIndexStats {
  exists: boolean
  enabled: boolean
  indexName: string
  aliasName: string
  concreteIndexName: string | null
  documentCount: number
  sizeBytes: number | null
  health: string
}

export interface StartReindexResult {
  state: ReindexJobState
  alreadyRunning: boolean
}

export const REINDEX_STATUS_LABEL: Record<ReindexStatus, string> = {
  Idle: 'Bezczynny',
  Running: 'W toku',
  Succeeded: 'Zakończony',
  Failed: 'Błąd',
  Cancelled: 'Anulowany'
}

export const REINDEX_PHASE_LABEL: Record<ReindexPhase, string> = {
  Counting: 'Zliczanie kursów',
  PreparingIndex: 'Tworzenie indeksu',
  Indexing: 'Indeksowanie',
  SwitchingAlias: 'Przełączanie aliasu',
  CleaningUp: 'Sprzątanie',
  Done: 'Zakończono'
}

export function isTerminalStatus(status: ReindexStatus): boolean {
  return status === 'Succeeded' || status === 'Failed' || status === 'Cancelled'
}