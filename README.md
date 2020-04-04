# Project name

Energon is a framework to create and run software energy consumption tests. 

## Project Description

This is the second release of Energon.

## Project History

The first release of Energon was edited by Davide Morelli, a PhD Student of Computer Science at the University of Pise.                                                                                                                                                           His work gave attention also to the hardware infrastructure underlying (classified by Pattern of Memory Access, CPU, Compilers, etc...) to estimate the energy consumptions of a running generic algorithm.

## Prerequisites

Before you begin, ensure you have met the following requirements:

It requires a couple of PC with TCP connections, a phidgets ammeter and .NET installed on both a tested PC and a control PC.
* You have a (or a couple) of Windows Machine(s) with TCP connections;
* You have a Phidgets ammeter
* You have installed the Phidget21.NET.dll compatible for your OS: https://www.phidgets.com/docs21/Language_-_C_Sharp#Libraries_and_Drivers.
* You have installed the .NET Framework Runtime version 4.0 or above: https://dotnet.microsoft.com/download/dotnet-framework/net40.
* You have installed the WCF Activation Component: https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-install-and-configure-wcf-activation-components

## Using Energon

To use Energon, follow these steps:

* Open Energon solution through your Visual Studio IDE and press "Start" button to execute the application.
* Once it's running, tt will appear a form in which you'll have to enter the code to be compiled (the largest in the left) and the parameters to be passed at run time (the smallest in the right). 
* Once entered, click on the "Run" button to proceed with the execution of the entered code.
* The results will be stored in an existing SQL server table (to create it take a look at the file: [EndPointWCFLibraries/DataAccess.cs](EndPointWCFLibraries/DataAccess.cs)) and visible in new files created locally in the client machine.

## Contributors

Thanks to the following people who have contributed to this project:

* [@dannyc84](https://github.com/dannyc84)
* Valerio Di Bernardo (https://www.linkedin.com/in/valeriodibernardo)

## Contact

If you want to contact me you can reach me at daniele.crivello84@libero.it.

## License

This project uses the following license: [MIT](LICENSE.md).
