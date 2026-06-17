import { inject } from '@angular/core';
import {
  HttpErrorResponse,
  HttpEvent,
  HttpRequest,
  HttpInterceptorFn,
  HttpHandlerFn,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthService } from './auth.service';

export const jwtInterceptor: HttpInterceptorFn = (
  request: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();
  let authRequest = request;

  if (token) {
    authRequest = request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  return next(authRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      if (
        error.status === 401 &&
        !request.url.endsWith('/refresh') &&
        authService.getRefreshToken()
      ) {
        return authService.refreshToken().pipe(
          switchMap(() => {
            const refreshedToken = authService.getAccessToken();
            const retryRequest = request.clone({
              setHeaders: {
                Authorization: `Bearer ${refreshedToken}`,
              },
            });
            return next(retryRequest);
          })
        );
      }

      return throwError(() => error);
    })
  );
};
