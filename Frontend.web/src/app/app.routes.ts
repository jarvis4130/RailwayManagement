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
import { AdminDashboardComponent } from './pages/admin-dashboard/admin-dashboard.component';
import { adminGuard } from './guards/admin.guard';
import { StationManagementComponent } from './pages/station-management/station-management.component';
import { TrainListComponent } from './pages/train-list/train-list.component';
import { AddScheduleComponent } from './pages/addschedule/addschedule.component';
import { UpdateTrainComponent } from './pages/update-train/update-train.component';
import { ForgotPasswordComponent } from './pages/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './pages/reset-password/reset-password.component';

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
    component: BookTicketComponent,
  },
  {
    path: 'my-ticket',
    component: MyTicketComponent,
  },
  {
    path: 'my-ticket/:id',
    component: TicketDetailComponent,
  },
  {
    path: 'forgot-password',
    component: ForgotPasswordComponent,
  },
  {
    path: 'reset-password',
    component: ResetPasswordComponent,
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    children: [
      {
        path: 'dashboard',
        component: AdminDashboardComponent,
      },
      {
        path: 'station-management',
        component: StationManagementComponent,
      },
      {
        path: 'train-list',
        component: TrainListComponent,
      },
      {
        path: 'add-train-schedule',
        component: AddScheduleComponent,
      },
      {
        path: 'update-train-schedule',
        component: UpdateTrainComponent,
      },
    ],
  },
  {
    path: '**',
    component: PageNotFoundComponent,
  },
];
