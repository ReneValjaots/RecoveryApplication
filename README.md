# Recovery Application

Welcome to the **Recovery Application**. This project is designed to help users keep track of their injuries and provide recovery exercises to aid in their recovery process.

There are two types of users for this application:
- **Users:** Regular users who can track their injuries and create recovery plans.
- **Doctors:** Medical professionals who log in with a special account, created using a secret key, to develop recovery plans for patients with severe injuries.

## How It Works
- **Injury Tracking:** Users can mark their injuries in the application.
- **Severity Survey:** A small survey is provided to determine whether the injury is severe.
  - If the injury is severe, a doctor must create a tailored recovery plan for the user.
  - If the injury is minor, users can create their own recovery plan using the provided recovery exercises.

## Live Demo
Check out the live version of the application here:
[Recovery Application Live Demo](https://recovery.itb2203.tautar.ee/).

## Technologies Used
- **Backend:** C# REST API
- **Frontend:** Nuxt3
- **Authentication:** JWT for handling user-related authentication and authorization.