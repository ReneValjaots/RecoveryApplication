<template>
    <div class="recovery-plans-container">
      <h1 class="main-title">Recovery Plans</h1>
  
      <div class="button-container">
      <UButton 
        label="Create New Recovery Plan" 
        @click="navigateToCreatePlan" 
        color="emerald" 
        class="create-button"
      />
    </div>
  
      <div class="plans-grid">
        <div
          v-for="plan in recoveryPlans"
          :key="plan.id"
          @click="navigateToPlan(plan.id)"
          :class="['plan-box', { 'plan-doctor': plan.isCreatedByDoctor }]">
          <h2>{{ plan.name }}</h2>
        </div>
      </div>
    </div>
  </template>
  
  <script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import { useToast } from '#imports';
  import { useApi } from '~/composables/useApi';
  import { useCookie } from '#app';
  import type { RecoveryPlan } from '~/types/recoveryPlan';
  
  const router = useRouter();
  const toast = useToast();
  const { customFetch } = useApi();
  
  const recoveryPlans = ref<RecoveryPlan[]>([]);
  
  const fetchRecoveryPlans = async () => {
    const token = useCookie('token').value || '';
    try {
      const plans = await customFetch<RecoveryPlan[]>('/doctor/recovery-plans', {
        method: 'GET',
        headers: { Authorization: `Bearer ${token}` },
      });
      recoveryPlans.value = plans;
    } catch (error) {
      toast.add({ title: 'Error', description: 'Unable to fetch recovery plans.', color: 'red', timeout: 3000 });
    }
  };
  
  const navigateToPlan = (planId: number) => {
    router.push(`/doctorPlans/${planId}`);
  };
  
  const navigateToCreatePlan = () => {
    router.push('/doctor-create-plan');
  };
  
  onMounted(fetchRecoveryPlans);
  </script>
  
  <style scoped>
  .recovery-plans-container {
    padding: 20px;
    display: flex;
    flex-direction: column;
  }
  
  .plans-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
    gap: 20px;
    margin-top: 20px;
  }

  .button-container {
    display: flex;
    justify-content: center;
    margin-bottom: 20px;
    }
  
  .plan-box {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 180px;
    height: 180px;
    background-color: #f9fafb;
    border: 1px solid #e5e7eb;
    border-radius: 8px;
    cursor: pointer;
    text-align: center;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    transition: transform 0.2s, box-shadow 0.2s;
  }
  
  .plan-box:hover {
    transform: translateY(-4px);
    box-shadow: 0 6px 12px rgba(0, 0, 0, 0.15);
  }
  
  .main-title {
    font-size: 2em;
    margin-bottom: 20px;
    text-align: center;
  }
  
  .create-button {
    width: 16.666667%; 
    display: flex; 
    justify-content: center; 
  }
</style>