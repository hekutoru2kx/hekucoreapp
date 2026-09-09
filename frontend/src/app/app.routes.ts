import { Routes } from '@angular/router';
import { Landing } from './pages/landing/landing';
import { Login } from './pages/user/login/login';
import { Register } from './pages/user/register/register';
import { Dashboard } from './pages/dashboard/dashboard';
import { authGuard } from './guards/auth-guard';
import { claimGuard } from './guards/claim-guard';
import { UserManagement } from './pages/admin/user-management/user-management';
import { UserRoleAssignment } from './pages/admin/user-role-assignment/user-role-assignment';
import { ChangePassword } from './pages/change-password/change-password';
import { Profile } from './pages/profile/profile';
import { PersonManagement } from './pages/admin/person-management/person-management';
import { RoleManagement } from './pages/admin/role-management/role-management';
import { AppSettingsPage } from './pages/admin/app-settings/app-settings';
import { ConfirmEmail } from './pages/user/confirm-email/confirm-email';


export const routes: Routes = [
  { path: '', component: Landing },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'confirm-email', component: ConfirmEmail },
  { path: 'dashboard', component: Dashboard, canActivate: [authGuard] },
  { path: 'admin/users', component: UserManagement, canActivate: [authGuard, claimGuard('UserManagementPermission', 'Read')] },
  { path: 'admin/users/:id/roles', component: UserRoleAssignment, canActivate: [authGuard, claimGuard('UserManagementPermission', 'Update')] },
  { path: 'change-password', component: ChangePassword, canActivate: [authGuard] } ,
  { path: 'profile', component: Profile, canActivate: [authGuard] },
  { path: 'admin/persons', component: PersonManagement, canActivate: [authGuard, claimGuard('PersonsPermission', 'Read')] },
  { path: 'admin/roles', component: RoleManagement, canActivate: [authGuard, claimGuard('RolesPermission', 'Read')] },
  { path: 'admin/settings', component: AppSettingsPage, canActivate: [authGuard, claimGuard('AppSettingsPermission', 'Read')] }
];