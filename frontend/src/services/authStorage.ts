import type { User } from '../types';

const TOKEN_KEY = 'cpms_token';
const USER_KEY = 'cpms_user';

export const authStorage = {
  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  },
  setToken(token: string): void {
    localStorage.setItem(TOKEN_KEY, token);
  },
  clearToken(): void {
    localStorage.removeItem(TOKEN_KEY);
  },
  getUser(): User | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) {
      return null;
    }

    try {
      const parsed = JSON.parse(raw) as Partial<User>;
      if (!parsed || !parsed.id || !parsed.username) {
        return null;
      }

      return {
        ...parsed,
        permissionCodes: Array.isArray(parsed.permissionCodes) ? parsed.permissionCodes : [],
      } as User;
    } catch {
      return null;
    }
  },
  setUser(user: User): void {
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  },
  clearUser(): void {
    localStorage.removeItem(USER_KEY);
  },
  clearAll(): void {
    this.clearToken();
    this.clearUser();
  },
};
