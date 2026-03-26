import { useQuery } from '@tanstack/react-query';
import { supplierService } from '../services/supplierService';
import { projectService } from '../services/projectService';
import { contractService } from '../services/contractService';

export function useSupplierLookup() {
  return useQuery({
    queryKey: ['lookup-suppliers'],
    queryFn: async () => {
      const result = await supplierService.getPaged({ pageNumber: 1, pageSize: 200, isActive: true });
      return result.items;
    },
  });
}

export function useProjectLookup() {
  return useQuery({
    queryKey: ['lookup-projects'],
    queryFn: async () => {
      const result = await projectService.getPaged({ pageNumber: 1, pageSize: 200, isActive: true });
      return result.items;
    },
  });
}

export function useContractLookup() {
  return useQuery({
    queryKey: ['lookup-contracts'],
    queryFn: async () => {
      const result = await contractService.getPaged({ pageNumber: 1, pageSize: 200, isActive: true });
      return result.items;
    },
  });
}
