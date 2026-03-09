import { API_ORIGIN } from '../api/api';

export function resolveMediaUrl(pathOrUrl) {
  if (!pathOrUrl) {
    return '';
  }

  if (/^(https?:)?\/\//i.test(pathOrUrl) || pathOrUrl.startsWith('data:') || pathOrUrl.startsWith('blob:')) {
    return pathOrUrl;
  }

  const normalizedPath = pathOrUrl
    .replace(/\\/g, '/')
    .replace(/^\.\//, '');

  return normalizedPath.startsWith('/')
    ? `${API_ORIGIN}${normalizedPath}`
    : `${API_ORIGIN}/${normalizedPath}`;
}
