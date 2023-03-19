# Ufynd API

## Prerequirements
* Jetbrains Rider or Visual Studio 2019 (or higher)
* .NET Core SDK (version 6.0 or higher)
* Akka.NET
* Redis

## Setup
to set it up.
* `.NET 6`: Download .NET 6 [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) and proceed to your OS link for installation guide
  * Windows Installation Guide: https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net60
  * Linux Installation Guide: https://learn.microsoft.com/en-us/dotnet/core/install/linux
  * macOS Installation Guide: https://learn.microsoft.com/en-us/dotnet/core/install/macos
* `IDEs`: Click [here](https://www.jetbrains.com/rider/download/) to download Jetbrains Rider and 
[here](https://visualstudio.microsoft.com/downloads/) for Visual Studio 2022
* `SMTP` with Gmail: Follow this [link](https://support.google.com/mail/answer/185833?hl=en-GB) to create an 
app password from your gmail account. Update the appsettings with the credentials in order to send emails.
* `Redis`: Install redis (docker) on your PC using the documentation from this [page](https://redis.io/docs/stack/get-started/install/docker/)

## How To Run
* Open solution in Jetbrains Rider or Visual Studio
* Build the solution.
* Run the <b>Ufynd.Arrivals.Api</b> project which accepts a list of hotels and filters based on `hotel Id` and `arrival date`.
* Run the <b>Ufynd.Reporting.Api</b> project which generates an excel report based on the list of hotels provided. This project also can
send automated emails with the generated files as attachment but in order to activate this, update the `UseAutomatedEmailSender` in the projects
appsettings to `true`, run the <b>Ufynd.EmailSender.Job</b> at the same and make the request together with the email address.
