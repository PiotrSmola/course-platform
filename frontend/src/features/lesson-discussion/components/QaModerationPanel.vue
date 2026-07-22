<template>
  <div class="qa-moderation">
    <div class="filters">
      <select v-model="selectedCourseId" class="filter-select">
        <option value="">Wszystkie kursy</option>
        <option v-for="course in courseOptions" :key="course.id" :value="course.id">
          {{ course.title }}
        </option>
      </select>
      <label class="checkbox">
        <input v-model="unansweredOnly" type="checkbox" />
        Tylko bez odpowiedzi instruktora
      </label>
    </div>

    <div v-if="queueQuery.isLoading.value" class="state">Ładowanie kolejki Q&A...</div>
    <div v-else-if="queueQuery.isError.value" class="state">Nie udało się załadować kolejki Q&A.</div>
    <div v-else-if="!items.length" class="state">Brak pytań do moderacji.</div>

    <ul v-else class="questions">
      <li v-for="question in items" :key="question.id" class="question glass-card">
        <div class="question-meta">
          <span class="course">{{ question.courseTitle }}</span>
          <span class="separator">·</span>
          <span class="lesson">{{ question.lessonTitle }}</span>
        </div>

        <div class="post-header">
          <strong>{{ question.authorName }}</strong>
          <time>{{ formatDate(question.createdAt) }}</time>
          <span v-if="!question.hasInstructorAnswer" class="badge badge-warning">Bez odpowiedzi</span>
          <span v-else class="badge badge-ok">Odpowiedziano</span>
        </div>

        <p class="post-body">{{ question.body }}</p>

        <div class="question-actions">
          <button
            type="button"
            class="btn btn-ghost danger"
            :disabled="removeQuestion.isPending.value"
            @click="onDeleteQuestion(question)"
          >
            Usuń pytanie
          </button>
        </div>

        <div v-if="question.answersPreview.length" class="answers-section">
          <p class="answers-label">
            Odpowiedzi ({{ question.answersCount }})
          </p>
          <ul class="answers">
            <li v-for="answer in question.answersPreview" :key="answer.id" class="answer">
              <div class="post-header">
                <strong>{{ answer.authorName }}</strong>
                <span v-if="answer.isInstructorAnswer" class="badge">Instruktor</span>
                <time>{{ formatDate(answer.createdAt) }}</time>
              </div>
              <p class="post-body">{{ answer.body }}</p>
              <button
                type="button"
                class="btn-link danger"
                :disabled="removeAnswer.isPending.value"
                @click="onDeleteAnswer(question, answer.id)"
              >
                Usuń odpowiedź
              </button>
            </li>
          </ul>
          <p v-if="question.answersCount > question.answersPreview.length" class="preview-note">
            + {{ question.answersCount - question.answersPreview.length }} więcej odpowiedzi
          </p>
        </div>

        <form class="reply-form" @submit.prevent="onSubmitReply(question)">
          <textarea
            :value="replyDrafts[question.id] ?? ''"
            rows="2"
            placeholder="Napisz odpowiedź jako instruktor..."
            @input="onReplyInput(question.id, ($event.target as HTMLTextAreaElement).value)"
          />
          <button
            class="btn btn-primary"
            type="submit"
            :disabled="replyToQuestion.isPending.value || !(replyDrafts[question.id] ?? '').trim()"
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
        :disabled="pageNumber <= 1"
        @click="pageNumber--"
      >
        Poprzednia
      </button>
      <span>{{ pageNumber }} / {{ totalPages }}</span>
      <button
        type="button"
        class="btn btn-ghost"
        :disabled="pageNumber >= totalPages"
        @click="pageNumber++"
      >
        Następna
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useModerationQueue, useModerationMutations } from '@/features/lesson-discussion/composables/useQaModeration'
import type { ModerationQuestionDto } from '@/features/lesson-discussion/types/moderation.types'

