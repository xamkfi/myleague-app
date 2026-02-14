import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../../context/AuthContext';
import { authService } from '../../../api/auth/authService';
import LanguageToggle from '../../../components/LanguageToggle/LanguageToggle';
import './LoginPage.scss';

type LoginStep = 'email' | 'code';

function LoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { login } = useAuth();

  const [step, setStep] = useState<LoginStep>('email');
  const [email, setEmail] = useState('');
  const [code, setCode] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const parseApiError = (err: unknown): string => {
    if (!(err instanceof Error)) return t('auth.unexpectedError', 'An unexpected error occurred');
    try {
      const parsed = JSON.parse(err.message) as { title?: string; errors?: string[] };
      if (parsed.errors && parsed.errors.length > 0) {
        return parsed.errors.join(', ');
      }
      return parsed.title || err.message;
    } catch {
      return err.message;
    }
  };

  const handleEmailSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const result = await authService.requestLoginCode(email.trim());
      setSuccessMessage(t('auth.codeSent', 'A login code has been sent to your email.'));
      setStep('code');

      // In development, auto-fill the code field
      if (result.devCode) {
        setCode(result.devCode);
      }
    } catch (err: unknown) {
      setError(parseApiError(err));
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCodeSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const tokens = await authService.verifyLoginCode(email.trim(), code.trim());
      await login(tokens);
      navigate('/admin', { replace: true });
    } catch (err: unknown) {
      setError(parseApiError(err));
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleBack = () => {
    setStep('email');
    setCode('');
    setError(null);
    setSuccessMessage(null);
  };

  return (
    <div className="login-page">
      <div className="login-center">
        <div className="login-top">
          <div className="login-brand-row">
            <h1 className="login-brand">MAHL</h1>
            <LanguageToggle />
          </div>
          <p className="login-brand-sub">{t('admin.view', 'Admin view')}</p>
        </div>

        <div className="login-card">
          <div className="login-header">
            <h2 className="login-title">{t('auth.loginTitle', 'Admin Login')}</h2>
            <p className="login-subtitle">
              {step === 'email'
                ? t('auth.loginSubtitle', 'Enter your email to receive a login code.')
                : t('auth.enterCode', 'Enter the 6-digit code sent to your email.')}
            </p>
          </div>

          {error && (
            <div className="login-error">
              <span className="login-error-icon">!</span>
              <span>{error}</span>
            </div>
          )}

          {successMessage && step === 'code' && (
            <div className="login-success">
              <span>{successMessage}</span>
            </div>
          )}

          {step === 'email' ? (
            <form onSubmit={handleEmailSubmit} className="login-form">
              <div className="login-field">
                <label htmlFor="email" className="login-label">
                  {t('auth.emailLabel', 'Email')}
                </label>
                <input
                  id="email"
                  type="email"
                  className="login-input"
                  placeholder={t('auth.emailPlaceholder', 'your@email.com')}
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  autoFocus
                  disabled={isSubmitting}
                />
              </div>
              <button type="submit" className="login-button" disabled={isSubmitting || !email.trim()}>
                {isSubmitting
                  ? t('auth.sending', 'Sending...')
                  : t('auth.sendCode', 'Send Code')}
              </button>
            </form>
          ) : (
            <form onSubmit={handleCodeSubmit} className="login-form">
              <div className="login-field">
                <label htmlFor="code" className="login-label">
                  {t('auth.codeLabel', 'Login Code')}
                </label>
                <input
                  id="code"
                  type="text"
                  className="login-input login-input-code"
                  placeholder={t('auth.codePlaceholder', '000000')}
                  value={code}
                  onChange={(e) => {
                    const val = e.target.value.replace(/\D/g, '').slice(0, 6);
                    setCode(val);
                  }}
                  required
                  autoFocus
                  disabled={isSubmitting}
                  maxLength={6}
                  inputMode="numeric"
                  autoComplete="one-time-code"
                />
              </div>
              <button
                type="submit"
                className="login-button"
                disabled={isSubmitting || code.trim().length !== 6}
              >
                {isSubmitting
                  ? t('auth.verifying', 'Verifying...')
                  : t('auth.verify', 'Verify')}
              </button>
              <button type="button" className="login-back-button" onClick={handleBack} disabled={isSubmitting}>
                {t('auth.back', 'Back')}
              </button>
            </form>
          )}
        </div>
      </div>
    </div>
  );
}

export default LoginPage;
