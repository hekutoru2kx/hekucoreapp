import { Component, inject, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { Content } from '../../services/content';

// A small, reusable upload control for a singleton image slot (see the backend's ContentItem
// "slot" concept) — not the full content list/panel, which no page in this core needs yet.
// The caller owns which feature's route this talks to (`path`, e.g.
// '/user/person/profile-picture') and how the resulting picture is displayed; this component
// only handles picking a file, showing progress/errors, and removal.
@Component({
  selector: 'app-avatar-upload',
  imports: [CommonModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, TranslocoModule],
  templateUrl: './avatar-upload.html',
  styleUrl: './avatar-upload.scss',
})
export class AvatarUpload {
  private content = inject(Content);
  private transloco = inject(TranslocoService);

  pictureUrl = input<string | null>(null);
  path = input.required<string>();

  uploaded = output<number>();
  removed = output<void>();

  uploading = signal(false);
  errorMessage = signal<string | null>(null);

  onFileSelected(event: Event): void {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0];
    target.value = '';
    if (!file) return;

    this.uploading.set(true);
    this.errorMessage.set(null);
    this.content.upload(this.path(), file).subscribe({
      next: (result) => {
        this.uploading.set(false);
        this.uploaded.emit(result.id);
      },
      error: (err) => {
        this.uploading.set(false);
        this.errorMessage.set(err.error || this.transloco.translate('avatarUpload.uploadError'));
      }
    });
  }

  onRemove(): void {
    if (!window.confirm(this.transloco.translate('avatarUpload.confirmRemove'))) return;

    this.uploading.set(true);
    this.errorMessage.set(null);
    this.content.remove(this.path()).subscribe({
      next: () => {
        this.uploading.set(false);
        this.removed.emit();
      },
      error: (err) => {
        this.uploading.set(false);
        this.errorMessage.set(err.error || this.transloco.translate('avatarUpload.removeError'));
      }
    });
  }
}
