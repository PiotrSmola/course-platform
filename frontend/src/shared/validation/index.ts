import { z } from 'zod'

const nameRegex = new RegExp("^[\\p{L}\\s'-]+$", "u")
const titleRegex = new RegExp("^[\\p{L}\\p{N}\\s\\-_.,!?()]+$", "u")
const languageRegex = new RegExp("^[\\p{L}\\s]+$", "u")

export const sharedMessages = {
  required: (field: string) => `${field} jest wymagane`,
  requiredM: (field: string) => `${field} jest wymagany`,
  maxLength: (field: string, max: number) => `${field} może mieć maksymalnie ${max} znaków`,
  emailInvalid: 'Podaj poprawny adres email',
  passwordMinLength: 'Hasło musi mieć minimum 6 znaków',
  passwordUppercase: 'Hasło musi zawierać co najmniej jedną wielką literę',
  passwordLowercase: 'Hasło musi zawierać co najmniej jedną małą literę',
  passwordDigit: 'Hasło musi zawierać co najmniej jedną cyfrę',
  passwordSpecial: 'Hasło musi zawierać co najmniej jeden znak specjalny',
  nameInvalid: (field: string) => `${field} może zawierać tylko litery, spacje, dywizy i apostrofy`,
  titleInvalid: 'Tytuł zawiera niedozwolone znaki',
  languageInvalid: 'Język może zawierać tylko litery i spacje',
  urlInvalid: 'Podaj poprawny adres URL',
  priceNegative: 'Cena nie może być ujemna',
  priceMax: 'Cena przekracza maksymalną dopuszczalną wartość',
  descriptionMax: 'Opis może mieć maksymalnie 5000 znaków',
  ratingRange: 'Ocena musi być w zakresie 1-5',
  commentMax: 'Komentarz może mieć maksymalnie 2000 znaków'
}

export const sharedSchemas = {
  name: (field: string) => z
    .string({ required_error: sharedMessages.required(field) })
    .min(1, sharedMessages.required(field))
    .max(50, sharedMessages.maxLength(field, 50))
    .regex(nameRegex, sharedMessages.nameInvalid(field)),

  email: z
    .string({ required_error: 'Email jest wymagany' })
    .min(1, 'Email jest wymagany')
    .email(sharedMessages.emailInvalid),

  password: z
    .string({ required_error: 'Hasło jest wymagane' })
    .min(1, 'Hasło jest wymagane')
    .min(6, sharedMessages.passwordMinLength)
    .regex(/[A-Z]/, sharedMessages.passwordUppercase)
    .regex(/[a-z]/, sharedMessages.passwordLowercase)
    .regex(/[0-9]/, sharedMessages.passwordDigit)
    .regex(/[^A-Za-z0-9]/, sharedMessages.passwordSpecial),

  courseTitle: z
    .string({ required_error: 'Tytuł jest wymagany' })
    .min(1, 'Tytuł jest wymagany')
    .max(200, sharedMessages.maxLength('Tytuł', 200))
    .regex(titleRegex, sharedMessages.titleInvalid),

  courseDescription: z
    .string({ required_error: 'Opis jest wymagany' })
    .min(1, 'Opis jest wymagany')
    .max(5000, sharedMessages.descriptionMax),

  shortDescription: z
    .string()
    .max(500, sharedMessages.maxLength('Krótki opis', 500))
    .optional(),

  price: z
    .number({ required_error: 'Cena jest wymagana', invalid_type_error: 'Podaj poprawną cenę' })
    .min(0, sharedMessages.priceNegative)
    .max(100000, sharedMessages.priceMax),

  thumbnailObjectKey: z
    .string()
    .max(1024, sharedMessages.maxLength('Thumbnail key', 1024))
    .optional(),

  language: z
    .string({ required_error: 'Język jest wymagany' })
    .min(1, 'Język jest wymagany')
    .max(50, sharedMessages.maxLength('Język', 50))
    .regex(languageRegex, sharedMessages.languageInvalid),

  rating: z
    .number({ required_error: 'Ocena jest wymagana', invalid_type_error: 'Podaj poprawną ocenę' })
    .min(1, sharedMessages.ratingRange)
    .max(5, sharedMessages.ratingRange),

  comment: z
    .string()
    .max(2000, sharedMessages.commentMax)
    .optional(),

  id: z
    .string({ required_error: 'Identyfikator jest wymagany' })
    .min(1, 'Identyfikator jest wymagany')
    .uuid('Podaj poprawny identyfikator UUID')
}

export type NameSchema = ReturnType<typeof sharedSchemas.name>
