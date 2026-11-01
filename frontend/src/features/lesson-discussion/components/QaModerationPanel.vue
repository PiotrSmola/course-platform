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
      <li v-for="question in items" :key="question.id" class="question">
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
            class="btn btn-sm danger"
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
            class="btn btn-primary btn-sm"
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
.qa-moderation {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.filters {
  display: flex;
  flex-wrap: wrap;
  gap: 12px 20px;
  align-items: center;
  padding-bottom: 18px;
  border-bottom: 1px solid $color-hairline;
}

.filter-select {
  min-width: 240px;
  max-width: 100%;
  height: 42px;
  padding: 0 14px;
  border: none;
  border-radius: $radius-md;
  background: rgba(13, 15, 19, 0.45);
  box-shadow: inset 0 0 0 1px $color-hairline;
  color: $color-ink;
  font: inherit;
  font-size: 0.9rem;

  &:focus {
    outline: none;
    box-shadow: inset 0 0 0 1px $color-hairline, 0 0 0 2px rgba($color-accent, 0.6);
  }
}

.checkbox {
  @include mono-label;
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 0.64rem;
  color: $color-muted;
  cursor: pointer;

  input {
    accent-color: $color-accent;
  }
}

.state {
  @include mono-label;
  text-align: center;
  padding: 40px 16px;
  color: $color-faint;
}

.questions {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
}

.question {
  padding: 22px 0;
  display: flex;
  flex-direction: column;
  gap: 12px;
  border-bottom: 1px solid $color-hairline;

  &:last-child {
    border-bottom: none;
  }
}

.question-meta {
  @include mono-label;
  font-size: 0.62rem;
  color: $color-faint;

  .course {
    color: $color-accent;
  }

  .separator {
    margin: 0 8px;
    color: $color-hairline;
  }
}

.post-header {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  align-items: center;

  strong {
    font-weight: 600;
  }

  time {
    font-family: $font-mono;
    font-size: 0.7rem;
    color: $color-faint;
  }
}

.post-body {
  margin: 0;
  max-width: 76ch;
  white-space: pre-wrap;
  line-height: 1.55;
  color: $color-muted;
}

.badge {
  @include tag;
  color: $color-accent;
  box-shadow: inset 0 0 0 1px rgba($color-accent, 0.35);

  &.badge-warning {
    color: $color-coral;
    box-shadow: inset 0 0 0 1px rgba($color-coral, 0.4);
  }

  &.badge-ok {
    color: $color-success;
    box-shadow: inset 0 0 0 1px rgba($color-success, 0.35);
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
  @include mono-label;
  margin: 0;
  font-size: 0.62rem;
  color: $color-faint;
}

.answers {
  list-style: none;
  margin: 0 0 0 4px;
  padding: 0 0 0 16px;
  border-left: 1px solid $color-hairline;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.answer {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.preview-note {
  @include mono-label;
  margin: 0;
  font-size: 0.62rem;
  color: $color-faint;
}

.reply-form {
  display: flex;
  flex-direction: column;
  gap: 10px;

  textarea {
    width: 100%;
    resize: vertical;
    border: none;
    border-radius: $radius-md;
    background: rgba(13, 15, 19, 0.45);
    box-shadow: inset 0 0 0 1px $color-hairline;
    color: $color-ink;
    padding: 12px 14px;
    font: inherit;
    font-size: 0.92rem;

    &::placeholder {
      color: $color-faint;
    }

    &:focus {
      outline: none;
      box-shadow: inset 0 0 0 1px $color-hairline, 0 0 0 2px rgba($color-accent, 0.6);
    }
  }
}

.btn-link {
  @include mono-label;
  align-self: flex-start;
  background: none;
  border: none;
  color: $color-faint;
  cursor: pointer;
  padding: 0;
  font-size: 0.62rem;

  &:hover,
  &.danger:hover {
    color: $color-danger;
  }
}

.btn.danger {
  --lg-tint: #{rgba($color-danger, 0.22)};

  &:hover {
    --lg-tint: #{rgba($color-danger, 0.32)};
  }
}

.pagination {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding-top: 16px;
  border-top: 1px solid $color-hairline;

  span {
    font-family: $font-mono;
    font-size: 0.76rem;
    color: $color-muted;
  }
}

.btn {
  width: fit-content;
  padding: 9px 18px;
  font-size: 0.84rem;
}

.btn-sm {
  padding: 7px 14px;
  font-size: 0.8rem;
}
</style>
