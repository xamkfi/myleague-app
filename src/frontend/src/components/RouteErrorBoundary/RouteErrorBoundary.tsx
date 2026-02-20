import { useRouteError } from 'react-router-dom';

function isChunkLoadError(error: unknown): boolean {
  return (
    error instanceof TypeError &&
    (error.message.includes('Failed to fetch dynamically imported module') ||
      error.message.includes('error loading dynamically imported module') ||
      error.message.includes('Importing a module script failed'))
  );
}

/**
 * React Router errorElement that gracefully handles chunk load failures
 * (e.g. after a new deployment) by offering a page reload.
 */
function RouteErrorBoundary() {
  const error = useRouteError();
  const chunkError = isChunkLoadError(error);

  const handleReload = () => {
    window.location.reload();
  };

  return (
    <div style={{
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      minHeight: '50vh',
      padding: '2rem',
      textAlign: 'center',
      fontFamily: 'system-ui, sans-serif',
    }}>
      {chunkError ? (
        <>
          <h2 style={{ marginBottom: '1rem' }}>
            Sovelluksesta on saatavilla uusi versio
          </h2>
          <p style={{ marginBottom: '1.5rem', color: '#666' }}>
            A new version of the application is available. Please reload the page.
          </p>
        </>
      ) : (
        <>
          <h2 style={{ marginBottom: '1rem' }}>
            Jokin meni pieleen
          </h2>
          <p style={{ marginBottom: '1.5rem', color: '#666' }}>
            Something went wrong. Please try reloading the page.
          </p>
        </>
      )}
      <button
        onClick={handleReload}
        style={{
          padding: '0.75rem 1.5rem',
          fontSize: '1rem',
          backgroundColor: '#2563eb',
          color: '#fff',
          border: 'none',
          borderRadius: '0.5rem',
          cursor: 'pointer',
        }}
      >
        Lataa sivu uudelleen / Reload page
      </button>
    </div>
  );
}

export default RouteErrorBoundary;
