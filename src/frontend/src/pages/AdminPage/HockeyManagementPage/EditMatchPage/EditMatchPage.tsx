import { useCallback, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import HockeyMatchForm, { type HockeyMatchFormValues } from '../components/HockeyMatchForm';
import { hockeyMatchService } from '../../../../api/hockey/hockeyMatchService';
import { hockeyTeamService } from '../../../../api/hockey/hockeyTeamService';
import { hockeySeasonService } from '../../../../api/hockey/hockeySeasonService';
import { hockeyTournamentService } from '../../../../api/hockey/hockeyTournamentService';
import type { HockeyMatchDto } from '../../../../types/hockey/hockeyTypes';
import { joinHockeyDateTime, splitHockeyDateTime } from '../../../../utils/hockeyLookups';

function EditHockeyMatchPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { matchId } = useParams<{ matchId: string }>();
  const [match, setMatch] = useState<HockeyMatchDto | null>(null);
  const [competitions, setCompetitions] = useState<Array<{ id: string; name: string }>>([]);
  const [teams, setTeams] = useState<Array<{ id: string; name: string }>>([]);
  const [values, setValues] = useState<HockeyMatchFormValues>({
    competitionId: '',
    homeTeamId: '',
    awayTeamId: '',
    date: '',
    hours: '',
    minutes: '',
    venue: '',
    matchType: 'League',
  });
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [showCancelConfirm, setShowCancelConfirm] = useState(false);
  const [showReactivateConfirm, setShowReactivateConfirm] = useState(false);

  const load = useCallback(async (): Promise<void> => {
    if (!matchId) {
      return;
    }
    const [loaded, teamList, seasons, tournaments] = await Promise.all([
      hockeyMatchService.getById(matchId),
      hockeyTeamService.getAll(),
      hockeySeasonService.getAll(),
      hockeyTournamentService.getAll(),
    ]);
    setMatch(loaded);
    setTeams(teamList.map((team) => ({ id: team.id, name: team.name })));
    setCompetitions([
      ...seasons.map((item) => ({ id: item.id, name: item.name })),
      ...tournaments.map((item) => ({ id: item.id, name: item.name })),
    ]);
    const split = splitHockeyDateTime(loaded.scheduledStartTime);
    setValues({
      competitionId: loaded.competitionId ?? '',
      homeTeamId: loaded.homeTeamId ?? '',
      awayTeamId: loaded.awayTeamId ?? '',
      date: split.date,
      hours: split.hours,
      minutes: split.minutes,
      venue: loaded.venue ?? '',
      matchType: loaded.matchType as HockeyMatchFormValues['matchType'],
    });
  }, [matchId]);

  useEffect(() => {
    void load().catch((err) => setError(err instanceof Error ? err.message : 'Failed to load match'));
  }, [load]);

  const handleSubmit = async (): Promise<void> => {
    if (!match) {
      return;
    }
    setSaving(true);
    setError(null);
    try {
      await hockeyMatchService.updateSchedule(match.id, joinHockeyDateTime(values.date, values.hours, values.minutes));
      await hockeyMatchService.updateVenue(match.id, values.venue);
      if (values.homeTeamId && values.awayTeamId) {
        await hockeyMatchService.assignTeams(match.id, values.homeTeamId, values.awayTeamId);
      }
      navigate('/admin/hockey/matches');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update match');
    } finally {
      setSaving(false);
    }
  };

  const handleCancelMatch = async (): Promise<void> => {
    if (!match) {
      return;
    }
    setSaving(true);
    try {
      await hockeyMatchService.setStatus(match.id, 'Cancelled');
      navigate('/admin/hockey/matches');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to cancel match');
    } finally {
      setSaving(false);
      setShowCancelConfirm(false);
    }
  };

  const handleReactivateMatch = async (): Promise<void> => {
    if (!match) {
      return;
    }
    setSaving(true);
    try {
      await hockeyMatchService.setStatus(match.id, 'Scheduled');
      navigate('/admin/hockey/matches');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to reactivate match');
    } finally {
      setSaving(false);
      setShowReactivateConfirm(false);
    }
  };

  const isTournament = String(match?.matchType ?? '').startsWith('Tournament');

  return (
    <PageTemplate title={t('hockey.matches.edit', 'Edit match')}>
      <ErrorPopup message={error} />
      {!match ? (
        <p>{t('common.loading', 'Loading...')}</p>
      ) : (
        <HockeyMatchForm
          mode="edit"
          competitionKind={isTournament ? 'tournament' : 'season'}
          values={values}
          competitions={competitions}
          teams={teams}
          loading={saving}
          matchStatus={match.status}
          showCancelConfirm={showCancelConfirm}
          showReactivateConfirm={showReactivateConfirm}
          onChange={setValues}
          onSubmit={() => void handleSubmit()}
          onCancel={() => navigate('/admin/hockey/matches')}
          onCancelMatch={() => void handleCancelMatch()}
          onReactivateMatch={() => void handleReactivateMatch()}
          onOpenCancelConfirm={() => setShowCancelConfirm(true)}
          onOpenReactivateConfirm={() => setShowReactivateConfirm(true)}
          onCloseCancelConfirm={() => setShowCancelConfirm(false)}
          onCloseReactivateConfirm={() => setShowReactivateConfirm(false)}
        />
      )}
    </PageTemplate>
  );
}

export default EditHockeyMatchPage;
