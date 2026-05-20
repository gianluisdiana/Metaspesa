export type PageSearchParams = Record<string, string | string[] | undefined>;
export type SearchParamSource = PageSearchParams | URLSearchParams;

function paramValue(
  params: SearchParamSource,
  key: string,
): string | undefined {
  if (params instanceof URLSearchParams) {
    return params.get(key) ?? undefined;
  }

  const value = params[key];
  return typeof value === 'string' ? value : undefined;
}

export function hasParam(params: SearchParamSource, key: string): boolean {
  if (params instanceof URLSearchParams) {
    return params.has(key);
  }

  return Object.prototype.hasOwnProperty.call(params, key);
}

export function positiveNumberParam(
  params: SearchParamSource,
  key: string,
  fallback: number,
): number {
  const value = Number(paramValue(params, key));
  return Number.isInteger(value) && value > 0 ? value : fallback;
}

export function stringParam(
  params: SearchParamSource,
  key: string,
): string | undefined {
  const value = paramValue(params, key);
  return value && value.length > 0 ? value : undefined;
}
