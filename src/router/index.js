import { createRouter, createWebHashHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import RoomDetailView from '../views/RoomDetailView.vue'
import LoginView from '../views/LoginView.vue'
import AdminDashboardView from '../views/AdminDashboardView.vue'
import PropertyFormView from '../views/PropertyFormView.vue'

const routes = [
  {
    path: '/',
    name: 'home',
    component: HomeView,
  },
  {
    path: '/room/:id',
    name: 'room-detail',
    component: RoomDetailView,
  },
  {
    path: '/login',
    name: 'login',
    component: LoginView,
  },
  {
    path: '/admin',
    name: 'admin',
    component: AdminDashboardView,
    meta: { requiresAuth: true }
  },
  {
    path: '/admin/create',
    name: 'create-property',
    component: PropertyFormView,
    meta: { requiresAuth: true }
  },
  {
    path: '/admin/edit/:propertyId',
    name: 'edit-property',
    component: PropertyFormView,
    meta: { requiresAuth: true }
  }
]

const router = createRouter({
  history: createWebHashHistory(),
  routes,
  scrollBehavior() {
    return { top: 0 }
  },
})

// Auth guard
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('auth_token')
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth)

  if (requiresAuth && !token) {
    next('/login')
  } else if (to.path === '/login' && token) {
    next('/admin')
  } else {
    next()
  }
})

export default router
