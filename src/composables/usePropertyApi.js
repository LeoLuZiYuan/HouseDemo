import { useAuth } from './useAuth'

export function usePropertyApi() {
  const { token } = useAuth()

  const apiBase = 'http://localhost:5000/api'

  const getAuthHeaders = () => ({
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token.value}`
  })

  const getProperties = async () => {
    const response = await fetch(`${apiBase}/admin/properties`, {
      method: 'GET',
      headers: getAuthHeaders()
    })

    if (!response.ok) {
      if (response.status === 401) throw new Error('Unauthorized')
      throw new Error(`Failed to fetch properties: ${response.statusText}`)
    }

    return await response.json()
  }

  const getPropertyById = async (id) => {
    const properties = await getProperties()
    return properties.find(p => p.id === id) || null
  }

  const createProperty = async (propertyData) => {
    const response = await fetch(`${apiBase}/admin/properties`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(propertyData)
    })

    if (!response.ok) {
      const error = await response.json()
      const errorMsg = error.errors?.join(', ') || error.message || 'Failed to create property'
      throw new Error(errorMsg)
    }

    return await response.json()
  }

  const updateProperty = async (id, propertyData) => {
    const response = await fetch(`${apiBase}/admin/properties/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(propertyData)
    })

    if (!response.ok) {
      const error = await response.json()
      const errorMsg = error.errors?.join(', ') || error.message || 'Failed to update property'
      throw new Error(errorMsg)
    }

    return await response.json()
  }

  const deleteProperty = async (id) => {
    const response = await fetch(`${apiBase}/admin/properties/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    })

    if (!response.ok) {
      const error = await response.json()
      const errorMsg = error.message || 'Failed to delete property'
      throw new Error(errorMsg)
    }

    // DELETE returns 204 No Content (no JSON response)
    return true
  }

  return {
    getProperties,
    getPropertyById,
    createProperty,
    updateProperty,
    deleteProperty
  }
}
