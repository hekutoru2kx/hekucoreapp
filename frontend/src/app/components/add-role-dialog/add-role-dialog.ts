import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule, MatListOption } from '@angular/material/list';
import { TranslocoModule } from '@jsverse/transloco';

export interface AddRoleDialogData {
  availableRoles: string[];
}

// Picks one or more role names to add to a user. Deliberately no date inputs here — the
// start/expiry window is set per row on the role-assignment page after the role is added,
// so a mistaken pick can be removed before it is ever saved.
@Component({
  selector: 'app-add-role-dialog',
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatListModule, TranslocoModule],
  templateUrl: './add-role-dialog.html',
  styleUrl: './add-role-dialog.scss',
})
export class AddRoleDialog {
  private dialogRef = inject(MatDialogRef<AddRoleDialog>);
  data = inject<AddRoleDialogData>(MAT_DIALOG_DATA);

  selected = signal<string[]>([]);

  onSelectionChange(selected: MatListOption[]): void {
    this.selected.set(selected.map(o => o.value));
  }

  add(): void {
    this.dialogRef.close(this.selected());
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
