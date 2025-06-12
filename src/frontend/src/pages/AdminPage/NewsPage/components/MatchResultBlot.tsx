import { Quill } from 'react-quill';
import "./MatchResult.scss";

export interface MatchResultValue {
  homeTeam: string;
  awayTeam: string;
  homeScore: string;
  awayScore: string;
  date: string;
  link: string;
}

// Fix the BlockEmbed import typing
const BlockEmbed = Quill.import('blots/block/embed') as any;

export class MatchResultBlot extends BlockEmbed {
  static blotName = 'matchResult';
  static tagName = 'div';
  static className = 'match-result-container';
  static scope = Quill.import('parchment').Scope.BLOCK_BLOT; // Add required scope property

  static create(value: MatchResultValue): HTMLElement {
    const node = super.create();

    // Check if it's a result (has scores) or upcoming match
    const isResult = value.homeScore !== 'vs' && value.awayScore !== '';
    
    node.innerHTML = `
      <div class="match-result-narrow">
        <div class="match-result-narrow__date">${value.date}</div>
        <div class="match-result-narrow__content">
          <div class="match-result-narrow__teams">
            ${isResult 
              ? `${value.homeTeam} ${value.homeScore} - ${value.awayScore} ${value.awayTeam}`
              : `${value.homeTeam} vs ${value.awayTeam}`
            }
          </div>
          <div class="match-result-narrow__info">
            ${isResult ? 'Lopputulos' : value.homeScore}
          </div>
        </div>
      </div>
    `;

    node.setAttribute('contenteditable', 'false');
    return node;
  }

  static value(node: HTMLElement): MatchResultValue {
    const dateElement = node.querySelector('.match-result-narrow__date');
    const teamsElement = node.querySelector('.match-result-narrow__teams');
    const infoElement = node.querySelector('.match-result-narrow__info');

    const date = dateElement?.textContent || '';
    const teamsText = teamsElement?.textContent || '';
    const info = infoElement?.textContent || '';

    // Parse teams and scores from the text
    let homeTeam = '', awayTeam = '', homeScore = '', awayScore = '';
    
    if (teamsText.includes(' vs ')) {
      // Upcoming match
      const parts = teamsText.split(' vs ');
      homeTeam = parts[0] || '';
      awayTeam = parts[1] || '';
      homeScore = 'vs';
      awayScore = '';
    } else {
      // Result with scores
      const scoreMatch = teamsText.match(/^(.+?)\s+(\d+)\s+-\s+(\d+)\s+(.+)$/);
      if (scoreMatch) {
        homeTeam = scoreMatch[1];
        homeScore = scoreMatch[2];
        awayScore = scoreMatch[3];
        awayTeam = scoreMatch[4];
      }
    }

    return { homeTeam, homeScore, awayTeam, awayScore, date, link: '#' };
  }
}

// Register the blot
Quill.register('blots/matchResult', MatchResultBlot); 