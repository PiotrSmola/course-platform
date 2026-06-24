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
