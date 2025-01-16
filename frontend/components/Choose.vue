<template>
    <div class="container-wrapper">
      <div class="container" @click="navigateTo('/injuries')">
        <p class="title">Injuries</p>
        <img src="/img/injury.png" alt="Injury Image" />
      </div>
  
      <div class="container" @click="navigateTo('/recovery-exercises')">
        <p class="title">Recovery Exercises</p>
        <img src="/img/exercise.png" alt="Exercise Image" />
      </div>
    </div>
  </template>
  
  <script setup>
  import { useRouter } from 'vue-router';
  
  const router = useRouter();
  
  const navigateTo = (path) => {
    router.push(path);
  };
  </script>
  
  <style scoped>
  .container-wrapper {
    display: flex;
    gap: 20px;
    justify-content: center;
    align-items: center;
    padding: 20px;
  }
  
  .container {
    padding: 20px;
    border: 2px solid #cccccc;
    width: 300px;
    height: 300px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: transform 0.2s;
    background-color: #f9f9f9;
    border-radius: 8px;
  }
  
  .container:hover {
    transform: scale(1.05);
    background-color: #e6e6e6;
  }
  
  .container img {
    width: 80px;
    height: auto;
    margin-top: 10px;
  }
  .title{
    font-weight: bold;
    font-size: 1.2em; 
  }
  </style>
  