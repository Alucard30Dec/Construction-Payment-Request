import dayjs from 'dayjs';

export const formatDate = (value?: string | null) => {
  if (!value) {
    return '-';
  }

  return dayjs(value).format('DD/MM/YYYY');
};

export const formatDateTime = (value?: string | null) => {
  if (!value) {
    return '-';
  }

  return dayjs(value).format('DD/MM/YYYY HH:mm');
};

export const formatCurrency = (value?: number | null) => {
  if (value == null) {
    return '0 đ';
  }

  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0,
  }).format(value);
};
