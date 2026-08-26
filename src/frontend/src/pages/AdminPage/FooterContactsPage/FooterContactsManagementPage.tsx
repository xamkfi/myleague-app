import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import Button from '../../../components/Button/Button';
import ConfirmationDialog from '../../../components/ConfirmationDialog/ConfirmationDialog';
import { footerContactService } from '../../../api/common/footerContactService';
import type { FooterContact, FooterContactRequest } from '../../../types/admin/footerContactTypes';
import FooterContactForm from './components/FooterContactForm';
import './FooterContactsManagementPage.scss';

function FooterContactsManagementPage() {
  const { t } = useTranslation();
  const [contacts, setContacts] = useState<FooterContact[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editing, setEditing] = useState<FooterContact | null>(null);
  const [contactToDelete, setContactToDelete] = useState<FooterContact | null>(null);

  const loadContacts = async (): Promise<void> => {
    const items = await footerContactService.getAll();
    setContacts(items);
  };

  useEffect(() => {
    let isMounted = true;

    const initialize = async (): Promise<void> => {
      try {
        setIsLoading(true);
        setErrorMessage(null);
        await loadContacts();
      } catch (error) {
        if (!isMounted) {
          return;
        }

        setErrorMessage(
          error instanceof Error
            ? error.message
            : t('admin.siteContent.footerContacts.loadFailed', 'Yhteystietojen lataus epäonnistui.'),
        );
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    void initialize();

    return () => {
      isMounted = false;
    };
  }, [t]);

  useEffect(() => {
    if (!successMessage) {
      return;
    }

    const timeout = setTimeout(() => setSuccessMessage(null), 5000);
    return () => clearTimeout(timeout);
  }, [successMessage]);

  const handleSave = async (payload: FooterContactRequest): Promise<void> => {
    try {
      setIsSaving(true);
      setErrorMessage(null);

      if (editing) {
        await footerContactService.update(editing.id, payload);
        setSuccessMessage(
          t('admin.siteContent.footerContacts.updateSuccess', 'Yhteystieto päivitetty.'),
        );
      } else {
        await footerContactService.create(payload);
        setSuccessMessage(
          t('admin.siteContent.footerContacts.createSuccess', 'Yhteystieto lisätty.'),
        );
      }

      await loadContacts();
      setIsFormOpen(false);
      setEditing(null);
    } catch (error) {
      setErrorMessage(
        error instanceof Error
          ? error.message
          : t('admin.siteContent.footerContacts.saveFailed', 'Tallennus epäonnistui.'),
      );
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async (): Promise<void> => {
    if (!contactToDelete) {
      return;
    }

    try {
      setIsSaving(true);
      await footerContactService.remove(contactToDelete.id);
      setSuccessMessage(
        t('admin.siteContent.footerContacts.deleteSuccess', 'Yhteystieto poistettu.'),
      );
      setContactToDelete(null);
      await loadContacts();
    } catch (error) {
      setErrorMessage(
        error instanceof Error
          ? error.message
          : t('admin.siteContent.footerContacts.deleteFailed', 'Poisto epäonnistui.'),
      );
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <PageTemplate title={t('admin.siteContent.footerContacts.pageTitle', 'Yhteystiedot')}>
      <div className="footer-contacts-page">
        {successMessage && (
          <p className="footer-contacts-page__alert footer-contacts-page__alert--success">
            {successMessage}
          </p>
        )}
        {errorMessage && (
          <p className="footer-contacts-page__alert footer-contacts-page__alert--error">
            {errorMessage}
          </p>
        )}

        <p className="footer-contacts-page__description">
          {t(
            'admin.siteContent.footerContacts.description',
            'Hallitse julkisen sivuston footerissa näytettäviä yhteystietoja. Jokainen yhteystieto näkyy omana laatikkona.',
          )}
        </p>

        {isFormOpen ? (
          <FooterContactForm
            contact={editing}
            isSaving={isSaving}
            onCancel={() => {
              setIsFormOpen(false);
              setEditing(null);
            }}
            onSave={(payload) => {
              void handleSave(payload);
            }}
          />
        ) : (
          <>
            <div className="footer-contacts-page__toolbar">
              <Button
                onClick={() => {
                  setEditing(null);
                  setIsFormOpen(true);
                }}
              >
                {t('admin.siteContent.footerContacts.add', 'Lisää yhteystieto')}
              </Button>
            </div>

            {isLoading ? (
              <p>{t('common.loading', 'Ladataan...')}</p>
            ) : contacts.length === 0 ? (
              <p>{t('admin.siteContent.footerContacts.empty', 'Yhteystietoja ei ole vielä.')}</p>
            ) : (
              <ul className="footer-contacts-page__list">
                {contacts.map((contact) => (
                  <li key={contact.id} className="footer-contacts-page__card">
                    <div>
                      <h3>{contact.title}</h3>
                      {contact.details && (
                        <p className="footer-contacts-page__details">{contact.details}</p>
                      )}
                      {contact.email && <p>{contact.email}</p>}
                      {contact.phone && <p>{contact.phone}</p>}
                      {contact.url && <p>{contact.url}</p>}
                    </div>
                    <div className="footer-contacts-page__card-actions">
                      <Button
                        variant="secondary"
                        size="sm"
                        onClick={() => {
                          setEditing(contact);
                          setIsFormOpen(true);
                        }}
                      >
                        {t('common.edit', 'Muokkaa')}
                      </Button>
                      <Button
                        variant="danger"
                        size="sm"
                        onClick={() => setContactToDelete(contact)}
                      >
                        {t('common.delete', 'Poista')}
                      </Button>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </>
        )}
      </div>

      <ConfirmationDialog
        isOpen={contactToDelete !== null}
        icon="⚠️"
        title={t('admin.siteContent.footerContacts.confirmDeleteTitle', 'Poista yhteystieto')}
        message={t(
          'admin.siteContent.footerContacts.confirmDelete',
          'Poistetaanko yhteystieto "{{title}}"?',
          { title: contactToDelete?.title ?? '' },
        )}
        confirmText={t('common.delete', 'Poista')}
        cancelText={t('common.cancel', 'Peruuta')}
        isLoading={isSaving}
        onConfirm={() => {
          void handleDelete();
        }}
        onCancel={() => setContactToDelete(null)}
      />
    </PageTemplate>
  );
}

export default FooterContactsManagementPage;
