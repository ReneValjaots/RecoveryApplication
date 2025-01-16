<template>
  <div>
    <h1 class="main-title">{{ title }}</h1>
    <div class="pageContent">

      <UModal v-model="isQuizModalOpen">
        <Quiz 
          :bodyPart="selectedBodyPart" 
          @scoreSubmitted="handleQuizScore" 
          @close="closeQuizModal"/>
      </UModal>

      <UModal v-model="isSevereModalOpen">
        <div class="severe-modal-content">
          <div>
            <h2 class="severe-message text-red-600">Warning</h2>
            <UButton
              padded
              color="gray"
              variant="link"
              icon="i-heroicons-x-mark-20-solid"
              @click="closeSevereModal"
              class="close-button"
            />
          </div>
          <p class="mb-4 mt-4">
            Your injury is too severe. A doctor will create a plan for your recovery.
          </p>
        </div>
      </UModal>

      <div class="injury-list">
        <div class="pb-3">
          <h2 class="section-title">Injuries:</h2>
          <UTextarea
            v-model="searchQuery"
            color="emerald"
            size="xs"
            variant="outline"
            placeholder="Search..."
            :rows="1"
          />
        </div>

        <ul class="injury-list-ul">
          <li v-for="(injuries, bodyPart) in groupedInjuries" :key="bodyPart" class="injury-item">
            <div @click="toggleBodyPart(bodyPart)" class="cursor-pointer font-semibold">
              {{ bodyPart }} 
              <span class="text-sm text-gray-500">{{ injuries.length }} injuries</span>
            </div>
            <ul v-show="expandedBodyParts.includes(bodyPart)">
              <li
                v-for="injury in injuries"
                :key="injury.id"
                class="injury-item"
              >
                <span>{{ injury.name }}</span>
              <UButton
                :disabled="isInjuryAdded(injury)"
                @click="initiateAddInjuryProcess(injury)"
                :color="isInjuryAdded(injury) ? 'gray' : 'emerald'"
              >
                {{ isInjuryAdded(injury) ? 'Added' : 'Add' }}
              </UButton>
            </li>
          </ul>
          </li>
        </ul>
      </div>

       <div class="current-injuries">
        <h2 class="section-title">User Injuries:</h2>
        <p v-if="userInjuries.length === 0" class="no-injuries">No injuries assigned.</p>
        <UTable v-else :sort="sort" :rows="userInjuries" :columns="columns" row-class="severe-injury-row" class="ml-2">
        <template #recoveryExercises-data="{ row }">
          <pre>{{ formatRecoveryExercises(row.recoveryExercises) }}</pre>
        </template>
        <template #actions-data="{ row }">
          <UButton @click="deleteInjuryFromUser(row.id)" color="red">Delete</UButton>
        </template>
      </UTable>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import Quiz from '~/components/Quiz.vue';
import type { Injury } from '~/types/injury';
import { useApi } from '~/composables/useApi';

const { customFetch } = useApi();
defineProps<{ title: String }>();

const searchQuery = ref('');
const injuries = ref<Injury[]>([]);
const userInjuries = ref<Injury[]>([]);
const expandedBodyParts = ref<string[]>([]);
const error = ref<string | null>(null);
const currentInjury = ref<Injury | null>(null);

const toast = useToast();
const sort = ref<{
  column: string;
  direction: 'desc' | 'asc';
}>({
  column: 'name',
  direction: 'desc',
});

const isQuizModalOpen = ref(false);
const isSevereModalOpen = ref(false);

const groupedInjuries = computed(() => {
  const filtered = injuries.value.filter((injury) =>
    injury.name.toLowerCase().includes(searchQuery.value.toLowerCase())
  );
  const grouped = filtered.reduce((groups, injury) => {
    if (!groups[injury.bodyPart]) {
      groups[injury.bodyPart] = [];
    }
    groups[injury.bodyPart].push(injury);
    return groups;
  }, {} as Record<string, Injury[]>);

  // Sort injuries alphabetically within each body part
  Object.keys(grouped).forEach((bodyPart) => {
    grouped[bodyPart].sort((a, b) => a.name.localeCompare(b.name));
  });

  return grouped;
});

const selectedBodyPart = ref<string>('');

const toggleBodyPart = (bodyPart: string) => {
  const index = expandedBodyParts.value.indexOf(bodyPart);
  if (index > -1) {
    expandedBodyParts.value.splice(index, 1);
  } else {
    expandedBodyParts.value.push(bodyPart);
  }
};

const initiateAddInjuryProcess = (injury: Injury) => {
  const injuryExists = userInjuries.value.some(userInjury => userInjury.id === injury.id);
  if (injuryExists) {
    toast.add({ title: 'Error', description: `${injury.name} already assigned to the user`, color: 'red', timeout: 3000 });
  } else {
    currentInjury.value = injury; 
    selectedBodyPart.value = injury.bodyPart;
    isQuizModalOpen.value = true; 
  }
};

