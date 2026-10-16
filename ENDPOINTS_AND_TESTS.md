# RaceDay API endpoint inventory

Base path: `/api`. Swagger UI is available at `/swagger` in Development.

## Authentication
- `POST /api/auth/register` — public registration (Organiser or Participant)
- `POST /api/auth/login` — public login; establishes session cookie
- `POST /api/auth/logout` — clears session

## User profile
- `GET /api/users/me` — authenticated user's profile
- `PUT /api/users/me` — update own profile

## Events
- `GET /api/events` — public list
- `GET /api/events/{id}` — public event details
- `POST /api/events` — Organiser creates an event
- `PUT /api/events/{id}` — owning Organiser updates event
- `DELETE /api/events/{id}` — owning Organiser deletes event

## Categories
- `GET /api/categories` — public list
- `GET /api/events/{eventId}/categories` — public categories for an event
- `POST /api/events/{eventId}/categories` — owning Organiser creates category
- `PUT /api/categories/{id}` — owning Organiser updates category
- `DELETE /api/categories/{id}` — owning Organiser deletes category

## Enrolments
- `POST /api/events/{eventId}/enrolments` — Participant enrols in a category
- `GET /api/enrolments/me` — Participant's own enrolments
- `GET /api/events/{eventId}/enrolments` — owning Organiser lists event enrolments
- `GET /api/enrolments/{id}` — Participant sees own enrolment; Organiser sees enrolment for own event
- `DELETE /api/enrolments/{id}` — Participant cancels own enrolment

## Results
- `POST /api/enrolments/{enrolmentId}/result` — owning Organiser records result
- `GET /api/events/{eventId}/results` — public event results
- `GET /api/results/me` — Participant's own results
- `GET /api/results/{id}` — authenticated user with ownership checks
- `PUT /api/results/{id}` — owning Organiser updates result
- `DELETE /api/results/{id}` — owning Organiser deletes result

## Automated tests
Run from Visual Studio Test Explorer or from the solution folder with `dotnet test`. The included integration smoke tests cover public routes, missing records, invalid login/role, and unauthenticated access to protected routes. They are a baseline and should be expanded with successful organiser/participant workflows before final submission.
