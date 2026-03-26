import { createContext, useEffect, useMemo, useState, type PropsWithChildren } from 'react';
import { message } from 'antd';
import { authService } from '../services/authService';
import { authStorage } from '../services/authStorage';
import type { LoginRequest, User, UserRole } from '../types';

interface AuthContextValue {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (payload: LoginRequest) => Promise<void>;
  logout: () => Promise<void>;
  hasAnyRole: (roles: UserRole[]) => boolean;
  hasPermission: (permissionCode: string) => boolean;
  hasAnyPermission: (permissionCodes: string[]) => boolean;
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: PropsWithChildren) {
  const [token, setToken] = useState<string | null>(authStorage.getToken());
  const [user, setUser] = useState<User | null>(authStorage.getUser());

  useEffect(() => {
    if (!token) {
      return;
    }

    if (!user) {
      authService
        .me()
        .then((currentUser) => {
          const normalizedUser: User = {
            ...currentUser,
            permissionCodes: currentUser.permissionCodes ?? [],
          };
          setUser(normalizedUser);
          authStorage.setUser(normalizedUser);
        })
        .catch(() => {
          authStorage.clearAll();
          setToken(null);
          setUser(null);
        });
    }
  }, [token, user]);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      token,
      isAuthenticated: Boolean(token && user),
      login: async (payload: LoginRequest) => {
        const response = await authService.login(payload);
        const normalizedUser: User = {
          ...response.user,
          permissionCodes: response.user.permissionCodes ?? [],
        };
        setToken(response.token);
        setUser(normalizedUser);
        authStorage.setToken(response.token);
        authStorage.setUser(normalizedUser);
      },
      logout: async () => {
        try {
          await authService.logout();
        } catch {
          message.warning('Phiên đăng nhập đã hết hạn.');
        }

        authStorage.clearAll();
        setToken(null);
        setUser(null);
      },
      hasAnyRole: (roles: UserRole[]) => {
        if (!user) {
          return false;
        }

        return roles.includes(user.role);
      },
      hasPermission: (permissionCode: string) => {
        if (!user || !permissionCode) {
          return false;
        }

        return user.permissionCodes?.includes(permissionCode) ?? false;
      },
      hasAnyPermission: (permissionCodes: string[]) => {
        if (!user || permissionCodes.length === 0) {
          return false;
        }

        return permissionCodes.some((x) => user.permissionCodes?.includes(x));
      },
    }),
    [token, user],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
