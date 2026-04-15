<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { trailerScenes } from '../data/trailerScenes'

const currentIndex = ref(0)
const isTransitioning = ref(false)
const isPaused = ref(false)
const isCollapsed = ref(false)
let timer: ReturnType<typeof setInterval> | undefined

const scene = computed(() => trailerScenes[currentIndex.value])
const totalScenes = trailerScenes.length

function nextScene() {
  isTransitioning.value = true
  setTimeout(() => {
    currentIndex.value = (currentIndex.value + 1) % totalScenes
    isTransitioning.value = false
  }, 500)
}

function prevScene() {
  isTransitioning.value = true
  setTimeout(() => {
    currentIndex.value = (currentIndex.value - 1 + totalScenes) % totalScenes
    isTransitioning.value = false
  }, 500)
}

function goToScene(index: number) {
  if (index === currentIndex.value) return
  isTransitioning.value = true
  setTimeout(() => {
    currentIndex.value = index
    isTransitioning.value = false
  }, 500)
}

function startAutoPlay() {
  timer = setInterval(() => {
    if (!isPaused.value) nextScene()
  }, 6000)
}

function togglePause() {
  isPaused.value = !isPaused.value
}

onMounted(() => startAutoPlay())
onUnmounted(() => {
  if (timer) clearInterval(timer)
})
</script>

<template>
  <div data-testid="trailer-section" class="mb-8">
    <div class="flex items-center justify-between mb-3">
      <h2 class="text-lg font-semibold text-indigo-800">🎬 Game Trailer</h2>
      <button
        data-testid="trailer-collapse-toggle"
        class="text-xs text-gray-400 hover:text-indigo-600 transition"
        @click="isCollapsed = !isCollapsed"
      >
        {{ isCollapsed ? '▼ Show Trailer' : '▲ Hide Trailer' }}
      </button>
    </div>

    <div v-if="!isCollapsed" data-testid="trailer-player" class="relative overflow-hidden rounded-2xl shadow-lg">
      <!-- Background layer -->
      <div
        class="bg-gradient-to-br transition-all duration-700 ease-in-out"
        :class="[
          scene.bgGradient,
          isTransitioning ? 'opacity-0 scale-105' : 'opacity-100 scale-100',
        ]"
      >
        <!-- Decorative sparkles -->
        <div class="absolute inset-0 overflow-hidden pointer-events-none">
          <div class="absolute top-4 left-8 text-white/10 text-6xl animate-pulse">✦</div>
          <div class="absolute top-12 right-12 text-white/10 text-4xl animate-pulse" style="animation-delay: 1s">✧</div>
          <div class="absolute bottom-8 left-1/3 text-white/10 text-5xl animate-pulse" style="animation-delay: 2s">✦</div>
        </div>

        <!-- Content -->
        <div class="relative px-6 py-8 md:px-10 md:py-12 min-h-[260px] flex flex-col justify-between">
          <!-- Scene counter -->
          <div class="flex items-center justify-between mb-4">
            <span class="text-xs font-mono text-white/40 uppercase tracking-widest">
              Scene {{ currentIndex + 1 }} / {{ totalScenes }}
            </span>
            <span class="text-xs font-mono text-white/30">
              ~{{ scene.durationSeconds }}s prompt
            </span>
          </div>

          <!-- Title -->
          <div class="mb-4">
            <h3 class="text-2xl md:text-3xl font-bold text-white drop-shadow-lg">
              {{ scene.title }}
            </h3>
          </div>

          <!-- Video prompt -->
          <div class="mb-4 bg-black/30 rounded-lg p-4 backdrop-blur-sm border border-white/10">
            <p class="text-xs font-mono text-white/50 mb-1 uppercase tracking-wider">Text-to-Video Prompt</p>
            <p class="text-sm text-white/80 leading-relaxed italic">
              "{{ scene.prompt }}"
            </p>
          </div>

          <!-- Punchline subtitle -->
          <p data-testid="trailer-subtitle" class="text-base md:text-lg text-white/90 font-medium">
            {{ scene.subtitle }}
          </p>

          <!-- Controls -->
          <div class="flex items-center justify-between mt-6">
            <div class="flex items-center gap-2">
              <button
                data-testid="trailer-prev"
                class="w-8 h-8 rounded-full bg-white/10 hover:bg-white/20 text-white flex items-center justify-center transition"
                @click="prevScene"
              >
                ◀
              </button>
              <button
                data-testid="trailer-pause"
                class="w-8 h-8 rounded-full bg-white/10 hover:bg-white/20 text-white flex items-center justify-center transition"
                @click="togglePause"
              >
                {{ isPaused ? '▶' : '⏸' }}
              </button>
              <button
                data-testid="trailer-next"
                class="w-8 h-8 rounded-full bg-white/10 hover:bg-white/20 text-white flex items-center justify-center transition"
                @click="nextScene"
              >
                ▶
              </button>
            </div>

            <!-- Scene dots -->
            <div class="flex gap-1.5">
              <button
                v-for="(s, i) in trailerScenes"
                :key="s.id"
                class="w-2 h-2 rounded-full transition-all"
                :class="i === currentIndex ? 'bg-white scale-125' : 'bg-white/30 hover:bg-white/50'"
                @click="goToScene(i)"
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
