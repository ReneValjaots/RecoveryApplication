<template>
  <div>
    <h1 class="main-title">{{ selectedRecoveryExercise ? selectedRecoveryExercise.name : title }}</h1>
    <div class="pageContent">
      <div class="recoveryExercise-list">
        <div class="pb-3">
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

          <ul class="recoveryExercise-ul">
            <li
              v-for="recoveryExercise in filteredRecoveryExercises"
              :key="recoveryExercise.id"
              class="recoveryExercise-item"
            >
              <span>{{ recoveryExercise.name }}</span>
              <UButton 
                color="emerald" 
                @click="selectRecoveryExercise(recoveryExercise)">
                  View Details
              </UButton>
            </li>
          </ul>
      </div>

      <div v-if="selectedRecoveryExercise" class="current-recoveryExercise">
        
        <div class="recoveryExercise-description">
          <h3 class="subtitle-green">Description</h3>
          <p class="description-text">{{ selectedRecoveryExercise.description }}</p>
        </div>

        <div class="image-and-exercises">
          <div
            v-if="selectedRecoveryExercise.injuries && selectedRecoveryExercise.injuries.length > 0"
            class="recoveryExercise-injuries">
            <h3 class="subtitle-green">Recovery Exercises</h3>
            <ul class="injury-list">
              <li
                v-for="injury in selectedRecoveryExercise.injuries"
                :key="injury.id"
                class="injury-item"
                @click="navigateToInjury(injury.id)">
                {{ injury.name }}
              </li>
            </ul>
          </div>
              <!-- Image -->
              <div v-if="imageExists" class="recoveryExercise-image-container">
            <img :src="imageSrc" class="recoveryExercise-image" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { useApi } from '~/composables/useApi';
  import type { RecoveryExercise } from '~/types/recoveryExercise';
  import { useRoute, useRouter } from 'vue-router';
  

  defineProps<{ title: string }>();

  const { customFetch } = useApi();
  const searchQuery = ref('');
  const route = useRoute(); 
  const router = useRouter();
  const recoveryExercises = ref<RecoveryExercise[]>([]);
  const selectedRecoveryExercise = ref<RecoveryExercise | null>(null);
  const imageExists = ref(false);
  const imageSrc = ref('');
  const error = ref('');

  const filteredRecoveryExercises = computed((): RecoveryExercise[] => {
    const filtered = recoveryExercises.value.filter((exercise) =>
      exercise.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    );

    // Sort exercises alphabetically
    return filtered.sort((a, b) => a.name.localeCompare(b.name));
  });

  const navigateToInjury = (injuryId: number) => {
  router.push({
    path: '/injuries',
    query: { injuryId },
  });
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

    const exerciseId = Number(route.query.exerciseId);
    if (exerciseId && recoveryExercises.value.length > 0) {
      const foundExercise = recoveryExercises.value.find(
        (exercise) => exercise.id === exerciseId
      );

      selectedRecoveryExercise.value = foundExercise || recoveryExercises.value[0];
    } else if (recoveryExercises.value.length > 0) {
      selectedRecoveryExercise.value = recoveryExercises.value[0];
    }

    checkImage();
  } catch (err) {
    console.error('Failed to fetch recovery exercises:', err);
  }
};

  const selectRecoveryExercise = (recoveryExercise: RecoveryExercise) => {
    selectedRecoveryExercise.value = recoveryExercise;
    checkImage();
  };

  const checkImage = () => {
    if (selectedRecoveryExercise.value) {
      const src = getImageSrc(selectedRecoveryExercise.value.name);
      const img = new Image();

      img.onload = () => {
        imageExists.value = true;
        imageSrc.value = src;
      };

      img.onerror = () => {
        imageExists.value = false;
      };

      img.src = src; 
    }
  };

  const getImageSrc = (name: string) => {
    const formattedName = name.toLowerCase().replace(/ /g, '');
    return `/img/${formattedName}.jpg`;
  };

  onMounted(() => {
    fetchRecoveryExercises();
  });
</script>

<style scoped>
.pageContent {
  display: flex;
  flex-wrap: nowrap;
  justify-content: space-between;
  gap: 20px;
  width: 100%;
  padding: 20px;
  box-sizing: border-box;
  height: calc(100vh - 100px);
  overflow: hidden;
  background-color: #f8fafc;
}


.main-title {
  font-size: 2em;
  text-align: center;
  flex-grow: 1;
  font-weight: bold;
}


.recoveryExercise-list {
  flex: 1;
  padding: 10px 6px 10px 10px;
  max-width: 30%;
  min-width: 250px;
  height: 100%;
  background-color: #ffffff;
  border-radius: 8px;
  box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.1);
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

.recoveryExercise-ul {
  max-height: 100%;
  overflow-y: auto;
  flex: 1;
}

.recoveryExercise-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background-color: #ffffff;
  margin-bottom: 8px;
  margin-right: 6px;
  transition: transform 0.3s, box-shadow 0.3s;
}

.recoveryExercise-item:hover {
  box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.1);
}

.current-recoveryExercise {
  flex: 3;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  padding: 20px;
  background-color: #ffffff;
  border-radius: 12px;
  box-shadow: 0px 8px 16px rgba(0, 0, 0, 0.1);
  overflow: auto;
  animation: fadeIn 0.5s ease-in-out;
}

.recoveryExercise-description {
  grid-column: 1 / 2;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.05);
  overflow-y: auto;
}

.subtitle-green {
  font-weight: 700;
  font-size: 1.5em;
  margin-bottom: 15px;
  color: #003e29;
  text-transform: uppercase;
}

.description-text {
  font-size: 1.1em;
  line-height: 1.8;
  color: black;
}

.image-and-exercises {
  grid-column: 2 / 3;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.recoveryExercise-image-container {
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 20px;
  background-color: #fafafa;
  border-radius: 8px;
  box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.05);
  max-height: 100%; 
}

.recoveryExercise-image {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain; 
  border-radius: 8px;
}

.recoveryExercise-injuries {
  flex: 1;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.05);
}

.injury-item {
  font-size: 1em;
  color: black; /* Deep green */
  margin-bottom: 10px;
  padding: 10px 15px;
  border: 1px solid #d1fae5;
  border-radius: 8px;
  background-color: #f0fdfa;
  transition: background-color 0.3s, transform 0.3s;
}

.recoveryExercise-injuries li:hover {
  background-color: #cce7e1;
  transform: scale(1.05);
}

@media screen and (max-width: 768px) {
  .pageContent {
    flex-direction: column;
    gap: 20px;
  }

  .current-recoveryExercise {
    grid-template-columns: 1fr;
  }

  .recoveryExercise-list {
    max-width: 100%;
  }
}
</style>