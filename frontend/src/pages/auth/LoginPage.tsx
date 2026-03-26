import { Button, Card, Form, Input, Typography, message } from 'antd';
import { LockOutlined, UserOutlined } from '@ant-design/icons';
import { Navigate, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { useAuth } from '../../hooks/useAuth';
import { getErrorMessage } from '../../utils/apiError';
import loginGraphic from '../../assets/login-graphic.svg';
import { DatabaseConnectionTag } from '../../components/DatabaseConnectionTag';

interface LoginFormValues {
  username: string;
  password: string;
}

export function LoginPage() {
  const navigate = useNavigate();
  const { login, isAuthenticated } = useAuth();
  const [submitting, setSubmitting] = useState(false);

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  const onFinish = async (values: LoginFormValues) => {
    setSubmitting(true);
    try {
      await login(values);
      message.success('Đăng nhập thành công.');
      navigate('/dashboard', { replace: true });
    } catch (error) {
      message.error(getErrorMessage(error));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="login-shell">
      <div className="login-panel">
        <div className="login-visual">
          <img src={loginGraphic} alt="Construction Illustration" />
          <Typography.Title level={3}>Construction Payment Management</Typography.Title>
          <Typography.Text>
            Theo dõi hồ sơ thanh toán xây dựng, duyệt nhiều cấp và kiểm soát chứng từ tập trung.
          </Typography.Text>
        </div>

        <Card className="login-card">
          <Typography.Title level={3} style={{ marginBottom: 4, textAlign: 'center' }}>
            Đăng nhập CPMS
          </Typography.Title>
          <Typography.Paragraph style={{ textAlign: 'center', color: '#64748b' }}>
            Vui lòng nhập thông tin tài khoản để tiếp tục.
          </Typography.Paragraph>
          <div style={{ display: 'flex', justifyContent: 'center', marginBottom: 12 }}>
            <DatabaseConnectionTag />
          </div>

          <Form<LoginFormValues> layout="vertical" onFinish={onFinish}>
            <Form.Item
              label="Tên đăng nhập"
              name="username"
              rules={[{ required: true, message: 'Vui lòng nhập tên đăng nhập.' }]}
            >
              <Input prefix={<UserOutlined />} placeholder="admin" />
            </Form.Item>

            <Form.Item
              label="Mật khẩu"
              name="password"
              rules={[{ required: true, message: 'Vui lòng nhập mật khẩu.' }]}
            >
              <Input.Password prefix={<LockOutlined />} placeholder="********" />
            </Form.Item>

            <Button type="primary" htmlType="submit" loading={submitting} block size="large">
              Đăng nhập
            </Button>
          </Form>
        </Card>
      </div>
    </div>
  );
}
