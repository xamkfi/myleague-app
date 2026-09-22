import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './RegisterPage.css';

const REGISTER_EMAIL = 'pasi@mahl.fi';

function RegisterPage() {
  const { t } = useTranslation();
  const [formSubmitted, setFormSubmitted] = useState(false);

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const name = String(form.get('name') ?? '').trim();
    const email = String(form.get('email') ?? '').trim();
    const phone = String(form.get('phone') ?? '').trim();
    const ageGroup = String(form.get('age-group') ?? '').trim();
    const experience = String(form.get('experience') ?? '').trim();
    const notes = String(form.get('notes') ?? '').trim();

    const subject = encodeURIComponent(t('registerPage.mailSubject', { name }));
    const body = encodeURIComponent(
      [
        `${t('registerPage.name')}: ${name}`,
        `${t('registerPage.email')}: ${email}`,
        `${t('registerPage.phone')}: ${phone}`,
        `${t('registerPage.ageGroup')}: ${ageGroup}`,
        `${t('registerPage.experience')}: ${experience}`,
        `${t('registerPage.notes')}: ${notes}`,
      ].join('\n'),
    );

    window.location.href = `mailto:${REGISTER_EMAIL}?subject=${subject}&body=${body}`;
    setFormSubmitted(true);
  };

  return (
    <PageTemplate title={t('nav.register')} fullBleed>
      <div className="register-page">
        <header className="register-page__hero">
          <h1 className="register-page__title">{t('nav.register')}</h1>
          <p className="register-page__intro">{t('registerPage.intro')}</p>
        </header>

        <div className="register-container">
          {formSubmitted ? (
            <div className="thank-you-message">
              <h2>{t('registerPage.thankYouTitle')}</h2>
              <p>{t('registerPage.thankYouBody')}</p>
            </div>
          ) : (
            <form className="register-form" onSubmit={handleSubmit}>
              <div className="form-group">
                <label htmlFor="name">{t('registerPage.name')}</label>
                <input type="text" id="name" name="name" required />
              </div>

              <div className="form-group">
                <label htmlFor="email">{t('registerPage.email')}</label>
                <input type="email" id="email" name="email" required />
              </div>

              <div className="form-group">
                <label htmlFor="phone">{t('registerPage.phone')}</label>
                <input type="tel" id="phone" name="phone" />
              </div>

              <div className="form-group">
                <label htmlFor="age-group">{t('registerPage.ageGroup')}</label>
                <select id="age-group" name="age-group" required>
                  <option value="">{t('registerPage.ageGroupPlaceholder')}</option>
                  <option value="u12">{t('registerPage.ageGroupU12')}</option>
                  <option value="u15">{t('registerPage.ageGroupU15')}</option>
                  <option value="u18">{t('registerPage.ageGroupU18')}</option>
                  <option value="adult">{t('registerPage.ageGroupAdult')}</option>
                  <option value="senior">{t('registerPage.ageGroupSenior')}</option>
                </select>
              </div>

              <div className="form-group">
                <label htmlFor="experience">{t('registerPage.experience')}</label>
                <select id="experience" name="experience" required>
                  <option value="">{t('registerPage.experiencePlaceholder')}</option>
                  <option value="beginner">{t('registerPage.experienceBeginner')}</option>
                  <option value="intermediate">{t('registerPage.experienceIntermediate')}</option>
                  <option value="advanced">{t('registerPage.experienceAdvanced')}</option>
                </select>
              </div>

              <div className="form-group">
                <label htmlFor="notes">{t('registerPage.notes')}</label>
                <textarea id="notes" name="notes" rows={4} />
              </div>

              <button type="submit" className="submit-button">{t('registerPage.submit')}</button>
            </form>
          )}
        </div>
      </div>
    </PageTemplate>
  );
}

export default RegisterPage;
