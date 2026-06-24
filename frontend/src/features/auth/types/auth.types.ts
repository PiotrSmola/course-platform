export interface AuthResponse {
  id: string
  email: string | null
  firstName: string
  lastName: string
  token: string
  roles: string[]
}

export interface CurrentUser {
  id: string
  email: string | null
  firstName: string
  lastName: string
  roles: string[]
}
