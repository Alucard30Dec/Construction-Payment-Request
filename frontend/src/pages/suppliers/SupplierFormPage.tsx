import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Button, Card, Form, Input, Space, Switch, Typography, message } from 'antd';
import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { supplierService, type SupplierPayload } from '../../services/supplierService';
import { getErrorMessage } from '../../utils/apiError';

export function SupplierFormPage() {
  const [form] = Form.useForm<SupplierPayload>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const params = useParams<{ id: string }>();
  const id = params.id;

  const detailQuery = useQuery({
    queryKey: ['supplier-detail', id],
    queryFn: () => supplierService.getById(id as string),
    enabled: Boolean(id),
  });

  useEffect(() => {
    if (detailQuery.data) {
      form.setFieldsValue(detailQuery.data);
    }
  }, [detailQuery.data, form]);

  const saveMutation = useMutation({
    mutationFn: async (payload: SupplierPayload) => {
      if (id) {
        return supplierService.update(id, payload);
      }

      return supplierService.create(payload);
    },
    onSuccess: () => {
      message.success(id ? 'Cập nhật nhà cung cấp thành công.' : 'Tạo nhà cung cấp thành công.');
      void queryClient.invalidateQueries({ queryKey: ['suppliers'] });
      navigate('/suppliers');
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  return (
    <div style={{ display: 'grid', gap: 16 }}>
      <div className="page-header">
        <Typography.Title level={3} style={{ margin: 0 }}>
          {id ? 'Sửa nhà cung cấp' : 'Thêm nhà cung cấp'}
        </Typography.Title>
      </div>

      <Card loading={detailQuery.isLoading}>
        <Form<SupplierPayload>
          form={form}
          layout="vertical"
          initialValues={{ isActive: true }}
          onFinish={(values) => saveMutation.mutate(values)}
        >
          <div className="filter-grid">
            <Form.Item label="Mã nhà cung cấp" name="code" rules={[{ required: true }]}>
              <Input />
            </Form.Item>
            <Form.Item label="Tên nhà cung cấp" name="name" rules={[{ required: true }]}>
              <Input />
            </Form.Item>
            <Form.Item label="Mã số thuế" name="taxCode" rules={[{ required: true }]}>
              <Input />
            </Form.Item>
            <Form.Item label="Người liên hệ" name="contactPerson">
              <Input />
            </Form.Item>
            <Form.Item label="Điện thoại" name="phone">
              <Input />
            </Form.Item>
            <Form.Item label="Email" name="email" rules={[{ type: 'email', message: 'Email không hợp lệ.' }]}>
              <Input />
            </Form.Item>
            <Form.Item label="Tài khoản ngân hàng" name="bankAccountNumber">
              <Input />
            </Form.Item>
            <Form.Item label="Ngân hàng" name="bankName">
              <Input />
            </Form.Item>
            <Form.Item label="Chi nhánh" name="bankBranch">
              <Input />
            </Form.Item>
          </div>

          <Form.Item label="Địa chỉ" name="address">
            <Input.TextArea rows={2} />
          </Form.Item>

          <Form.Item label="Ghi chú" name="notes">
            <Input.TextArea rows={3} />
          </Form.Item>

          <Form.Item label="Kích hoạt" name="isActive" valuePropName="checked">
            <Switch />
          </Form.Item>

          <Space>
            <Button onClick={() => navigate('/suppliers')}>Hủy</Button>
            <Button type="primary" htmlType="submit" loading={saveMutation.isPending}>
              Lưu
            </Button>
          </Space>
        </Form>
      </Card>
    </div>
  );
}
