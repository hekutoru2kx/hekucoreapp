import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslocoModule } from '@jsverse/transloco';
import { UserRoleHistoryItem } from '../../services/admin';

export interface RoleHistoryDialogData {
  userName: string;
  history: UserRoleHistoryItem[];
}

@Component({
  selector: 'app-role-history-dialog',
  imports: [CommonModule, MatDialogModule, MatTableModule, MatButtonModule, MatIconModule, MatTooltipModule, TranslocoModule],
  templateUrl: './role-history-dialog.html',
  styleUrl: './role-history-dialog.scss',
})
export class RoleHistoryDialog {
  data = inject<RoleHistoryDialogData>(MAT_DIALOG_DATA);

  displayedColumns = ['roleName', 'createdAt', 'createdBy', 'revokedAt', 'revokedBy'];
}
