import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './RegisterPage.css';

function RegisterPage() {
  const { t } = useTranslation();
  const [formSubmitted, setFormSubmitted] = useState(false);

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setFormSubmitted(true);
  };

  return (
    <PageTemplate title={t('nav.register')}>
      <div className="register-container">
        {formSubmitted ? (
          <div className="thank-you-message">
            <h2>{t('registerPage.thankYouTitle')}</h2>
            <p>{t('registerPage.thankYouBody')}</p>
          </div>
        ) : (
          <>
            <p className="register-intro">{t('registerPage.intro')}</p>

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
                  <option value="">{t('registerPage.selectAgeGroup')}</option>
                  <option value="u12">{t('registerPage.under12')}</option>
                  <option value="u15">{t('registerPage.under15')}</option>
                  <option value="u18">{t('registerPage.under18')}</option>
                  <option value="adult">{t('registerPage.adult')}</option>
                  <option value="senior">{t('registerPage.senior')}</option>
                </select>
              </div>

              <div className="form-group">
                <label htmlFor="experience">{t('registerPage.experience')}</label>
                <select id="experience" name="experience" required>
                  <option value="">{t('registerPage.selectExperience')}</option>
                  <option value="beginner">{t('registerPage.beginner')}</option>
                  <option value="intermediate">{t('registerPage.intermediate')}</option>
                  <option value="advanced">{t('registerPage.advanced')}</option>
                </select>
              </div>

              <div className="form-group">
                <label htmlFor="notes">{t('registerPage.notes')}</label>
                <textarea id="notes" name="notes" rows={4}></textarea>
              </div>

              <button type="submit" className="submit-button">{t('registerPage.submit')}</button>
            </form>
          </>
        )}
      </div>
    </PageTemplate>
  );
}

export default RegisterPage; 