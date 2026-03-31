import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      redirect: '/dashboard',
    },
    {
      path: '/login',
      name: 'Login',
      component: () => import('../views/LoginView.vue'),
      meta: { guest: true },
    },
    {
      path: '/register',
      name: 'Register',
      component: () => import('../views/RegisterView.vue'),
      meta: { guest: true },
    },
    {
      path: '/dashboard',
      name: 'Dashboard',
      component: () => import('../views/DashboardView.vue'),
      meta: { auth: true },
    },
    {
      path: '/profile',
      name: 'Profile',
      component: () => import('../views/ProfileView.vue'),
      meta: { auth: true },
    },
    {
      path: '/battle',
      name: 'Battle',
      component: () => import('../views/BattleView.vue'),
      meta: { auth: true },
    },
    {
      path: '/battle/:id',
      name: 'BattleDetail',
      component: () => import('../views/BattleDetailView.vue'),
      meta: { auth: true },
      props: true,
    },
    {
      path: '/bank',
      name: 'Bank',
      component: () => import('../views/BankView.vue'),
      meta: { auth: true },
    },
    {
      path: '/broom',
      name: 'BroomGame',
      component: () => import('../views/BroomGameView.vue'),
      meta: { auth: true },
    },
    {
      path: '/fellowship',
      name: 'Fellowship',
      component: () => import('../views/FellowshipView.vue'),
      meta: { auth: true },
    },
    {
      path: '/leaderboard',
      name: 'Leaderboard',
      component: () => import('../views/LeaderboardView.vue'),
    },
    {
      path: '/items',
      name: 'Items',
      component: () => import('../views/ItemsView.vue'),
    },
    {
      path: '/admin',
      name: 'Admin',
      component: () => import('../views/AdminView.vue'),
      meta: { auth: true, admin: true },
    },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (to.meta.auth && !auth.isLoggedIn) {
    return { name: 'Login', query: { redirect: to.fullPath } }
  }
  if (to.meta.admin && !auth.isAdmin) {
    return { name: 'Dashboard' }
  }
  if (to.meta.guest && auth.isLoggedIn) {
    return { name: 'Dashboard' }
  }
})

export default router
