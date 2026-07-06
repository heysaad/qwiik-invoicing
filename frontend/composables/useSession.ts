import type { SessionState } from '~/types/api';

const emptySession: SessionState = {
  tenantId: '',
  email: '',
  accessToken: ''
};

export function useSession() {
  const session = useState<SessionState>('qwiik-session', () => ({ ...emptySession }));

  function loadSession() {
    if (!import.meta.client) return;
    const stored = localStorage.getItem('qwiik-session');
    if (!stored) return;
    session.value = { ...emptySession, ...JSON.parse(stored) };
  }

  function saveSession(next: Partial<SessionState>) {
    session.value = { ...session.value, ...next };
    if (import.meta.client) {
      localStorage.setItem('qwiik-session', JSON.stringify(session.value));
    }
  }

  function clearSession() {
    session.value = { ...emptySession };
    if (import.meta.client) {
      localStorage.removeItem('qwiik-session');
    }
  }

  return {
    session,
    loadSession,
    saveSession,
    clearSession
  };
}
