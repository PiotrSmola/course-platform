import { z } from 'zod'
import { sharedMessages } from '@/shared/validation'

export const discussionBodySchema = z.object({
  body: z
    .string({ required_error: 'Treść jest wymagana' })
    .min(1, 'Treść jest wymagana')
    .max(2000, sharedMessages.commentMax)
})

export type DiscussionBodyFormValues = z.infer<typeof discussionBodySchema>
