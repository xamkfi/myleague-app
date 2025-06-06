import { useState } from 'react';
import teamData from './teamDetails.json'
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import { useTranslation } from 'react-i18next';

export default function TeamPage() {
    
    const { t } = useTranslation(); 
    const { teamDetails } = teamData;
    const [activeTab, setActiveTab] = useState('MATCHES');
    const navigate = useNavigate();

    return (
        <PageTemplate title={t('nav.team')}>
        <div className="w-full mx-auto text-left">
            <div className="bg-white rounded-lg p-4 shadow-sm">
                {/* Team Header */}
                <div className="flex items-center gap-4 mb-6 pl-2">
                    <div className="relative">
                        <img 
                            src={teamDetails.logo} 
                            alt={`${teamDetails.name} logo`} 
                            className="w-20 h-20 object-contain"
                        />
                    </div>
                    <div>
                        <h1 className="text-2xl font-bold">{teamDetails.name}</h1>
                        <p className="text-gray-600">{teamDetails.fullName}</p>
                        <div className="mt-1 flex items-center gap-4 text-sm text-gray-500">
                            <span>{teamDetails.sportType}</span>
                            <span>•</span>
                            <span>Founded {teamDetails.founded}</span>
                            <span>•</span>
                            <span>{teamDetails.homeVenue}</span>
                        </div>
                    </div>
                </div>

                {/* League Position Summary */}
                <div className="bg-blue-50 p-4 rounded-lg mb-6">
                    <h2 className="text-blue-900 font-semibold mb-2">League Position</h2>
                    <div className="grid grid-cols-6 gap-2 text-center">
                        <div className="bg-white p-2 rounded shadow-sm">
                            <div className="text-xl font-bold text-blue-600">{teamDetails.leaguePosition.position}</div>
                            <div className="text-xs text-gray-500">Position</div>
                        </div>
                        <div className="bg-white p-2 rounded shadow-sm">
                            <div className="text-xl font-bold">{teamDetails.leaguePosition.played}</div>
                            <div className="text-xs text-gray-500">Played</div>
                        </div>
                        <div className="bg-white p-2 rounded shadow-sm">
                            <div className="text-xl font-bold text-green-600">{teamDetails.leaguePosition.won}</div>
                            <div className="text-xs text-gray-500">Won</div>
                        </div>
                        <div className="bg-white p-2 rounded shadow-sm">
                            <div className="text-xl font-bold text-gray-600">{teamDetails.leaguePosition.drawn}</div>
                            <div className="text-xs text-gray-500">Drawn</div>
                        </div>
                        <div className="bg-white p-2 rounded shadow-sm">
                            <div className="text-xl font-bold text-red-600">{teamDetails.leaguePosition.lost}</div>
                            <div className="text-xs text-gray-500">Lost</div>
                        </div>
                        <div className="bg-white p-2 rounded shadow-sm">
                            <div className="text-xl font-bold">{teamDetails.leaguePosition.points}</div>
                            <div className="text-xs text-gray-500">Points</div>
                        </div>
                    </div>
                </div>

                {/* Navigation Tabs */}
                <div className="border-b border-gray-200 mb-4">
                    <nav className="flex gap-6 text-sm">
                        <button 
                            className={`pb-2 font-medium ${activeTab === 'MATCHES' ? 'text-pink-500 border-b-2 border-pink-500' : 'text-gray-500'}`}
                            onClick={() => setActiveTab('MATCHES')}
                        >
                            MATCHES
                        </button>
                        <button 
                            className={`pb-2 font-medium ${activeTab === 'RESULTS' ? 'text-pink-500 border-b-2 border-pink-500' : 'text-gray-500'}`}
                            onClick={() => setActiveTab('RESULTS')}
                        >
                            RESULTS
                        </button>
                        <button 
                            className={`pb-2 font-medium ${activeTab === 'SQUAD' ? 'text-pink-500 border-b-2 border-pink-500' : 'text-gray-500'}`}
                            onClick={() => setActiveTab('SQUAD')}
                        >
                            SQUAD
                        </button>
                        <button 
                            className={`pb-2 font-medium ${activeTab === 'STATS' ? 'text-pink-500 border-b-2 border-pink-500' : 'text-gray-500'}`}
                            onClick={() => setActiveTab('STATS')}
                        >
                            STATS
                        </button>
                                                <button 
                            className={`pb-2 font-medium ${activeTab === 'NEWS' ? 'text-pink-500 border-b-2 border-pink-500' : 'text-gray-500'}`}
                            onClick={() => setActiveTab('NEWS')}
                        >
                            NEWS
                        </button>
                    </nav>
                </div>

                {/* Tab Content */}
                {activeTab === 'SQUAD' && (
                    <div>
                        <table className="w-full">
                            <thead>
                                <tr className="border-b text-left text-gray-500 text-sm">
                                    <th className="py-2 px-2 w-14">#</th>
                                    <th className="py-2">Player</th>
                                    <th className="py-2 text-center">Apps</th>
                                    <th className="py-2 text-center">Goals</th>
                                    <th className="py-2 text-center">Assists</th>
                                </tr>
                            </thead>
                            <tbody>
                                {teamDetails.squad.map((player, index)=> (
                                    <tr key={player.id} className={`border-b hover:bg-gray-50 ${index % 2 == 0 ? "bg-cyan-50": ""}`}>
                                        <td className="py-3 px-2 font-semibold">{player.jerseyNumber}</td>
                                        <td className="py-3">
                                            <div className="font-medium cursor-pointer hover:text-gray-500" onClick={() => navigate(`/player/${player.id}`)}>{player.name}</div>
                                            <div className="text-xs text-gray-500">{player.position}</div>
                                        </td>
                                        <td className="py-3 text-center">{player.stats.appearances}</td>
                                        <td className="py-3 text-center">{player.stats.goals}</td>
                                        <td className="py-3 text-center">{player.stats.assists}</td>
                                    </tr>
                                ))}
                            </tbody>
                            <thead>
                                <tr className="border-b text-left text-gray-500 text-sm">
                                    <th className="py-2 text-center">Coach</th>
                                </tr>
                            </thead>
                            <tbody>
                            <tr className="border-b hover:bg-gray-50">
                                <td className="py-3 text-center">Valmentaja </td>
                            </tr>
                            </tbody>

                        </table>
                    </div>
                )}

                {activeTab === 'MATCHES' && (
                    <div>

                        <h3 className="font-semibold text-gray-700 mb-3">Upcoming Matches</h3>
                        {teamDetails.upcomingMatches.map(match => (
                            <div key={match.id} className="border rounded-lg mb-3 overflow-hidden cursor-pointer hover:text-gray-500">
                                <div className="bg-gray-50 px-3 py-2 text-sm text-gray-600 border-b">
                                    {match.date}
                                </div>
                                <div className="p-3">
                                    <div className="flex justify-between items-center">
                                        <div className="flex items-center gap-2">
                                            {match.isHome ? (
                                                <>
                                                    <span className="font-medium">{teamDetails.name}</span>
                                                    <span>vs</span>
                                                    <span>{match.opponent}</span>
                                                </>
                                            ) : (
                                                <>
                                                    <span>{match.opponent}</span>
                                                    <span>vs</span>
                                                    <span className="font-medium">{teamDetails.name}</span>
                                                </>
                                            )}
                                        </div>

                                        <div className="flex items-center gap-3">
                                        <span>{match.kickoff} - {match.venue}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        ))}
                        {teamDetails.recentMatches.length === 0 && (
                            <div className="text-center py-4 text-gray-500">No recent matches</div>
                        )}
                    </div>
                )}
                {activeTab === "RESULTS" && (
                    <div>
                        <h3 className="font-semibold text-gray-700 mb-3">Recent Matches</h3>
                        {teamDetails.recentMatches.map(match => (
                            <div key={match.id} className="border rounded-lg mb-3 overflow-hidden cursor-pointer hover:text-gray-500">
                                <div className="bg-gray-50 px-3 py-2 text-sm text-gray-600 border-b">
                                    {match.date}
                                </div>
                                <div className="p-3">
                                    <div className="flex justify-between items-center">
                                        <div className="flex items-center gap-2">
                                            {match.isHome ? (
                                                <>
                                                    <span className="font-medium">{teamDetails.name}</span>
                                                    <span>vs</span>
                                                    <span>{match.opponent}</span>
                                                </>
                                            ) : (
                                                <>
                                                    <span>{match.opponent}</span>
                                                    <span>vs</span>
                                                    <span className="font-medium">{teamDetails.name}</span>
                                                </>
                                            )}
                                        </div>
                                        <div className="flex items-center gap-3">
                                            <span className="text-sm">
                                                {match.score.home} - {match.score.away}
                                            </span>
                                            <span className={`px-2 py-1 text-xs rounded-full ${
                                                match.result === "W" 
                                                ? "bg-green-100 text-green-800" 
                                                : match.result === "L"
                                                ? "bg-red-100 text-red-800"
                                                : "bg-gray-100 text-gray-800"
                                            }`}>
                                                {match.result}
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
                {activeTab === 'STATS' && (
                    <div>
                        <div className="grid grid-cols-2 gap-4">
                            <div className="border rounded-lg p-4">
                                <h3 className="text-sm font-semibold text-gray-500 mb-2">Goal Statistics</h3>
                                <div className="space-y-2">
                                    <div>
                                        <div className="flex justify-between text-sm mb-1">
                                            <span>Goals Scored</span>
                                            <span className="font-semibold">32</span>
                                        </div>
                                        <div className="bg-gray-200 h-2 rounded-full overflow-hidden">
                                            <div className="bg-green-500 h-full" style={{ width: '70%' }}></div>
                                        </div>
                                    </div>
                                    <div>
                                        <div className="flex justify-between text-sm mb-1">
                                            <span>Goals Conceded</span>
                                            <span className="font-semibold">18</span>
                                        </div>
                                        <div className="bg-gray-200 h-2 rounded-full overflow-hidden">
                                            <div className="bg-red-500 h-full" style={{ width: '40%' }}></div>
                                        </div>
                                    </div>
                                    <div>
                                        <div className="flex justify-between text-sm mb-1">
                                            <span>Goal Difference</span>
                                            <span className="font-semibold text-green-600">+14</span>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div className="border rounded-lg p-4">
                                <h3 className="text-sm font-semibold text-gray-500 mb-2">Form (Last 5)</h3>
                                <div className="flex gap-2 mt-4">
                                    <div className="w-8 h-8 rounded-full bg-green-500 text-white flex items-center justify-center font-bold">W</div>
                                    <div className="w-8 h-8 rounded-full bg-green-500 text-white flex items-center justify-center font-bold">W</div>
                                    <div className="w-8 h-8 rounded-full bg-gray-300 text-gray-700 flex items-center justify-center font-bold">D</div>
                                    <div className="w-8 h-8 rounded-full bg-green-500 text-white flex items-center justify-center font-bold">W</div>
                                    <div className="w-8 h-8 rounded-full bg-red-500 text-white flex items-center justify-center font-bold">L</div>
                                </div>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
        </PageTemplate>
    );
} 