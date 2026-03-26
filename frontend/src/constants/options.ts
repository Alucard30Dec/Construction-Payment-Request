import type { PaymentMethod, PaymentRequestStatus, PaymentStatus, UserRole, ContractType } from '../types';

export const roleOptions: { label: string; value: UserRole }[] = [
  { label: 'Admin', value: 'Admin' },
  { label: 'Nhân viên', value: 'Employee' },
  { label: 'Trưởng bộ phận', value: 'DepartmentManager' },
  { label: 'Giám đốc', value: 'Director' },
  { label: 'Kế toán', value: 'Accountant' },
  { label: 'Viewer', value: 'Viewer' },
];

export const paymentMethodOptions: { label: string; value: PaymentMethod }[] = [
  { label: 'Chuyển khoản', value: 'BankTransfer' },
  { label: 'Tiền mặt', value: 'Cash' },
  { label: 'Khác', value: 'Other' },
];

export const contractTypeOptions: { label: string; value: ContractType }[] = [
  { label: 'Thi công', value: 'Construction' },
  { label: 'Mua sắm', value: 'Procurement' },
  { label: 'Dịch vụ', value: 'Service' },
  { label: 'Tư vấn', value: 'Consulting' },
  { label: 'Khác', value: 'Other' },
];

export const paymentStatusOptions: { label: string; value: PaymentStatus }[] = [
  { label: 'Chưa thanh toán', value: 'Unpaid' },
  { label: 'Thanh toán một phần', value: 'PartiallyPaid' },
  { label: 'Đã thanh toán', value: 'Paid' },
];

export const paymentRequestStatusOptions: { label: string; value: PaymentRequestStatus }[] = [
  { label: 'Nháp', value: 'Draft' },
  { label: 'Đã gửi', value: 'Submitted' },
  { label: 'Chờ trưởng bộ phận', value: 'PendingDepartmentApproval' },
  { label: 'Chờ giám đốc', value: 'PendingDirectorApproval' },
  { label: 'Chờ kế toán', value: 'PendingAccounting' },
  { label: 'Từ chối', value: 'Rejected' },
  { label: 'Trả chỉnh sửa', value: 'ReturnedForEdit' },
  { label: 'Đã thanh toán', value: 'Paid' },
  { label: 'Đã hủy', value: 'Cancelled' },
];
