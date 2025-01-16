<template>
    <div>
      <div class="header-container">
        <h1 class="main-title">Create Recovery Plan</h1>
        <UButton 
          @click="createRecoveryPlan" 
          :disabled="!isPlanReady" 
          color="emerald" 
          size="lg" 
          class="mr-2 mt-2"
        >
          Create Plan
        </UButton>
      </div>

        <div v-if="patients.length === 0" class="no-patients">No patients linked to you.</div>
        <div v-else>
          <div class="patient-plan-container">
            <div class="patient-selection">
              <label class="label">Select Patient</label>
              <UInputMenu 
                v-model="selectedPatientUsername" 
                :options="patientUsernames"
                placeholder="Select a patient"
              />
            </div>

            <div class="plan-name">
              <label class="label">Plan Name</label>
              <UInput
                v-model="newPlanName"
                placeholder="Enter plan name"
                :disabled="!selectedPatientUsername"
              />
              <p v-if="!isNameValid && newPlanName.length != 0" class="text-red-500 text-sm mt-1 ml-1">{{ nameErrorMessage }}</p>
            </div>
          </div>

        <div v-if="selectedPatientUsername" class="exercise-container">
          <div class="exercise-list">
            <div>
              <h2 class="section-title">Recovery Exercises:</h2>
              <UTextarea 
                v-model="searchQuery" 
                color="emerald" 
                size="xs" 
                variant="outline" 
                placeholder="Search..." 
                :rows="1"
                class="search-bar"
              />
            </div>
            
            <ul class="exercise-list-ul">
              <li 
                v-for="exercise in filteredExercises" 
                :key="exercise.id" 
                class="exercise-item"
              >
                <span>{{ exercise.name }}</span>   
                <UButton 
                  :disabled="isExerciseAdded(exercise)" 
                  @click="addExercise(exercise)" 
                  color="emerald"
                >
                Add to plan
                </UButton>
              </li>
            </ul>
          </div>

          <div class="current-exercises">
            <h2 class="section-title">Current Recovery Exercises</h2>
            <UTable v-if="workoutDays.length" :columns="columns" :rows="flattenedExercises">
              <template #actions-data="{ row }">
                <UButton @click="removeExercise(row.dayNumber, row.id)" color="red">Remove</UButton>
              </template>
            </UTable>
            <p v-else>No recovery exercises added yet. </br> 
              Must add an exercise to create a plan!
          </p>
          </div>
        </div>
        </div>

        <UModal v-model="isAddModalOpen">
            <div class="bg-white rounded-lg shadow-md p-6">
                <h3 class="text-lg font-semibold text-center">Add Exercise to Plan</h3>
                
                <div class="mt-4 space-y-4">
                    <div>
                        <label for="dayNumber" class="block text-sm font-medium">Day Number <span class="text-red-500">*</span></label>
                        <UInput 
                            id="dayNumber" 
                            v-model="modalData.dayNumber" 
                            type="number" 
                            placeholder="Enter day number" 
                            :min="1"
                            required 
                            class="w-full" />
                    </div>
                    <div>
                        <label for="sets" class="block text-sm font-medium">Sets</label>
                        <UInput 
                            id="sets" 
                            v-model="modalData.sets" 
                            type="number" 
                            placeholder="Enter sets" 
                            :min="0"
                            class="w-full" />
                    </div>
                    <div>
                        <label for="reps" class="block text-sm font-medium">Reps</label>
                        <UInput 
                            id="reps" 
                            v-model="modalData.reps" 
                            type="number" 
                            placeholder="Enter reps" 
                            :min="0"
                            class="w-full" />
                    </div>
                    <div>
                        <label for="duration" class="block text-sm font-medium">Duration</label>
                        <UInput 
                            id="duration" 
                            v-model="modalData.duration" 
                            type="text" 
                            placeholder="Enter duration (e.g., 00:30 or 01:30:00)" 
                            class="w-full" 
                            />
                    </div>
                </div>

                <div class="mt-6 flex justify-end space-x-4">
                <UButton color="gray" variant="ghost" @click="closeAddModal">Cancel</UButton>
                <UButton color="emerald" @click="confirmAddExercise">Add to Plan</UButton>
                </div>
            </div>
        </UModal>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useApi } from '~/composables/useApi';
