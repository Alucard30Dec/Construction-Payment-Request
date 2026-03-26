import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Button, Card, Form, Input, Space, Switch, Typography, message } from 'antd';
import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { projectService, type ProjectPayload } from '../../services/projectService';
import { getErrorMessage } from '../../utils/apiError';

export function ProjectFormPage() {
  const [form] = Form.useForm<ProjectPayload>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const params = useParams<{ id: string }>();
  const id = params.id;

  const detailQuery = useQuery({
    queryKey: ['project-detail', id],
    queryFn: () => projectService.getById(id as string),
    enabled: Boolean(id),
  });

  useEffect(() => {
    if (detailQuery.data) {
      form.setFieldsValue(detailQuery.data);
    }
  }, [detailQuery.data, form]);

  const saveMutation = useMutation({
    mutationFn: async (payload: ProjectPayload) => {
      if (id) {
        return projectService.update(id, payload);
      }

      return projectService.create(payload);
    },
    onSuccess: () => {
      message.success(id ? 'Cập nhật dự án thành công.' : 'Tạo dự án thành công.');
      void queryClient.invalidateQueries({ queryKey: ['projects'] });
      navigate('/projects');
    },
    onError: (error) => message.error(getErrorMessage(error)),
  });

  return (
    <div style={{ display: 'grid', gap: 16 }}>
      <div className="page-header">
        <Typography.Title level={3} style={{ margin: 0 }}>
          {id ? 'Sửa dự án' : 'Thêm dự án'}
        </Typography.Title>
      </div>

      <Card loading={detailQuery.isLoading}>
        <Form<ProjectPayload>
          form={form}
          layout="vertical"
          initialValues={{ isActive: true }}
          onFinish={(values) => saveMutation.mutate(values)}
        >
          <div className="filter-grid">
            <Form.Item label="Mã dự án" name="code" rules={[{ required: true }]}>
              <Input />
            </Form.Item>
            <Form.Item label="Tên dự án" name="name" rules={[{ required: true }]}>
              <Input />
            </Form.Item>
            <Form.Item label="Địa điểm" name="location">
              <Input />
            </Form.Item>
            <Form.Item label="Phòng ban" name="department">
              <Input />
            </Form.Item>
            <Form.Item label="Quản lý dự án" name="projectManager">
              <Input />
            </Form.Item>
          </div>

          <Form.Item label="Kích hoạt" name="isActive" valuePropName="checked">
            <Switch />
          </Form.Item>

          <Space>
            <Button onClick={() => navigate('/projects')}>Hủy</Button>
            <Button type="primary" htmlType="submit" loading={saveMutation.isPending}>
              Lưu
            </Button>
          </Space>
        </Form>
      </Card>
    </div>
  );
}
