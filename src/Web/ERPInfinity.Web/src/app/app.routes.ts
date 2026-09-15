import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'pos', pathMatch: 'full' },
  { path: '**', redirectTo: 'pos' }
];
