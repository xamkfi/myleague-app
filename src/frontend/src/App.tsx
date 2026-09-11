import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { routes } from './router/routes';
import { AuthProvider } from './context/AuthContext';
import { AudienceProvider } from './context/AudienceContext';
import './App.scss';

// Initialize i18n
import './i18n/i18n';

const router = createBrowserRouter(routes);

function App() {
  return (
    <AuthProvider>
      <AudienceProvider>
        <RouterProvider router={router} />
      </AudienceProvider>
    </AuthProvider>
  );
}

export default App;
