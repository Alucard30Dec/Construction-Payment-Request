import { useQuery } from '@tanstack/react-query';
import { supplierService } from '../services/supplierService';
import { projectService } from '../services/projectService';
import { contractService } from '../services/contractService';
import type { ContractQuery } from '../services/contractService';

export function useSupplierLookup() {
  return useQuery({
    queryKey: ['lookup-suppliers'],
    queryFn: async () => {
      const result = await supplierService.getPaged({ pageNumber: 1, pageSize: 200, isActive: true });
      return result.items;
    },
    staleTime: 5 * 60_000,
    gcTime: 15 * 60_000,
    refetchOnMount: false,
  });
}

export function useProjectLookup() {
  return useQuery({
    queryKey: ['lookup-projects'],
    queryFn: async () => {
      const result = await projectService.getPaged({ pageNumber: 1, pageSize: 200, isActive: true });
      return result.items;
    },
    staleTime: 5 * 60_000,
    gcTime: 15 * 60_000,
    refetchOnMount: false,
  });
}

interface ContractLookupParams {
  projectId?: ContractQuery['projectId'];
  supplierId?: ContractQuery['supplierId'];
}

export function useContractLookup(params?: ContractLookupParams) {
  const projectId = params?.projectId;
  const supplierId = params?.supplierId;

  return useQuery({
    queryKey: ['lookup-contracts', projectId, supplierId],
    queryFn: async () => {
      const result = await contractService.getPaged({
        pageNumber: 1,
        pageSize: 200,
        isActive: true,
        projectId,
        supplierId,
      });
      return result.items;
    },
    enabled: Boolean(projectId),
    staleTime: 5 * 60_000,
    gcTime: 15 * 60_000,
    refetchOnMount: false,
  });
}
