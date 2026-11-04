<template>
  <div class="property-form-container">
    <div class="form-header">
      <h1>{{ mode === 'edit' ? 'Edit Property' : 'Create New Property' }}</h1>
      <router-link to="/admin" class="btn-back">← Back to Dashboard</router-link>
    </div>

    <form @submit.prevent="handleSubmit" class="property-form">
      <!-- Error Messages -->
      <div v-if="errorMessage" class="error-message">
        {{ errorMessage }}
      </div>

      <div v-if="validationErrors.length > 0" class="error-list">
        <ul>
          <li v-for="(err, idx) in validationErrors" :key="idx">{{ err }}</li>
        </ul>
      </div>

      <!-- Success Message -->
      <div v-if="successMessage" class="success-message">
        {{ successMessage }}
      </div>

      <!-- Form Fields -->
      <div class="form-group">
        <label for="name">Property Name *</label>
        <input
          id="name"
          v-model="form.name"
          type="text"
          placeholder="e.g., Cozy Studio in Downtown"
          required
        />
      </div>

      <div class="form-row">
        <div class="form-group">
          <label for="area">Area (m²) *</label>
          <input
            id="area"
            v-model="form.area"
            type="number"
            placeholder="e.g., 45"
            step="0.01"
            required
          />
        </div>

        <div class="form-group">
          <label for="monthlyRent">Monthly Rent *</label>
          <input
            id="monthlyRent"
            v-model="form.monthlyRent"
            type="number"
            placeholder="e.g., 1200"
            step="0.01"
            required
          />
        </div>
      </div>

      <div class="form-group">
        <label for="shortDesc">Short Description *</label>
        <textarea
          id="shortDesc"
          v-model="form.shortDesc"
          placeholder="Brief description (one or two sentences)"
          rows="2"
          required
        ></textarea>
      </div>

      <div class="form-group">
        <label for="description">Full Description *</label>
        <textarea
          id="description"
          v-model="form.description"
          placeholder="Detailed description of the property, amenities, etc."
          rows="5"
          required
        ></textarea>
      </div>

      <div class="form-group">
        <label for="features">Features (comma-separated)</label>
        <textarea
          id="features"
          v-model="form.features"
          placeholder="e.g., WiFi, Parking, Air Conditioning, Gym"
          rows="3"
        ></textarea>
        <small>Enter features separated by commas</small>
      </div>

      <div class="form-group">
        <label for="coverImage">Cover Image URL *</label>
        <input
          id="coverImage"
          v-model="form.coverImage"
          type="url"
          placeholder="https://example.com/image.jpg"
          required
        />
      </div>

      <div class="form-group">
        <label for="images">Additional Image URLs (comma-separated)</label>
        <textarea
          id="images"
          v-model="form.images"
          placeholder="https://example.com/image1.jpg, https://example.com/image2.jpg"
          rows="3"
        ></textarea>
        <small>Enter image URLs separated by commas</small>
      </div>

      <!-- Submit Button -->
      <div class="form-actions">
        <button type="submit" class="btn-submit" :disabled="isLoading">
          {{ isLoading ? 'Submitting...' : (mode === 'edit' ? 'Update Property' : 'Create Property') }}
        </button>
        <router-link to="/admin" class="btn-cancel">Cancel</router-link>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { usePropertyApi } from '@/composables/usePropertyApi'

const router = useRouter()
const route = useRoute()
const { createProperty, updateProperty, getPropertyById } = usePropertyApi()

const mode = computed(() => route.params.propertyId ? 'edit' : 'create')
const propertyId = computed(() => route.params.propertyId ? parseInt(route.params.propertyId) : null)

const form = ref({
  name: '',
  area: '',
  monthlyRent: '',
  shortDesc: '',
  description: '',
  features: '',
  coverImage: '',
  images: ''
})

const isLoading = ref(false)
const errorMessage = ref('')
const successMessage = ref('')
const validationErrors = ref([])

onMounted(async () => {
  // If in edit mode, load the property data
  if (mode.value === 'edit' && propertyId.value) {
    try {
      const property = await getPropertyById(propertyId.value)
      if (property) {
        form.value = {
          name: property.name || '',
          area: property.area || '',
          monthlyRent: property.monthlyRent || '',
          shortDesc: property.shortDesc || '',
          description: property.description || '',
          features: property.features?.join(', ') || '',
          coverImage: property.coverImage || '',
          images: property.images?.join(', ') || ''
        }
      } else {
        errorMessage.value = 'Property not found'
      }
    } catch (err) {
      errorMessage.value = `Failed to load property: ${err.message}`
    }
  }
})

