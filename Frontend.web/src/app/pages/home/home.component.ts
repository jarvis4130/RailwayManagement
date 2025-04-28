import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  imports: [ReactiveFormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})

export class HomeComponent {
  searchForm: FormGroup;
  today: string = new Date().toISOString().split('T')[0];

  constructor(private fb: FormBuilder, private router: Router) {
    this.searchForm = this.fb.group({
      source: ['', Validators.required],
      destination: ['', Validators.required],
      journeyDate: [''],    // optional
      classTypeId: [''],     // optional
    });
  }

  onSearch() {
    if (this.searchForm.invalid) {
      console.log('Form is invalid');
      return;
    }

    const searchData = this.searchForm.value;
    console.log('Search data:', searchData);

    if (!searchData.classTypeId) {
      delete searchData.classTypeId;
    }  

    if (!searchData.journeyDate) {
      delete searchData.journeyDate;
    }  

    // Later: Navigate to results page and pass searchData
    this.router.navigate(['/search-results'], { queryParams: searchData });
  }
}

