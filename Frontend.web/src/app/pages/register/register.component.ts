import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule,CommonModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent {
  registerForm: FormGroup;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
      confirmPassword: ['', Validators.required],
      aadharNumber: ['', Validators.required],
    });
  }

  onSubmit() {
    if (this.registerForm.invalid) return;

    const formData = this.registerForm.value;

    if (formData.password !== formData.confirmPassword) {
      this.errorMessage = 'Passwords do not match';
      return;
    }

    this.auth.register(formData).subscribe({
      next: (res) => {
        localStorage.setItem('token', res.token);
        this.router.navigate(['/home']); // After successful registration, redirect to home
      },
      error: (err) => {
        this.errorMessage = err.error?.error || 'Registration failed';
      },
    });
  }
}
