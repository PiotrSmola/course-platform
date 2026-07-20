import { describe, expect, it } from 'vitest'
import { getApiErrorMessage } from './apiError'

describe('getApiErrorMessage', () => {
  it('returns top-level error message', () => {
    const error = {
      response: {
        data: { error: 'Something went wrong' }
      }
    }

    expect(getApiErrorMessage(error)).toBe('Something went wrong')
  })

  it('returns first validation error message', () => {
    const error = {
      response: {
        data: {
          errors: {
            email: ['Email jest wymagany'],
            password: ['Hasło jest wymagane']
          }
        }
      }
    }

    expect(getApiErrorMessage(error)).toBe('Email jest wymagany')
  })

  it('returns undefined when response data is missing', () => {
    expect(getApiErrorMessage({})).toBeUndefined()
    expect(getApiErrorMessage(undefined)).toBeUndefined()
  })
})
