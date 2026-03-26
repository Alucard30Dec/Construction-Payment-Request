export type UserRole =
  | 'Admin'
  | 'Employee'
  | 'DepartmentManager'
  | 'Director'
  | 'Accountant'
  | 'Viewer';

export type PaymentRequestStatus =
  | 'Draft'
  | 'Submitted'
  | 'PendingDepartmentApproval'
  | 'PendingDirectorApproval'
  | 'PendingAccounting'
  | 'Rejected'
  | 'ReturnedForEdit'
  | 'Paid'
  | 'Cancelled';

export type ApprovalAction = 'Submit' | 'Approve' | 'Reject' | 'ReturnForEdit' | 'ConfirmPayment';
export type PaymentMethod = 'BankTransfer' | 'Cash' | 'Other';
export type ContractType = 'Construction' | 'Procurement' | 'Service' | 'Consulting' | 'Other';
export type PaymentStatus = 'Unpaid' | 'PartiallyPaid' | 'Paid';
export type AttachmentOwnerType = 'PaymentRequest' | 'Contract' | 'PaymentConfirmation';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface User {
  id: string;
  username: string;
  fullName: string;
  email?: string;
  role: UserRole;
  roleProfileId?: string;
  roleProfileCode?: string;
  roleProfileName?: string;
  permissionCodes: string[];
  department?: string;
  isActive: boolean;
  createdAt: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  user: User;
}

export interface Supplier {
  id: string;
  code: string;
  name: string;
  taxCode: string;
  address?: string;
  contactPerson?: string;
  phone?: string;
  email?: string;
  bankAccountNumber?: string;
  bankName?: string;
  bankBranch?: string;
  notes?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Project {
  id: string;
  code: string;
  name: string;
  location?: string;
  department?: string;
  projectManager?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Contract {
  id: string;
  contractNumber: string;
  name: string;
  supplierId: string;
  supplierName: string;
  projectId: string;
  projectName: string;
  signedDate: string;
  contractValue: number;
  contractType: ContractType;
  notes?: string;
  attachmentPath?: string;
  attachmentCount: number;
  attachments: Attachment[];
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Attachment {
  id: string;
  ownerType: AttachmentOwnerType;
  paymentRequestId?: string;
  contractId?: string;
  paymentConfirmationId?: string;
  fileName: string;
  storedFileName: string;
  filePath: string;
  contentType: string;
  fileSize: number;
  uploadedByUserId: string;
  createdAt: string;
}

export interface PaymentRequestApprovalHistory {
  id: string;
  approverUserId: string;
  approverUsername: string;
  approverFullName: string;
  stepOrder: number;
  action: ApprovalAction;
  actionAt: string;
  comment?: string;
}

export interface PaymentConfirmation {
  id: string;
  paymentDate: string;
  paymentReferenceNumber?: string;
  bankTransactionNumber?: string;
  paidAmount: number;
  accountingNote?: string;
  paymentStatus: PaymentStatus;
  attachments: Attachment[];
  createdAt: string;
}

export interface PaymentRequest {
  id: string;
  requestCode: string;
  title: string;
  projectId: string;
  projectName: string;
  supplierId: string;
  supplierName: string;
  contractId?: string;
  contractName?: string;
  requestType: string;
  invoiceNumber: string;
  invoiceDate: string;
  dueDate: string;
  requestedAmount: number;
  paymentMethod: PaymentMethod;
  currentStatus: PaymentRequestStatus;
  createdByUserId: string;
  createdByUsername: string;
  submittedAt?: string;
  approvedAt?: string;
  paidAt?: string;
  createdAt: string;
  attachmentCount: number;
}

export interface PaymentRequestDetail extends PaymentRequest {
  description?: string;
  amountBeforeVat: number;
  vatRate: number;
  vatAmount: number;
  amountAfterVat: number;
  advanceDeduction: number;
  retentionAmount: number;
  otherDeduction: number;
  notes?: string;
  updatedAt: string;
  attachments: Attachment[];
  approvalHistories: PaymentRequestApprovalHistory[];
  paymentConfirmation?: PaymentConfirmation;
}

export interface ApprovalMatrix {
  id: string;
  minAmount: number;
  maxAmount: number;
  requireDirectorApproval: boolean;
  department?: string;
  projectId?: string;
  projectName?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface DashboardSummary {
  totalRequests: number;
  pendingApprovalCount: number;
  paidCount: number;
  overdueCount: number;
  dueSoonCount: number;
  totalRequestedAmount: number;
  paidAmount: number;
  approvalRatePercent: number;
  paidRatePercent: number;
  averageApprovalHours: number;
  statusSummaries: { status: string; count: number }[];
  monthlyAmountSummaries: { year: number; month: number; totalAmount: number }[];
  amountByProject: { id: string; name: string; totalAmount: number }[];
  amountBySupplier: { id: string; name: string; totalAmount: number }[];
  topOverdueRequests: {
    id: string;
    requestCode: string;
    title: string;
    projectName: string;
    supplierName: string;
    dueDate: string;
    requestedAmount: number;
    overdueDays: number;
  }[];
}

export interface AuditLog {
  id: string;
  userId?: string;
  username?: string;
  action: string;
  entityName: string;
  entityId: string;
  oldValue?: string;
  newValue?: string;
  createdAt: string;
}

export interface PermissionCatalogItem {
  code: string;
  name: string;
  group: string;
  description: string;
}

export interface RoleProfile {
  id: string;
  code: string;
  name: string;
  description?: string;
  isSystem: boolean;
  isActive: boolean;
  userCount: number;
  grantedPermissionCodes: string[];
}

export interface CurrentUserPermissions {
  roleProfileId?: string;
  roleProfileCode?: string;
  roleProfileName?: string;
  permissionCodes: string[];
}

export interface DatabaseHealth {
  status: string;
  configuredProvider: string;
  provider: string;
  userCount?: number;
  message?: string;
  utc: string;
}
