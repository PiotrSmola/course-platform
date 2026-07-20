import { z } from 'zod'
import { sharedSchemas, sharedMessages } from '@/shared/validation'

export const createReviewSchema = z.object({
  rating: sharedSchemas.rating,
  comment: z
    .string({ required_error: 'Komentarz jest wymagany' })
    .min(1, 'Komentarz jest wymagany')
    .max(2000, sharedMessages.commentMax)
})

export const updateReviewSchema = createReviewSchema

export type CreateReviewFormValues = z.infer<typeof createReviewSchema>
export type UpdateReviewFormValues = z.infer<typeof updateReviewSchema>
