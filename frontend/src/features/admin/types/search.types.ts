export enum ReindexStatus {
  Idle = 0,
  Running = 1,
  Succeeded = 2,
  Failed = 3,
  Cancelled = 4
}

export enum ReindexPhase {
  Counting = 0,
  PreparingIndex = 1,
  Indexing = 2,
  SwitchingAlias = 3,
  CleaningUp = 4,
  Done = 5
}

export enum ReindexLogLevel {
  Info = 0,
  Warning = 1,
  Error = 2
}

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
  [ReindexStatus.Idle]: 'Bezczynny',
  [ReindexStatus.Running]: 'W toku',
  [ReindexStatus.Succeeded]: 'Zakończony',
  [ReindexStatus.Failed]: 'Błąd',
  [ReindexStatus.Cancelled]: 'Anulowany'
}

export const REINDEX_PHASE_LABEL: Record<ReindexPhase, string> = {
  [ReindexPhase.Counting]: 'Zliczanie kursów',
  [ReindexPhase.PreparingIndex]: 'Tworzenie indeksu',
  [ReindexPhase.Indexing]: 'Indeksowanie',
  [ReindexPhase.SwitchingAlias]: 'Przełączanie aliasu',
  [ReindexPhase.CleaningUp]: 'Sprzątanie',
  [ReindexPhase.Done]: 'Zakończono'
}

export const REINDEX_LOG_LEVEL_LABEL: Record<ReindexLogLevel, string> = {
  [ReindexLogLevel.Info]: 'Info',
  [ReindexLogLevel.Warning]: 'Warning',
  [ReindexLogLevel.Error]: 'Error'
}

export const REINDEX_LOG_LEVEL_CLASS: Record<ReindexLogLevel, string> = {
  [ReindexLogLevel.Info]: 'level-info',
  [ReindexLogLevel.Warning]: 'level-warning',
  [ReindexLogLevel.Error]: 'level-error'
}

export function isTerminalStatus(status: ReindexStatus): boolean {
  return status === ReindexStatus.Succeeded
    || status === ReindexStatus.Failed
    || status === ReindexStatus.Cancelled
}