import { useCookie } from '#app';
import type { PatientInjuryAssignment } from '~/types/patientInjuryAssignment';
import type { RecoveryExercise } from '~/types/recoveryExercise';
import type { RecoveryPlanWithAppUserInfo } from '~/types/recoveryPlan';

const { customFetch } = useApi();

const patients = ref<PatientInjuryAssignment[]>([]);
const selectedPatientUsername  = ref('');
const recoveryExercises = ref<RecoveryExercise[]>([]);
const selectedExercise = ref<RecoveryExercise | null>(null);
const newPlanName = ref('');
const searchQuery = ref('');
const toast = useToast();
const plan = ref<RecoveryPlanWithAppUserInfo | null>(null);
const router = useRouter();

const isAddModalOpen = ref(false);
const workoutDays = ref<
  { dayNumber: number; exercises: { id: number; sets: number; reps: number; duration: string }[] }[]
>([]);
const modalData = ref({ dayNumber: 1, sets: 0, reps: 0, duration: "" });

const flattenedExercises = computed(() => {
  return workoutDays.value.flatMap((day) =>
    day.exercises.map((exercise) => {
      const exerciseDetails = recoveryExercises.value.find((ex) => ex.id === exercise.id);
      return {
        dayNumber: day.dayNumber,
        id: exercise.id,
        name: exerciseDetails?.name,
        sets: exercise.sets,
        reps: exercise.reps,
        duration: exercise.duration,
      };
    })
  );
});

const openAddModal = (exercise: RecoveryExercise) => {
  selectedExercise.value = exercise;
  modalData.value = { dayNumber: 1, sets: 0, reps: 0, duration: "" };
  isAddModalOpen.value = true;
};

const closeAddModal = () => {
  isAddModalOpen.value = false;
  selectedExercise.value = null;
};

const columns = [
  { key: "dayNumber", label: "Day", sortable: true },
  { key: "name", label: "Exercise Name", sortable: true },
  { key: "sets", label: "Sets", sortable: true },
  { key: "reps", label: "Reps", sortable: true },
  { key: "duration", label: "Duration", sortable: true },
  { key: "actions", label: "Actions" },
];

const patientUsernames = computed(() =>
  Array.from(new Set(patients.value.map((p) => p.username)))
);

const selectedPatientId = computed(() =>
  patients.value.find((p) => p.username === selectedPatientUsername.value)?.appUserId || ''
);

const isNameValid = computed(() => {
  const trimmedName = newPlanName.value.trim();
  return trimmedName.length > 0 && trimmedName.length <= 40;
});

const nameErrorMessage = computed(() => {
  if (newPlanName.value.trim().length === 0) {
    return 'Must enter a name';
  } else if (newPlanName.value.length > 40) {
    return 'Plan name cannot exceed 40 characters';
  }
  return '';
});

const filteredExercises = computed(() => {
  const filtered = recoveryExercises.value.filter((exercise) =>
      exercise.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    );

    // Sort exercises alphabetically
    return filtered.sort((a, b) => a.name.localeCompare(b.name));
});

const isPlanReady = computed(() =>
  selectedPatientId.value && isNameValid.value && workoutDays.value.length > 0
);

const fetchPatients = async () => {
  const token = useCookie('token').value || null;
  try {
      const response = await customFetch<PatientInjuryAssignment[]>('/doctor/patients', {
      method: 'GET',
      headers: { Authorization: `Bearer ${token}` },
      });
      patients.value = response;
  } catch (error) {
      console.error("Failed to fetch patients:", error);
  }
};

