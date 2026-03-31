<script setup lang="ts">
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()

function logout() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <nav class="bg-indigo-900 text-white shadow-lg" data-testid="navbar">
    <div class="max-w-7xl mx-auto px-4 flex items-center justify-between h-16">
      <router-link to="/dashboard" class="text-xl font-bold tracking-wide flex items-center gap-2">
        🧙 Wizard RPG
      </router-link>

      <div v-if="auth.isLoggedIn" class="flex items-center gap-4 text-sm">
        <router-link to="/dashboard" class="hover:text-indigo-300 transition">Dashboard</router-link>
        <router-link to="/battle" class="hover:text-indigo-300 transition">Battle</router-link>
        <router-link to="/bank" class="hover:text-indigo-300 transition">Bank</router-link>
        <router-link to="/broom" class="hover:text-indigo-300 transition">Broom Racing</router-link>
        <router-link to="/fellowship" class="hover:text-indigo-300 transition">Fellowship</router-link>
        <router-link to="/leaderboard" class="hover:text-indigo-300 transition">Leaderboard</router-link>
        <router-link to="/items" class="hover:text-indigo-300 transition">Items</router-link>
        <router-link v-if="auth.isAdmin" to="/admin" class="hover:text-yellow-300 transition font-semibold">
          ⚡ Admin
        </router-link>

        <div class="border-l border-indigo-700 pl-4 flex items-center gap-3">
          <router-link to="/profile" class="hover:text-indigo-300 transition">
            {{ auth.player?.username }}
          </router-link>
          <button
            data-testid="logout-btn"
            class="bg-indigo-700 hover:bg-indigo-600 px-3 py-1 rounded text-sm transition"
            @click="logout"
          >
            Logout
          </button>
        </div>
      </div>

      <div v-else class="flex items-center gap-3 text-sm">
        <router-link to="/login" class="hover:text-indigo-300 transition">Login</router-link>
        <router-link
          to="/register"
          class="bg-indigo-600 hover:bg-indigo-500 px-3 py-1 rounded transition"
        >
          Register
        </router-link>
      </div>
    </div>
  </nav>
</template>
