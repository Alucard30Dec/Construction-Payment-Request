import { useQuery } from '@tanstack/react-query';
import { systemHealthService } from '../services/systemHealthService';

export function useDatabaseHealth() {
  return useQuery({
    queryKey: ['db-health'],
    queryFn: ({ signal }) => systemHealthService.getDatabaseHealth(signal),
    staleTime: 15_000,
    refetchInterval: false,
    retry: 0,
  });
}
