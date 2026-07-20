import { describe, expect, it } from 'vitest'
import { loginSchema, registerSchema } from './auth.schema'

describe('loginSchema', () => {
  it('rejects empty password', () => {
    const result = loginSchema.safeParse({
      email: 'user@example.com',
      password: ''
    })

    expect(result.success).toBe(false)
    if (!result.success) {
      expect(result.error.issues.some((issue) => issue.path.includes('password'))).toBe(true)
    }
  })
})

describe('registerSchema', () => {
  it('rejects weak password', () => {
    const result = registerSchema.safeParse({
      firstName: 'Jan',
      lastName: 'Kowalski',
      email: 'user@example.com',
      password: 'weak'
    })

    expect(result.success).toBe(false)
    if (!result.success) {
      expect(result.error.issues.some((issue) => issue.path.includes('password'))).toBe(true)
    }
  })
})