const props = defineProps<{
  courseOptions?: { id: string; title: string }[]
}>()

const selectedCourseId = ref('')
const unansweredOnly = ref(false)
const pageNumber = ref(1)
const pageSize = 20
const replyDrafts = reactive<Record<string, string>>({})

const filters = computed(() => ({
  courseId: selectedCourseId.value || undefined,
  unansweredByInstructorOnly: unansweredOnly.value,
  pageNumber: pageNumber.value,
  pageSize
}))

watch([selectedCourseId, unansweredOnly], () => {
  pageNumber.value = 1
})

const queueQuery = useModerationQueue(filters)
const { replyToQuestion, removeQuestion, removeAnswer } = useModerationMutations()

const courseOptions = computed(() => props.courseOptions ?? [])
const items = computed(() => queueQuery.data.value?.items ?? [])
const totalPages = computed(() => queueQuery.data.value?.totalPages ?? 0)

function formatDate(value: string) {
  return new Date(value).toLocaleString('pl-PL', {
    dateStyle: 'medium',
    timeStyle: 'short'
  })
}

function onReplyInput(questionId: string, value: string) {
  replyDrafts[questionId] = value
}

async function onSubmitReply(question: ModerationQuestionDto) {
  const body = (replyDrafts[question.id] ?? '').trim()
  if (!body) return

  await replyToQuestion.mutateAsync({
    courseId: question.courseId,
    lessonId: question.lessonId,
    questionId: question.id,
    body
  })
  replyDrafts[question.id] = ''
}

function onDeleteQuestion(question: ModerationQuestionDto) {
  if (!window.confirm('Czy na pewno chcesz usunąć to pytanie?')) return

  removeQuestion.mutate({
    courseId: question.courseId,
    lessonId: question.lessonId,
    questionId: question.id
  })
}

function onDeleteAnswer(question: ModerationQuestionDto, answerId: string) {
  if (!window.confirm('Czy na pewno chcesz usunąć tę odpowiedź?')) return

  removeAnswer.mutate({
    courseId: question.courseId,
    lessonId: question.lessonId,
    questionId: question.id,
    answerId
  })
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.qa-moderation {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.filters {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  align-items: center;
}

.filter-select {
  min-width: 220px;
  padding: 10px 14px;
  border-radius: 14px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  background: rgba(255, 255, 255, 0.04);
  color: $color-ink;
  font: inherit;
}

.checkbox {
  display: flex;
  align-items: center;
  gap: 8px;
  color: $color-muted;
  font-size: 0.9rem;
  cursor: pointer;

  input {
    accent-color: $color-gold;
  }
}

.state {
  text-align: center;
  padding: 32px;
  color: $color-muted;
}

.questions {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.question {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.question-meta {
  font-size: 0.85rem;
  color: $color-muted;

  .course {
    font-weight: 600;
    color: $color-ink;
  }

  .separator {
    margin: 0 6px;
  }
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

  &.badge-warning {
    color: #fbbf24;
    background: rgba(251, 191, 36, 0.12);
  }

  &.badge-ok {
    color: #4ade80;
    background: rgba(74, 222, 128, 0.12);
  }
}

.question-actions {
  display: flex;
  gap: 8px;
}

.answers-section {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.answers-label {
  margin: 0;
  font-size: 0.85rem;
  font-weight: 600;
  color: $color-muted;
}

.answers {
  list-style: none;
  margin: 0;
  padding: 0 0 0 12px;
  border-left: 2px solid rgba(255, 255, 255, 0.08);
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.answer {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.preview-note {
  margin: 0;
  font-size: 0.82rem;
  color: $color-muted;
}

.reply-form {
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

  &.danger {
    color: #f87171;

    &:hover {
      color: #fca5a5;
    }
  }
}

.danger {
  color: #f87171;
}

.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;
  color: $color-muted;
}

.btn {
  width: fit-content;
  padding: 10px 18px;
}
</style>
