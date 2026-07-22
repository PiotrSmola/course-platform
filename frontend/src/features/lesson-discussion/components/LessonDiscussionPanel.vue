<template>
  <section class="lesson-discussion glass">
    <h2>Pytania i odpowiedzi</h2>
    <p class="subtitle">Zadaj pytanie dotyczące tej lekcji lub odpowiedz innym kursantom.</p>

    <form class="question-form" @submit.prevent="onSubmitQuestion">
      <textarea
        v-model="questionBody"
        rows="3"
        placeholder="Napisz pytanie..."
        :disabled="createQuestion.isPending.value"
      />
      <p v-if="questionErrors.body" class="field-error">{{ questionErrors.body }}</p>
      <button class="btn btn-primary" type="submit" :disabled="createQuestion.isPending.value || !questionMeta.valid">
        Zadaj pytanie
      </button>
    </form>

    <div v-if="discussionQuery.isLoading.value" class="state">Ładowanie dyskusji...</div>
    <div v-else-if="discussionQuery.isError.value" class="state">Nie udało się załadować dyskusji.</div>
    <div v-else-if="!items.length" class="state">Brak pytań — bądź pierwszy.</div>

    <ul v-else class="questions">
      <li v-for="question in items" :key="question.id" class="question">
        <div class="post-header">
          <strong>{{ question.authorName }}</strong>
          <time>{{ formatDate(question.createdAt) }}</time>
        </div>
        <p class="post-body">{{ question.body }}</p>
        <button
          v-if="question.canDelete"
          type="button"
          class="btn-link"
          :disabled="deleteQuestion.isPending.value"
          @click="deleteQuestion.mutate(question.id)"
        >
          Usuń pytanie
        </button>

        <ul class="answers">
          <li v-for="answer in question.answers" :key="answer.id" class="answer">
            <div class="post-header">
              <strong>{{ answer.authorName }}</strong>
              <span v-if="answer.isInstructorAnswer" class="badge">Instruktor</span>
              <time>{{ formatDate(answer.createdAt) }}</time>
            </div>
            <p class="post-body">{{ answer.body }}</p>
            <button
              v-if="answer.canDelete"
              type="button"
              class="btn-link"
              :disabled="deleteAnswer.isPending.value"
              @click="deleteAnswer.mutate({ questionId: question.id, answerId: answer.id })"
            >
              Usuń odpowiedź
            </button>
          </li>
        </ul>

        <form class="answer-form" @submit.prevent="onSubmitAnswer(question.id)">
          <textarea
            :value="answerDrafts[question.id] ?? ''"
            rows="2"
            placeholder="Napisz odpowiedź..."
            @input="onAnswerInput(question.id, ($event.target as HTMLTextAreaElement).value)"
          />
          <button
            class="btn btn-ghost"
            type="submit"
            :disabled="createAnswer.isPending.value || !(answerDrafts[question.id] ?? '').trim()"
          >
            Odpowiedz
          </button>
        </form>
      </li>
    </ul>

    <div v-if="totalPages > 1" class="pagination">
      <button
        type="button"
        class="btn btn-ghost"
        :disabled="page <= 1"
        @click="page -= 1"
      >
        Poprzednia
      </button>
      <span>{{ page }} / {{ totalPages }}</span>
      <button
        type="button"
        class="btn btn-ghost"
        :disabled="page >= totalPages"
        @click="page += 1"
      >
        Następna
      </button>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import {
  useCreateLessonAnswer,
  useCreateLessonQuestion,
  useDeleteLessonAnswer,
  useDeleteLessonQuestion,
  useLessonDiscussion
} from '@/features/lesson-discussion/composables/useLessonDiscussion'
import { discussionBodySchema } from '@/features/lesson-discussion/schemas/discussion.schema'

const props = defineProps<{
  courseId: string
  lessonId: string
}>()

const page = ref(1)
const answerDrafts = reactive<Record<string, string>>({})

watch(
  () => props.lessonId,
  () => {
    page.value = 1
    Object.keys(answerDrafts).forEach((key) => delete answerDrafts[key])
  }
)

const discussionQuery = useLessonDiscussion(
  () => props.courseId,
  () => props.lessonId,
  page
)
const createQuestion = useCreateLessonQuestion(() => props.courseId, () => props.lessonId)
const createAnswer = useCreateLessonAnswer(() => props.courseId, () => props.lessonId)
const deleteQuestion = useDeleteLessonQuestion(() => props.courseId, () => props.lessonId)
const deleteAnswer = useDeleteLessonAnswer(() => props.courseId, () => props.lessonId)

const items = computed(() => discussionQuery.data.value?.items ?? [])
const totalPages = computed(() => discussionQuery.data.value?.totalPages ?? 0)

const {
  handleSubmit: handleQuestionSubmit,
  defineField: defineQuestionField,
  errors: questionErrors,
  meta: questionMeta,
  resetForm: resetQuestionForm
} = useForm({
  validationSchema: toTypedSchema(discussionBodySchema)
})

const [questionBody] = defineQuestionField('body')

const onSubmitQuestion = handleQuestionSubmit(async (values) => {
  await createQuestion.mutateAsync(values.body)
  resetQuestionForm()
})

function onAnswerInput(questionId: string, value: string) {
  answerDrafts[questionId] = value
}

async function onSubmitAnswer(questionId: string) {
  const body = (answerDrafts[questionId] ?? '').trim()
  if (!body) return
  await createAnswer.mutateAsync({ questionId, body })
  answerDrafts[questionId] = ''
}

function formatDate(value: string) {
  return new Date(value).toLocaleString('pl-PL', {
    dateStyle: 'medium',
    timeStyle: 'short'
  })
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.lesson-discussion {
  --lg-r: 28px;
  --lg-blur: 0px;
  padding: 28px;
  display: flex;
  flex-direction: column;
  gap: 20px;

  h2 {
    margin: 0;
    font-size: 1.25rem;
  }
}

.subtitle {
  margin: 0;
  color: $color-muted;
  font-size: 0.95rem;
}

.question-form,
.answer-form {
  display: flex;
  flex-direction: column;
  gap: 10px;

  textarea {
    width: 100%;
    resize: vertical;
    border-radius: 16px;
    border: 1px solid rgba(255, 255, 255, 0.1);
    background: rgba(255, 255, 255, 0.04);
    color: $color-ink;
    padding: 12px 14px;
    font: inherit;
  }
}

.field-error {
  margin: 0;
  color: #f87171;
  font-size: 0.85rem;
}

.state {
  color: $color-muted;
}

.questions,
.answers {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.question {
  padding: 16px 0;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.answers {
  margin-left: 12px;
  padding-left: 12px;
  border-left: 2px solid rgba(255, 255, 255, 0.08);
}

.answer {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.post-header {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;

  time {
    color: $color-muted;
    font-size: 0.82rem;
  }
}

.post-body {
  margin: 0;
  white-space: pre-wrap;
  line-height: 1.5;
}

.badge {
  font-size: 0.75rem;
  font-weight: 700;
  color: $color-gold;
  background: rgba(245, 158, 11, 0.12);
  padding: 4px 8px;
  border-radius: 999px;
}

.btn-link {
  align-self: flex-start;
  background: none;
  border: none;
  color: $color-muted;
  cursor: pointer;
  padding: 0;
  font-size: 0.85rem;

  &:hover {
    color: $color-ink;
  }
}

.pagination {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  color: $color-muted;
}

.btn {
  width: fit-content;
  padding: 12px 18px;
}
</style>