const handleQuizScore = async ({ isTooSevere }: { isTooSevere: boolean }) => {
  isQuizModalOpen.value = false;
  if (isTooSevere) {
    isSevereModalOpen.value = true;
    if (currentInjury.value) {
      try {
        await addInjuryToUser(currentInjury.value, true);
      }
      catch {
        toast.add({ title: 'Error', description: 'Failed to assign severe injury to user', color: 'red', timeout: 3000 });
      }
    }
  } else if (currentInjury.value) {
    try {
      await addInjuryToUser(currentInjury.value);
    } catch {
      toast.add({ title: 'Error', description: 'Failed to assign injury to user', color: 'red', timeout: 3000 });
    }
  }
};

const closeQuizModal = () => {
  isQuizModalOpen.value = false;
};

const closeSevereModal = () => {
  isSevereModalOpen.value = false;
};

const formatRecoveryExercises = (exercises: any[]) => {
  return exercises.map(exercise => exercise.name).join(', ');
};

const columns = [
  { key: 'name', label: 'Name', sortable: true },
  { key: 'description', label: 'Description', sortable: true },
  { key: 'recoveryExercises', label: 'Recovery Exercises', slot: true },
  { key: 'actions', label: 'Actions' }
];

const fetchUserInjuries = async () => {
  const token = useCookie('token').value || null;

  try {
    const response = await customFetch<any[]>('userinjury/user/injuries', {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    userInjuries.value = response;
  } catch (err) {
    error.value = 'Failed to fetch user injuries: ' + (err as Error).message;
  }
};

const fetchAllInjuries = async () => {
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
    error.value = 'Failed to fetch all injuries: ' + (err as Error).message;
  }
};

const addInjuryToUser = async (injury: Injury, isTooSevere: boolean | null = null) => {
  const exists = userInjuries.value.some(userInjury => userInjury.id === injury.id);

  if (exists) {
    toast.add({ title: 'Error', description: `${injury.name} already assigned to the user`, color: 'red', timeout: 3000 });
    return;
  }

  const token = useCookie('token').value || null;
  const payload = {
    injuryId: injury.id, 
    isTooSevere: isTooSevere !== null ? isTooSevere : false 
  };
  console.log('Payload:', payload);
  try {
    await customFetch(`userinjury/assign`, {
      method: 'PUT',
      headers: {
        Authorization: `Bearer ${token}`
      },
      body: JSON.stringify(payload),
    });
    userInjuries.value.push(injury);
  } catch (err) {
    error.value = 'Failed to add injury to user';
    toast.add({ title: 'Error', description: 'Failed to add injury to user', color: 'red', timeout: 3000 });
  }
}

const deleteInjuryFromUser = async (injuryId: number) => {
  const token = useCookie('token').value || null;

  try {
    await customFetch(`userinjury/unlink/${injuryId}`, {
      method: 'PATCH',
      headers: {
        Authorization: `Bearer ${token}`
      },
    });
    userInjuries.value = userInjuries.value.filter(injury => injury.id !== injuryId); 
  } catch (err) {
    error.value = 'Failed to remove injury from user';
  }
};
const isInjuryAdded = (injury: Injury) => {
  return userInjuries.value.some(userInjury => userInjury.id === injury.id);
};

onMounted(() => {
  fetchUserInjuries();
  fetchAllInjuries();
});
</script>

<style scoped>
.main-title {
  font-size: 2em;
  text-align: center;
  flex-grow: 1;
  font-weight: bold;
}

.injury-list-ul{
  max-height: 100%;
  overflow-y: auto;
  flex: 1;
}

.pageContent {
  display: flex;
  flex-direction: wrap;
  justify-content: space-between;
  width: 100%;
  padding: 10px;
  box-sizing: border-box;
  height: calc(100vh - 100px);
  overflow-y: hidden;
}

.injury-list {
  flex: 1;
  padding: 10px 6px 10px 10px;
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

.injury-item {
  display: block;
  margin-bottom: 10px;
}

.injury-item > div {
  cursor: pointer;
  padding: 10px 15px;
  margin-right: 6px;
  background-color: #f3f4f6;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  transition: background-color 0.3s, box-shadow 0.3s;
}

.injury-item ul li {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background-color: #ffffff;
  margin-bottom: 8px;
  margin-top: 8px;
  margin-right: 6px;
  transition: transform 0.3s, box-shadow 0.3s;
}

.current-injuries {
  flex: 3; 
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.no-injuries {
  text-align: center;
  font-size: 1em;
  margin: 10px 0;
}

.centered-alert {
  text-align: center;
}

.severe-modal-content {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.close-button {
  position: absolute;
  top: 0.5rem;
  right: 0.5rem;
}

.severe-message {
  margin-top: 0.5rem;
  font-weight: bold;
  text-align: center; 
  line-height: 1.5; 
}

.contact-button {
  margin: 0.5rem;
}

.severe-injury-row {
  background-color: #ffe5e5;
  color: #d32f2f;
}
</style>