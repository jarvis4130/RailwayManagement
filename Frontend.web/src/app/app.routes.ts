import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { SearchComponent } from './pages/search/search.component';
import { PageNotFoundComponent } from './pages/page-not-found/page-not-found.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { SearchResultsComponent } from './pages/search-results/search-results.component';
import { BookTicketComponent } from './pages/book-ticket/book-ticket.component';
import { MyTicketComponent } from './pages/my-ticket/my-ticket.component';
import { TicketDetailComponent } from './pages/ticket-detail/ticket-detail.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full',
  },
  {
    path: 'home',
    component: HomeComponent,
  },
  {
    path: 'search',
    component: SearchComponent,
  },
  {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: 'signup',
    component: RegisterComponent,
  },
  {
    path: 'search-results',
    component: SearchResultsComponent,
  },
  { 
    path: 'book-ticket',
    component: BookTicketComponent
  },
  {
    path:'my-ticket',
    component:MyTicketComponent
  },
  {
    path:'my-ticket/:id',
    component:TicketDetailComponent
  },
  {
    path: '**',
    component: PageNotFoundComponent,
  },
];
