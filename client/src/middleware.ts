import { NextResponse, type NextRequest } from 'next/server';

const AUTH_PATHS = ['/auth/login', '/auth/register'];

export function middleware(request: NextRequest) {
  const token = request.cookies.get('auth_token')?.value;
  const { pathname } = request.nextUrl;

  const isAuthPath = AUTH_PATHS.some(p => pathname.startsWith(p));

  if (!token && !isAuthPath) {
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
