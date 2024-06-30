# SNP42XSimulator
An emulator used to replace the real SNP42X lock during development.

## Development
Target framework is .NET Standard 2.0.

## Usage
1. Get a ParkingLotAgent instance with the serial port for communicating.
    ```
    ParkingLotAgent agent = ParkingLotAgent.getInstance("COM3");
    ```
2. Add some parking spaces to the "parking lot".
    ```
    agent.AddParkingSpace(1);  // add a parking space with address #1
    agent.AddParkingSpace(2);  // add a parking space with address #2
    agent.AddParkingSpace(9);  // add a parking space with address #9
    ...
    ```
3. Now we have a model of parking lot and it's parking spaces, we can state it.
    ```
    agent.Start();
    ```
4. Then you can set Loop Sensor Status of specific parking space, so that it replies as your setting while getting a Parking Status (PS) request.
    ```
    agent.SetLoopSensorStatus(5, LoopSensorStatus.OFF);  // set parking space #5's loop sensor OFF.
    ```
5. Don't forget stop the parking lot agent while all tasks are done.
    ```
    agent.Stop();
    ```

ParkingLostService.cs in project WebSimulateSNP42X shows how to use this API.

## To do
1. Simulate irregular conditions like
    * Irregal locking (object on the lock plate to prevent incline)
    * Error during decline/incline
    * Sensor status Error
2. Simulate Mat switch status
3. Get real use cases like ...
    * How does ForceON/ForceOFF change to another status?