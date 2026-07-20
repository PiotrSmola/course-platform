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

export const forgotPasswordSchema = z.object({
  email: sharedSchemas.email
})

export const resetPasswordSchema = z
  .object({
    newPassword: sharedSchemas.password,
    confirmPassword: z
      .string({ required_error: 'Potwierdzenie hasła jest wymagane' })
      .min(1, 'Potwierdzenie hasła jest wymagane')
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: 'Hasła muszą być identyczne',
    path: ['confirmPassword']
  })

export type RegisterFormValues = z.infer<typeof registerSchema>
export type LoginFormValues = z.infer<typeof loginSchema>
export type ForgotPasswordFormValues = z.infer<typeof forgotPasswordSchema>
export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>
