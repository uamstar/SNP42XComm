# SanicaSNP42X
API for communicating with SNP42X(SNP421/SNP422) parking lock.

## Development
Target framework is .Net Standard 2.0.

## Usage
1. Create a management object with serial port configures.
    ```
    Manager manager = Manager.getInstance(
                    Comport.PortName, 
                    Comport.BaudRate,
                    Comport.Parity,
                    Comport.DataBits,
                    Comport.StopBits);
    ```
2. (Optional) Set a logger to show logs.
    ```
    manager.SetLogger(logger);
    ```
3. Create a connector to a SNP42X of specific address.
    ```
    RS485Cytel cytel1 = manager.CreateRS485Cytel(0x01);    // address 0x01
    // create more connectors
    RS485Cytel cytel2 = manager.CreateRS485Cytel(0x02);    // address 0x02
    RS485Cytel cytel7 = manager.CreateRS485Cytel(0x07);
    ...
    RS485Cytel cytel10 = manager.CreateRS485Cytel(0x0A);
    ```
4. Set up event handlers for every RS485Cytel object.
    ```
    // handle response for Plate Status command
    cytel1.ParkingPlateStatusResponse += PSRespReceivied;
    // handle response for Parking Control command
    cytel1.ParkingControlResponse += PCRespReceivied;
    // If no response is replied in 5 seconds after a command is sent, 
    // a NoResponse notification would be sent.
    cytel1.NoResponse += NoRespReceivied;
    ```
5. Then you can send commands with RS485Cytel objects.
    ```
    cytel1.Polling();
    cytel1.GetLockingPlateStatus();
    cytel1.TurnLoopOn();
    cytel1.TurnLoopOff();
    cytel1.TurnLockUp();
    cytel1.TurnLockDown();
    ```

MainWindow.xaml.cs in project SanicaSNP42XDemo shows how to use this API.
