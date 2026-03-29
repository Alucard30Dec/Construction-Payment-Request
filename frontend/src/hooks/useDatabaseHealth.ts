import { useQuery } from '@tanstack/react-query';
import { systemHealthService } from '../services/systemHealthService';

export function useDatabaseHealth() {
  return useQuery({
    queryKey: ['db-health'],
    queryFn: ({ signal }) => systemHealthService.getDatabaseHealth(signal),
    staleTime: 60_000,
    gcTime: 10 * 60_000,
    refetchOnMount: false,
    refetchInterval: false,
    retry: 0,
  });
}
