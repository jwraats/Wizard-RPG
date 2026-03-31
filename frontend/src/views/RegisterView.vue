<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()
const router = useRouter()

const username = ref('')
const email = ref('')
const password = ref('')
const referralCode = ref('')
const house = ref('')
const error = ref('')
const loading = ref(false)

async function handleRegister() {
  error.value = ''
  loading.value = true
  try {
    await auth.register({
      username: username.value,
      email: email.value,
      password: password.value,
      referralCode: referralCode.value || undefined,
      house: house.value || undefined,
    })
    router.push('/dashboard')
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'response' in e) {
      const axiosErr = e as { response?: { data?: { message?: string } } }
      error.value = axiosErr.response?.data?.message || 'Registration failed'
    } else {
      error.value = 'Registration failed'
    }
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="max-w-md mx-auto mt-16">
    <div class="bg-white rounded-xl shadow-lg p-8">
      <h1 class="text-3xl font-bold text-center mb-6 text-indigo-900">🧙 Create Account</h1>

      <div v-if="error" data-testid="register-error" class="bg-red-50 text-red-700 rounded-lg p-3 mb-4 text-sm">
        {{ error }}
      </div>

      <form data-testid="register-form" @submit.prevent="handleRegister">
        <div class="mb-4">
          <label class="block text-sm font-medium text-gray-700 mb-1">Username</label>
          <input
            v-model="username"
            type="text"
            required
            data-testid="username-input"
            class="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none"
            placeholder="MerlinTheGreat"
          />
        </div>

        <div class="mb-4">
          <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
          <input
            v-model="email"
            type="email"
            required
            data-testid="email-input"
            class="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none"
            placeholder="wizard@example.com"
          />
        </div>

        <div class="mb-4">
          <label class="block text-sm font-medium text-gray-700 mb-1">Password</label>
          <input
            v-model="password"
            type="password"
            required
            data-testid="password-input"
            class="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none"
            placeholder="••••••••"
          />
        </div>

        <div class="mb-6">
          <label class="block text-sm font-medium text-gray-700 mb-1">Referral Code (optional)</label>
          <input
            v-model="referralCode"
            type="text"
            data-testid="referral-input"
            class="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none"
            placeholder="ABC123"
          />
        </div>

        <div class="mb-4">
          <label class="block text-sm font-medium text-gray-700 mb-1">House (optional)</label>
          <select
            v-model="house"
            data-testid="house-select"
            class="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none"
          >
            <option value="">Choose later</option>
            <option value="Pyromancers">🔥 Pyromancers (Fire &amp; Courage)</option>
            <option value="Frostwardens">❄️ Frostwardens (Ice &amp; Wisdom)</option>
            <option value="Stormcallers">⚡ Stormcallers (Lightning &amp; Speed)</option>
            <option value="Earthshapers">🌿 Earthshapers (Earth &amp; Strength)</option>
          </select>
        </div>

        <button
          type="submit"
          :disabled="loading"
          data-testid="register-submit"
          class="w-full bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-2 rounded-lg transition disabled:opacity-50"
        >
          {{ loading ? 'Creating…' : 'Create Account' }}
        </button>
      </form>

      <p class="text-center text-sm text-gray-500 mt-4">
        Already have an account?
        <router-link to="/login" class="text-indigo-600 hover:underline">Sign In</router-link>
      </p>
    </div>
  </div>
</template>
