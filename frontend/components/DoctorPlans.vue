<template>
  <div class="recovery-plans-container">
    <div>
        <!-- Modal Header -->
        <div class="modal-header flex items-center justify-between border-b pb-4">
          <h2 class="text-2xl font-semibold text-center w-full mt-2">Update Recovery Plan</h2>
          <UButton
            icon="i-heroicons-x-mark-20-solid"
            color="gray"
            variant="ghost"
            @click="pushLanding"
            class="close-button"
          /> 
        </div>

        <!-- Modal Body -->
        <div>
          <!-- Plan Name and Delete Button -->
          <div class="flex justify-between items-center">
            <div class="w-1/2">
              <label for="plan-name" class="block text-sm font-medium text-gray-700 ml-2 mt-2">Plan Name</label>
              <UInput 
                id="plan-name"
                v-model="selectedPlanName"
                color="emerald"
                placeholder="Enter new plan name"
                class="w-full mt-2 ml-2"
              />
            </div>
            <UButton 
              @click="openModal()" 
              color="red" 
              variant="solid" 
              class="mr-3 mt-3"
            >
              Delete Recovery Plan
            </UButton>
          </div>

          <!-- Left (25%) and Right (75%) Sections -->
          <div class="flex-gap-container">
            <div class="exercise-list">
              <h3 class="section-title">Recovery Exercises</h3>
              <UTextarea
                v-model="searchQuery"
                color="green"
                size="xs"
                variant="outline"
                placeholder="Search..."
                class="mb-4 ml-2"
                :rows="1"
              />
              <ul class="exercise-list">
              <li
                v-for="exercise in filteredExercises"
                :key="exercise.id"
                class="exercise-item flex justify-between items-center ml-2"
              >
              <span>{{ exercise.name }}</span>
              <UButton 
                @click="addExerciseToPlan(exercise)" 
                color="emerald" 
                size="sm"
              >
                Add to Plan
              </UButton>
              </li>
              </ul>
            </div>

            <!-- Right Section -->
            <div class="current-exercises">
              <h3 class="section-title">Current Recovery Exercises</h3>
              <UTable 
                v-if="workoutDays.length" 
                :sort="sort"
                :columns="columns" 
                :rows="flattenedExercises"
                class="mr-2"
              >
                <template #actions-data="{ row }">
                  <UButton 
                    @click="deleteExerciseFromRecoveryPlan(row.id, row.dayNumber)" 
                    color="red" 
                    size="sm"
                  >
                    Remove
                  </UButton>
                </template>
              </UTable>
              <p v-else class="text-center text-gray-600">No recovery exercises added yet.</p>
            </div>
          </div>
        </div>

        <!-- Modal Footer -->
        <div class="modal-footer mt-3 mb-3 flex justify-end gap-4 border-t pt-4">
          <UButton 
            @click="pushLanding" 
            color="gray" 
            variant="ghost"
          >
            Cancel
          </UButton>
          <UButton 
            @click="updateRecoveryPlan" 
            color="emerald" 
            :disabled="!isEditNameValid"
            class="mr-3"
          >
            Save Changes
          </UButton>
        </div>

      <!-- Delete Confirmation Modal -->
      <UModal v-model="isModalOpen">
        <div class="bg-white rounded-lg shadow-md p-6">
          <h3 class="text-lg font-semibold text-center">
            Are you sure you want to delete this plan?<br />
            This action cannot be undone.
          </h3>
          <div class="button-container flex justify-center gap-4 mt-4">
            <UButton 
              @click="deleteSelectedPlan(plan?.id)" 
              color="red" 
              variant="solid" 
              size="lg"
            >
              Delete
            </UButton>
            <UButton 
              @click="closeModal" 
              color="gray" 
              variant="ghost" 
              size="lg"
            >
              Cancel
            </UButton>
          </div>
        </div>
      </UModal>

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
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useApi } from '~/composables/useApi';
import { useToast } from '#imports';
import { useCookie } from '#app';
import type { RecoveryPlanWithAppUserInfo } from '~/types/recoveryPlan';
import type { RecoveryExercise } from '~/types/recoveryExercise';

const { customFetch } = useApi();
const toast = useToast();

const plan = ref<RecoveryPlanWithAppUserInfo | null>(null);
const recoveryExercises = ref<RecoveryExercise[]>([]);
const isAddModalOpen = ref(false);
const isModalOpen = ref(false);
const route = useRoute();
const router = useRouter();
const selectedPlanName = ref('');
const searchQuery = ref('');
const workoutDays = ref<
  { dayNumber: number; exercises: { id: number; sets: number; reps: number; duration: string }[] }[]
>([]);
const modalData = ref({ dayNumber: 1, sets: 0, reps: 0, duration: "" });
const selectedExercise = ref<RecoveryExercise | null>(null);

const sort = ref<{
  column: string;
  direction: 'desc' | 'asc';
}>({
  column: 'name',
  direction: 'desc',
});

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

const columns = [
  { key: "dayNumber", label: "Day", sortable: true },
  { key: "name", label: "Exercise Name", sortable: true },
  { key: "sets", label: "Sets", sortable: true },
  { key: "reps", label: "Reps", sortable: true },
  { key: "duration", label: "Duration", sortable: true },
  { key: "actions", label: "Actions" },
];

const isEditNameValid = computed(() => selectedPlanName.value.trim().length > 0 && selectedPlanName.value.length <= 40);

