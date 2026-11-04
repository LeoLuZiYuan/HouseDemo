import { test, expect } from '@playwright/test';

/**
 * E2E CRUD Property Tests
 * 
 * Tests complete property lifecycle:
 * - Create property (admin)
 * - Read property (public + admin)
 * - Delete property (admin)
 */

test.describe('Property CRUD Operations', () => {
  test('Public users can view properties list', async ({ page }) => {
    const propertiesResponse = await page.evaluate(() =>
      fetch('http://localhost:5088/api/properties')
        .then(r => r.json())
    );

    expect(Array.isArray(propertiesResponse)).toBe(true);
    expect(propertiesResponse.length).toBeGreaterThan(0);
  });

  test('Public users can read property details', async ({ page }) => {
    // First, get a property ID
    const properties = await page.evaluate(() =>
      fetch('http://localhost:5088/api/properties')
        .then(r => r.json())
    );

    expect(properties.length).toBeGreaterThan(0);
    const propertyId = properties[0].id;

    // Then fetch its details
    const detailResponse = await page.evaluate((id) =>
      fetch(`http://localhost:5088/api/properties/${id}`)
        .then(async r => ({ 
          status: r.status, 
          data: await r.json()
        }))
    , propertyId);

    expect(detailResponse.status).toBe(200);
    expect(detailResponse.data.name).toBeTruthy();
    expect(detailResponse.data.monthlyRent).toBeTruthy();
  });

  test('Admin can create a new property', async ({ page }) => {
    // Get auth token
    const authToken = await page.evaluate(() =>
      fetch('http://localhost:5088/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: 'landlord@housedemoll.com',
          password: 'demo123456'
        })
      }).then(r => r.json())
        .then(r => r.token)
    );

    const TEST_PROPERTY = {
      name: `E2E Created ${Date.now()}`,
      area: '150',
      monthlyRent: '8000',
      shortDesc: 'Created by E2E test',
      description: 'Created by automated E2E test',
      features: ['WiFi', 'Parking'],
      coverImage: 'https://picsum.photos/seed/e2e-create/800/600',
      images: ['https://picsum.photos/seed/e2e-create-1/800/600']
    };

    const createResponse = await page.evaluate(({ body, token }) =>
      fetch('http://localhost:5088/api/admin/properties', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(body)
      }).then(async r => ({
        status: r.status,
        data: await r.json()
      }))
    , { body: TEST_PROPERTY, token: authToken });

    expect([200, 201]).toContain(createResponse.status);
    expect(createResponse.data.id).toBeTruthy();
    expect(createResponse.data.name).toBe(TEST_PROPERTY.name);
  });

  test('Admin can delete a property', async ({ page }) => {
    // Get auth token
    const authToken = await page.evaluate(() =>
      fetch('http://localhost:5088/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: 'landlord@housedemoll.com',
          password: 'demo123456'
        })
      }).then(r => r.json())
        .then(r => r.token)
    );

    const TEST_PROPERTY = {
      name: `E2E Delete Test ${Date.now()}`,
      area: '150',
      monthlyRent: '8000',
      shortDesc: 'Delete test',
      description: 'Delete test property',
      features: ['WiFi'],
      coverImage: 'https://picsum.photos/seed/e2e-delete/800/600',
      images: ['https://picsum.photos/seed/e2e-delete-1/800/600']
    };

    // Create a property
    const createResponse = await page.evaluate(({ body, token }) =>
      fetch('http://localhost:5088/api/admin/properties', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(body)
      }).then(async r => ({
        data: await r.json()
      }))
    , { body: TEST_PROPERTY, token: authToken });

    const propertyId = createResponse.data.id;

    // Delete it
    const deleteResponse = await page.evaluate(({ id, token }) =>
      fetch(`http://localhost:5088/api/admin/properties/${id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      }).then(r => ({ status: r.status }))
    , { id: propertyId, token: authToken });

    // 204 No Content is also a successful delete
    expect([200, 204]).toContain(deleteResponse.status);

    // Verify it's deleted
    const verifyResponse = await page.evaluate((id) =>
      fetch(`http://localhost:5088/api/properties/${id}`)
        .then(r => ({ status: r.status }))
    , propertyId);

    expect(verifyResponse.status).toBe(404);
  });
});
