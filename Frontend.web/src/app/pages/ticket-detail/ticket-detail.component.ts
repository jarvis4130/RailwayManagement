import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TicketService } from '../../services/ticket.service';
import { UserService } from '../../services/user.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-ticket-detail',
  imports: [CommonModule],
  templateUrl: './ticket-detail.component.html',
  styleUrl: './ticket-detail.component.css',
})
export class TicketDetailComponent {
  ticket: any;
  ticketId: number = 0;

  showModal: boolean = false;
  passengerToCancel: number | null = null;

  userId:string='';

  constructor(
    private http:HttpClient,
    private route: ActivatedRoute,
    private user: UserService,
    private ticketService: TicketService,
  ) {}

  ngOnInit() {
    this.ticketId = +this.route.snapshot.paramMap.get('id')!;
    this.loadTicket();
  }

  loadTicket() {
    this.ticketService.getTicketById(this.ticketId).subscribe({
      next: (res) => {
        this.ticket = res;
        console.log(this.ticket);
      },
      error: (err) => {
        console.error('Error fetching ticket:', err);
      },
    });
  }

  openModal(passengerId: number) {
    this.passengerToCancel = passengerId;
    this.showModal = true;
  }
  
  closeModal() {
    this.showModal = false;
    this.passengerToCancel = null;
  }

  confirmCancel() {
    if (this.passengerToCancel !== null) {
      this.cancelPassenger(this.passengerToCancel);
    }
    this.closeModal();
  }

  cancelPassenger(passengerId: number) {
    this.user.getUserProfile().subscribe((response)=>{
      this.userId=response.id;
      console.log(response.id)
      const body = {
        passengerId: passengerId,
        userId: this.userId// or getUsername if backend expects that
      };

      this.http.post('http://localhost:5039/api/Ticket/cancel-passenger', body).subscribe(() => {
        alert('Passenger cancelled!');
        this.loadTicket(); // refresh ticket
      });
    }); 
  }
}
