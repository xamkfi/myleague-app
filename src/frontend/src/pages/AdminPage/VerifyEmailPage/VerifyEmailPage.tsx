import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { authService } from '../../../api/auth/authService';
import './VerifyEmailPage.scss';

type VerificationState = 'loading' | 'success' | 'error';

interface VerifyEmailPageProps {
  /** Which area this verification belongs to. Controls labels and the login link. */
  variant?: 'admin' | 'teamLeader';
}

function VerifyEmailPage({ variant = 'admin' }: VerifyEmailPageProps) {
  const isTeamLeader = variant === 'teamLeader';
  const [searchParams] = useSearchParams();
  const [state, setState] = useState<VerificationState>('loading');
  const [errorMessage, setErrorMessage] = useState<string>('');

  useEffect(() => {
    const token = searchParams.get('token');

    if (!token) {
      setState('error');
      setErrorMessage('No verification token found in the link. Please use the link from your invitation email.');
      return;
    }

    let cancelled = false;

    authService.verifyAdminEmail(token)
      .then(() => {
        if (!cancelled) setState('success');
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setState('error');
          const message = err instanceof Error ? err.message : 'Verification failed. Please try again.';
          setErrorMessage(message);
        }
      });

    return () => { cancelled = true; };
  }, [searchParams]);

  return (
    <div className="verify-email-page">
      <div className="verify-email-center">
        <div className="verify-email-top">
          <h1 className="verify-email-brand">MAHL</h1>
          <p className="verify-email-brand-sub">{isTeamLeader ? 'Team leader invitation' : 'Admin invitation'}</p>
        </div>

        <div className="verify-email-card">
          {state === 'loading' && (
            <div className="verify-email-spinner">
              <div className="spinner" />
              <p>Verifying your email address…</p>
            </div>
          )}

          {state === 'success' && (
            <>
              <span className="verify-email-icon" role="img" aria-label="Success">✅</span>
              <h2 className="verify-email-title">Email verified!</h2>
              <p className="verify-email-message">
                {isTeamLeader
                  ? 'Your account is now active. Here is how to log in to the team leader area:'
                  : 'Your account is now active. Here is how to log in to the admin panel:'}
              </p>

              <div className="verify-email-instructions">
                <h4>How to log in</h4>
                <ol>
                  <li>Click the button below to go to the login page.</li>
                  <li>Enter the email address where you received this invitation.</li>
                  <li>You will receive a <strong>6-digit login code</strong> by email.</li>
                  <li>{isTeamLeader
                    ? 'Enter the code on the login page to access the team leader area.'
                    : 'Enter the code on the login page to access the admin panel.'}</li>
                </ol>
              </div>

              <Link to={isTeamLeader ? '/team-leader/login' : '/admin/login'} className="verify-email-button">
                {isTeamLeader ? 'Go to team leader login' : 'Go to admin login'}
              </Link>
            </>
          )}

          {state === 'error' && (
            <>
              <span className="verify-email-icon" role="img" aria-label="Error">❌</span>
              <h2 className="verify-email-title">Verification failed</h2>
              <div className="verify-email-error">
                <span>{errorMessage || 'This verification link is invalid or has expired. Please contact an administrator to resend the invitation.'}</span>
              </div>
              <p className="verify-email-message">
                If you need assistance, please contact your system administrator.
              </p>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

export default VerifyEmailPage;
