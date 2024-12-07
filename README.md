# AzureUserEntraIDApp

AzureUserEntraIDApp is a Blazor WebAssembly application that provides functionalities for managing users in an Azure environment. The application allows users to display, create, modify, disable, and delete user accounts. It also includes a feedback form for users to submit their feedback ,then do Sentiment Analysis using Azure AI Language and then save the feedback,sentiment,user email and phone number to SQL Server Database using Data API

## Features

- **Display All Users**: View a list of all users.
- **Create User**: Add a new user to Azure Entra ID using Graph API  
- **Modify User**: Edit existing user in Azure Entra ID using Graph API
- **Disable User**: Disable a user account  in Azure Entra ID using Graph API
- **Delete User**: Remove a user account  in Azure Entra ID using Graph API
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
   
   git clone https://github.com/shreyasrastogi/AzureUserEntraIDApp.git
   
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
    - `Feedback.razor`: Component for submitting feedback , doing **sentiment Analysis** using **Azure AI Language** and saving to **SQL Database** using **Data API**.
    - `AboutUs.razor`: Component for displaying information about the team.
    - `EditUser.razor`: Component for modifying exisitng user.
    - `DisableUser.razor`: Component for Disabling existing account.
    - `DeleteUser.razor`: Component for deleting a user account.
  - **Shared**: Contains shared components and layouts.
    - `MainLayout.razor`: Main layout component.
    - `NavMenu.razor`: Navigation menu component.
  - **wwwroot**: Contains static files such as CSS, JavaScript, and images.
    - `index.html`: The main HTML file for the Blazor WebAssembly application.
    - `logo.png`: The logo image used as the favicon.
- **BlazorApp.NUnitTests**: Contains the unit tests for the application.
    - `FeedbackTests.cs`: Unit tests for the `Feedback.razor` component.
    - `DisableUserTests.cs`: Unit tests for the `DisableUser.razor` component.
    - `DeleteUserTests.cs`: Unit tests for the `DeleteUser.razor` component.
    - `CreateUserTests.cs`: Unit tests for the `CreateUser.razor` component.
    - `EditUserTests.cs`: Unit tests for the `EditUser.razor` component.
    - `HomeTests.cs`: Unit tests for the `Home.razor` component.
 
    # API Project Documentation

## Overview
The API project is an Azure Functions project targeting .NET 8. It includes various functions for handling user-related operations and feedback processing. The project leverages Microsoft Graph API for user management and integrates with Azure for authentication and authorization.

## Project Structure

### Functions
- **CreateUserFunction.cs**: Handles the creation of new users.
- **UpdateUserFunction.cs**: Handles the updating of existing user information.
- **DisableUserFunction.cs**: Handles disabling user accounts.
- **GetUserFunction.cs**: Retrieves user information.
- **GetUsersFunction.cs**: Retrieves a list of users.
- **DeleteUserFunction.cs**: Handles the deletion of user accounts.
- **UserFeedbackFunction.cs**: Processes user feedback and performs sentiment analysis.

### Models
- **NewUser.cs**: Represents the data model for a new user.
- **UserFeedback.cs**: Represents the data model for user feedback.

### Properties
- **launchSettings.json**: Contains settings for launching the project locally.

### Configuration Files
- **local.settings.json**: Contains local settings for the Azure Functions project.
- **host.json**: Contains global configuration options for all functions in the project.

### Project File
- **Api.csproj**: The project file that defines the dependencies and build settings for the API project.



## Configuration

### Update Favicon

To update the favicon, replace the `logo.png` file in the `wwwroot` directory and update the `index.html` file to reference the new favicon.

## Contributing

Contributtors to this current project are Shreyas Rastogi & Urvashi Mehta

## License

NA

## Contact

For any questions or feedback, please reach out to [shreyasrastogi@gmail.com](mailto:shreyasrastogi@gmail.com).


    
