import { Navigate, useParams } from 'react-router-dom';
import type { SportKind } from '../../utils/sportRoutes';

interface RedirectToPlayerProps {
  sport?: SportKind;
}

export default function RedirectToPlayer({ sport }: RedirectToPlayerProps) {
  const { id } = useParams<{ id: string }>();
  const query = sport ? `?sport=${sport}` : '';
  return <Navigate to={`/player/${id ?? ''}${query}`} replace />;
}
