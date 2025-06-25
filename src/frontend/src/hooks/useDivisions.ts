import { useState, useEffect } from 'react';
import type { DivisionType } from '../types/common/divisionType';
import { divisionService } from '../api/common/divisionService';

export const useDivisions = () => {
  const [divisions, setDivisions] = useState<DivisionType[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadDivisions = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await divisionService.getAll();
      setDivisions(response.data);
    } catch (err) {
      console.log('Error loading divisions:', err);
      setError('Failed to load divisions');
      setDivisions([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDivisions();
  }, []);

  return {
    divisions,
    loading,
    error,
    refetch: loadDivisions
  };
}; 