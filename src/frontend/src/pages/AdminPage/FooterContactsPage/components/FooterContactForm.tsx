import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import Button from '../../../../components/Button/Button';
import type { FooterContact, FooterContactRequest } from '../../../../types/admin/footerContactTypes';
import './FooterContactForm.scss';

interface FooterContactFormProps {
  contact: FooterContact | null;
  isSaving: boolean;
  onCancel: () => void;
  onSave: (payload: FooterContactRequest) => void;
}

function FooterContactForm({
  contact,
  isSaving,
  onCancel,
  onSave,
}: FooterContactFormProps) {
  const { t } = useTranslation();
  const [title, setTitle] = useState(contact?.title ?? '');
  const [details, setDetails] = useState(contact?.details ?? '');
  const [email, setEmail] = useState(contact?.email ?? '');
  const [phone, setPhone] = useState(contact?.phone ?? '');
  const [url, setUrl] = useState(contact?.url ?? '');
  const [sortOrder, setSortOrder] = useState(String(contact?.sortOrder ?? 0));

  const handleSubmit = (event: FormEvent<HTMLFormElement>): void => {
    event.preventDefault();

    const parsedOrder = Number.parseInt(sortOrder, 10);

    onSave({
      title: title.trim(),
      details: details.trim() || null,
      email: email.trim() || null,
      phone: phone.trim() || null,
      url: url.trim() || null,
      sortOrder: Number.isNaN(parsedOrder) ? 0 : parsedOrder,
    });
  };

  return (
    <form className="footer-contact-form" onSubmit={handleSubmit}>
      <h3>
        {contact
          ? t('admin.siteContent.footerContacts.editTitle', 'Muokkaa yhteystietoa')
          : t('admin.siteContent.footerContacts.addTitle', 'Lisää yhteystieto')}
      </h3>

      <label className="footer-contact-form__field">
        <span>{t('admin.siteContent.footerContacts.fields.title', 'Nimi / otsikko')}</span>
        <input
          type="text"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          required
          maxLength={200}
        />
      </label>

      <label className="footer-contact-form__field">
        <span>{t('admin.siteContent.footerContacts.fields.details', 'Lisätiedot (esim. osoite)')}</span>
        <textarea
          value={details}
          onChange={(event) => setDetails(event.target.value)}
          rows={3}
          maxLength={500}
        />
      </label>

      <label className="footer-contact-form__field">
        <span>{t('admin.siteContent.footerContacts.fields.email', 'Sähköposti')}</span>
        <input
          type="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          maxLength={200}
        />
      </label>

      <label className="footer-contact-form__field">
        <span>{t('admin.siteContent.footerContacts.fields.phone', 'Puhelinnumero')}</span>
        <input
          type="tel"
          value={phone}
          onChange={(event) => setPhone(event.target.value)}
          maxLength={50}
        />
      </label>

      <label className="footer-contact-form__field">
        <span>{t('admin.siteContent.footerContacts.fields.url', 'Verkkosivu tai linkki')}</span>
        <input
          type="url"
          value={url}
          onChange={(event) => setUrl(event.target.value)}
          maxLength={500}
          placeholder="https://"
        />
      </label>

      <label className="footer-contact-form__field">
        <span>{t('admin.siteContent.footerContacts.fields.sortOrder', 'Järjestys')}</span>
        <input
          type="number"
          min={0}
          value={sortOrder}
          onChange={(event) => setSortOrder(event.target.value)}
        />
      </label>

      <div className="footer-contact-form__actions">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={isSaving}>
          {t('common.cancel', 'Peruuta')}
        </Button>
        <Button type="submit" isLoading={isSaving} disabled={!title.trim()}>
          {t('common.save', 'Tallenna')}
        </Button>
      </div>
    </form>
  );
}

export default FooterContactForm;
