export interface LessonResourceDto {
  id: string
  title: string
  contentType: string
  sizeBytes: number
  order: number
}

export interface PresignLessonResourceUploadResponse {
  resourceId: string
  objectKey: string
  url: string
}

export interface LessonResourceDownloadResponse {
  url: string
}

export interface ConfirmLessonResourceUploadRequest {
  resourceId: string
  objectKey: string
  title: string
  contentType: string
  order?: number
}

export const ALLOWED_LESSON_RESOURCE_CONTENT_TYPES = [
  'application/pdf',
  'application/zip',
  'text/plain',
  'application/json'
] as const

export const MAX_LESSON_RESOURCE_BYTES = 25 * 1024 * 1024

export const LESSON_RESOURCE_ACCEPT =
  '.pdf,.zip,.txt,.json,application/pdf,application/zip,text/plain,application/json'
