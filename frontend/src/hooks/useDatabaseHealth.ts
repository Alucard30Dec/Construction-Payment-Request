import { useQuery } from '@tanstack/react-query';
import { systemHealthService } from '../services/systemHealthService';

export function useDatabaseHealth() {
  return useQuery({
    queryKey: ['db-health'],
    queryFn: ({ signal }) => systemHealthService.getDatabaseHealth(signal),
    staleTime: 0,
    gcTime: 10 * 60_000,
    refetchOnMount: 'always',
    refetchOnReconnect: true,
    refetchInterval: (query) => (query.state.data?.status === 'ok' ? 60_000 : 1_000),
    refetchIntervalInBackground: true,
    retry: 0,
  });
}
