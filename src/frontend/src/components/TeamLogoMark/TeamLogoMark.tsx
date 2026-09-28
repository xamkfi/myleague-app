import { useEffect, useState } from 'react';
import { resolveLogoUrl } from '../../utils/resolveLogoUrl';
import { teamMarkLabel } from '../../utils/teamMarkLabel';

interface TeamLogoMarkProps {
  logo?: string | null;
  name: string;
  mark?: string | null;
  imageClassName: string;
  fallbackClassName: string;
}

export default function TeamLogoMark({
  logo,
  name,
  mark,
  imageClassName,
  fallbackClassName,
}: TeamLogoMarkProps) {
  const [failed, setFailed] = useState(false);
  const trimmedLogo = resolveLogoUrl(logo) ?? '';

  useEffect(() => {
    setFailed(false);
  }, [trimmedLogo]);

  if (trimmedLogo !== '' && !failed) {
    return (
      <img
        className={imageClassName}
        src={trimmedLogo}
        alt=""
        onError={() => setFailed(true)}
      />
    );
  }

  const shortName = mark?.trim() ?? '';

  return (
    <span className={fallbackClassName} aria-hidden="true">
      {shortName || teamMarkLabel(name) || '·'}
    </span>
  );
}
