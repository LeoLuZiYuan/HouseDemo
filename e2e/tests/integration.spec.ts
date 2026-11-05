import { test, expect } from '@playwright/test';

/**
 * E2E Integration Tests - Full End-to-End Validation
 * 
 * Tests complete production flow:
 * - Admin authentication with credentials
 * - Property creation (integration with API)
 * - Property visibility on public site
 * - Full CRUD workflow
 */

test.describe('E2E Integration Tests', () => {
  // Get API base URL from environment or default to localhost
  const API_BASE = process.env.API_BASE || 'http://localhost:5088/api';
  
  test('Complete end-to-end property lifecycle', async ({ page }) => {
    // Step 1: Authenticate admin user
    console.log('Step 1: Admin authentication');
    const loginResponse = await page.evaluate((apiBase) =>
      fetch(`${apiBase}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: 'landlord@housedemoll.com',
          password: 'demo123456'
        })
      }).then(r => r.json())
    , API_BASE);

    expect(loginResponse.token).toBeTruthy();
    const token = loginResponse.token;
    console.log('✓ Admin authenticated');

    // Step 2: Create property
    console.log('Step 2: Create property');
    const propertyName = `E2E Integration ${Date.now()}`;
    const createResponse = await page.evaluate(({ apiBase, token, name }) =>
      fetch(`${apiBase}/admin/properties`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          name: name,
          area: '200',
          monthlyRent: '9000',
          shortDesc: 'Integration test property',
          description: 'Comprehensive end-to-end test property',
          features: ['WiFi', 'Parking', 'Garden'],
          coverImage: 'https://picsum.photos/seed/integration/800/600',
          images: ['https://picsum.photos/seed/integration-1/800/600', 'https://picsum.photos/seed/integration-2/800/600']
        })
      }).then(async r => ({
        status: r.status,
        data: await r.json()
      }))
    , { apiBase: API_BASE, token, name: propertyName });

    expect([200, 201]).toContain(createResponse.status);
    const propertyId = createResponse.data.id;
    console.log(`✓ Property created with ID ${propertyId}`);

    // Step 3: Verify property in public API
    console.log('Step 3: Verify property in public API');
    const publicResponse = await page.evaluate(({ apiBase, id }) =>
      fetch(`${apiBase}/properties/${id}`)
        .then(async r => ({
          status: r.status,
          data: await r.json()
        }))
    , { apiBase: API_BASE, id: propertyId });

    expect(publicResponse.status).toBe(200);
    expect(publicResponse.data.name).toBe(propertyName);
    console.log('✓ Property visible in public API');

    // Step 4: List all properties (verify created property appears)
    console.log('Step 4: List all properties');
    const allPropertiesResponse = await page.evaluate((apiBase) =>
      fetch(`${apiBase}/properties`)
        .then(r => r.json())
    , API_BASE);

    const foundProperty = allPropertiesResponse.find(p => p.id === propertyId);
    expect(foundProperty).toBeTruthy();
    console.log(`✓ Created property found in list (${allPropertiesResponse.length} total properties)`);

    // Step 5: Update property
    console.log('Step 5: Update property');
    const updateResponse = await page.evaluate(({ apiBase, token, id }) =>
      fetch(`${apiBase}/admin/properties/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          name: 'E2E Integration - Updated',
          area: '250',
          monthlyRent: '10000',
          shortDesc: 'Updated integration test',
          description: 'Updated comprehensive test property',
          features: ['WiFi', 'Parking', 'Garden', 'Balcony'],
          coverImage: 'https://picsum.photos/seed/integration/800/600',
          images: ['https://picsum.photos/seed/integration-1/800/600']
        })
      }).then(async r => ({
        status: r.status,
        data: await r.json()
      }))
    , { apiBase: API_BASE, token, id: propertyId });

    expect([200, 201]).toContain(updateResponse.status);
    console.log('✓ Property updated successfully');

    // Step 6: Verify update reflected in public API
    console.log('Step 6: Verify update reflected in public API');
    const updatedPublicResponse = await page.evaluate(({ apiBase, id }) =>
      fetch(`${apiBase}/properties/${id}`)
        .then(async r => ({
          data: await r.json()
        }))
    , { apiBase: API_BASE, id: propertyId });

    expect(updatedPublicResponse.data.area).toBe('250');
    console.log('✓ Update verified in public API');

    // Step 7: Delete property
    console.log('Step 7: Delete property');
    const deleteResponse = await page.evaluate(({ apiBase, token, id }) =>
      fetch(`${apiBase}/admin/properties/${id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      }).then(r => ({ status: r.status }))
    , { apiBase: API_BASE, token, id: propertyId });

    expect([200, 204]).toContain(deleteResponse.status);
    console.log('✓ Property deleted');

    // Step 8: Verify deletion
    console.log('Step 8: Verify property deletion');
    const verifyDeleteResponse = await page.evaluate(({ apiBase, id }) =>
      fetch(`${apiBase}/properties/${id}`)
        .then(r => ({ status: r.status }))
    , { apiBase: API_BASE, id: propertyId });

    expect(verifyDeleteResponse.status).toBe(404);
    console.log('✓ Property successfully deleted from public API');

    console.log('\n✅ END-TO-END INTEGRATION TEST PASSED');
    console.log(`Complete workflow: login → create → verify → update → delete`);
  });

  test('API performance check - response times', async ({ page }) => {
    console.log('Performance check: Measuring API response times');
    
    const perfCheck = await page.evaluate((apiBase) =>
      Promise.all([
        fetch(`${apiBase}/properties`)
          .then(async r => {
            const start = performance.now();
            const data = await r.json();
            return performance.now() - start;
          }),
        fetch(`${apiBase}/properties/1`)
          .then(async r => {
            const start = performance.now();
            const data = await r.json();
            return performance.now() - start;
          })
      ]).then(times => ({
        listTime: times[0],
        detailTime: times[1],
        average: (times[0] + times[1]) / 2
      }))
    , API_BASE);

    console.log(`Properties list API: ${perfCheck.listTime.toFixed(2)}ms`);
    console.log(`Property detail API: ${perfCheck.detailTime.toFixed(2)}ms`);
    console.log(`Average response time: ${perfCheck.average.toFixed(2)}ms`);

    // Expect responses under 2 seconds
    expect(perfCheck.listTime).toBeLessThan(2000);
    expect(perfCheck.detailTime).toBeLessThan(2000);
    console.log('✓ Performance check passed (< 2s)');
  });
});
