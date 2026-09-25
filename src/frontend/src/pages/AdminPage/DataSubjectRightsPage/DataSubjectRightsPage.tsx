import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import Button from '../../../components/Button/Button';
import { personApi } from '../../../api/admin/personApi';
import {
  dataSubjectRightsApi,
  type DataSubjectCopy,
  type RectifyDataSubjectPayload,
} from '../../../api/admin/dataSubjectRightsApi';
import type { Person } from '../../../types/admin/personTypes';
import './DataSubjectRightsPage.scss';

interface RectifyForm {
  firstName: string;
  lastName: string;
  birthDate: string;
  email: string;
  phone: string;
  street1: string;
  city: string;
  postalCode: string;
  country: string;
  accountEmail: string;
}

function emptyForm(): RectifyForm {
  return {
    firstName: '',
    lastName: '',
    birthDate: '',
    email: '',
    phone: '',
    street1: '',
    city: '',
    postalCode: '',
    country: '',
    accountEmail: '',
  };
}

function formFromCopy(copy: DataSubjectCopy): RectifyForm {
  return {
    firstName: copy.firstName,
    lastName: copy.lastName,
    birthDate: copy.birthDate ? copy.birthDate.slice(0, 10) : '',
    email: copy.contactInfo?.email ?? '',
    phone: copy.contactInfo?.phone ?? '',
    street1: copy.address?.street1 ?? '',
    city: copy.address?.city ?? '',
    postalCode: copy.address?.postalCode ?? '',
    country: copy.address?.country ?? '',
    accountEmail: copy.account?.email ?? '',
  };
}

