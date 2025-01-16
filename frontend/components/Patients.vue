<template>
  <div class="container mx-auto">
    <h1 class="main-title text-2xl font-bold mb-6">Patients</h1>

    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <div class="box">
        <h2 class="text-lg font-semibold mb-4">All Patients</h2>
        <div class="patient-list grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="patient in allPatients"
            :key="patient.appUserId"
            class="patient-card"
          >
            <div class="patient-details">
              <h3 class="font-medium break-words">Username: {{ patient.username }}</h3>
              <p class="text-sm break-words">Injury: {{ getInjuryNameById(patient.injuryId) }}</p>
            </div>
            <UButton 
              @click="assignDoctor(patient.appUserId, patient.injuryId)" 
              class="mt-2 w-full flex justify-center" 
              color="emerald" 
              size="sm">
              Assign to Me
            </UButton>
          </div>
        </div>
      </div>

      <div class="box">
        <h2 class="text-lg font-semibold mb-4">My Patients</h2>
        <div class="patient-list grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="patient in myPatients"
            :key="patient.appUserId"
            class="patient-card"
          >
            <div class="patient-details">
              <h3 class="font-medium break-words">Username: {{ patient.username }}</h3>
              <p class="text-sm break-words">Injury: {{ getInjuryNameById(patient.injuryId) }}</p>
            </div>
            <UButton 
              @click="unassignDoctor(patient.appUserId, patient.injuryId)" 
              class="mt-2 w-full flex justify-center" 
              color="emerald" 
              size="sm">
              Unassign
            </UButton>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useApi } from '~/composables/useApi'; 
import type { PatientInjuryAssignment } from '~/types/patientInjuryAssignment';

const { customFetch } = useApi();

type PatientInfo = {
  appUserId: string,
  injuryId: number,
  username: string
}

const allPatients = ref<PatientInfo[]>([]);
const myPatients = ref<PatientInjuryAssignment[]>([]);
const injuries = ref<Injury[]>([]);

const fetchAllPatients = async () => {
  const token = useCookie('token').value || null;
  try {
    const response = await customFetch<PatientInfo[]>('doctor/patients/available', {
      method: 'GET',
      headers: { Authorization: `Bearer ${token}` },
    });
    allPatients.value = response;
  } catch (error) {
    console.error('Failed to fetch all patients:', error);
  }
};

const fetchMyPatients = async () => {
  const token = useCookie('token').value || null;
  try {
    const response = await customFetch<PatientInjuryAssignment[]>('doctor/patients', {
      method: 'GET',
      headers: { Authorization: `Bearer ${token}` },
    });
    myPatients.value = response;
  } catch (error) {
    console.error('Failed to fetch my patients:', error);
  }
};

const assignDoctor = async (userId: string, injuryId: number) => {
  const token = useCookie('token').value || null;
  const payload = {
    appUserId: userId,
    injuryId: injuryId, 
  };

  try {
    await customFetch('doctor/assign-doctor', {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}` },
      body: JSON.stringify(payload),
    });
    const patientToAssign = allPatients.value.find(
      (patient) => patient.appUserId === userId && patient.injuryId === injuryId
    );
    if (patientToAssign) {
      myPatients.value.push({
        ...patientToAssign,
        isTooSevere: false,
      });
      allPatients.value = allPatients.value.filter(
        (patient) => !(patient.appUserId === userId && patient.injuryId === injuryId)
      );
    }
  } catch (error) {
    console.error('Failed to assign doctor:', error);
  }
};

const unassignDoctor = async (userId: string, injuryId: number) => {
  const token = useCookie('token').value || null;
  const payload = {
    appUserId: userId,
    injuryId: injuryId, 
  };

  try {
    await customFetch('doctor/unassign-doctor', {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${token}` },
      body: JSON.stringify(payload),
    });
    const patientToUnassign = myPatients.value.find(
      (patient) => patient.appUserId === userId && patient.injuryId === injuryId
    );
    if (patientToUnassign) {
      allPatients.value.push(patientToUnassign);
      myPatients.value = myPatients.value.filter(
        (patient) => !(patient.appUserId === userId && patient.injuryId === injuryId)
      );
    }
  } catch (error) {
    console.error('Failed to unassign doctor:', error);
  }
};

const fetchInjuries = async () => {
  const token = useCookie('token').value || null;

  try {
    const response = await customFetch<Injury[]>('injury', {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    injuries.value = response;
  } catch (err) {
    console.error( 'Failed to fetch injuries: ' + (err as Error).message);
  }
};

const getInjuryNameById = (injuryId: number): string => {
  const injury = injuries.value.find((injury) => injury.id === injuryId);
  return injury ? injury.name : 'Unknown Injury'; 
};


onMounted(() => {
  fetchMyPatients();
  fetchInjuries();
  fetchAllPatients();
});
</script>

<style scoped>
.main-title {
  text-align: center;
  margin-top: 5px;
}

.box {
  background: #ffffff;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
  height: calc(100vh - 150px);
}

.patient-list {
  overflow-y: auto;
  padding-right: 5px;
}

.patient-card {
  background: #f9f9f9;
  border: 1px solid #e5e7eb;
  border-radius: 6px;
  padding: 12px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
  display: flex;
  flex-direction: column;
  justify-content: space-between; 
  gap: 8px;
  height: 100%;
}

.patient-details {
  flex-grow: 1;
}
</style>