const openAddModal = (exercise: RecoveryExercise) => {
  selectedExercise.value = exercise;
  modalData.value = { dayNumber: 1, sets: 0, reps: 0, duration: "" };
  isAddModalOpen.value = true;
};

const closeAddModal = () => {
  isAddModalOpen.value = false;
  selectedExercise.value = null;
};

const openModal = () => (isModalOpen.value = true);
const closeModal = () => (isModalOpen.value = false);

const fetchRecoveryPlan = async () => {
  const token = useCookie('token').value || null;
  try {
    const response = await customFetch<RecoveryPlanWithAppUserInfo>(`doctor/recovery-plan/${route.params.id}`, {
      method: 'GET',
      headers: { Authorization: `Bearer ${token}` },
    });

    plan.value = response;
    selectedPlanName.value = plan.value.name;
    
    workoutDays.value = plan.value.workoutDays.map(day => ({
      dayNumber: day.dayNumber,
      exercises: day.exercises.map(exercise => ({
        id: exercise.id,
        name: exercise.name,
        sets: exercise.sets,
        reps: exercise.reps,
        duration: exercise.duration ?? '',
      })),
    }));

  } catch (error) {
    console.error('Error fetching recovery plan:', error);
  }
};

const pushLanding = () => { 
  router.push('/doctor-plan-landing'); 
};

const fetchRecoveryExercises = async () => {
  const token = useCookie('token').value || null;
  try {
    const response = await customFetch<RecoveryExercise[]>('recoveryexercise', {
      method: 'GET',
      headers: { Authorization: `Bearer ${token}` },
    });
    recoveryExercises.value = response;
  } catch {
    toast.add({ title: 'Error', description: 'Failed to fetch exercises.', color: 'red', timeout: 3000 });
  }
};

const confirmAddExercise = () => {
  if (!selectedExercise.value) return;

  const { dayNumber, sets, reps, duration } = modalData.value;
  const exerciseId = selectedExercise.value.id;

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

const updateRecoveryPlan = async () => {
  const token = useCookie('token').value || null;

  const payload = {
    name: selectedPlanName.value.trim(),
    workoutDays: workoutDays.value,
  };

  try {
    await customFetch(`doctor/update-plan/${route.params.id}`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify(payload), 
    });

    toast.add({ title: 'Success', description: 'Plan updated successfully.', color: 'green', timeout: 3000 });
    pushLanding();
  } catch (error) {
    console.error('Error updating recovery plan:', error);
    toast.add({ title: 'Error', description: 'Failed to update recovery plan.', color: 'red', timeout: 3000 });
  }
};

const deleteSelectedPlan = async (planId: number | undefined) => {
  if (!planId) return;
  const token = useCookie('token').value || null;
  try {
    await customFetch(`doctor/delete-plan/${planId}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${token}` },
    });

    toast.add({ title: 'Success', description: 'Plan deleted successfully.', color: 'green', timeout: 3000 });
    plan.value = null;
    pushLanding();
  } catch {
    toast.add({ title: 'Error', description: 'Failed to delete recovery plan.', color: 'red', timeout: 3000 });
  }
};

const addExerciseToPlan = (exercise: RecoveryExercise) => {
  openAddModal(exercise);
};

const deleteExerciseFromRecoveryPlan = (exerciseId: number, dayNumber: number) => {
  const day = workoutDays.value.find(d => d.dayNumber === dayNumber);
  if (day) {
    day.exercises = day.exercises.filter(exercise => exercise.id !== exerciseId);
    if (day.exercises.length === 0) {
      workoutDays.value = workoutDays.value.filter(d => d.dayNumber !== dayNumber);
    }
  }
};

const filteredExercises = computed(() => {
  const filtered = recoveryExercises.value.filter((exercise) =>
      exercise.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    );

    // Sort exercises alphabetically
    return filtered.sort((a, b) => a.name.localeCompare(b.name));
});

onMounted(() => {
  fetchRecoveryPlan();
  fetchRecoveryExercises();
});
</script>

<style scoped>
.modal-header {
  border-bottom: 1px solid #eaeaea;
}

.modal-body {
  flex-grow: 1;
  overflow-y: auto;
  max-height: calc(100vh - 120px); 
}

.modal-footer {
  background-color: white; 
  position: sticky;
  bottom: 0;
}

.recovery-plans-container {
  display: flex;
  flex-direction: column;
}

.plans-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 20px;
  width: 100%;
  max-width: 100%;
  justify-content: center; 
  margin-top: 20px;
}

.plan-box {
  width: 180px;
  height: 180px;
  background-color: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
  text-align: center;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.plan-box:hover {
  transform: translateY(-4px);
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.15);
}

.button-container {
  display: flex; 
  justify-content: center; 
  margin-top: 1rem; 
  gap: 1rem; 
}

.main-title {
  text-align: center;
  font-weight: bold;
  max-width: fit-content;
  margin: 0 auto;
  font-size: 1.5em;
}

.exercise-list {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  overflow-y: auto;
}

.current-exercises {
  flex: 3;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  height: 100%;
}

.section-title {
  text-align: center;
  font-weight: bold;
  font-size: 1em;
  margin: 10px 0;
  padding-top: 10px;
}

.exercise-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 3px 10px;
}

.flex-gap-container {
  display: flex;
  gap: 1rem;
  height: calc(100vh - 250px);
}
</style>