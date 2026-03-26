import type { DatabaseHealth } from '../types';

function resolveHealthUrl(): string {
  const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL?.trim();

  if (!configuredBaseUrl || configuredBaseUrl === '/api') {
    return '/health/db';
  }

  if (configuredBaseUrl.startsWith('http://') || configuredBaseUrl.startsWith('https://')) {
    return configuredBaseUrl.replace(/\/api\/?$/i, '') + '/health/db';
  }

  return '/health/db';
}

export const systemHealthService = {
  async getDatabaseHealth(signal?: AbortSignal): Promise<DatabaseHealth> {
    const response = await fetch(resolveHealthUrl(), {
      method: 'GET',
      headers: {
        Accept: 'application/json',
      },
      signal,
    });

    if (!response.ok) {
      throw new Error(`Health check failed: HTTP ${response.status}`);
    }

    const data = (await response.json()) as DatabaseHealth;
    return data;
  },
};
