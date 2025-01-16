<template>
    <div class="statistics-container">
      <h1 class="main-title">Injury Statistics</h1>
  
      <div class="statistics-content">
        <div class="injury-group">
          <h2 class="section-title">Active Injuries</h2>
          <div class="injury-cards">
            <div
              v-for="injury in activeInjuries"
              :key="injury.injuryId"
              class="injury-card active"
            >
              <h3 class="injury-name">{{ injury.injuryName }}</h3>
              <p class="injury-date">Start Date: {{ formatDate(injury.startDate) }}</p>
            </div>
          </div>
          <p v-if="activeInjuries.length === 0" class="no-injuries">No active injuries found.</p>
        </div>
  
        <div class="injury-group">
          <h2 class="section-title">Inactive Injuries</h2>
          <div class="injury-cards">
            <div
              v-for="injury in inactiveInjuries"
              :key="injury.injuryId"
              class="injury-card inactive"
            >
              <h3 class="injury-name">{{ injury.injuryName }}</h3>
              <p class="injury-date">Start Date: {{ formatDate(injury.startDate) }}</p>
              <p class="injury-date">End Date: {{ formatDate(injury.endDate) }}</p>
            </div>
          </div>
          <p v-if="inactiveInjuries.length === 0" class="no-injuries">No inactive injuries found.</p>
        </div>
      </div>
    </div>
  </template>
  
  <script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import { useApi } from '~/composables/useApi';
  
  interface InjuryHistory {
    injuryId: number;
    injuryName: string;
    startDate: string;
    endDate: string | null;
  }
  
  const { customFetch } = useApi();
  
  const activeInjuries = ref<InjuryHistory[]>([]);
  const inactiveInjuries = ref<InjuryHistory[]>([]);
  const error = ref('');
  
  const fetchInjuryHistory = async () => {
    const token = useCookie('token').value || null;
  
    try {
      const response = await customFetch<InjuryHistory[]>('statistics/user/injury-history', {
        method: 'GET',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
  
      activeInjuries.value = response.filter(injury => !injury.endDate);
      inactiveInjuries.value = response.filter(injury => injury.endDate);
    } catch (err) {
      error.value = 'Failed to fetch injury history: ' + (err as Error).message;
    }
  };
  
  const formatDate = (date: string | null): string => {
    if (!date) return 'N/A';
    const options: Intl.DateTimeFormatOptions = { year: 'numeric', month: 'long', day: 'numeric' };
    return new Intl.DateTimeFormat('en-US', options).format(new Date(date));
  };
  
  onMounted(() => {
    fetchInjuryHistory();
  });
  </script>
  
  <style scoped>
  .statistics-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 20px;
    background-color: #f9f9f9;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    max-width: 900px;
    margin: 20px auto;
  }
  
  .main-title {
    font-size: 2em;
    font-weight: bold;
    margin-bottom: 20px;
    color: #333;
  }
  
  .statistics-content {
    display: flex;
    flex-direction: column;
    width: 100%;
  }
  
  .injury-group {
    margin-bottom: 30px;
  }
  
  .section-title {
    font-size: 1.5em;
    font-weight: bold;
    margin-bottom: 10px;
    color: #2c3e50;
  }
  
  .injury-cards {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
    gap: 15px;
  }
  
  .injury-card {
    background: #ffffff;
    border: 1px solid #ddd;
    border-radius: 12px;
    padding: 20px;
    transition: transform 0.2s ease-in-out, box-shadow 0.2s ease-in-out;
    cursor: pointer;
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
  }
  
  .injury-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
  }
  
  .injury-card.active {
    border-left: 5px solid #27ae60;
  }
  
  .injury-card.inactive {
    border-left: 5px solid #c0392b;
  }
  
  .injury-name {
    font-size: 1.2em;
    font-weight: bold;
    margin-bottom: 10px;
    color: #34495e;
  }
  
  .injury-date {
    font-size: 0.95em;
    color: #7f8c8d;
  }
  
  .no-injuries {
    text-align: center;
    font-style: italic;
    color: #7f8c8d;
  }
  </style>  