function downloadJson(filename: string, value: unknown): void {
  const blob = new Blob([JSON.stringify(value, null, 2)], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = filename;
  anchor.click();
  URL.revokeObjectURL(url);
}

function payloadFromForm(form: RectifyForm): RectifyDataSubjectPayload {
  const addressFilled = [form.street1, form.city, form.postalCode, form.country].some((value) => value.trim() !== '');
  const contactFilled = form.email.trim() !== '' || form.phone.trim() !== '';
  return {
    firstName: form.firstName.trim(),
    lastName: form.lastName.trim(),
    birthDate: form.birthDate ? new Date(`${form.birthDate}T00:00:00Z`).toISOString() : null,
    address: addressFilled
      ? {
          street1: form.street1.trim() || null,
          street2: null,
          city: form.city.trim() || null,
          postalCode: form.postalCode.trim() || null,
          country: form.country.trim() || null,
        }
      : null,
    contactInfo: contactFilled
      ? {
          email: form.email.trim() || null,
          phone: form.phone.trim() || null,
          alternativePhone: null,
        }
      : null,
    accountEmail: form.accountEmail.trim() || null,
  };
}

function DataSubjectRightsPage() {
  const { t } = useTranslation();
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<Person[]>([]);
  const [copy, setCopy] = useState<DataSubjectCopy | null>(null);
  const [form, setForm] = useState<RectifyForm>(emptyForm());
  const [isSearching, setIsSearching] = useState(false);
  const [isBusy, setIsBusy] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const applyCopy = (next: DataSubjectCopy): void => {
    setCopy(next);
    setForm(formFromCopy(next));
  };

  const run = async (action: () => Promise<DataSubjectCopy>): Promise<void> => {
    try {
      setIsBusy(true);
      setErrorMessage(null);
      setSuccessMessage(null);
      applyCopy(await action());
      setSuccessMessage(t('admin.dataSubjectRights.saved', 'Saved.'));
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : t('admin.dataSubjectRights.actionFailed', 'The request failed.');
      setErrorMessage(message);
    } finally {
      setIsBusy(false);
    }
  };

  const onSearch = async (event: FormEvent): Promise<void> => {
    event.preventDefault();
    try {
      setIsSearching(true);
      setErrorMessage(null);
      setSuccessMessage(null);
      const response = await personApi.search(query, 1, 10);
      setResults(response.data);
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : t('admin.dataSubjectRights.loadFailed', 'Search failed.');
      setErrorMessage(message);
      setResults([]);
    } finally {
      setIsSearching(false);
    }
  };

  const onSelect = async (personId: string): Promise<void> => {
    try {
      setIsBusy(true);
      setErrorMessage(null);
      setSuccessMessage(null);
      applyCopy(await dataSubjectRightsApi.getCopy(personId));
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : t('admin.dataSubjectRights.loadFailed', 'Failed to load the copy.');
      setErrorMessage(message);
    } finally {
      setIsBusy(false);
    }
  };

  const onRectify = (event: FormEvent): void => {
    event.preventDefault();
    if (!copy) {
      return;
    }
    void run(() => dataSubjectRightsApi.rectify(copy.personId, payloadFromForm(form)));
  };

  const onErase = (): void => {
    if (!copy) {
      return;
    }
    const confirmed = window.confirm(t(
      'admin.dataSubjectRights.eraseConfirm',
      'Remove this person\'s identity data? League records stay under an anonymized name when a legal basis remains.',
    ));
    if (!confirmed) {
      return;
    }
    void run(() => dataSubjectRightsApi.erase(copy.personId));
  };

  const anonymized = copy?.anonymizedAt != null;

  return (
    <PageTemplate title={t('admin.dataSubjectRights.pageTitle', 'Data subject rights')}>
      <div className="data-subject-rights">
        <p className="data-subject-rights__description">
          {t(
            'admin.dataSubjectRights.description',
            'Fulfill a verified request: give a copy, correct data, export data the person supplied, restrict processing, record an objection, or erase identity data.',
          )}
        </p>

        {errorMessage && <div className="data-subject-rights__alert data-subject-rights__alert--error">{errorMessage}</div>}
        {successMessage && <div className="data-subject-rights__alert data-subject-rights__alert--success">{successMessage}</div>}

        <form className="data-subject-rights__panel" onSubmit={(event) => { void onSearch(event); }}>
          <div className="data-subject-rights__search">
            <div className="data-subject-rights__field">
              <label htmlFor="data-subject-search">{t('admin.dataSubjectRights.searchLabel', 'Find a person')}</label>
              <input
                id="data-subject-search"
                value={query}
                onChange={(event) => setQuery(event.target.value)}
                placeholder={t('admin.dataSubjectRights.searchPlaceholder', 'Name')}
              />
            </div>
            <Button type="submit" isLoading={isSearching} disabled={query.trim() === ''}>
              {t('admin.dataSubjectRights.search', 'Search')}
            </Button>
          </div>
          {results.length === 0 ? (
            <p className="data-subject-rights__hint">{t('admin.dataSubjectRights.noResults', 'No search results yet.')}</p>
          ) : (
            <ul className="data-subject-rights__results">
              {results.map((person) => (
                <li key={person.id} className="data-subject-rights__result">
                  <span>{person.fullName}</span>
                  <Button type="button" variant="secondary" size="sm" onClick={() => { void onSelect(person.id); }}>
                    {t('admin.dataSubjectRights.select', 'Open')}
                  </Button>
                </li>
              ))}
            </ul>
          )}
        </form>

        {copy && (
          <>
            <section className="data-subject-rights__panel">
              <h2>{copy.fullName}</h2>
              <div className="data-subject-rights__flags">
                <span className="data-subject-rights__flag">
                  {copy.isProcessed
                    ? t('admin.dataSubjectRights.processed', 'Personal data is processed')
                    : t('admin.dataSubjectRights.notProcessed', 'Personal data is not processed')}
                </span>
                {copy.isProcessingRestricted && (
                  <span className="data-subject-rights__flag">{t('admin.dataSubjectRights.restricted', 'Processing restricted')}</span>
                )}
                {copy.hasObjectedToLegitimateInterest && (
                  <span className="data-subject-rights__flag">{t('admin.dataSubjectRights.objected', 'Objected to legitimate interest')}</span>
                )}
                {anonymized && (
                  <span className="data-subject-rights__flag">{t('admin.dataSubjectRights.anonymized', 'Identity removed')}</span>
                )}
              </div>
              <p className="data-subject-rights__hint">
                {copy.account
                  ? t('admin.dataSubjectRights.account', 'Account {{email}}, role {{role}}, active: {{active}}', {
                      email: copy.account.email,
                      role: copy.account.role,
                      active: copy.account.isActive ? t('admin.dataSubjectRights.yes', 'yes') : t('admin.dataSubjectRights.no', 'no'),
                    })
                  : t('admin.dataSubjectRights.noAccount', 'No user account.')}
              </p>
              <p className="data-subject-rights__hint">
                {copy.retentionReason
                  ? t('admin.dataSubjectRights.retention', 'Records that still have a basis: {{reason}}', { reason: copy.retentionReason })
                  : t('admin.dataSubjectRights.noRetention', 'No league, club-admin, or account link blocks a hard delete.')}
              </p>
              <div className="data-subject-rights__actions">
                <Button type="button" variant="secondary" onClick={() => downloadJson(`data-subject-${copy.personId}.json`, copy)}>
                  {t('admin.dataSubjectRights.downloadCopy', 'Download copy')}
                </Button>
                <Button type="button" variant="secondary" onClick={() => downloadJson(`data-subject-portable-${copy.personId}.json`, copy.portableData)}>
                  {t('admin.dataSubjectRights.downloadPortable', 'Download portable data')}
                </Button>
              </div>
            </section>

            <form className="data-subject-rights__panel" onSubmit={onRectify}>
              <h2>{t('admin.dataSubjectRights.rectifyTitle', 'Rectify')}</h2>
              <p className="data-subject-rights__hint">
                {t('admin.dataSubjectRights.rectifyHint', 'Correct the name, birth date, address, contact details, and account email the person supplied.')}
              </p>
              <div className="data-subject-rights__grid">
                <div className="data-subject-rights__field">
                  <label htmlFor="ds-first-name">{t('admin.dataSubjectRights.firstName', 'First name')}</label>
                  <input id="ds-first-name" value={form.firstName} disabled={anonymized || isBusy} onChange={(event) => setForm({ ...form, firstName: event.target.value })} />
                </div>
                <div className="data-subject-rights__field">
                  <label htmlFor="ds-last-name">{t('admin.dataSubjectRights.lastName', 'Last name')}</label>
                  <input id="ds-last-name" value={form.lastName} disabled={anonymized || isBusy} onChange={(event) => setForm({ ...form, lastName: event.target.value })} />
                </div>
                <div className="data-subject-rights__field">
                  <label htmlFor="ds-birth-date">{t('admin.dataSubjectRights.birthDate', 'Birth date')}</label>
                  <input id="ds-birth-date" type="date" value={form.birthDate} disabled={anonymized || isBusy} onChange={(event) => setForm({ ...form, birthDate: event.target.value })} />
                </div>
                <div className="data-subject-rights__field">
                  <label htmlFor="ds-account-email">{t('admin.dataSubjectRights.accountEmail', 'Account email')}</label>
                  <input id="ds-account-email" type="email" value={form.accountEmail} disabled={anonymized || isBusy || !copy.account} onChange={(event) => setForm({ ...form, accountEmail: event.target.value })} />
                </div>
                <div className="data-subject-rights__field">
                  <label htmlFor="ds-email">{t('admin.dataSubjectRights.email', 'Contact email')}</label>
                  <input id="ds-email" type="email" value={form.email} disabled={anonymized || isBusy} onChange={(event) => setForm({ ...form, email: event.target.value })} />
                </div>
                <div className="data-subject-rights__field">
                  <label htmlFor="ds-phone">{t('admin.dataSubjectRights.phone', 'Phone')}</label>
                  <input id="ds-phone" value={form.phone} disabled={anonymized || isBusy} onChange={(event) => setForm({ ...form, phone: event.target.value })} />
                </div>
                <div className="data-subject-rights__field">
                  <label htmlFor="ds-street">{t('admin.dataSubjectRights.street', 'Street')}</label>
                  <input id="ds-street" value={form.street1} disabled={anonymized || isBusy} onChange={(event) => setForm({ ...form, street1: event.target.value })} />
                </div>
                <div className="data-subject-rights__field">
                  <label htmlFor="ds-city">{t('admin.dataSubjectRights.city', 'City')}</label>
                  <input id="ds-city" value={form.city} disabled={anonymized || isBusy} onChange={(event) => setForm({ ...form, city: event.target.value })} />
                </div>
                <div className="data-subject-rights__field">
                  <label htmlFor="ds-postal">{t('admin.dataSubjectRights.postalCode', 'Postal code')}</label>
                  <input id="ds-postal" value={form.postalCode} disabled={anonymized || isBusy} onChange={(event) => setForm({ ...form, postalCode: event.target.value })} />
                </div>
                <div className="data-subject-rights__field">
                  <label htmlFor="ds-country">{t('admin.dataSubjectRights.country', 'Country')}</label>
                  <input id="ds-country" value={form.country} disabled={anonymized || isBusy} onChange={(event) => setForm({ ...form, country: event.target.value })} />
                </div>
              </div>
              <Button type="submit" disabled={anonymized || isBusy} isLoading={isBusy}>
                {t('admin.dataSubjectRights.saveRectification', 'Save correction')}
              </Button>
            </form>

            <section className="data-subject-rights__panel">
              <h2>{t('admin.dataSubjectRights.processingTitle', 'Restriction and objection')}</h2>
              <p className="data-subject-rights__hint">
                {t('admin.dataSubjectRights.restrictHint', 'Restriction suspends the account. Lifting it does not turn the account back on; do that on System Users.')}
              </p>
              <div className="data-subject-rights__actions">
                {copy.isProcessingRestricted ? (
                  <Button type="button" variant="secondary" disabled={anonymized || isBusy} onClick={() => { void run(() => dataSubjectRightsApi.setRestriction(copy.personId, false)); }}>
                    {t('admin.dataSubjectRights.liftRestriction', 'Lift restriction')}
                  </Button>
                ) : (
                  <Button type="button" variant="secondary" disabled={isBusy} onClick={() => { void run(() => dataSubjectRightsApi.setRestriction(copy.personId, true)); }}>
                    {t('admin.dataSubjectRights.restrict', 'Restrict processing')}
                  </Button>
                )}
                {copy.hasObjectedToLegitimateInterest ? (
                  <Button type="button" variant="secondary" disabled={anonymized || isBusy} onClick={() => { void run(() => dataSubjectRightsApi.setObjection(copy.personId, false)); }}>
                    {t('admin.dataSubjectRights.withdrawObjection', 'Withdraw objection')}
                  </Button>
                ) : (
                  <Button type="button" variant="secondary" disabled={isBusy} onClick={() => { void run(() => dataSubjectRightsApi.setObjection(copy.personId, true)); }}>
                    {t('admin.dataSubjectRights.object', 'Object to legitimate interest')}
                  </Button>
                )}
              </div>
            </section>

            <section className="data-subject-rights__panel">
              <h2>{t('admin.dataSubjectRights.eraseTitle', 'Erasure')}</h2>
              <p className="data-subject-rights__hint">
                {t('admin.dataSubjectRights.eraseHint', 'Identity, contact details, and the login address are removed. Match and statistics rows stay when a league basis remains, under the name Removed Person.')}
              </p>
              <Button type="button" variant="danger" disabled={anonymized || isBusy} onClick={onErase}>
                {t('admin.dataSubjectRights.erase', 'Erase personal data')}
              </Button>
            </section>
          </>
        )}
      </div>
    </PageTemplate>
  );
}

export default DataSubjectRightsPage;
