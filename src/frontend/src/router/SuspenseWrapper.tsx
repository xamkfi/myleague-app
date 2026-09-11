import { Suspense } from 'react';
import LoadingSpinner from '../components/LoadingSpinner/LoadingSpinner';

function RouteLoadingFallback() {
  return (
    <div
      style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        minHeight: '100vh',
      }}
    >
      <LoadingSpinner size="lg" text="Loading..." />
    </div>
  );
}

export default function SuspenseWrapper({ children }: { children: React.ReactNode }) {
  return <Suspense fallback={<RouteLoadingFallback />}>{children}</Suspense>;
}
