# Mock Sensor Server and Client

- Tcp Server
- Tcp Client
- Supports a number of commands
- Logging and Error Handling enabled
- Mulithreaded using ThreadPool from Task.Run
- Requires dotnet8

### How to Run

#### server

`dotnet run --project sensor-server`

#### client

`dotnet run --project sensor-client GET_STATUS`
