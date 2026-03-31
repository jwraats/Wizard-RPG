<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { usePlayerStore } from '../stores/player'

const playerStore = usePlayerStore()
const editing = ref(false)
const editUsername = ref('')
const editEmail = ref('')
const saveError = ref('')

onMounted(async () => {
  await playerStore.fetchProfile()
  if (playerStore.profile) {
    editUsername.value = playerStore.profile.username
    editEmail.value = playerStore.profile.email
  }
})

function startEdit() {
  if (playerStore.profile) {
    editUsername.value = playerStore.profile.username
    editEmail.value = playerStore.profile.email
  }
  editing.value = true
}

async function saveProfile() {
  saveError.value = ''
  try {
    await playerStore.updateProfile(editUsername.value, editEmail.value)
    editing.value = false
  } catch {
    saveError.value = 'Failed to update profile'
  }
}
</script>

<template>
  <div data-testid="profile-view">
    <h1 class="text-3xl font-bold text-indigo-900 mb-6">👤 Profile</h1>

    <div v-if="playerStore.loading" class="text-center py-12 text-gray-500">Loading…</div>

    <div v-else-if="playerStore.profile" class="max-w-2xl">
      <div class="bg-white rounded-xl shadow p-6">
        <div v-if="!editing">
          <div class="grid grid-cols-2 gap-4 text-sm">
            <div>
              <span class="text-gray-500">Username</span>
              <p class="font-semibold" data-testid="profile-username">{{ playerStore.profile.username }}</p>
            </div>
            <div>
              <span class="text-gray-500">Email</span>
              <p class="font-semibold">{{ playerStore.profile.email }}</p>
            </div>
            <div>
              <span class="text-gray-500">Level</span>
              <p class="font-semibold">{{ playerStore.profile.level }}</p>
            </div>
            <div>
              <span class="text-gray-500">Experience</span>
              <p class="font-semibold">{{ playerStore.profile.experience }} XP</p>
            </div>
            <div>
              <span class="text-gray-500">Gold</span>
              <p class="font-semibold text-yellow-600">🪙 {{ playerStore.profile.goldCoins }}</p>
            </div>
            <div>
              <span class="text-gray-500">Referral Code</span>
              <p class="font-mono text-sm bg-gray-100 px-2 py-0.5 rounded inline-block">
                {{ playerStore.profile.referralCode }}
              </p>
            </div>
            <div>
              <span class="text-gray-500">Magic Power</span>
              <p class="font-semibold text-purple-600">{{ playerStore.profile.magicPower }}</p>
            </div>
            <div>
              <span class="text-gray-500">Strength</span>
              <p class="font-semibold text-red-600">{{ playerStore.profile.strength }}</p>
            </div>
            <div>
              <span class="text-gray-500">Wisdom</span>
              <p class="font-semibold text-blue-600">{{ playerStore.profile.wisdom }}</p>
            </div>
            <div>
              <span class="text-gray-500">Speed</span>
              <p class="font-semibold text-green-600">{{ playerStore.profile.speed }}</p>
            </div>
            <div>
              <span class="text-gray-500">Joined</span>
              <p class="font-semibold">{{ new Date(playerStore.profile.createdAt).toLocaleDateString() }}</p>
            </div>
          </div>
          <button
            data-testid="edit-profile-btn"
            class="mt-6 bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg transition text-sm"
            @click="startEdit"
          >
            Edit Profile
          </button>
        </div>

        <div v-else>
          <div v-if="saveError" class="bg-red-50 text-red-700 rounded-lg p-3 mb-4 text-sm">{{ saveError }}</div>
          <form @submit.prevent="saveProfile" class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Username</label>
              <input
                v-model="editUsername"
                type="text"
                data-testid="edit-username"
                class="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 outline-none"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
              <input
                v-model="editEmail"
                type="email"
                data-testid="edit-email"
                class="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-500 outline-none"
              />
            </div>
            <div class="flex gap-3">
              <button
                type="submit"
                data-testid="save-profile-btn"
                class="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg transition text-sm"
              >
                Save
              </button>
              <button
                type="button"
                class="bg-gray-200 hover:bg-gray-300 text-gray-700 px-4 py-2 rounded-lg transition text-sm"
                @click="editing = false"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>
