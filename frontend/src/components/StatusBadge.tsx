import { Tag } from 'antd';
import type { PaymentRequestStatus } from '../types';
import { paymentRequestStatusOptions } from '../constants/options';

interface Props {
  status: PaymentRequestStatus;
}

const statusColorMap: Record<PaymentRequestStatus, string> = {
  Draft: 'default',
  Submitted: 'blue',
  PendingDepartmentApproval: 'geekblue',
  PendingDirectorApproval: 'purple',
  PendingAccounting: 'gold',
  Rejected: 'red',
  ReturnedForEdit: 'orange',
  Paid: 'green',
  Cancelled: 'volcano',
};

export function StatusBadge({ status }: Props) {
  const label = paymentRequestStatusOptions.find((x) => x.value === status)?.label ?? status;
  return <Tag color={statusColorMap[status]}>{label}</Tag>;
}
