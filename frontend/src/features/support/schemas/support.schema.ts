import { z } from 'zod'

export const supportMessageSchema = z.object({
  name: z.string().trim().min(2, 'Wpisz swoje imię i nazwisko').max(120, 'Maksymalnie 120 znaków'),
  email: z.string().trim().email('Wpisz prawidłowy adres e-mail'),
  subject: z.string().trim().min(3, 'Temat musi mieć co najmniej 3 znaki').max(160, 'Maksymalnie 160 znaków'),
  message: z.string().trim().min(20, 'Wiadomość musi mieć co najmniej 20 znaków').max(4000, 'Maksymalnie 4000 znaków')
})

export type SupportMessageForm = z.infer<typeof supportMessageSchema>
