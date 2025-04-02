# MeasureThat.net

MeasureThat.net is the website to create and run JavaScript benchmarks. It uses BenchmarkJS (v.2.1.1) as a test runner.

Running at: https://www.measurethat.net/



get started.

# Check out the application's source code:
Open the terminal window. Create folder `source` for the application and check out the code:
```bash
$ mkdir source
$ cd source/
$ git clone https://github.com/thecoderok/MeasureThat.net.git
Cloning into 'MeasureThat.net'...
remote: Counting objects: 2324, done.
remote: Compressing objects: 100% (17/17), done.
remote: Total 2324 (delta 2), reused 0 (delta 0), pack-reused 2305
Receiving objects: 100% (2324/2324), 1.16 MiB | 650.00 KiB/s, done.
Resolving deltas: 100% (1478/1478), done.
Checking connectivity... done.
vitalii@vitalii-vm:~/source$ 
```

# Build the application

Now everything is ready to build the application. Step into the folder with source code (`MeasureThat.net/src/BenchmarkLab$ 
`) and run restore dotnet, npm and bower packages (it will take couple of minutes):
```bash
dotnet restore
npm install
bower install
```

Build frontend:
```bash
gulp
```

Build the application:

```bash
dotnet build
```

Build should succeed:

![build_the_application](https://cloud.githubusercontent.com/assets/3173477/19282172/087a00ea-8fa2-11e6-95e1-d9fd4eea31b0.png)

# Prepare the configuration file

Open appsettings.json file in the text editor and:
* Disable External authentication: set `UseFacebookAuthentication` / `UseGoogleAuthentication` / `UseTwitterAuthentication` / `UseMicrosoftAuthenticaiton` to `false`
(Please let me know in the commens if you want to be able to use External authentication, I can explain how it can be done )
* Set `RequireEmailConfirmation` to false
* Disable reCaptcha: set `ReCaptchaEnabled` to `false`. 

Result should look like this:

```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "6fbb4f00-bf94-4fe8-a0e3-a5b4e1283fc2"
  },
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  },
  "UseFacebookAuthentication": false,
  "UseGoogleAuthentication": false,
  "UseTwitterAuthentication": false,
  "UseMicrosoftAuthenticaiton": false,
  "ResultsConfig": {
    "UploadResultsToDb": true,
    "UploadGuestUserResultsToDb": true
  },
  "GoogleAnalytics": {
    "Enabled": true,
    "Identifier": "UA-83528903-1"
  },
  "AllowGuestUsersToCreateBenchmarks": true,
  "SenderEmail": "MeasureThat@outlook.com",
  "SenderName": "MeasureThat Admin",
  "RequireEmailConfirmation": false,
  "ReCaptchaEnabled": false 
}
```


# Prepare the database
Now we need to prepare the database.
Switch to postgres user and run the postgres client: 

```bash
$ sudo -i -u postgres
$ psql
```

If you need to set the passowrd for the postgres user, enter command the password to do so. This may be needed right after PostgreSQL was installed.

```bash
postgres=# \password postgres
Enter new password: 
Enter it again:
```

Create the database `MeasureThat`:
```bash
postgres=# create database MeasureThat;
CREATE DATABASE
```

You can exit from postgres cleint and switch back to the original user:
```bash
postgres=# \q
postgres@vitalii-vm:~$ exit
logout
vitalii@vitalii-vm:~/source/MeasureThat.net/src/BenchmarkLab$ 
```
## Set the connection string ([using Secret Manager tool](http://the-coderok.azurewebsites.net/2016/06/14/Using-Secret-Manager-tool-to-store-connection-strings/)):
```bash
$ dotnet user-secrets set ConnectionStrings:DefaultConnection 'User ID=postgres;Password=root;Host=localhost;Port=5432;Database=MeasureThat;
Pooling=true;'
```
## Create the database schema:
```bash
$ dotnet ef migrations add testPg
Project BenchmarkLab (.NETCoreApp,Version=v1.0) was previously compiled. Skipping compilation.
Done. To undo this action, use 'dotnet ef migrations remove'

$ dotnet ef database update
Project BenchmarkLab (.NETCoreApp,Version=v1.0) will be compiled because Input items added from last build
Compiling BenchmarkLab for .NETCoreApp,Version=v1.0
Compilation succeeded.
    0 Warning(s)
    0 Error(s)
Time elapsed 00:00:04.6049020
 
Done.
```

# Run the application

Set the ASPNETCORE_ENVIRONMENT variable to Development
```bash
$ export ASPNETCORE_ENVIRONMENT=Development
```

And (drum roll) run the application:
```bash
dotnet run
```

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/DuZXbB2q08k/0.jpg)](https://www.youtube.com/watch?v=DuZXbB2q08k)
