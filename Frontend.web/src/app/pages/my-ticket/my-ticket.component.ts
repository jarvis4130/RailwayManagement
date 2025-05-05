import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TicketService } from '../../services/ticket.service';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-my-ticket',
  imports: [CommonModule],
  templateUrl: './my-ticket.component.html',
  styleUrl: './my-ticket.component.css',
})
export class MyTicketComponent implements OnInit {
  tickets: any[] = [];

  constructor(
    private ticketService: TicketService, // Inject TicketService
    private router: Router,
    private auth: AuthService
  ) {}

  ngOnInit() {
    const currentUser = this.auth.getCurrentUser();
    if (!currentUser || !currentUser.username) {
      // User is not logged in, redirect to login
      localStorage.setItem('returnUrl', '/my-ticket');
      this.router.navigate(['/login']);
      return;
    }
    const username=currentUser.username;
    // Use the TicketService to get tickets
    this.ticketService.getUserTickets(username).subscribe((data) => {
      console.log(data);
      this.tickets = data;
    });
  }

  viewTicket(id: number) {
    this.router.navigate(['/my-ticket', id]);
  }
}