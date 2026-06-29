import type { AxiosError } from 'axios'

export type ApiErrorResponse = { error?: string }

export function getApiErrorMessage(error: unknown): string | undefined {
  const axiosError = error as AxiosError<ApiErrorResponse> | undefined
  return axiosError?.response?.data?.error
}

