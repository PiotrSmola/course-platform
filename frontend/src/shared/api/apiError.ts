import type { AxiosError } from 'axios'

export type ApiErrorResponse = {
  error?: string
  errors?: Record<string, string[]>
}

export function getApiErrorMessage(error: unknown): string | undefined {
  const axiosError = error as AxiosError<ApiErrorResponse> | undefined
  const data = axiosError?.response?.data
  if (!data) return undefined

  if (data.error) return data.error

  if (data.errors) {
    const firstWithMessage = Object.values(data.errors).find((messages) => messages?.length)
    if (firstWithMessage && firstWithMessage.length > 0) {
      return firstWithMessage[0]
    }
  }

  return undefined
}

