<template>
  <div class="quiz-editor">
    <button type="button" class="btn btn-ghost" @click="open = !open">
      {{ open ? 'Ukryj quiz' : 'Edytuj quiz' }}
    </button>

    <div v-if="open" class="editor-panel">
      <div v-if="quizQuery.isLoading.value" class="state">Ładowanie quizu...</div>
      <template v-else>
        <div class="form-group">
          <label>Tytuł quizu</label>
          <input v-model="draft.title" type="text" />
        </div>
        <div class="form-group form-group--narrow">
          <label>Próg (%)</label>
          <input v-model.number="draft.passThresholdPercent" type="number" min="1" max="100" />
        </div>

        <div v-for="(question, qIndex) in draft.questions" :key="qIndex" class="question-block">
          <div class="form-group">
            <label>Pytanie {{ qIndex + 1 }}</label>
            <input v-model="question.prompt" type="text" />
          </div>
          <div v-for="(option, oIndex) in question.options" :key="oIndex" class="option-row">
            <input v-model="option.text" type="text" placeholder="Opcja" />
            <label class="correct">
              <input v-model="option.isCorrect" type="checkbox" />
              Poprawna
            </label>
            <button type="button" class="btn btn-ghost danger" @click="removeOption(qIndex, oIndex)">×</button>
          </div>
          <div class="row-actions">
            <button type="button" class="btn btn-ghost" @click="addOption(qIndex)">+ Opcja</button>
            <button type="button" class="btn btn-ghost danger" @click="removeQuestion(qIndex)">Usuń pytanie</button>
          </div>
        </div>

        <div class="row-actions">
          <button type="button" class="btn btn-ghost" @click="addQuestion">+ Pytanie</button>
          <button
            type="button"
            class="btn btn-primary"
            :disabled="upsert.isPending.value"
            @click="save"
          >
            Zapisz quiz
          </button>
          <button
            v-if="quizQuery.data.value"
            type="button"
            class="btn btn-ghost danger"
            :disabled="remove.isPending.value"
            @click="remove.mutate()"
          >
            Usuń quiz
          </button>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import {
  useDeleteQuiz,
  useLessonQuizForInstructor,
  useUpsertQuiz
} from '@/features/quizzes/composables/useQuizzes'
import type { QuizQuestionInput } from '@/features/quizzes/types/quiz.types'

const props = defineProps<{
  courseId: string
  lessonId: string
}>()

const open = ref(false)
const quizQuery = useLessonQuizForInstructor(
  () => props.courseId,
  () => props.lessonId,
  open
)
const upsert = useUpsertQuiz(() => props.courseId, () => props.lessonId)
const remove = useDeleteQuiz(() => props.courseId, () => props.lessonId)

const draft = reactive<{
  title: string
  passThresholdPercent: number
  questions: QuizQuestionInput[]
}>({
  title: 'Quiz lekcji',
  passThresholdPercent: 70,
  questions: [emptyQuestion(0)]
})

watch(
  () => quizQuery.data.value,
  (quiz) => {
    if (!quiz) {
      draft.title = 'Quiz lekcji'
      draft.passThresholdPercent = 70
      draft.questions = [emptyQuestion(0)]
      return
    }
    draft.title = quiz.title
    draft.passThresholdPercent = quiz.passThresholdPercent
    draft.questions = quiz.questions.map((q, index) => ({
      prompt: q.prompt,
      order: index,
      options: q.options.map((o, oIndex) => ({
        text: o.text,
        isCorrect: o.isCorrect,
        order: oIndex
      }))
    }))
  },
  { immediate: true }
)

function emptyQuestion(order: number): QuizQuestionInput {
  return {
    prompt: '',
    order,
    options: [
      { text: '', isCorrect: true, order: 0 },
      { text: '', isCorrect: false, order: 1 }
    ]
  }
}

function addQuestion() {
  draft.questions.push(emptyQuestion(draft.questions.length))
}

function removeQuestion(index: number) {
  if (draft.questions.length <= 1) return
  draft.questions.splice(index, 1)
}

function addOption(questionIndex: number) {
  const question = draft.questions[questionIndex]
  question.options.push({
    text: '',
    isCorrect: false,
    order: question.options.length
  })
}

function removeOption(questionIndex: number, optionIndex: number) {
  const question = draft.questions[questionIndex]
  if (question.options.length <= 2) return
  question.options.splice(optionIndex, 1)
}

async function save() {
  await upsert.mutateAsync({
    title: draft.title,
    passThresholdPercent: draft.passThresholdPercent,
    questions: draft.questions.map((q, index) => ({
      prompt: q.prompt,
      order: index,
      options: q.options.map((o, oIndex) => ({
        text: o.text,
        isCorrect: o.isCorrect,
        order: oIndex
      }))
    }))
  })
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.quiz-editor {
  margin-top: 8px;
  grid-column: 1 / -1;
}

.editor-panel {
  margin-top: 12px;
  padding: 16px;
  border-radius: 16px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;

  input {
    border-radius: 12px;
    border: 1px solid rgba(255, 255, 255, 0.1);
    background: rgba(255, 255, 255, 0.04);
    color: $color-ink;
    padding: 10px 12px;
  }
}

.form-group--narrow {
  max-width: 140px;
}

.question-block {
  padding-top: 8px;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.option-row {
  display: flex;
  gap: 8px;
  align-items: center;

  input[type='text'] {
    flex: 1;
    border-radius: 12px;
    border: 1px solid rgba(255, 255, 255, 0.1);
    background: rgba(255, 255, 255, 0.04);
    color: $color-ink;
    padding: 10px 12px;
  }
}

.correct {
  display: flex;
  gap: 6px;
  align-items: center;
  white-space: nowrap;
  font-size: 0.85rem;
  color: $color-muted;
}

.row-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.danger {
  color: #f87171;
}

.state {
  color: $color-muted;
}
</style>
