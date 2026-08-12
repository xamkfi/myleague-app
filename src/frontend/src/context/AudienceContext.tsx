import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import {
  AUDIENCE_REGISTRY,
  DEFAULT_AUDIENCE,
  getAudienceById,
  type AudienceDefinition,
  type AudienceThemeId,
} from '../audience/audienceRegistry';

const STORAGE_KEY = 'myleague.audience';

interface AudienceContextValue {
  audience: AudienceDefinition;
  selectedAudienceId: AudienceThemeId;
  setAudience: (id: AudienceThemeId) => void;
  audiences: readonly AudienceDefinition[];
}

const AudienceContext = createContext<AudienceContextValue | undefined>(undefined);

function readStoredAudienceId(): AudienceThemeId {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    const match = getAudienceById(stored);
    if (match) {
      return match.id;
    }
  } catch {
    // Ignore storage access errors (private mode, SSR, etc.)
  }
  return DEFAULT_AUDIENCE.id;
}

function applyAudienceToDocument(themeId: AudienceThemeId): void {
  document.documentElement.dataset.audience = themeId;
}

interface AudienceProviderProps {
  children: ReactNode;
}

export function AudienceProvider({ children }: AudienceProviderProps) {
  const [selectedAudienceId, setSelectedAudienceId] = useState<AudienceThemeId>(() => {
    const initialId = readStoredAudienceId();
    applyAudienceToDocument(initialId);
    return initialId;
  });

  const audience = useMemo(
    () => getAudienceById(selectedAudienceId) ?? DEFAULT_AUDIENCE,
    [selectedAudienceId],
  );

  const setAudience = useCallback((id: AudienceThemeId) => {
    const next = getAudienceById(id) ?? DEFAULT_AUDIENCE;
    setSelectedAudienceId(next.id);
    applyAudienceToDocument(next.themeId);
    try {
      localStorage.setItem(STORAGE_KEY, next.id);
    } catch {
      // Ignore storage write errors
    }
  }, []);

  useEffect(() => {
    applyAudienceToDocument(audience.themeId);
  }, [audience.themeId]);

  const value = useMemo<AudienceContextValue>(
    () => ({
      audience,
      selectedAudienceId,
      setAudience,
      audiences: AUDIENCE_REGISTRY,
    }),
    [audience, selectedAudienceId, setAudience],
  );

  return <AudienceContext.Provider value={value}>{children}</AudienceContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAudience(): AudienceContextValue {
  const context = useContext(AudienceContext);
  if (context === undefined) {
    throw new Error('useAudience must be used within an AudienceProvider');
  }
  return context;
}
