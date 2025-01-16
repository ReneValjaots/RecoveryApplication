<template>
    <div class="recovery-plans-container">
      <h1 class="main-title">My Recovery Plans</h1>
      <UButton label="Create New Recovery Plan" @click="openModal" color="emerald" class="create-button"/>
  
      <UModal v-model="isModalOpen">
        <div class="bg-white rounded-lg shadow-md p-6">
          <h3 class="text-lg font-semibold text-center">Create a new recovery plan</h3>
          
          <div class="mt-4">
            <label class="label" for="plan-name">Plan Name</label>
            <UInput 
              id="plan-name"
              v-model="newPlanName" 
              placeholder="Enter plan name" 
              size="lg"
              color="gray"
            />
            <p v-if="!isNameValid" class="error-message">{{ nameErrorMessage }}</p>
            <p v-else class="margin-message"></p>
          </div>
  
          <div class="button-container">
            <UButton 
              @click="createRecoveryPlan" 
              :disabled="!isNameValid" 
              color="emerald" 
              variant="solid" 
              size="lg" 
              class="modal-button">
                OK
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
  
        <div class="plans-grid">
          <div
            v-for="plan in recoveryPlans"
            :key="plan.id"
            @click="selectPlan(plan.id)"
            :class="['plan-box', { 'plan-doctor': plan.isCreatedByDoctor }]">
            <h2>{{ plan.name }}</h2>
          </div>
        </div>
    </div>
</template>
    
<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import type { RecoveryPlan } from '~/types/recoveryPlan';
  import { useApi } from '~/composables/useApi';
  import { useCookie } from '#app';

  const { customFetch } = useApi();
  const router = useRouter();

  const toast = useToast();
  const error = ref('');

  const recoveryPlans = ref<RecoveryPlan[]>([]);
  const isModalOpen = ref(false);
  const newPlanName = ref('');

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

  const fetchRecoveryPlans = async () => {
    const token = useCookie('token').value || null;
    try {
      const response = await customFetch<RecoveryPlan[]>('/recoveryplan', {
        method: 'GET',
        headers: { Authorization: `Bearer ${token}` },
      });
      console.log(response);
      recoveryPlans.value = response;
    } catch (error) {
      toast.add({ title: "Error", description: `Error fetching plans`, color: 'red', timeout: 3000 });
    }
  };

  const openModal = () => {
    isModalOpen.value = true;
  };

  const closeModal = () => {
    isModalOpen.value = false;
    newPlanName.value = '';
  };

  const createRecoveryPlan = async () => {
    const token = useCookie('token').value || null;
    try {
        const response = await customFetch<RecoveryPlan>('recoveryplan', {
            method: 'POST',
            headers: {
                Authorization: `Bearer ${token}`
            },
            body: JSON.stringify(newPlanName.value),
        });
        
        recoveryPlans.value.push(response);
        toast.add({ title: "Success", description: `Recovery plan named ${response.name} created successfully`, color: 'green', timeout: 3000 });
        newPlanName.value = '';
        closeModal(); 
    } catch (err) {
        error.value = 'Failed to create recovery plan: ' + (err as Error).message;
        toast.add({ title: "Error", description: `Failed to create recovery plan`, color: 'red', timeout: 3000 });
    }
  };

  const selectPlan = (planId: number) => {
    router.push(`/plans/${planId}`);
  };

  onMounted(fetchRecoveryPlans);
</script>

<style scoped>
.recovery-plans-container {
  display: flex;
  flex-direction: column;
  padding: 20px;
}

.main-title {
  font-size: 2em;
  font-weight: bold;
  color: #333;
  text-align: center;
  margin-bottom: 20px;
}

.plans-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 20px;
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
  margin-bottom: 20px;
}

.modal-button {
  width: 16.666667%; 
  display: flex; 
  justify-content: center; 
}

.plan-doctor {
  background-color: #bfdbfe;
  border-color: #60a5fa; 
}

.create-button {
  padding: 10px 10px;
  margin: 0 auto;
  font-size: 1em;
}

.error-message {
  min-height: 1.5rem; 
  margin-top: 0.25rem;
  margin-left: 0.25rem;
  font-size: 0.875rem; 
  color: #ff0000; 
}

.margin-message {
  min-height: 1.5rem; 
  margin-top: 0.25rem;
  margin-left: 0.25rem;
}
</style>