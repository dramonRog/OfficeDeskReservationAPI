import { Component } from '@angular/core';

@Component({
  selector: 'app-help-support',
  templateUrl: './help-support.html',
  styleUrls: ['./help-support.css'],
  standalone: false
})
export class HelpSupportComponent {

  public faqs = [
    {
      question: "How do I book a desk or a meeting room?",
      answer: "Navigate to the 'Home' page and click on 'Book a Desk' or 'Browse Rooms'. You can select your preferred date, time, and specific location. Once confirmed, it will appear in your 'My Bookings' tab.",
      isOpen: false
    },
    {
      question: "Can I modify or cancel my reservation?",
      answer: "Yes. Go to the 'My Bookings' section. You will see a list of your upcoming reservations. Click the pencil icon to edit the time/desk, or the trash can icon to cancel it completely.",
      isOpen: false
    },
    {
      question: "What happens if I forget to check in?",
      answer: "If you do not occupy your desk within 2 hours of your reservation start time, the system may automatically release the desk so other colleagues can use it.",
      isOpen: false
    },
    {
      question: "How do I report a broken monitor or desk?",
      answer: "Please reach out directly to the IT or Facility Management team using the contact email provided on this page. Include the Desk Identifier in your message.",
      isOpen: false
    },
    {
      question: "Why can't I see the 'Add Room' or 'Add Desk' buttons?",
      answer: "These actions are restricted to Administrators and Managers. Standard users can only view the available resources and make bookings.",
      isOpen: false
    }
  ];

  toggleFaq(index: number): void {
    this.faqs[index].isOpen = !this.faqs[index].isOpen;
  }
}
