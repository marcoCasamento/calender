# Senior Backend Assignment – Calendar API

As part of the interview process, we’d like to evaluate your technical and architectural skills through a short assignment. The goal is to assess your ability to design and implement a clean, maintainable, and scalable API.

## Assignment Overview

You’ve just joined our backend team, which is currently developing an appointment system for a veterinary clinic. The system allows clinic staff to manage pet owners, and their appointments, but now we would like to implement new features.


## ✅ Task 1: List Vet Appointments

Implement an endpoint to retrieve all appointments assigned to a specific veterinarian within a given date range.

Input:
  - Vet ID
  - Start date
  - End date

Output:
  -  List of appointments including time, animal name, owner name, and status



## ✅ Task 2: Update Appointment Status

Allow a veterinarian to update the status of an appointment.

Rules:
  -  Valid status values: Scheduled, Completed, Canceled
  -  An appointment cannot be canceled within 1 hour of its scheduled start time
  -  When an appointment is canceled, send a mock email notification to the pet owner (e.g., Console.WriteLine("Email sent to owner@example.com"))


## ✅ Task 3: Use EF Core with a Local Database

Use Entity Framework Core with either:
  -  An In-Memory provider (for simplicity)
  -  Or SQLite (if you’d like to demonstrate something closer to production)


## ✅ Task 4: Find Error and Fix It
Review the provided code and identify any bugs or issues if exits.


## 📦 Delivery Instructions
  -  Submit your solution as a GitHub repository or a ZIP file
  -  If you had more time, include a short note explaining:
     -  What you would have improved
     -  How you’d scale or evolve the solution


If you have any questions or need clarification, feel free to ask. We’re looking forward to seeing your solution!