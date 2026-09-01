import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { tokenInterceptor } from './core/interceptors/token.interceptor';
import { erroInterceptor } from './core/interceptors/erro.interceptor';
import { I18n } from 'carbon-components-angular/i18n';
import { carbonEmPortugues } from './core/carbon-ptbr';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([tokenInterceptor, erroInterceptor])),
    // O I18n normalmente vem junto com os módulos do Carbon, mas o inicializador roda
    // antes de qualquer componente carregar — sem declará-lo aqui, o app não sobe.
    I18n,
    carbonEmPortugues
  ]
};
