import { useEffect, useState } from 'react';
import { teamMarkLabel } from '../../utils/teamMarkLabel';

interface TeamLogoMarkProps {
  logo?: string | null;
  name: string;
  imageClassName: string;
  fallbackClassName: string;
}

export default function TeamLogoMark({
  logo,
  name,
  imageClassName,
  fallbackClassName,
}: TeamLogoMarkProps) {
  const [failed, setFailed] = useState(false);
  const trimmedLogo = logo?.trim() ?? '';

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

  return (
    <span className={fallbackClassName} aria-hidden="true">
      {teamMarkLabel(name) || '·'}
    </span>
  );
}
