<template>
  <div class="my-users-container">
    <h1 class="main-title">My Patients</h1>

    <div v-if="loading" class="loading-spinner">Loading...</div>
    <div v-else-if="groupedPatients.length === 0" class="no-patients">No patients assigned to you.</div>
    <div v-else class="patients-list">
      <div v-for="patient in groupedPatients" :key="patient.username" class="patient-card">
        <h2>Username: {{ patient.username }}</h2>
        <p>Injury IDs: {{ patient.injuryIds.join(', ') }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useApi } from '~/composables/useApi';
import { useAuthStore } from '~/stores/useAuthStore';

const { customFetch } = useApi();
const authStore = useAuthStore();

const patients = ref([]); // Original patients array
const loading = ref(true);

// Group patients by username and combine injuries
const groupedPatients = computed(() => {
  const grouped = {};

  // Group injuries by username
  patients.value.forEach(patient => {
    if (!grouped[patient.username]) {
      grouped[patient.username] = { username: patient.username, injuryIds: [] };
    }
    grouped[patient.username].injuryIds.push(patient.injuryId);
  });

  // Convert object to array
  return Object.values(grouped);
});

const fetchPatients = async () => {
  const token = useCookie('token').value || null;
  try {
    const response = await customFetch('/doctor/patients', {
      method: 'GET',
      headers: { Authorization: `Bearer ${token}` },
    });

    patients.value = response;
  } catch (error) {
    console.error("Failed to fetch patients:", error);
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  authStore.initialize();
  fetchPatients();
});
</script>

<style scoped>
.my-users-container {
  padding: 20px;
}

.main-title {
  font-size: 2em;
  margin-bottom: 20px;
  text-align: center;
}

.loading-spinner,
.no-patients {
  text-align: center;
  font-size: 1.2em;
  color: #888;
}

.patients-list {
  display: flex;
  flex-wrap: wrap;
  gap: 20px;
  justify-content: center;
}

.patient-card {
  background: #f9f9f9;
  padding: 20px;
  border: 1px solid #ddd;
  border-radius: 8px;
  width: 300px;
  text-align: center;
}

.patient-card h2 {
  font-size: 1.5em;
  margin-bottom: 10px;
}

.patient-card p {
  font-size: 1em;
  margin: 0;
}
</style>