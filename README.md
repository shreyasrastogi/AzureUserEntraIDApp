# AzureUserEntraIDApp

AzureUserEntraIDApp is a Blazor WebAssembly application that provides functionalities for managing users in an Azure environment. The application allows users to display, create, modify, disable, and delete user accounts. It also includes a feedback form for users to submit their feedback ,then do Sentiment Analysis using Azure AI Language and then save the feedback,sentiment,user email and phone number to SQL Server Database using Data API

## Features

- **Display All Users**: View a list of all users.
- **Create User**: Add a new user to the system.
- **Modify User**: Edit existing user details.
- **Disable User**: Disable a user account.
- **Delete User**: Remove a user from the system.
- **Feedback**: Submit feedback about the application.
- **Sentiment Analysis**: Sentiment Analysis of user sentiment using **Azure AI Language**.
- **SaveFeedback to SQL Server DB**: Save Feedback , user email and user phone number to MS SQL Server DB using **Data API** .

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or later
- [Node.js](https://nodejs.org/) (for building and running the application)

### Installation

1. **Clone the repository**:
   
   git clone https://github.com/yourusername/AzureUserEntraIDApp.git
   
   cd AzureUserEntraIDApp

3. **Restore dependencies**:
    dotnet restore
   
4. **Build the application**:
    dotnet build
   
5. **Run the application**:
    dotnet run
   
### Running Tests

To run the unit tests, use the following command:
dotnet test

## Project Structure

- **AzureUserEntraIDApp**: The main Blazor WebAssembly project.
  - **Pages**: Contains the Razor components for different pages.
    - `Home.razor`: Home page component.
    - `CreateUser.razor`: Component for creating a new user.
    - `UserList.razor`: Component for displaying and managing the user list.
    - `Feedback.razor`: Component for submitting feedback , doing sentiment Analysis using Azure AI Language and saving to Database using Data API.
    - `AboutUs.razor`: Component for displaying information about the team.
    - `EditUser.razor`: Component for modifying exisitng user.
    - `DisableUser.razor`: Component for Disabling existing account.
    - `DeleteUser.razor`: Component for deleting a user account.
  - **Shared**: Contains shared components and layouts.
    - `MainLayout.razor`: Main layout component.
    - `NavMenu.razor`: Navigation menu component.
  - **wwwroot**: Contains static files such as CSS, JavaScript, and images.
    - `index.html`: The main HTML file for the Blazor WebAssembly application.
    - `manifest.json`: Web app manifest file.
    - `logo.png`: The logo image used as the favicon.
- **BlazorApp.NUnitTests**: Contains the unit tests for the application.
  - `FeedbackTests.cs`: Unit tests for the `Feedback.razor` component.
  - `DisableUserTests.cs`: Unit tests for the `DisableUser.razor` component.
  - `HomeTests.cs`: Unit tests for the `Home.razor` component.

## Configuration

### Update Favicon

To update the favicon, replace the `logo.png` file in the `wwwroot` directory and update the `index.html` file to reference the new favicon.

### Update `manifest.json`

Ensure the `manifest.json` file in the `wwwroot` directory includes the new logo image.
{ "name": "AzureUserEntraIDApp", "short_name": "AzureUserEntraIDApp", "start_url": "/", "display": "standalone", "background_color": "#ffffff", "theme_color": "#000000", "icons": [ { "src": "logo.png", "sizes": "192x192", "type": "image/png" }, { "src": "logo.png", "sizes": "512x512", "type": "image/png" } ] }


## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any changes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact

For any questions or feedback, please reach out to [support@example.com](mailto:support@example.com).


    
