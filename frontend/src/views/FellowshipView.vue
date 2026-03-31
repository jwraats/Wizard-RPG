<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useFellowshipStore } from '../stores/fellowship'
import { useAuthStore } from '../stores/auth'

const fellowshipStore = useFellowshipStore()
const auth = useAuthStore()

const newName = ref('')
const joinCode = ref('')
const showCreate = ref(false)
const showJoin = ref(false)
const actionError = ref('')

onMounted(async () => {
  await Promise.all([fellowshipStore.fetchMine(), fellowshipStore.fetchAll()])
})

async function createFellowship() {
  actionError.value = ''
  if (!newName.value.trim()) return
  try {
    await fellowshipStore.create(newName.value.trim())
    showCreate.value = false
    newName.value = ''
  } catch {
    actionError.value = fellowshipStore.error
  }
}

async function joinFellowship() {
  actionError.value = ''
  if (!joinCode.value.trim()) return
  try {
    await fellowshipStore.join(joinCode.value.trim())
    showJoin.value = false
    joinCode.value = ''
  } catch {
    actionError.value = fellowshipStore.error
  }
}

async function leaveFellowship() {
  if (!fellowshipStore.myFellowship) return
  try {
    await fellowshipStore.leave(fellowshipStore.myFellowship.id)
  } catch {
    actionError.value = fellowshipStore.error
  }
}
</script>

<template>
  <div data-testid="fellowship-view">
    <h1 class="text-3xl font-bold text-indigo-900 mb-6">🤝 Fellowships</h1>

    <div v-if="actionError" class="bg-red-50 text-red-700 rounded-lg p-3 mb-4 text-sm">{{ actionError }}</div>

    <!-- My Fellowship -->
    <div class="bg-white rounded-xl shadow p-6 mb-6">
      <h2 class="text-lg font-semibold text-indigo-800 mb-4">My Fellowship</h2>

      <div v-if="fellowshipStore.myFellowship" data-testid="my-fellowship">
        <div class="flex items-center justify-between mb-4">
          <div>
            <h3 class="text-xl font-bold">{{ fellowshipStore.myFellowship.name }}</h3>
            <p class="text-sm text-gray-500">
              Owner: {{ fellowshipStore.myFellowship.ownerUsername }} |
              Gold/hr: 🪙{{ fellowshipStore.myFellowship.goldPerHour }}
            </p>
            <p class="text-xs text-gray-400 mt-1">
              Referral Code: <span class="font-mono bg-gray-100 px-1.5 py-0.5 rounded">{{ fellowshipStore.myFellowship.referralCode }}</span>
            </p>
          </div>
          <button
            v-if="fellowshipStore.myFellowship.ownerId !== auth.player?.id"
            class="bg-red-100 text-red-700 hover:bg-red-200 px-3 py-1.5 rounded-lg text-sm transition"
            data-testid="leave-fellowship-btn"
            @click="leaveFellowship"
          >
            Leave
          </button>
        </div>

        <h4 class="text-sm font-semibold text-gray-700 mb-2">Members ({{ fellowshipStore.myFellowship.members.length }})</h4>
        <div class="space-y-2">
          <div
            v-for="member in fellowshipStore.myFellowship.members"
            :key="member.id"
            class="flex items-center justify-between border rounded-lg p-3 text-sm"
          >
            <div>
              <span class="font-semibold">{{ member.username }}</span>
              <span class="text-xs text-gray-500 ml-2">Joined {{ new Date(member.joinedAt).toLocaleDateString() }}</span>
            </div>
            <span class="text-xs bg-indigo-100 text-indigo-700 px-2 py-0.5 rounded">
              {{ member.contributionPercent }}% contribution
            </span>
          </div>
        </div>
      </div>

      <div v-else class="text-center py-4">
        <p class="text-gray-400 mb-4">You're not in a fellowship yet</p>
        <div class="flex justify-center gap-3">
          <button
            data-testid="create-fellowship-btn"
            class="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg text-sm transition"
            @click="showCreate = true"
          >
            Create Fellowship
          </button>
          <button
            data-testid="join-fellowship-btn"
            class="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-lg text-sm transition"
            @click="showJoin = true"
          >
            Join Fellowship
          </button>
        </div>
      </div>
    </div>

    <!-- Create Form -->
    <div v-if="showCreate" class="bg-white rounded-xl shadow p-6 mb-6">
      <h2 class="text-lg font-semibold mb-3">Create a Fellowship</h2>
      <form @submit.prevent="createFellowship" class="flex gap-2">
        <input
          v-model="newName"
          type="text"
          placeholder="Fellowship name"
          data-testid="fellowship-name-input"
          class="flex-1 border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-500 outline-none"
        />
        <button type="submit" class="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg text-sm transition">
          Create
        </button>
        <button type="button" class="bg-gray-200 hover:bg-gray-300 px-4 py-2 rounded-lg text-sm transition" @click="showCreate = false">
          Cancel
        </button>
      </form>
    </div>

    <!-- Join Form -->
    <div v-if="showJoin" class="bg-white rounded-xl shadow p-6 mb-6">
      <h2 class="text-lg font-semibold mb-3">Join a Fellowship</h2>
      <form @submit.prevent="joinFellowship" class="flex gap-2">
        <input
          v-model="joinCode"
          type="text"
          placeholder="Referral code"
          data-testid="fellowship-code-input"
          class="flex-1 border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-green-500 outline-none"
        />
        <button type="submit" class="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-lg text-sm transition">
          Join
        </button>
        <button type="button" class="bg-gray-200 hover:bg-gray-300 px-4 py-2 rounded-lg text-sm transition" @click="showJoin = false">
          Cancel
        </button>
      </form>
    </div>

    <!-- All Fellowships -->
    <div class="bg-white rounded-xl shadow p-6">
      <h2 class="text-lg font-semibold text-indigo-800 mb-4">All Fellowships</h2>
      <div v-if="fellowshipStore.fellowships.length" class="space-y-3">
        <div
          v-for="f in fellowshipStore.fellowships"
          :key="f.id"
          class="border rounded-lg p-4"
        >
          <div class="flex justify-between items-center">
            <div>
              <span class="font-semibold">{{ f.name }}</span>
              <span class="text-xs text-gray-500 ml-2">by {{ f.ownerUsername }}</span>
            </div>
            <span class="text-xs bg-gray-100 px-2 py-0.5 rounded">{{ f.members.length }} members</span>
          </div>
        </div>
      </div>
      <div v-else class="text-gray-400 text-sm">No fellowships exist yet</div>
    </div>
  </div>
</template>
