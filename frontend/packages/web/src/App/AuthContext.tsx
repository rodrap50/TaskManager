import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';
import { getStoredToken, setStoredToken, clearStoredToken, UNAUTHORIZED_EVENT } from '@taskmanager/shared';

export interface AuthUser {
    id: string;
    username: string;
    isAdmin: boolean;
}

interface AuthContextValue {
    user: AuthUser | null;
    signIn: (token: string) => void;
    logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

interface JwtPayload {
    sub?: string;
    username?: string;
    isAdmin?: string;
    exp?: number;
}

function decodeJwt(token: string): JwtPayload | null {
    const payload = token.split('.')[1];
    if (!payload) return null;
    try {
        const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
        const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4);
        return JSON.parse(atob(padded)) as JwtPayload;
    } catch {
        return null;
    }
}

function userFromToken(token: string | null): AuthUser | null {
    if (!token) return null;

    const payload = decodeJwt(token);
    if (!payload?.sub || !payload.username) return null;
    if (typeof payload.exp === 'number' && payload.exp * 1000 <= Date.now()) return null;

    return { id: payload.sub, username: payload.username, isAdmin: payload.isAdmin === 'true' };
}

export function AuthProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<AuthUser | null>(() => userFromToken(getStoredToken()));

    useEffect(() => {
        const handleUnauthorized = () => {
            clearStoredToken();
            setUser(null);
        };
        window.addEventListener(UNAUTHORIZED_EVENT, handleUnauthorized);
        return () => window.removeEventListener(UNAUTHORIZED_EVENT, handleUnauthorized);
    }, []);

    const signIn = (token: string) => {
        setStoredToken(token);
        setUser(userFromToken(token));
    };

    const logout = () => {
        clearStoredToken();
        setUser(null);
    };

    return (
        <AuthContext.Provider value={{ user, signIn, logout }}>
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error('useAuth must be used within AuthProvider');
    return ctx;
}
