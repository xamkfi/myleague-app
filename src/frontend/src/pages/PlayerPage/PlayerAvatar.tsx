interface PlayerAvatarProps {
  name: string;
  logoUrl?: string | null;
  logoAlt: string;
  isCreator?: boolean;
}

const CREATOR_NAME = 'Tuomas Reijonen';

const CREATOR_AVATAR_SRC =
  'https://media4.giphy.com/media/v1.Y2lkPTc5MGI3NjExanpjZzBqNm1xYnp1d3Y5c2V5OWxoeTg2ZjV5dHpldHQ4anI0dDd5MSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/sPE5g5cHJ3dNm/giphy.gif';

function isCreatorPlayer(name: string): boolean {
  return name === CREATOR_NAME;
}

function playerInitials(name: string): string {
  const parts = name.trim().split(/\s+/).filter((part) => part.length > 0);
  if (parts.length === 0) {
    return '?';
  }
  const first = parts[0] ?? '';
  if (parts.length === 1) {
    return first.slice(0, 2).toLocaleUpperCase('fi-FI');
  }
  const last = parts[parts.length - 1] ?? first;
  return `${first.charAt(0)}${last.charAt(0)}`.toLocaleUpperCase('fi-FI');
}

interface PlayerNameHeadingProps {
  name: string;
}

function BicepMark({ mirror = false }: { mirror?: boolean }) {
  return (
    <span className={`creator-bicep${mirror ? ' creator-bicep--mirror' : ''}`} aria-hidden="true">
      💪
    </span>
  );
}

export function PlayerNameHeading({ name }: PlayerNameHeadingProps) {
  const creator = isCreatorPlayer(name);

  return (
    <div className={`player-name${creator ? ' player-name--creator' : ''}`}>
      {creator && <BicepMark />}
      {name}
      {creator && <BicepMark mirror />}
    </div>
  );
}

export function PlayerAvatar({
  name,
  logoUrl,
  logoAlt,
  isCreator = false,
}: PlayerAvatarProps) {
  const creator = isCreator || isCreatorPlayer(name);
  const showInitials = !creator && !logoUrl;

  return (
    <div
      className={`player-avatar-large${creator ? ' player-avatar--creator' : ''}${showInitials ? ' player-avatar--initials' : ''}`}
    >
      {creator ? (
        <img className="creator-avatar-img" src={CREATOR_AVATAR_SRC} alt="Macho King" />
      ) : logoUrl ? (
        <img className="team-logo-img" src={logoUrl} alt={logoAlt} />
      ) : (
        <span className="player-avatar-initials" aria-hidden="true">
          {playerInitials(name)}
        </span>
      )}
    </div>
  );
}
