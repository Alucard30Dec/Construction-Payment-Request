export function getErrorMessage(error: unknown): string {
  const fallback = 'Có lỗi hệ thống xảy ra. Vui lòng thử lại.';

  if (!error || typeof error !== 'object') {
    return fallback;
  }

  const axiosLikeError = error as {
    response?: {
      data?: unknown;
      status?: number;
    };
    request?: unknown;
    message?: string;
    code?: string;
  };
  const status = axiosLikeError.response?.status;
  const responseData = axiosLikeError.response?.data as
    | {
        message?: string;
        Message?: string;
        errors?: Record<string, string[]>;
        Errors?: Record<string, string[]>;
      }
    | string
    | undefined;

  if (typeof responseData === 'string' && responseData.trim()) {
    return responseData;
  }

  const responseErrors =
    (typeof responseData === 'object' && responseData !== null && responseData.errors) ||
    (typeof responseData === 'object' && responseData !== null && responseData.Errors);

  if (responseErrors && typeof responseErrors === 'object') {
    for (const field of Object.keys(responseErrors)) {
      const messages = responseErrors[field];
      if (messages?.length > 0 && messages[0]) {
        return messages[0];
      }
    }
  }

  const responseMessage =
    (typeof responseData === 'object' && responseData !== null && responseData.message) ||
    (typeof responseData === 'object' && responseData !== null && responseData.Message);

  if (responseMessage) {
    return responseMessage;
  }

  if (!axiosLikeError.response && axiosLikeError.request) {
    return 'Không thể kết nối đến máy chủ. Vui lòng kiểm tra mạng hoặc thử lại.';
  }

  if (axiosLikeError.code === 'ECONNABORTED') {
    return 'Kết nối tới máy chủ bị quá thời gian. Vui lòng thử lại.';
  }

  if (status === 400) {
    return 'Dữ liệu gửi lên chưa hợp lệ.';
  }

  if (status === 404) {
    return 'Không tìm thấy dữ liệu cần xử lý.';
  }

  if (status === 500) {
    return fallback;
  }

  return axiosLikeError.message ?? fallback;
}
