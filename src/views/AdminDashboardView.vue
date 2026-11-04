<template>
  <div class="admin-dashboard">
    <div class="dashboard-header">
      <h1>Admin Dashboard</h1>
      <router-link to="/admin/create" class="btn-add-property">+ Add New Property</router-link>
    </div>

    <!-- Error Message -->
    <div v-if="errorMessage" class="error-message">
      {{ errorMessage }}
      <button @click="errorMessage = ''" class="btn-close">×</button>
    </div>

    <!-- Success Message -->
    <div v-if="successMessage" class="success-message">
      {{ successMessage }}
      <button @click="successMessage = ''" class="btn-close">×</button>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="loading-state">
      <p>Loading properties...</p>
    </div>

    <!-- Empty State -->
    <div v-else-if="properties.length === 0" class="empty-state">
      <p>No properties yet. <router-link to="/admin/create">Add one to get started</router-link></p>
    </div>

    <!-- Properties Table -->
    <div v-else class="table-container">
      <table class="properties-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Area (m²)</th>
            <th>Monthly Rent</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="property in properties" :key="property.id" class="property-row">
            <td>{{ property.id }}</td>
            <td class="name-cell">{{ property.name }}</td>
            <td>{{ property.area }}</td>
            <td>${{ property.monthlyRent }}</td>
            <td class="actions-cell">
              <router-link :to="`/admin/edit/${property.id}`" class="btn-edit">Edit</router-link>
              <button @click="confirmDelete(property)" class="btn-delete">Delete</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Refresh Button -->
    <div v-if="!isLoading && properties.length > 0" class="actions-footer">
      <button @click="loadProperties" class="btn-refresh">↻ Refresh</button>
    </div>

    <!-- Delete Confirmation Dialog -->
    <div v-if="deleteConfirm.show" class="modal-overlay" @click.self="cancelDelete">
      <div class="modal">
        <h2>Confirm Delete</h2>
        <p>Are you sure you want to delete <strong>{{ deleteConfirm.property?.name }}</strong>? This action cannot be undone.</p>
        <div class="modal-actions">
          <button @click="performDelete" class="btn-confirm-delete" :disabled="isDeleting">
            {{ isDeleting ? 'Deleting...' : 'Delete' }}
          </button>
          <button @click="cancelDelete" class="btn-cancel" :disabled="isDeleting">Cancel</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { usePropertyApi } from '@/composables/usePropertyApi'

const { getProperties, deleteProperty } = usePropertyApi()

const properties = ref([])
const isLoading = ref(true)
const isDeleting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const deleteConfirm = ref({
  show: false,
  property: null
})

onMounted(() => {
  loadProperties()
})

const loadProperties = async () => {
  isLoading.value = true
  errorMessage.value = ''

  try {
    properties.value = await getProperties()
  } catch (err) {
    errorMessage.value = `Failed to load properties: ${err.message}`
    properties.value = []
  } finally {
    isLoading.value = false
  }
}

const confirmDelete = (property) => {
  deleteConfirm.value = {
    show: true,
    property
  }
}

const cancelDelete = () => {
  deleteConfirm.value = {
    show: false,
    property: null
  }
}

const performDelete = async () => {
  if (!deleteConfirm.value.property) return

  isDeleting.value = true
  errorMessage.value = ''

  try {
    await deleteProperty(deleteConfirm.value.property.id)
    successMessage.value = `Property "${deleteConfirm.value.property.name}" deleted successfully`
    cancelDelete()
    await loadProperties()
  } catch (err) {
    errorMessage.value = `Failed to delete property: ${err.message}`
  } finally {
    isDeleting.value = false
  }
}
</script>

<style scoped>
.admin-dashboard {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  border-bottom: 2px solid #ddd;
  padding-bottom: 1rem;
}

.dashboard-header h1 {
  margin: 0;
  font-size: 1.8rem;
  color: #333;
}

.btn-add-property {
  padding: 0.75rem 1.5rem;
  background: #28a745;
  color: white;
  text-decoration: none;
  border-radius: 4px;
  font-weight: 600;
  transition: background 0.3s;
}

.btn-add-property:hover {
  background: #218838;
}

.error-message,
.success-message {
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.error-message {
  background: #f8d7da;
  border: 1px solid #f5c6cb;
  color: #721c24;
}

.success-message {
  background: #d4edda;
  border: 1px solid #c3e6cb;
  color: #155724;
}

.btn-close {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: inherit;
  padding: 0;
  margin-left: 1rem;
}

.loading-state,
.empty-state {
  text-align: center;
  padding: 3rem;
  background: #f9f9f9;
  border-radius: 4px;
  color: #666;
}

.empty-state a {
  color: #007bff;
  text-decoration: none;
  font-weight: 600;
}

.empty-state a:hover {
  text-decoration: underline;
}

.table-container {
  overflow-x: auto;
  border-radius: 4px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.properties-table {
  width: 100%;
  border-collapse: collapse;
  background: white;
}

.properties-table thead {
  background: #f5f5f5;
  font-weight: 600;
  border-bottom: 2px solid #ddd;
}

.properties-table th {
  padding: 1rem;
  text-align: left;
  color: #333;
}

.properties-table td {
  padding: 1rem;
  border-bottom: 1px solid #eee;
}

.properties-table tbody tr:hover {
  background: #f9f9f9;
}

.name-cell {
  font-weight: 500;
  color: #333;
  max-width: 300px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.actions-cell {
  display: flex;
  gap: 0.5rem;
}

.btn-edit,
.btn-delete {
  padding: 0.4rem 0.8rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  text-decoration: none;
  display: inline-block;
  font-size: 0.85rem;
  font-weight: 600;
  transition: background 0.3s;
}

.btn-edit {
  background: #007bff;
  color: white;
}

.btn-edit:hover {
  background: #0056b3;
}

.btn-delete {
  background: #dc3545;
  color: white;
}

.btn-delete:hover {
  background: #c82333;
}

.actions-footer {
  text-align: center;
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid #ddd;
}

.btn-refresh {
  padding: 0.5rem 1rem;
  background: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 600;
  transition: background 0.3s;
}

.btn-refresh:hover {
  background: #0056b3;
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal {
  background: white;
  border-radius: 8px;
  padding: 2rem;
  max-width: 400px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.15);
}

.modal h2 {
  margin: 0 0 1rem 0;
  color: #333;
}

.modal p {
  color: #666;
  margin-bottom: 1.5rem;
  line-height: 1.5;
}

.modal-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
}

.btn-confirm-delete {
  padding: 0.5rem 1rem;
  background: #dc3545;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 600;
}

.btn-confirm-delete:hover:not(:disabled) {
  background: #c82333;
}

.btn-confirm-delete:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-cancel {
  padding: 0.5rem 1rem;
  background: #6c757d;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 600;
}

.btn-cancel:hover:not(:disabled) {
  background: #5a6268;
}

.btn-cancel:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

@media (max-width: 768px) {
  .dashboard-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .properties-table {
    font-size: 0.9rem;
  }

  .properties-table th,
  .properties-table td {
    padding: 0.75rem 0.5rem;
  }

  .actions-cell {
    flex-direction: column;
    gap: 0.25rem;
  }

  .btn-edit,
  .btn-delete {
    width: 100%;
    text-align: center;
  }
}
</style>
