### concurrent programming

#### requirements
- `dotnet core`

#### steps to get DBM running (macOS)
- `cd` into `concurrent-utils` and `dotnet new classlib -n Reubs.Concurrent.Utils -o .` - remove the generated `Class1.cs` and then `dotnet build`
- `cd` into `dms` and `dotnet new sln`
- add `concurrent-utils` to the solution `dotnet sln add ../concurrent-utils/Reubs.Concurrent.Utils.csproj`
- create a console app `dotnet new console -n dms` - be sure to retain original program code in `Program.cs`
- `dotnet sln add dms.csproj`
- `dotnet add dms.csproj reference ../concurrent-utils/Reubs.Concurrent.Utils.csproj`
- now run the dms `dotnet run --project dms.csproj`

#### connect to the DBM server
- `nc localhost 64125`
- now you can start issuing commands. i.e., `crub 100` will create 100 rubbish rows then you can read them with `r 1-100`
- want to know more? so do I. I haven't looked at this since 2015, though you can take a look at `QueryManager` and figure out the available commands that way