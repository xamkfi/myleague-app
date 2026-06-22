import { useEffect, useState } from 'react';
import AdminPageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import { siteSettingsService, type FooterContactPerson } from '../../../api/common/siteSettingsService';
import LoadingSpinner from '../../../components/LoadingSpinner/LoadingSpinner';
import ErrorPopup from '../../../components/ErrorPopup/ErrorPopup';
import '../../../styles/AdminTable.scss';
import './FooterContactManagementPage.scss';

export default function FooterContactManagementPage() {
  const [organizationName, setOrganizationName] = useState('');
  const [organizationAddress, setOrganizationAddress] = useState('');
  const [organizationDraftName, setOrganizationDraftName] = useState('');
  const [organizationDraftAddress, setOrganizationDraftAddress] = useState('');
  const [isOrganizationEditing, setIsOrganizationEditing] = useState(false);
  const [contactForm, setContactForm] = useState<FooterContactPerson>({ nameOrRole: '', email: '', phone: '' });
  const [editingContactIndex, setEditingContactIndex] = useState<number | null>(null);
  const [savedContactPersons, setSavedContactPersons] = useState<FooterContactPerson[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [lastModifiedInfo, setLastModifiedInfo] = useState<{ by: string | null; at: string | null }>({ by: null, at: null });

  useEffect(() => {
    const loadData = async () => {
      try {
        setIsLoading(true);
        setError(null);

        const data = await siteSettingsService.getFooterContact();
        setOrganizationName(data.organizationName || '');
        setOrganizationAddress(data.organizationAddress || '');
        setOrganizationDraftName(data.organizationName || '');
        setOrganizationDraftAddress(data.organizationAddress || '');
        setSavedContactPersons(data.contactPersons?.filter((x) => x.nameOrRole || x.email || x.phone) ?? []);
        setLastModifiedInfo({ by: data.lastModifiedBy, at: data.updatedAt });
      } catch (err) {
        console.error('Failed to load footer contact settings:', err);
        setError(err instanceof Error ? err.message : 'Yhteystietojen lataus epäonnistui.');
      } finally {
        setIsLoading(false);
      }
    };

    loadData();
  }, []);

  const updateContactForm = (field: keyof FooterContactPerson, value: string) => {
    setContactForm((prev) => ({ ...prev, [field]: value }));
  };

  const clearContactForm = () => {
    setContactForm({ nameOrRole: '', email: '', phone: '' });
    setEditingContactIndex(null);
  };

  const persistSettings = async (
    nextOrganizationName: string,
    nextOrganizationAddress: string,
    nextContactPersons: FooterContactPerson[],
    successText: string,
  ) => {
    try {
      setIsSaving(true);
      setError(null);
      setSuccessMessage(null);

      const payload = {
        organizationName: nextOrganizationName.trim(),
        organizationAddress: nextOrganizationAddress.trim(),
        contactPersons: nextContactPersons
          .map((person) => ({
            nameOrRole: person.nameOrRole.trim(),
            email: person.email.trim(),
            phone: person.phone.trim(),
          }))
          .filter((person) => person.nameOrRole || person.email || person.phone),
      };

      const saved = await siteSettingsService.updateFooterContact(payload);
      const normalizedSavedPersons = saved.contactPersons?.filter((x) => x.nameOrRole || x.email || x.phone) ?? [];

      setOrganizationName(saved.organizationName || '');
      setOrganizationAddress(saved.organizationAddress || '');
      setOrganizationDraftName(saved.organizationName || '');
      setOrganizationDraftAddress(saved.organizationAddress || '');
      setSavedContactPersons(normalizedSavedPersons);
      setLastModifiedInfo({ by: saved.lastModifiedBy, at: saved.updatedAt });
      setSuccessMessage(successText);
    } catch (err) {
      console.error('Failed to save footer contact settings:', err);
      setError(err instanceof Error ? err.message : 'Tallennus epäonnistui.');
    } finally {
      setIsSaving(false);
    }
  };

  const handleSaveOrganization = async () => {
    if (!organizationDraftName.trim() || !organizationDraftAddress.trim()) {
      setError('Organisaation nimi ja osoite ovat pakollisia.');
      return;
    }

    if (savedContactPersons.length === 0) {
      setError('Lisää vähintään yksi yhteyshenkilö ennen organisaation tallennusta.');
      return;
    }

    await persistSettings(
      organizationDraftName,
      organizationDraftAddress,
      savedContactPersons,
      'Organisaation tiedot tallennettu onnistuneesti.',
    );

    setIsOrganizationEditing(false);
  };

  const handleEditOrganization = () => {
    setOrganizationDraftName(organizationName);
    setOrganizationDraftAddress(organizationAddress);
    setIsOrganizationEditing(true);
    setSuccessMessage(null);
  };

  const handleCancelOrganizationEdit = () => {
    setOrganizationDraftName(organizationName);
    setOrganizationDraftAddress(organizationAddress);
    setIsOrganizationEditing(false);
  };

  const handleSaveContact = async () => {
    if (!contactForm.nameOrRole.trim() || !contactForm.email.trim() || !contactForm.phone.trim()) {
      setError('Yhteyshenkilön nimi/rooli, sähköposti ja puhelin ovat pakollisia.');
      return;
    }

    const nextContacts = [...savedContactPersons];
    const normalizedForm: FooterContactPerson = {
      nameOrRole: contactForm.nameOrRole.trim(),
      email: contactForm.email.trim(),
      phone: contactForm.phone.trim(),
    };

    if (editingContactIndex === null) {
      nextContacts.push(normalizedForm);
    } else {
      nextContacts[editingContactIndex] = normalizedForm;
    }

    await persistSettings(
      organizationName,
      organizationAddress,
      nextContacts,
      editingContactIndex === null
        ? 'Yhteyshenkilö tallennettu onnistuneesti.'
        : 'Yhteyshenkilön tiedot päivitetty onnistuneesti.',
    );

    clearContactForm();
  };

  const handleEditContact = (index: number) => {
    setEditingContactIndex(index);
    setContactForm(savedContactPersons[index]);
    setSuccessMessage(null);
  };

  const handleDeleteContact = async (index: number) => {
    if (savedContactPersons.length <= 1) {
      setError('Vähintään yksi yhteyshenkilö on pakollinen.');
      return;
    }

    const nextContacts = savedContactPersons.filter((_, i) => i !== index);
    await persistSettings(
      organizationName,
      organizationAddress,
      nextContacts,
      'Yhteyshenkilö poistettu onnistuneesti.',
    );

    if (editingContactIndex === index) {
      clearContactForm();
    }
  };

  if (isLoading) {
    return (
      <AdminPageTemplate title="Footerin yhteystiedot">
        <div className="footer-contact-management-page__loading">
          <LoadingSpinner text="Ladataan footerin yhteystietoja..." />
        </div>
      </AdminPageTemplate>
    );
  }

  return (
    <AdminPageTemplate title="Footerin yhteystiedot">
      <div className="footer-contact-management-page">
        <ErrorPopup message={error} />

        {successMessage && (
          <div className="footer-contact-management-page__success">{successMessage}</div>
        )}

        <div className="footer-contact-management-page__card">
          <div className="footer-contact-management-page__header-row">
            <h2 className="footer-contact-management-page__card-title">Organisaatio</h2>
            {!isOrganizationEditing ? (
              <button
                type="button"
                className="footer-contact-management-page__add-btn"
                onClick={handleEditOrganization}
                disabled={isSaving}
              >
                Muokkaa
              </button>
            ) : (
              <div className="footer-contact-management-page__contact-actions">
                <button
                  type="button"
                  className="footer-contact-management-page__add-btn"
                  onClick={handleCancelOrganizationEdit}
                  disabled={isSaving}
                >
                  Peruuta
                </button>
                <button
                  type="button"
                  className="footer-contact-management-page__save-btn"
                  onClick={handleSaveOrganization}
                  disabled={isSaving}
                >
                  {isSaving ? 'Tallennetaan...' : 'Tallenna'}
                </button>
              </div>
            )}
          </div>

          {!isOrganizationEditing ? (
            <div className="footer-contact-management-page__locked-organization">
              <div className="footer-contact-management-page__locked-row">
                <span className="footer-contact-management-page__locked-label">Nimi</span>
                <span>{organizationName || '-'}</span>
              </div>
              <div className="footer-contact-management-page__locked-row">
                <span className="footer-contact-management-page__locked-label">Osoite</span>
                <span>{organizationAddress || '-'}</span>
              </div>
            </div>
          ) : (
            <>
              <div className="footer-contact-management-page__field">
                <label>Nimi</label>
                <input
                  type="text"
                  className="footer-contact-management-page__input"
                  value={organizationDraftName}
                  onChange={(e) => setOrganizationDraftName(e.target.value)}
                  disabled={isSaving}
                />
              </div>

              <div className="footer-contact-management-page__field">
                <label>Osoite</label>
                <textarea
                  className="footer-contact-management-page__textarea"
                  value={organizationDraftAddress}
                  onChange={(e) => setOrganizationDraftAddress(e.target.value)}
                  disabled={isSaving}
                />
              </div>
            </>
          )}
        </div>

        <div className="footer-contact-management-page__card">
          <div className="footer-contact-management-page__header-row">
            <h2 className="footer-contact-management-page__card-title">Yhteyshenkilöt</h2>
          </div>

          <div className="footer-contact-management-page__person-card">
            <div className="footer-contact-management-page__person-header">
              <h3>{editingContactIndex === null ? 'Lisää yhteyshenkilö' : `Muokkaa yhteyshenkilöä #${editingContactIndex + 1}`}</h3>
            </div>

            <input
              type="text"
              className="footer-contact-management-page__input"
              placeholder="Nimi / rooli"
              value={contactForm.nameOrRole}
              onChange={(e) => updateContactForm('nameOrRole', e.target.value)}
              disabled={isSaving}
            />

            <input
              type="email"
              className="footer-contact-management-page__input"
              placeholder="Sähköposti"
              value={contactForm.email}
              onChange={(e) => updateContactForm('email', e.target.value)}
              disabled={isSaving}
            />

            <input
              type="text"
              className="footer-contact-management-page__input"
              placeholder="Puhelin"
              value={contactForm.phone}
              onChange={(e) => updateContactForm('phone', e.target.value)}
              disabled={isSaving}
            />

            <div className="footer-contact-management-page__contact-actions">
              {editingContactIndex !== null && (
                <button
                  type="button"
                  className="footer-contact-management-page__add-btn"
                  onClick={clearContactForm}
                  disabled={isSaving}
                >
                  Peruuta muokkaus
                </button>
              )}

              <button
                type="button"
                className="footer-contact-management-page__save-btn"
                onClick={handleSaveContact}
                disabled={isSaving}
              >
                {isSaving ? 'Tallennetaan...' : editingContactIndex === null ? 'Tallenna yhteyshenkilö' : 'Päivitä yhteyshenkilö'}
              </button>
            </div>
          </div>
        </div>

        <div className="footer-contact-management-page__card">
          <h2 className="footer-contact-management-page__card-title">Tallennetut yhteyshenkilöt</h2>

          <div className="admin-table__wrapper">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Nimi / rooli</th>
                  <th>Sähköposti</th>
                  <th>Puhelin</th>
                  <th className="admin-table__actions-col">Toiminnot</th>
                </tr>
              </thead>
              <tbody>
                {savedContactPersons.length === 0 && (
                  <tr>
                    <td colSpan={4} className="footer-contact-management-page__empty-cell">
                      Tallennettuja yhteyshenkilöitä ei ole.
                    </td>
                  </tr>
                )}

                {savedContactPersons.map((person, index) => (
                  <tr key={`${person.nameOrRole}-${person.email}-${index}`}>
                    <td>{person.nameOrRole || '-'}</td>
                    <td>{person.email || '-'}</td>
                    <td>{person.phone || '-'}</td>
                    <td className="admin-table__actions-col">
                      <div className="footer-contact-management-page__table-actions">
                        <button
                          type="button"
                          className="footer-contact-management-page__table-action-btn"
                          onClick={() => handleEditContact(index)}
                          disabled={isSaving}
                        >
                          Muokkaa
                        </button>
                        <button
                          type="button"
                          className="footer-contact-management-page__table-action-btn footer-contact-management-page__table-action-btn--danger"
                          onClick={() => handleDeleteContact(index)}
                          disabled={isSaving || savedContactPersons.length <= 1}
                        >
                          Poista
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        <div className="footer-contact-management-page__footer-row">
          <div className="footer-contact-management-page__meta">
            {lastModifiedInfo.at ? `Viimeksi muokattu: ${new Date(lastModifiedInfo.at).toLocaleString('fi-FI')} (${lastModifiedInfo.by || 'Tuntematon'})` : 'Ei tallennushistoriaa'}
          </div>
        </div>
      </div>
    </AdminPageTemplate>
  );
}
