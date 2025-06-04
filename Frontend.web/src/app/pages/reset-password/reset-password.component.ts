import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './reset-password.component.html',
  
  styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent implements OnInit {
  form: FormGroup;
  token: string = '';
  successMessage = '';
  errorMessage = '';
  formSubmitted=false;

  constructor(
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private authService: AuthService
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
    }, { validators: this.passwordsMatchValidator });
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.token = params['token'] || '';
    });
  }

  passwordsMatchValidator(form: FormGroup) {
    const newPassword = form.get('newPassword')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    return newPassword === confirmPassword ? null : { mismatch: true };
  }

  submit() {
    if (this.form.invalid || !this.token) {
      this.errorMessage = 'Please fix the errors in the form.';
      return;
    }

    this.formSubmitted=true;

    const { email, newPassword } = this.form.value;

    this.authService.resetPassword({ email, token: this.token, newPassword }).subscribe({
      next: () => {
        this.successMessage = 'Password has been reset successfully.';
        this.errorMessage = '';
      },
      error: (err) => {
        this.successMessage = '';
        this.errorMessage = err.error?.message || 'Failed to reset password.';
      }
    });
  }
}
