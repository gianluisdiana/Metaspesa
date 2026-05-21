import { vi } from 'vitest';

vi.mock('next/image', () => ({
  default: ({
    alt,
    src,
  }: Readonly<{
    alt: string;
    src: string;
  }>) => (
    // eslint-disable-next-line @next/next/no-img-element
    <img alt={alt} src={src} />
  ),
}));
