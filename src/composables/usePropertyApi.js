import { useAuth } from './useAuth'

export function usePropertyApi() {
  const { token } = useAuth()

  const apiBase = import.meta.env.VITE_API_BASE

  const getAuthHeaders = () => ({
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token.value}`
  })

  // Public endpoint - no auth required
  const getProperties = async () => {
    try {
      const response = await fetch(`${apiBase}/properties`, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
      })

      if (!response.ok) {
        console.error('Failed to fetch properties:', response.statusText)
        return []
      }

      return await response.json()
    } catch (error) {
      console.error('Error fetching properties:', error)
      return []
    }
  }

  // Admin endpoint - requires auth
  const getAdminProperties = async () => {
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
    // For admin views, use admin endpoint which has all properties
    try {
      const response = await fetch(`${apiBase}/admin/properties/${id}`, {
        method: 'GET',
        headers: getAuthHeaders()
      })

      if (!response.ok) {
        return null
      }

      return await response.json()
    } catch (error) {
      console.error('Error fetching property:', error)
      return null
    }
  }

  const getPublicPropertyById = async (id) => {
    // For public views, search the public properties array
    const properties = await getProperties()
    return properties.find(p => p.id === parseInt(id)) || null
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
    getAdminProperties,
    getPropertyById,
    getPublicPropertyById,
    createProperty,
    updateProperty,
    deleteProperty
  }
}