const validateForm = () => {
  validationErrors.value = []

  if (!form.value.name?.trim()) {
    validationErrors.value.push('Property name is required')
  }

  if (!form.value.area || isNaN(form.value.area) || parseFloat(form.value.area) <= 0) {
    validationErrors.value.push('Area must be a positive number')
  }

  if (!form.value.monthlyRent || isNaN(form.value.monthlyRent) || parseFloat(form.value.monthlyRent) <= 0) {
    validationErrors.value.push('Monthly rent must be a positive number')
  }

  if (!form.value.shortDesc?.trim()) {
    validationErrors.value.push('Short description is required')
  }

  if (!form.value.description?.trim()) {
    validationErrors.value.push('Full description is required')
  }

  if (!form.value.coverImage?.trim()) {
    validationErrors.value.push('Cover image URL is required')
  } else if (!isValidUrl(form.value.coverImage)) {
    validationErrors.value.push('Cover image must be a valid URL')
  }

  if (form.value.images?.trim()) {
    const urls = form.value.images.split(',').map(u => u.trim()).filter(u => u)
    for (const url of urls) {
      if (!isValidUrl(url)) {
        validationErrors.value.push(`Invalid image URL: ${url}`)
        break
      }
    }
  }

  return validationErrors.value.length === 0
}

const isValidUrl = (url) => {
  try {
    new URL(url)
    return true
  } catch {
    return false
  }
}

const handleSubmit = async () => {
  errorMessage.value = ''
  successMessage.value = ''

  if (!validateForm()) {
    return
  }

  isLoading.value = true

  try {
    if (mode.value === 'create') {
      await createProperty(form.value)
      successMessage.value = 'Property created successfully!'
      setTimeout(() => {
        router.push('/admin')
      }, 1500)
    } else {
      await updateProperty(propertyId.value, form.value)
      successMessage.value = 'Property updated successfully!'
      setTimeout(() => {
        router.push('/admin')
      }, 1500)
    }
  } catch (err) {
    errorMessage.value = err.message || 'Failed to save property'
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
.property-form-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
}

.form-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  border-bottom: 2px solid #ddd;
  padding-bottom: 1rem;
}

.form-header h1 {
  margin: 0;
  font-size: 1.8rem;
  color: #333;
}

.btn-back {
  padding: 0.5rem 1rem;
  background: #f0f0f0;
  border: 1px solid #ccc;
  border-radius: 4px;
  text-decoration: none;
  color: #333;
  cursor: pointer;
  transition: background 0.3s;
}

.btn-back:hover {
  background: #e0e0e0;
}

.property-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group label {
  font-weight: 600;
  color: #333;
  font-size: 0.95rem;
}

.form-group input,
.form-group textarea {
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  font-family: inherit;
}

.form-group input:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #007bff;
  box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
}

.form-group small {
  color: #666;
  font-size: 0.85rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

.error-message,
.error-list {
  padding: 1rem;
  background: #f8d7da;
  border: 1px solid #f5c6cb;
  border-radius: 4px;
  color: #721c24;
  margin-bottom: 1rem;
}

.error-list ul {
  margin: 0;
  padding-left: 1.5rem;
}

.error-list li {
  margin: 0.25rem 0;
}

.success-message {
  padding: 1rem;
  background: #d4edda;
  border: 1px solid #c3e6cb;
  border-radius: 4px;
  color: #155724;
  margin-bottom: 1rem;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1rem;
}

.btn-submit,
.btn-cancel {
  padding: 0.75rem 1.5rem;
  font-size: 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  text-decoration: none;
  display: inline-block;
  text-align: center;
  transition: background 0.3s;
  font-weight: 600;
}

.btn-submit {
  background: #007bff;
  color: white;
}

.btn-submit:hover:not(:disabled) {
  background: #0056b3;
}

.btn-submit:disabled {
  background: #6c757d;
  cursor: not-allowed;
  opacity: 0.6;
}

.btn-cancel {
  background: #6c757d;
  color: white;
}

.btn-cancel:hover {
  background: #5a6268;
}

@media (max-width: 600px) {
  .form-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .form-row {
    grid-template-columns: 1fr;
  }

  .form-actions {
    flex-direction: column;
  }

  .btn-submit,
  .btn-cancel {
    width: 100%;
  }
}
</style>
