<template>
    <div class="home-page">
      <header class="header">
        <h1 class="title">Welcome to Our App</h1>
        <p class="subtitle">Create a Perfect Recovery Plan</p>
        <button @click="scrollToContent" class="cta-button">Explore Now</button>
      </header>

      <section class="section features">
        <h2 class="section-title">Our Features</h2>
        <div class="feature-cards">
          <div class="feature-card" v-for="feature in features" :key="feature.title" @click="navigateTo(feature.route)">
            <img :src="feature.icon" alt="" class="feature-icon" />
            <h3 class="feature-title">{{ feature.title }}</h3>
            <p class="feature-description">{{ feature.description }}</p>
          </div>
        </div>
      </section>
  
      <section class="parallax-section">
        <div class="parallax-content">
          <h2>Experience the Difference</h2>
          <p>Discover unparalleled possibilities to help you on recovering.</p>
        </div>
      </section>
  
      <footer class="footer">
        <p>© 2024 Our App. All rights reserved.</p>
      </footer>
    </div>
</template>

<script setup>
  import { ref } from 'vue';
  import { useRouter } from 'vue-router';

    const router = useRouter();

    const navigateTo = (route) => {
    if (route) {
        router.push(route);
    }
    };

    const features = ref([
    {
        title: "Create a recovery plan",
        description: "Make a personalized recovery plan to help yourself against injuries.",
        icon: "/img/pencil.png",
        route: "/landingpage"
    },
    {
        title: "View injuries and exercises",
        description: "Examine different injuries and their corresponfing exercises.",
        icon: "/img/document.png",
        route: "/choose"
    },
    {
        title: "Contact professionals",
        description: "Get personal help from our professionals to help you recover.",
        icon: "/img/user.png",
        route: null
    }
    ]);
  
  const scrollToContent = () => {
    const section = document.querySelector(".section");
    section.scrollIntoView({ behavior: "smooth" });
  };
</script>

<style scoped>
.home-page {
  font-family: Arial, sans-serif;
  color: #fff;
  overflow-x: hidden;
}

.header {
  height: 100vh;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  background-image: url("/img/homepagepic.webp");
  background-size: cover;
  background-position: middle;     
  background-attachment: fixed;
  text-align: center;
  padding: 20px;
  color: #ffffff;
}

.title {
  font-size: 3em;
  font-weight: bold;
  animation: fadeIn 2s ease-in-out;
}

.subtitle {
  font-size: 1.5em;
  margin: 0.5rem 0 2rem;
  animation: slideIn 2s ease-in-out;
}

.cta-button {
  padding: 10px 20px;
  font-size: 1.2em;
  background-color: #fff;
  color: #ff4e50;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  animation: bounce 2s infinite alternate;
}

.section {
  padding: 4rem 2rem;
  background-color: #282c34;
  color: #ffffff;
  text-align: center;
}

.section-title {
  font-size: 2.5em;
  margin-bottom: 2rem;
  color: #f9d423;
}

.feature-cards {
  display: flex;
  gap: 2rem;
  justify-content: center;
  flex-wrap: wrap;
}

.feature-card {
  background: rgba(255, 255, 255, 0.1);
  padding: 1.5rem;
  width: 250px;
  border-radius: 10px;
  text-align: center;
  transition: transform 0.3s ease;
}

.feature-card:hover {
  transform: scale(1.05);
}

.feature-icon {
  width: 64px;
  margin-bottom: 1rem;
}

.feature-title {
  font-size: 1.5em;
  color: #ff4e50;
}

.parallax-section {
  background-image: url('/img/homepagepic.webp');
  background-size: cover;
  background-position: bottom;     
  background-attachment: fixed;  
  height: 60vh;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #ff4e50;
  text-align: center;
  position: relative;
}

.parallax-content h2 {
  font-size: 2.5em;
  margin-bottom: 1rem;
}

.parallax-content p {
  font-size: 1.2em;
  max-width: 600px;
  margin: 0 auto;
  opacity: 0.9;
}

.footer {
  background-color: #282c34;
  padding: 1rem;
  text-align: center;
  color: #aaa;
}

@keyframes fadeIn {
  0% { opacity: 0; }
  100% { opacity: 1; }
}

@keyframes slideIn {
  0% { transform: translateY(20px); opacity: 0; }
  100% { transform: translateY(0); opacity: 1; }
}

@keyframes bounce {
  0% { transform: translateY(0); }
  100% { transform: translateY(-10px); }
}
</style>