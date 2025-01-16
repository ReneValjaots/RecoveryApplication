<template>
  <div>
    <h1 class="main-title">{{ selectedInjury ? selectedInjury.name : title }}</h1>
    <div class="pageContent">
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
                <UButton color="emerald" @click="selectInjury(injury)">
                  View Details
                </UButton>
              </li>
            </ul>
          </li>
        </ul>
      </div>

      <div v-if="selectedInjury" class="current-injury">
        <!-- Description -->
        <div class="injury-description">
          <h3 class="subtitle">Description</h3>
          <p class="description-text">{{ selectedInjury.description }}</p>
        </div>

        <div class="image-and-exercises">
          <div
            v-if="selectedInjury.recoveryExercises && selectedInjury.recoveryExercises.length > 0"
            class="injury-exercises">
            <h3 class="subtitle-red">Recovery Exercises</h3>
            <ul class="exercise-list">
              <li
                v-for="exercise in selectedInjury.recoveryExercises"
                :key="exercise.id"
                class="exercise-item"
                @click="navigateToExercise(exercise.id)">
                {{ exercise.name }}
              </li>
            </ul>

          </div>
              <!-- Image -->
              <div v-if="imageExists" class="injury-image-container">
            <img :src="imageSrc" alt="Injury Image" class="injury-image" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useApi } from '~/composables/useApi';
import type { Injury } from '~/types/injury';
import { useRouter, useRoute } from 'vue-router';

const router = useRouter();

defineProps<{ title: string }>();

const { customFetch } = useApi();
const searchQuery = ref('');
const route = useRoute();
const injuries = ref<Injury[]>([]);
const expandedBodyParts = ref<string[]>([]);
const selectedInjury = ref<Injury | null>(null);
const imageExists = ref(false);
const imageSrc = ref('');
const error = ref('');

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

const toggleBodyPart = (bodyPart: string) => {
  const index = expandedBodyParts.value.indexOf(bodyPart);
  if (index > -1) {
    expandedBodyParts.value.splice(index, 1);
  } else {
    expandedBodyParts.value.push(bodyPart);
  }
};

const navigateToExercise = (exerciseId: number) => {
  router.push({
    path: '/recovery-exercises',
    query: { exerciseId },
  });
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

    const injuryId = Number(route.query.injuryId);
    if (injuryId && injuries.value.length > 0) {
      const foundInjury = injuries.value.find(
        (injury) => injury.id === injuryId
      );

      selectedInjury.value = foundInjury || injuries.value[0];
    } else if (injuries.value.length > 0) {
      selectedInjury.value = injuries.value[0];
    }

    checkImage();
  } catch (err) {
    error.value = 'Failed to fetch injuries: ' + (err as Error).message;
  }
};

const selectInjury = (injury: Injury) => {
  selectedInjury.value = injury;
  checkImage();
};

const checkImage = () => {
  if (selectedInjury.value) {
    const src = getImageSrc(selectedInjury.value.name);
    const img = new Image();
    img.src = src;

    img.onload = () => {
      imageExists.value = true;
      imageSrc.value = src;
    };

    img.onerror = () => {
      imageExists.value = false;
    };
  }
};

const getImageSrc = (name: string) => {
  const formattedName = name.toLowerCase().replace(/ /g, '');
  return `/img/${formattedName}.jpg`;
};

onMounted(() => {
  fetchInjuries();
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

.injury-list {
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

.injury-list-ul {
  max-height: 100%;
  overflow-y: auto;
  flex: 1;
}

.injury-item {
  margin-bottom: 15px;
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

.injury-item > div:hover {
  background-color: #e0e7ff;
  box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.1);
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

.injury-item ul li:hover {
  transform: scale(1.002);
  box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.1);
}

.current-injury {
  flex: 3;
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  grid-gap: 20px;
  padding: 20px;
  background-color: #ffffff;
  border-radius: 12px;
  box-shadow: 0px 8px 16px rgba(0, 0, 0, 0.1);
  overflow: auto;
  animation: fadeIn 0.5s ease-in-out;
}

.injury-description {
  grid-column: 1 / 2;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.05);
  overflow-y: auto;
}

.subtitle {
  font-weight: 700;
  font-size: 1.5em;
  margin-bottom: 15px;
  color: #003e29;
  text-transform: uppercase;
}

.subtitle-red {
  font-weight: 700;
  font-size: 1.4em;
  margin-bottom: 10px;
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

.injury-image-container {
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 20px;
  background-color: #fafafa;
  border-radius: 8px;
  box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.05);
  max-height: 500px;
}

.injury-image {
  max-height: 100%;
  max-width: 100%;
  border-radius: 8px;
  object-fit: contain;
}

.injury-exercises {
  flex: 1;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.05);
}

.exercise-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.exercise-item {
  font-size: 1em;
  color: black; /* Deep green */
  margin-bottom: 10px;
  padding: 10px 15px;
  border: 1px solid #d1fae5;
  border-radius: 8px;
  background-color: #f0fdfa;
  transition: background-color 0.3s, transform 0.3s;
}

.exercise-item:hover {
  background-color: #cce7e1;
  transform: scale(1.05);
}

@media screen and (max-width: 768px) {
  .pageContent {
    flex-direction: column;
    gap: 20px;
  }

  .current-injury {
    grid-template-columns: 1fr;
  }

  .injury-list {
    max-width: 100%;
  }
}
</style>