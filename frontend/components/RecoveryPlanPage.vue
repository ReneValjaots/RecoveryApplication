<template>
    <div>
        <div class="title-container">
            <h1 class="main-title">{{ plan ? plan.name : '' }} </h1>
            <UButton @click="openModal()" color="red" class="delete-plan-button">Delete Recovery Plan</UButton>
        </div>
    
        <UModal v-model="isModalOpen">
            <div class="bg-white rounded-lg shadow-md p-6">
                <h3 class="text-lg font-semibold text-center">
                    Are you sure you want to delete this plan?<br /> 
                    This action cannot be undone.
                </h3>

                <div class="button-container">
                    <UButton 
                        @click="deleteRecoveryPlan(plan?.id)" 
                        color="red" 
                        variant="solid" 
                        size="lg" 
                        class="modal-button">
                            Delete
                    </UButton>
                    <UButton 
                        color="gray" 
                        variant="ghost" 
                        @click="closeModal" 
                        size="lg" 
                        class="modal-button">
                            Cancel
                    </UButton>
                </div>
            </div>
        </UModal> 

        <div v-if="plan" class="pageContent">
            <div class="exercise-list">
                <div class="mb-3">
                    <h2 class="section-title">Recovery Exercises:</h2>
                    <UTextarea
                    v-model="searchQuery"
                    color="emerald"
                    size="xs"
                    variant="outline"
                    placeholder="Search..."
                    :rows="1"
                    />
                </div>
                    <ul class="exercise-list-ul">
                    <li
                        v-for="exercise in filteredExercises"
                        :key="exercise.id"
                        class="exercise-item">
                        <span>{{ exercise.name }}</span>
                        <UButton 
                        @click="addExerciseToPlan(exercise)" color="emerald">
                        Add to Plan
                        </UButton>
                    </li>
                    </ul>
                </div>

            <div class="current-exercises">
                <h2 class="section-title">Current Recovery Exercises</h2>
                <div v-if="!plan.workoutDays || plan.workoutDays.length === 0">
                    <p class="no-exercises">No recovery exercises found</p>
                </div>

                <UTable v-else :sort="sort" :columns="columns" :rows="flattenedExercises" class="ml-2">
                    <template #actions-data="{ row }">
                        <UButton @click="deleteExerciseFromRecoveryPlan(row.id, row.dayNumber)" color="red">Delete</UButton>
                    </template>
                </UTable>
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
    import { useApi } from '~/composables/useApi';
    import { useRoute, useRouter } from 'vue-router';
    import { ref, computed, onMounted } from 'vue';
    import type { RecoveryExercise } from '~/types/recoveryExercise';
    import { useCookie } from '#app';
    import { _emerald } from '#tailwind-config/theme/typography';

    const toast = useToast();
    const searchQuery = ref('');
    const error = ref('');
    const { customFetch } = useApi();
    const route = useRoute();
    const router = useRouter();
    const plan = ref<RecoveryPlan | null>(null);

    const recoveryExercises = ref<RecoveryExercise[]>([]);
    const isModalOpen = ref(false);

    const sort = ref<{
        column: string;
        direction: 'desc' | 'asc';
    }>({
        column: 'name',
        direction: 'desc',
    });

    const isAddModalOpen = ref(false);
    const modalData = ref({
        dayNumber: 1,
        sets: 0,
        reps: 0,
        duration: '',
    });

    const flattenedExercises = computed(() => {
        if (!plan.value || !plan.value.workoutDays) return [];
        return plan.value.workoutDays.flatMap((day) =>
            day.exercises.map((exercise) => ({
            dayNumber: day.dayNumber,
            ...exercise,
            }))
        );
    });

    const selectedExercise = ref<RecoveryExercise | null>(null);

    const openAddModal = (exercise: RecoveryExercise) => {
        selectedExercise.value = exercise;
        modalData.value = { dayNumber: 1, sets: 0, reps: 0, duration: '' };
        isAddModalOpen.value = true;
    };

    const closeAddModal = () => {
        isAddModalOpen.value = false;
        selectedExercise.value = null;
    };

    const openModal = () => {
        isModalOpen.value = true;
    };
    
    const closeModal = () => {
        isModalOpen.value = false;
    };

    const deleteRecoveryPlan = async (planId: number | undefined) => {
        if (!planId) return;
        const token = useCookie('token').value || null;

        try {
            await customFetch(`recoveryplan/${planId}`, {
                method: 'DELETE',
                headers: {
                    Authorization: `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
            });

            toast.add({ title: 'Success', description: 'Recovery plan deleted successfully', color: 'green', timeout: 3000 });
            plan.value = null;
            router.push('/landingpage')
        } catch (err) {
            error.value = 'Failed to delete recovery plan: ' + (err as Error).message;
            
            toast.add({ title: 'Error', description: 'Failed to delete recovery plan', color: 'red', timeout: 3000 });
        }
    };

    const confirmAddExercise = async () => {
        if (!modalData.value.dayNumber || !selectedExercise.value) {
            toast.add({ title: 'Error', description: 'Day number is mandatory.', color: 'red', timeout: 3000 });
            return;
        }

        const token = useCookie('token').value || null;

        try {
            await customFetch(`recoveryplan/assign/${selectedExercise.value.id}/${route.params.id}`, {
            method: 'PUT',
            headers: {
                Authorization: `Bearer ${token}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                dayNumber: modalData.value.dayNumber,
                sets: modalData.value.sets || 0,
                reps: modalData.value.reps || 0,
                duration: modalData.value.duration || '',
            }),
            });

            await fetchPlanDetails();
            closeAddModal();
        } catch (err) {
            console.error(err);
            toast.add({ title: 'Error', description: 'Failed to add exercise to the plan.', color: 'red', timeout: 3000 });
        }
    };


    const addExerciseToPlan = async (exercise: RecoveryExercise) => {
        openAddModal(exercise);
    };

    const fetchPlanDetails = async () => {
        const token = useCookie('token').value || null;
        try {
            const response = await customFetch<RecoveryPlan>(`recoveryplan/${route.params.id}`, {
                method: 'GET',
                headers: { Authorization: `Bearer ${token}` },
            });
            plan.value = response;
        } catch (error) {
            console.error("Error fetching plan details:", error);
        }
    };

    const deleteExerciseFromRecoveryPlan = async (exerciseId: number, dayNumber: number) => {
        const token = useCookie('token').value || null;
        try {
            await customFetch(`recoveryplan/unlink/${exerciseId}/${route.params.id}`, {
                method: 'PATCH',
                headers: {
                    Authorization: `Bearer ${token}`,
                },
                body: JSON.stringify({ dayNumber }),
            });
            await fetchPlanDetails();
        } catch (err) {
            error.value = 'Failed to remove a recovery exercise from recovery plan: ' + (err as Error).message;
            toast.add({ title: 'Error', description: 'Failed to remove a recovery exercise from recovery plan.', color: 'red', timeout: 3000 });
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
            error.value = 'Failed to fetch recovery exercises: ' + (err as Error).message;

            toast.add({ title: 'Error', description: 'Failed to fetch recovery exercises', color: 'red', timeout: 3000 });
        }
    };

    const filteredExercises = computed(() => {
        const filtered = recoveryExercises.value.filter((exercise) =>
            exercise.name.toLowerCase().includes(searchQuery.value.toLowerCase())
        );

        // Sort exercises alphabetically
        return filtered.sort((a, b) => a.name.localeCompare(b.name));
    });

    const columns = [
        { key: "dayNumber", label: "Day", sortable: true },
        { key: "name", label: "Exercise Name", sortable: true },
        { key: "description", label: "Description", sortable: true },
        { key: "sets", label: "Sets", sortable: true },
        { key: "reps", label: "Reps", sortable: true },
        { key: "duration", label: "Duration", sortable: true },
        { key: "actions", label: "Actions" },
    ];

    onMounted(() => {
        fetchPlanDetails();
        fetchRecoveryExercises();
    });