const fetchRecoveryExercises = async () => {
  const token = useCookie('token').value || null;
  try {
    const response = await customFetch<RecoveryExercise[]>('recoveryexercise', {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    recoveryExercises.value = response;
  } catch (err) {
    console.error('Failed to fetch recovery exercises: ' + (err as Error).message);
  }
};

const addExercise = async (exercise: RecoveryExercise) => {
  openAddModal(exercise);
};

const removeExercise = (dayNumber: number, exerciseId: number) => {
  const day = workoutDays.value.find((d) => d.dayNumber === dayNumber);
  if (day) {
    day.exercises = day.exercises.filter((ex) => ex.id !== exerciseId);
    if (day.exercises.length === 0) {
      workoutDays.value = workoutDays.value.filter((d) => d.dayNumber !== dayNumber);
    }
  }
};

const createRecoveryPlan = async () => {
  if (!isPlanReady.value) return;
  const token = useCookie('token').value || null;
  const payload = {
    name: newPlanName.value.trim(),
    appUserId: selectedPatientId.value,
    workoutDays: workoutDays.value,
  };

  try {
    await customFetch('doctor/create-plan', {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });

    toast.add({ title: 'Error', description: `Recovery plan "${newPlanName.value}" created successfully.`, color: 'green', timeout: 3000 });

    router.push('/doctor-plan-landing');
  } catch (error) {
    console.error('Failed to create recovery plan:', error);
    toast.add({ title: 'Error', description: 'Failed to create recovery plan. Please try again.', color: 'red', timeout: 3000 });
  }
};

const confirmAddExercise = () => {
  const exercise = selectedExercise.value;
  if (!exercise) return; 

  const { dayNumber, sets, reps, duration } = modalData.value;
  const exerciseId = exercise.id;

  let day = workoutDays.value.find((d) => d.dayNumber === dayNumber);
  if (!day) {
    day = { dayNumber, exercises: [] };
    workoutDays.value.push(day);
  }

  const existingExerciseIndex = day.exercises.findIndex((ex) => ex.id === exerciseId);

  if (existingExerciseIndex !== -1) {
    day.exercises[existingExerciseIndex] = { id: exerciseId, sets, reps, duration };
  } else {
    day.exercises.push({ id: exerciseId, sets, reps, duration });
  }

  closeAddModal();
};


const isExerciseAdded = (exercise: RecoveryExercise) => {
  return plan.value?.workoutDays?.some(day => day.exercises.some(ex => ex.id === exercise.id)) || false;
};

onMounted(() => {
  fetchPatients();
  fetchRecoveryExercises();
});
</script>
  
<style scoped>
.exercise-container {
  display: flex;
  gap: 1.5rem;
  width: 100%;
  height: calc(100vh - 25vh);
}

.exercise-list-ul {
  max-height: 100%; 
  overflow-y: auto; 
  flex: 1; 
  padding: 0rem;
  margin: 0;
}

.exercise-list {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.patient-plan-container {
  display: flex;
  gap: 2rem;
  margin-bottom: 0.5rem;
}

.patient-selection,
.plan-name {
  flex: 1;
  margin: 0.25%;
}

.scrollable {
  max-height: 90%;
  overflow-y: auto;
  flex: 1;
}

.current-exercises {
  flex: 3; 
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.header-container {
  display: flex;
  align-items: center;
  margin-bottom: 0.5rem;
  flex-shrink: 0;
}

.main-title {
  font-size: 2em;
  text-align: center;
  flex-grow: 1;
  font-weight: normal;
}

.no-patients {
  text-align: center;
  font-size: 1.2em;
  color: #888;
}

.label {
  display: block;
  font-weight: bold;
  margin-bottom: 8px;
  margin-left: 4px;
}

.exercise-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 3px 10px;
}

.section-title {
  text-align: center;
  font-weight: bold;
  font-size: 1em;
  margin: 10px 0;
}

.search-bar {
  width: 100%;
  margin-bottom: 5px;
  margin-top: 5px;
  margin-left: 5px;
}

.button-container {
  display: flex;
  justify-content: center;
}
</style>