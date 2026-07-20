import client from '@/shared/api/client'
import type { AuthResponse, CurrentUser } from '@/features/auth/types/auth.types'

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
  firstName: string
  lastName: string
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export async function refreshToken(data: RefreshTokenRequest): Promise<AuthResponse> {
  const response = await client.post('/auth/refresh', data)
  return response.data
}

export async function logout(refreshToken: string): Promise<void> {
  await client.post('/auth/logout', { refreshToken })
}

export async function login(data: LoginRequest): Promise<AuthResponse> {
  const response = await client.post('/auth/login', data)
  return response.data
}

export async function register(data: RegisterRequest): Promise<AuthResponse> {
  const response = await client.post('/auth/register', data)
  return response.data
}

export async function getCurrentUser(): Promise<CurrentUser> {
  const response = await client.get('/auth/me')
  return response.data
}

export interface ForgotPasswordRequest {
  email: string
}

export interface ResetPasswordRequest {
  email: string
  token: string
  newPassword: string
}

export interface ConfirmEmailRequest {
  email: string
  token: string
}

export async function forgotPassword(data: ForgotPasswordRequest): Promise<void> {
  await client.post('/auth/forgot-password', data)
}

export async function resetPassword(data: ResetPasswordRequest): Promise<void> {
  await client.post('/auth/reset-password', data)
}

export async function confirmEmail(data: ConfirmEmailRequest): Promise<void> {
  await client.post('/auth/confirm-email', data)
}
