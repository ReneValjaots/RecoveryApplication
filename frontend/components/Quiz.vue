<template>
  <div class="assessment-form">
    <div>
      <h2 class="title">Injury Assessment Quiz</h2>
      <UButton
        padded
        color="gray"
        variant="link"
        icon="i-heroicons-x-mark-20-solid"
        @click="$emit('close')"
        class="close-button"
      />
    </div>

    <form class="space-y-6">
      <div v-for="(question, index) in selectedQuestions" :key="index" class="form-group">
        <label>{{ question.text }}</label>
        <div class="rating-scale">
          <span class="severity-label">Not severe</span>
          <div class="radio-group">
            <div v-for="rating in 5" :key="rating" class="radio-option">
              <input
                type="radio"
                :value="rating"
                v-model="responses[index]"
                required
              />
              <label>{{ rating }}</label>
            </div>
          </div>
          <span class="severity-label">Really severe</span>
        </div>
      </div>
      <UButton
        :disabled="!isQuizFilled"
        @click="submitQuiz"
        color="emerald"
        class="submit-button"
      >
        Submit
      </UButton>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
const props = defineProps<{ bodyPart: string }>();

const emit = defineEmits(['scoreSubmitted', 'close']);

const bodyParts = [
  { name: 'Leg', additionalQuestions: [{ text: 'Weight-Bearing: Does putting weight on your leg cause discomfort?' }, { text: 'Flexibility: Can you stretch your leg fully without pain?' }] },
  { name: 'Foot', additionalQuestions: [{ text: 'Walking Ability: Can you walk normally without limping?' }, { text: 'Arch Pain: Do you feel any discomfort in the arch of your foot?' }] },
  { name: 'Ankle', additionalQuestions: [{ text: 'Ankle Mobility: Can you rotate your ankle without pain?' }, { text: 'Swelling: Is there noticeable swelling around your ankle?' }] },
  { name: 'Knee', additionalQuestions: [{ text: 'Weight-Bearing: Does putting weight on your leg cause knee pain?' }, { text: 'Flexion: Can you bend your knee fully without discomfort?' }] },
  { name: 'Shoulder', additionalQuestions: [{ text: 'Shoulder Strength: Can you lift your arm above your head without discomfort?' }, { text: 'Mobility: How difficult is it to rotate your arm in a circular motion?' }] },
  { name: 'Elbow', additionalQuestions: [{ text: 'Elbow Strength: Can you lift objects without elbow pain?' }, { text: 'Elbow Extension: Can you fully straighten your arm without discomfort?' }] },
  { name: 'Groin', additionalQuestions: [{ text: 'Groin Pain: Do you feel discomfort when moving your leg outward?' }, { text: 'Stretching: Can you stretch your groin muscles without pain?' }] },
  { name: 'Hip', additionalQuestions: [{ text: 'Hip Flexibility: Can you move your hip in all directions without discomfort?' }, { text: 'Weight-Bearing: Does putting weight on your hip cause pain?' }] },
  { name: 'Back', additionalQuestions: [{ text: 'Posture: Does maintaining an upright posture cause pain in your back?' }, { text: 'Flexion: Can you bend forward or backward without discomfort?' }] },
  { name: 'Neck', additionalQuestions: [{ text: 'Neck Rotation: Can you rotate your neck fully without discomfort?' }, { text: 'Neck Flexion: Can you bend your neck forward and backward without pain?' }] },
  { name: 'Hand', additionalQuestions: [{ text: 'Grip Strength: Can you firmly grip objects without pain?' }, { text: 'Finger Movement: Can you move your fingers freely without discomfort?' }] },
  { name: 'Chest', additionalQuestions: [{ text: 'Chest Expansion: Can you take a deep breath without discomfort?' }, { text: 'Upper Body Movement: Does moving your chest or arms cause pain?' }] },
  { name: 'Spine', additionalQuestions: [{ text: 'Flexibility: Can you twist your spine without discomfort?' }, { text: 'Posture: Does maintaining an upright posture cause spinal pain?' }] },
];

const commonQuestions = [
  { text: 'Pain Level: How intense is the pain in the injured area?' },
  { text: 'Swelling: How much swelling do you observe around the injury?' },
  { text: 'Mobility: How limited is your movement due to the injury?' },
  { text: 'Bruising: How noticeable is the bruising around the injury?' },
];

const selectedBodyPart = ref(bodyParts[0].name);

const selectedQuestions = computed<{ text: string }[]>(() => {
  const bodyPartData = bodyParts.find(part => part.name === props.bodyPart);
  return bodyPartData ? [...commonQuestions, ...bodyPartData.additionalQuestions] : [...commonQuestions];
});

const responses = ref<(number | null)[]>(Array(selectedQuestions.value.length).fill(null));

const isQuizFilled = computed(() => responses.value.every((response: number | null)  => response !== null));

const averageSeverity = computed(() => {
  const validResponses = responses.value.filter((r: number | null) => r !== null);
  if (validResponses.length === 0) return 0;
  return validResponses.reduce((sum: number, val: number | null) => sum + val!, 0) / validResponses.length;
});

const submitQuiz = () => {
  const score = averageSeverity.value;
  const isTooSevere = score > 3;
  emit('scoreSubmitted', { isTooSevere, bodyPart: selectedBodyPart.value, responses: responses.value });
};
</script>

<style scoped>
.assessment-form {
  width: 100%;
  margin: auto;
  padding: 20px;
  border: 1px solid #ccc;
  border-radius: 8px;
  background-color: #f9f9f9;
}

.body-part-selection {
  margin-bottom: 20px;
}

.close-button {
  position: absolute;
  top: 8px;
  right: 8px;
}

.title {
  text-align: center;
  font-size: 1.5rem;
  font-weight: bold;
  margin-bottom: 1rem;
}

.form-group {
  margin-bottom: 20px;
}

label {
  display: inline-block;
  font-weight: bold;
  margin-bottom: 10px;
}

.rating-scale {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: nowrap;
}

.severity-label {
  font-size: 0.875rem;
  color: #6b7280;
}

.radio-group {
  display: flex;
  justify-content: space-between;
  width: 60%;
}

.radio-option {
  text-align: center;
  flex: 1 1 0;
}

input[type="radio"] {
  margin-right: 5px;
}

.submit-button {
  display: block;
  width: 100%;
  padding: 10px 0;
  color: white;
}
</style>
