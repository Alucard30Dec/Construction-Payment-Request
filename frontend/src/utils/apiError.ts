export function getErrorMessage(error: unknown): string {
  const fallback = 'Có lỗi xảy ra, vui lòng thử lại.';

  if (!error || typeof error !== 'object') {
    return fallback;
  }

  const responseData = (error as {
    response?: {
      data?: unknown;
      status?: number;
    };
  }).response?.data as
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

  const responseMessage =
    (typeof responseData === 'object' && responseData !== null && responseData.message) ||
    (typeof responseData === 'object' && responseData !== null && responseData.Message);

  if (responseMessage) {
    return responseMessage;
  }

  const responseErrors =
    (typeof responseData === 'object' && responseData !== null && responseData.errors) ||
    (typeof responseData === 'object' && responseData !== null && responseData.Errors);

  if (responseErrors && typeof responseErrors === 'object') {
    const firstField = Object.keys(responseErrors)[0];
    if (firstField && responseErrors[firstField]?.length > 0) {
      return responseErrors[firstField][0];
    }
  }

  const message = (error as { message?: string }).message;
  const maybeAxiosLike = error as {
    message?: string;
    code?: string;
    request?: unknown;
    response?: unknown;
  };

  // Axios thường trả "Network Error" khi backend không chạy/sai URL/CORS bị chặn.
  if (!maybeAxiosLike.response && maybeAxiosLike.request) {
    return 'Không kết nối được API. Hãy kiểm tra backend đã chạy tại http://localhost:5000 và mở lại frontend.';
  }

  return message ?? fallback;
}
