import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-forgot-password',
  imports: [ReactiveFormsModule, CommonModule, FormsModule, RouterModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css',
})
export class ForgotPasswordComponent {
  form: FormGroup;
  successMessage = '';
  errorMessage = '';
  formSubmitted = false;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private authService: AuthService
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  submit() {
    this.formSubmitted = true;
    const email = this.form.value.email;

    this.authService.forgotPassword(email).subscribe({
      next: (res) => {
        this.successMessage = res; // Should show: "Reset link sent"
        this.errorMessage = '';
        this.formSubmitted = true;
      },
      error: (err) => {
        if (err.status === 404) {
          this.errorMessage = 'Email address not found.';
        } else {
          this.errorMessage = 'Something went wrong. Please try again later.';
          console.log(err);
        }
        this.successMessage = '';
      },
    });
  }
}
