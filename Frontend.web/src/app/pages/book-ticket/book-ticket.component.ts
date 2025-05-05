import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { TicketService } from '../../services/ticket.service';
import { StationService } from '../../services/station.service';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-book-ticket',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './book-ticket.component.html',
  styleUrls: ['./book-ticket.component.css'],
})
export class BookTicketComponent implements OnInit {
  insurance: boolean = false;
  passengers: any[] = [];

  trainId: number = 0;
  classType: string = '';
  journeyDate: string = '';
  source: string = '';
  destination: string = '';

  classTypeID: number = 0;
  sourceID: number = 0;
  destinationID: number = 0;

  fareCalculated: boolean = false;
  fare: number = 0;

  passengerError: string = '';
  loading: boolean = false; // for showing spinner when calculating fare

  Razorpay: any;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private http: HttpClient,
    private authService: AuthService,
    private ticketService: TicketService,
    private stationService: StationService,
    private userService: UserService
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.trainId = +params['trainId'];
      this.classType = params['classType'];
      this.journeyDate = params['journeyDate'];
      this.source = params['source'];
      this.destination = params['destination'];

      this.classTypeID = this.getClassTypeID(this.classType);
      this.fetchStationIDs(this.source, this.destination);
      
    });
  }

  fetchStationIDs(source: string, destination: string) {
    this.stationService.getStationIdByName(source).subscribe({
      next: (response) => {
        this.sourceID = response; // Assuming the response contains 'id'
      },
      error: (error) => {
        console.error('Error fetching source station ID:', error);
      },
    });

    this.stationService.getStationIdByName(destination).subscribe({
      next: (response) => {
        this.destinationID = response; // Assuming the response contains 'id'
      },
      error: (error) => {
        console.error('Error fetching destination station ID:', error);
      },
    });
  }

  addPassenger() {
    if (this.passengers.length >= 4) {
      this.passengerError = 'Maximum 4 passengers allowed.';
      return;
    }
    this.passengerError = '';
    this.passengers.push({ name: '', age: '', gender: '' });
  }

  removePassenger(index: number) {
    this.passengers.splice(index, 1);
  }

  calculateFare() {
    if (
      this.passengers.length === 0 ||
      this.passengers.some((p) => !p.name || !p.age || !p.gender)
    ) {
      this.passengerError =
        'Please enter the details for at least one passenger.';
      return; // Prevent further processing
    }

    this.passengerError = ''; // Clear error message if validation

    const username = this.authService.getCurrentUser()?.username;
    if (!username) {
      console.error('Username not found.');
      return;
    }

    const requestBody = {
      username: username,
      trainID: this.trainId,
      sourceID: this.sourceID,
      destinationID: this.destinationID,
      journeyDate: this.journeyDate,
      classTypeID: this.classTypeID,
      passengers: this.passengers.map((p) => ({
        name: p.name,
        gender: p.gender,
        age: p.age,
      })),
      paymentMode: 'Online',
      hasInsurance: this.insurance,
    };

    this.loading = true;

    this.ticketService.initiateBooking(requestBody).subscribe({
      next: (response) => {
        this.fare = response.fare;
        this.fareCalculated = true;
        this.loading = false;
        console.log('Fare calculated:', this.fare);
      },
      error: (error) => {
        console.error('Error calculating fare:', error);
        this.loading = false;
      },
    });
  }

  submitBooking() {
    if (!this.fareCalculated || this.passengers.length === 0) {
      this.passengerError = 'Please add passengers and calculate fare first.';
      return;
    }

    const username = this.authService.getCurrentUser()?.username;
    if (!username) {
      console.error('Username not found.');
      return;
    }

    this.userService.getUserProfile().subscribe({
      next: (response) => {
        const userId = response.id;
        if (!userId) {
          console.error('User ID not found.');
          return;
        }

        const bookingData = this.buildBookingData(userId);
        this.proceedToRazorpay(bookingData);
      },
      error: (err) => {
        console.error('Error fetching user profile:', err);
      },
    });
  }

  buildBookingData(userId: string): any {
    return {
      userId,
      trainID: this.trainId,
      sourceID: this.sourceID,
      destinationID: this.destinationID,
      journeyDate: this.journeyDate,
      classTypeID: this.classTypeID,
      passengers: this.passengers.map((p) => ({
        name: p.name,
        gender: p.gender,
        age: p.age,
      })),
      paymentMode: 'Online',
      hasInsurance: this.insurance,
      paidAmount:this.fare,
    };
  }

  proceedToRazorpay(bookingData: any) {
    let username = '';
    let email = '';
    this.userService.getUserProfile().subscribe({
      next: (response) => {
        username = response.username;
        email = response.email;
      },
      error: (err) => {
        console.error('Error fetching user profile:', err);
      },
    });

    this.ticketService.createRazorpayOrder(this.fare).subscribe({
      next: (order) => {
        const options = {
          key: 'rzp_test_ZqTZa5VDXGFveX',
          amount: order.amount,
          currency: order.currency,
          name: 'Railway Reservation',
          description: 'Train ticket booking',
          order_id: order.orderId,
          handler: (response: any) =>
            this.confirmBookingWithPayment(response, bookingData),
          prefill: {
            name: username,
            email: email,
          },
          theme: { color: '#213d77' },
        };

        const rzp = new (window as any).Razorpay(options);
        rzp.open();
      },
      error: (err) => {
        console.error('Failed to create Razorpay order:', err);
        alert('Payment initialization failed.');
      },
    });
  }

  confirmBookingWithPayment(razorpayResponse: any, bookingData: any) {
    const paymentData = {
      razorpayOrderId: razorpayResponse.razorpay_order_id,
      razorpayPaymentId: razorpayResponse.razorpay_payment_id,
      razorpaySignature: razorpayResponse.razorpay_signature,
      bookingInfo: bookingData,
    };

    this.ticketService.confirmPayment(paymentData).subscribe({
      next: (result) => {
        console.log(result);
        const ticketId = result.ticketId; // make sure backend returns this
        alert('Booking confirmed!');
        this.router.navigate([`/my-ticket/${ticketId}`]);
      },
      error: (err) => {
        console.error('Booking confirmation failed:', err);
        alert('Booking failed.');
      },
    });
  }

  getClassTypeID(classType: string): number {
    switch (classType.toLowerCase()) {
      case 'sleeper':
        return 1;
      case 'ac 3 tier':
        return 2;
      case 'ac 2 tier':
        return 3;
      case 'first class':
        return 4;
      default:
        return 1; // fallback
    }
  }
}
