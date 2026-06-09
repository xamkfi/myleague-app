import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type { 
  FloorballMatchDto,
  CreateFloorballMatchRequest,
  ChangeMatchSeasonRequest,
  ChangeMatchTeamsRequest,
  ChangeMatchVenueRequest,
  ChangeMatchDateTimeRequest
} from '../../../../types/floorball/floorballTypes';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { floorballMatchEventService } from '../../../../api/floorball/floorballMatchEventService';
import MatchForm from '../Components/MatchForm/MatchForm';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import './EditMatchPage.scss';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';

const EditMatchPage = () => {
  const { t } = useTranslation();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [matchData, setMatchData] = useState<FloorballMatchDto | null>(null);
  const navigate = useNavigate();
  const { matchId } = useParams<{ matchId: string }>();

  useEffect(() => {
    if (!matchId) {
      setError('Match ID is missing');
      return;
    }

    const fetchMatchData = async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await floorballMatchService.getById(matchId);
        if (response.success && response.data) {
          setMatchData(response.data);
        } else {
          setError('Failed to fetch match data');
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An unknown error occurred');
      } finally {
        setLoading(false);
      }
    };

    fetchMatchData();
  }, [matchId]);

  const handleCancelMatch = async (matchId: string) => {
    try {
      setLoading(true);
      setError(null);
      await floorballMatchEventService.cancelMatch(matchId);
      navigate('/admin/floorball/matches');
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'An unknown error occurred during cancellation';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleReactivateMatch = async (matchId: string) => {
    try {
      setLoading(true);
      setError(null);
      await floorballMatchEventService.reactivateMatch(matchId);
      navigate('/admin/floorball/matches');
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'An unknown error occurred during reactivation';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateMatch = async (updatedData: CreateFloorballMatchRequest) => {
    if (!matchData) {
      setError('Original match data is not available.');
      return;
    }

    try {
      setLoading(true);
      setError(null);

      const changes: Promise<unknown>[] = [];

      // Changing the competition (season / tournament) of an already-created match is not
      // currently supported by the backend — the previous controller exposed a route that
      // never existed in any controller's implementation, so the request silently 404'd. We
      // surface that explicitly here instead of pretending the change succeeded.
      if (updatedData.competitionId && updatedData.competitionId !== matchData.competitionId) {
        throw new Error(
          'Changing the competition (season/tournament) of an existing match is not supported. ' +
          'Delete the match and create a new one in the target competition instead.'
        );
      }

      // Detect ANY change to the team slots — including clearing a slot back to TBD or filling in
      // a previously empty slot. The form treats both fields as optional, so undefined ↔ null are
      // interchangeable from the form's perspective; normalize both to null for the API.
      const normalizedHome: string | null = updatedData.homeTeamId ?? null;
      const normalizedAway: string | null = updatedData.awayTeamId ?? null;
      const homeChanged: boolean = normalizedHome !== (matchData.homeTeamId ?? null);
      const awayChanged: boolean = normalizedAway !== (matchData.awayTeamId ?? null);
      if (homeChanged || awayChanged) {
        // Route through the new AssignMatchTeams endpoint so the backend can propagate the change
        // forward through the playoff bracket where applicable. Works for the "create with no
        // teams → fill them in later" flow as well as for jury overrides.
        changes.push(floorballMatchService.assignTeams(matchData.id, {
          homeTeamId: normalizedHome,
          awayTeamId: normalizedAway,
        }));
      }
      
      if (updatedData.venue !== matchData.venue) {
        changes.push(floorballMatchService.changeVenue(matchData.id, updatedData.venue || ''));
      }
      
      if (updatedData.scheduledDateTime !== matchData.scheduledDateTime) {
        changes.push(floorballMatchService.changeDateTime(matchData.id, updatedData.scheduledDateTime));
      }

      if (updatedData.refereeId && updatedData.refereeId !== matchData.refereeId) {
        changes.push(floorballMatchService.changeReferee(matchData.id, updatedData.refereeId));
      }

      if (changes.length === 0) {
        setError('No changes were detected.');
        return;
      }

      await Promise.all(changes);

      navigate('/admin/floorball/matches');

    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'An unknown error occurred during update';
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const handleFormSubmit = (
    formData: CreateFloorballMatchRequest | ChangeMatchSeasonRequest | ChangeMatchTeamsRequest | ChangeMatchVenueRequest | ChangeMatchDateTimeRequest
  ) => {
    return handleUpdateMatch(formData as CreateFloorballMatchRequest);
  };

  const handleCancel = () => {
    navigate('/admin/floorball/matches');
  };

  if (loading) {
    return <div>{t('floorball.matches.matchForm.loading', 'Ladataan ottelua...')}</div>;
  }

  // Detect tournament matches so the form switches its competition dropdown to tournaments.
  const isTournamentMatch: boolean = Boolean(
    matchData?.tournamentGroupId ||
      (matchData?.tournamentStage && matchData.tournamentStage !== 'None')
  );
  const pageTitle: string = isTournamentMatch
    ? t('floorball.matches.matchForm.editTournamentMatch', 'Muokkaa turnausottelua')
    : t('floorball.matches.matchForm.editSeasonMatch', 'Muokkaa kauden ottelua');

  return (
    <PageTemplate title={pageTitle}>
    <div className="match-management">
      <div className="match-management__content edit-match-page">
        <div className="page-header">
          <div className="header-left">
          </div>
          <div className="header-center">
            <h1>{pageTitle}</h1>
          </div>
          <div className="header-right"></div>
        </div>

        <ErrorPopup message={error} />

        <div className="form-container">
          {matchData ? (
            <MatchForm
              mode="edit"
              initialData={matchData}
              onSubmit={handleFormSubmit}
              onCancel={handleCancel}
              onCancelMatch={handleCancelMatch}
              onReactivateMatch={handleReactivateMatch}
              loading={loading}
              competitionKind={isTournamentMatch ? 'tournament' : 'season'}
            />
          ) : (
            <p>{t('floorball.matches.matchForm.notLoaded', 'Ottelun tietojen lataus epäonnistui.')}</p>
          )}
        </div>
      </div>
    </div>
    </PageTemplate>
  );
};

export default EditMatchPage;