</script>

<style scoped>
.title-container {
    display: flex;
    align-items: center;
    width: 100%;
    position: relative;
}

.exercise-list-ul{
    max-height: 100%; 
    overflow-y: auto; 
    flex: 1; 
    padding: 0rem;
    margin: 0;
}

.button-container {
    display: flex; 
    justify-content: center; 
    margin-top: 1rem; 
    gap: 1rem; 
}

.main-title {
  font-size: 1.5em;
  text-align: center;
  flex-grow: 1;
  font-weight: bold;
  margin-top: 10px;
}

.pageContent {
  display: flex;
  flex-direction: wrap;
  justify-content: space-between;
  width: 100%;
  padding: 10px;
  box-sizing: border-box;
  height: calc(100vh - 100px);
  overflow: hidden;
}

.exercise-list {
    flex: 1;
  padding: 10px;
  min-width: 250px;
  max-width: 30%;
  height: 100%;
  border-right: 1px solid #ccc;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
}

.section-title {
    text-align: center;
  font-weight: bold;
  font-size: 1em;
  margin: 10px 0;
  padding-top: 10px;
}

.centered-alert {
    text-align: center;
}

.exercise-item {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 3px 10px;
}

.current-exercises {
    flex: 3; 
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.no-exercises {
    text-align: center;
    font-size: 1em;
    margin: 10px 0;
}

.modal-button {
    width: 16.666667%; 
    display: flex; 
    justify-content: center; 
}

.delete-plan-button{
    position: absolute;
    top: 0;
    right: 0;
    font-size: 1em;
    margin-left: auto;
    margin-right: 10px;
    margin-top: 10px;
}
</style>
