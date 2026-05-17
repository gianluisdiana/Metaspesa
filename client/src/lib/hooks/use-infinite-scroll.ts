'use client';

import { RefObject, useEffect } from 'react';

const DEFAULT_ROOT_MARGIN = '600px 0px';

export function useInfiniteScroll({
  hasMore,
  onLoadMore,
  rootMargin = DEFAULT_ROOT_MARGIN,
  sentinelRef,
}: Readonly<{
  hasMore: boolean;
  onLoadMore: () => void;
  rootMargin?: string;
  sentinelRef: RefObject<HTMLElement | null>;
}>) {
  useEffect(() => {
    const sentinel = sentinelRef.current;
    if (!sentinel || !hasMore) {
      return;
    }

    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry?.isIntersecting) {
          onLoadMore();
        }
      },
      { rootMargin },
    );

    observer.observe(sentinel);
    return () => observer.disconnect();
  }, [hasMore, onLoadMore, rootMargin, sentinelRef]);
}
