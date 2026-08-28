import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import HockeyMatchForm, { type HockeyMatchFormValues } from '../components/HockeyMatchForm';
import { hockeyMatchService } from '../../../../api/hockey/hockeyMatchService';
import { hockeySeasonService } from '../../../../api/hockey/hockeySeasonService';
import { hockeyTournamentService } from '../../../../api/hockey/hockeyTournamentService';
import { hockeyTeamService } from '../../../../api/hockey/hockeyTeamService';
import { joinHockeyDateTime } from '../../../../utils/hockeyLookups';

interface CreateHockeyMatchPageProps {
  mode?: 'season' | 'tournament';
}

function CreateHockeyMatchPage({ mode = 'season' }: CreateHockeyMatchPageProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
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
    matchType: mode === 'tournament' ? 'TournamentGroup' : 'League',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async (): Promise<void> => {
      const [teamList, competitionList] = await Promise.all([
        hockeyTeamService.getAll(),
        mode === 'tournament' ? hockeyTournamentService.getAll() : hockeySeasonService.getAll(),
      ]);
      setTeams(teamList.map((team) => ({ id: team.id, name: team.name })));
      setCompetitions(competitionList.map((item) => ({ id: item.id, name: item.name })));
    };
    void load().catch((err) => setError(err instanceof Error ? err.message : 'Failed to load form data'));
  }, [mode]);

  const handleSubmit = async (): Promise<void> => {
    setLoading(true);
    setError(null);
    try {
      const created = await hockeyMatchService.create({
        scheduledStartTime: joinHockeyDateTime(values.date, values.hours, values.minutes),
        matchType: values.matchType,
        competitionId: values.competitionId || undefined,
        venue: values.venue || undefined,
      });
      if (values.homeTeamId && values.awayTeamId) {
        await hockeyMatchService.assignTeams(created.id, values.homeTeamId, values.awayTeamId);
      }
      navigate(`/admin/hockey/matches/manage/${created.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create match');
    } finally {
      setLoading(false);
    }
  };

  return (
    <PageTemplate title={t('hockey.matches.create', 'Create match')}>
      <ErrorPopup message={error} />
      <HockeyMatchForm
        mode="create"
        competitionKind={mode}
        values={values}
        competitions={competitions}
        teams={teams}
        loading={loading}
        onChange={setValues}
        onSubmit={() => void handleSubmit()}
        onCancel={() => navigate('/admin/hockey/matches')}
      />
    </PageTemplate>
  );
}

export default CreateHockeyMatchPage;
