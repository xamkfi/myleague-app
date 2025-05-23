import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './RegisterPage.css';

function RegisterPage() {
  const { t } = useTranslation();
  const [formSubmitted, setFormSubmitted] = useState(false);
  
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setFormSubmitted(true);
    // In a real app, this would send the form data to a server
  };
  
  return (
    <PageTemplate title={t('nav.register')}>
      <div className="register-container">
        {formSubmitted ? (
          <div className="thank-you-message">
            <h2>Thank you for registering!</h2>
            <p>We've received your information and will be in touch soon.</p>
          </div>
        ) : (
          <>
            <p className="register-intro">Fill out the form below to register for the upcoming season.</p>
            
            <form className="register-form" onSubmit={handleSubmit}>
              <div className="form-group">
                <label htmlFor="name">Name</label>
                <input type="text" id="name" name="name" required />
              </div>
              
              <div className="form-group">
                <label htmlFor="email">Email</label>
                <input type="email" id="email" name="email" required />
              </div>
              
              <div className="form-group">
                <label htmlFor="phone">Phone</label>
                <input type="tel" id="phone" name="phone" />
              </div>
              
              <div className="form-group">
                <label htmlFor="age-group">Age Group</label>
                <select id="age-group" name="age-group" required>
                  <option value="">Select an age group</option>
                  <option value="u12">Under 12</option>
                  <option value="u15">Under 15</option>
                  <option value="u18">Under 18</option>
                  <option value="adult">Adult</option>
                  <option value="senior">Senior (40+)</option>
                </select>
              </div>
              
              <div className="form-group">
                <label htmlFor="experience">Experience Level</label>
                <select id="experience" name="experience" required>
                  <option value="">Select your experience level</option>
                  <option value="beginner">Beginner</option>
                  <option value="intermediate">Intermediate</option>
                  <option value="advanced">Advanced</option>
                </select>
              </div>
              
              <div className="form-group">
                <label htmlFor="notes">Additional Notes</label>
                <textarea id="notes" name="notes" rows={4}></textarea>
              </div>
              
              <button type="submit" className="submit-button">Register</button>
            </form>
          </>
        )}
      </div>
    </PageTemplate>
  );
}

export default RegisterPage; 