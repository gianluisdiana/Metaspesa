import { NextResponse, type NextRequest } from 'next/server';

const AUTH_PATHS = ['/auth/login', '/auth/register'];
const PROTECTED_PATHS = ['/api/shopping', '/shopping'];

function isProtectedPath(pathname: string): boolean {
  return PROTECTED_PATHS.some(path => {
    return pathname === path || pathname.startsWith(`${path}/`);
  });
}

export function proxy(request: NextRequest) {
  const token = request.cookies.get('metaspesa_session')?.value;
  const { pathname } = request.nextUrl;

  const isAuthPath = AUTH_PATHS.some(p => pathname.startsWith(p));

  if (!token && isProtectedPath(pathname)) {
    return NextResponse.redirect(new URL('/auth/login', request.url));
  }
  if (token && isAuthPath) {
    return NextResponse.redirect(new URL('/markets', request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/((?!_next/static|_next/image|favicon.ico).*)'],
};
