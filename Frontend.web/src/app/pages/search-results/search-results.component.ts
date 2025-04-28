import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TrainService, TrainDTO } from '../../services/train.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-search-results',
  imports: [FormsModule, CommonModule],
  templateUrl: './search-results.component.html',
  styleUrl: './search-results.component.css',
})
export class SearchResultsComponent implements OnInit {
  searchParams: any = { classTypeId: null }; // to bind modify search fields
  trains: TrainDTO[] = [];
  today: string = new Date().toISOString().split('T')[0];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private trainService: TrainService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.searchParams = { ...params };
      console.log('Received params:', this.searchParams);
      this.fetchTrains();
    });
  }

  fetchTrains() {
    this.trainService.searchTrains(this.searchParams).subscribe({
      next: (res) => {
        this.trains = res;
        console.log('Received trains:', this.trains);
      },
      error: (err) => {
        console.error('Error fetching trains:', err);
      },
    });
  }

  modifySearch() {
    const cleanedParams = { ...this.searchParams };
    if (!cleanedParams.classTypeId) {
      delete cleanedParams.classTypeId;
    }
    if (!cleanedParams.journeyDate) {
      delete cleanedParams.journeyDate;
    }
    if (!cleanedParams.source) {
      delete cleanedParams.source;
    }
    if (!cleanedParams.destination) {
      delete cleanedParams.destination;
    }

    this.router.navigate(['/search-results'], {
      queryParams: cleanedParams,
    });
  }

  bookTicket(trainId: number, classType: string) {
    if (!this.authService.isLoggedIn()) {
      const returnUrl = this.router.createUrlTree(['/book-ticket'], {
        queryParams: {
          trainId,
          classType,
          source: this.searchParams.source,
          destination: this.searchParams.destination,
          journeyDate: this.searchParams.journeyDate
        }
      }).toString();
  
      localStorage.setItem('returnUrl', returnUrl);
      this.router.navigate(['/login']);
      return;
    }
  
    // If already logged in
    this.router.navigate(['/book-ticket'], {
      queryParams: {
        trainId,
        classType,
        source: this.searchParams.source,
        destination: this.searchParams.destination,
        journeyDate: this.searchParams.journeyDate
      }
    });
  }
  
  
}
