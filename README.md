# Code Review Checklist Generator

## Table of Contents

- [Project Description](#project-description)
- [Tech Stack](#tech-stack)
- [Getting Started Locally](#getting-started-locally)
- [Project Scope](#project-scope)
- [Project Status](#project-status)

## Project Description

Code Review Checklist Generator is a Minimum Viable Product (MVP) web application that helps bring structure to code reviews. Users paste a diff, a code snippet, or describe the change context, and the AI generates a tailored review checklist. The checklist can be edited, saved as a template for similar changes, checked off during review, and exported.

The goal of the MVP is to deliver a simple, fast tool that makes reviews more consistent and complete without extra manual overhead.

## Tech Stack

### Frontend

- **ASP.NET Core Razor Pages**: Server-rendered UI for fast, reliable pages.
- **C#**: Type-safe server-side implementation.

### Backend

- **ASP.NET Core**: Web framework for routing, middleware, and application logic.
- **Entity Framework Core**: ORM and migrations.
- **SQL Server (LocalDB)**: Local development database.
- **ASP.NET Core Identity**: Email/password authentication and user management.

### AI Integration

- **OpenRouter.ai**: AI model gateway used to generate personalized review checklists.

### Testing

- **xUnit**: Standard unit testing framework for .NET.

### CI/CD

- **GitHub Actions**: Build, test, and publish artifact pipeline.

## Getting Started Locally

### Prerequisites

- .NET SDK 9.0
- SQL Server LocalDB
- OpenRouter API key

### Setup

1. Clone the repository:

   ```bash
   git clone <repo-url>
   cd 10xDevs_CodeReviewChecklistGenerator
   ```

2. Set your OpenRouter API key (user secrets):

   ```powershell
   dotnet user-secrets set "OpenRouter:ApiKey" "<your_key>" --project 10xDevs_CodeReviewChecklistGenerator
   ```

3. Run the app:

   ```powershell
   dotnet run --project 10xDevs_CodeReviewChecklistGenerator
   ```

## Project Scope

### MVP Inclusions

- Generate a review checklist from a diff/code/context.
- Edit, check off, and export checklist items.
- Save and manage checklist templates.
- Email/password authentication.
- OpenRouter.ai integration.

### Exclusions from MVP

- VCS integrations (e.g., GitHub PR diffs).
- Real-time collaboration.
- Advanced analytics or scoring.
- Messaging platform integrations.

## Project Status

Active MVP under development.
