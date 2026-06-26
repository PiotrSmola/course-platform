import { z } from 'zod'
import { sharedSchemas } from '@/shared/validation'

export const registerSchema = z.object({
  firstName: sharedSchemas.name('Imię'),
  lastName: sharedSchemas.name('Nazwisko'),
  email: sharedSchemas.email,
  password: sharedSchemas.password
})

export const loginSchema = z.object({
  email: sharedSchemas.email,
  password: z
    .string({ required_error: 'Hasło jest wymagane' })
    .min(1, 'Hasło jest wymagane')
})

export type RegisterFormValues = z.infer<typeof registerSchema>
export type LoginFormValues = z.infer<typeof loginSchema>
