export interface AuthResponse {
  id: string
  email: string | null
  firstName: string
  lastName: string
  token: string
  refreshToken: string
  roles: string[]
}

export interface RegisterResponse {
  id: string
  email: string | null
  firstName: string
  lastName: string
  message: string
}

export interface CurrentUser {
  id: string
  email: string | null
  firstName: string
  lastName: string
  roles: string[]
}
