import { test, expect } from '@playwright/test';

/**
 * E2E Authentication Flow Tests
 * 
 * Tests core authentication and authorization workflows:
 * - User login with valid credentials
 * - Invalid login rejection
 * - Protected route access
 */

test.describe('Authentication Flow', () => {
  test('User can login with valid credentials', async ({ page }) => {
    // This test validates the authentication endpoint works correctly
    // Full UI testing via API since the SPA requires special routing setup
    
    const loginResponse = await page.evaluate(() =>
      fetch('http://localhost:5088/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: 'landlord@housedemoll.com',
          password: 'demo123456'
        })
      }).then(r => r.json())
    );

    expect(loginResponse.token).toBeTruthy();
    expect(loginResponse.token).toMatch(/^[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+$/);
  });

  test('User receives error for invalid credentials', async ({ page }) => {
    const loginResponse = await page.evaluate(() =>
      fetch('http://localhost:5088/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: 'landlord@housedemoll.com',
          password: 'wrongpassword'
        })
      }).then(r => ({ status: r.status, data: r.json() }))
    );

    await expect(loginResponse.status).toBe(401);
  });

  test('Protected admin endpoints require authentication', async ({ page }) => {
    const protectedResponse = await page.evaluate(() =>
      fetch('http://localhost:5088/api/admin/properties', {
        headers: { 'Authorization': 'Bearer invalid_token' }
      }).then(r => ({ status: r.status }))
    );

    expect(protectedResponse.status).toBe(401);
  });
});
