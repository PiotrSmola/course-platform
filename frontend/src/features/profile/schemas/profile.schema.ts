import { z } from 'zod'
import { sharedSchemas } from '@/shared/validation'

export const updateProfileSchema = z.object({
  firstName: sharedSchemas.name('Imię'),
  lastName: sharedSchemas.name('Nazwisko')
})

export type UpdateProfileFormValues = z.infer<typeof updateProfileSchema>
