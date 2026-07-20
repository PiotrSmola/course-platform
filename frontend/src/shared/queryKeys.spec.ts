import { describe, expect, it } from 'vitest'
import { queryKeys } from './queryKeys'

describe('queryKeys', () => {
  it('course(id) returns expected tuple', () => {
    const id = 'abc-123'
    expect(queryKeys.course(id)).toEqual(['course', id])
  })
})
