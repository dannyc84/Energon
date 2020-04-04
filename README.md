# Project name

Energon is a framework to create and run software energy consumption tests. 

## Prerequisites

Before you begin, ensure you have met the following requirements:

It requires a couple of PC with a TCP connections, a phidgets ammeter and .NET installed on both a tested PC and a control PC.
* You have a couple of Windows Machine(s) with a TCP connections;
* You have a Phidgets ammeter
* You have installed the Phidget21.NET.dll compatible for your OS: https://www.phidgets.com/docs21/Language_-_C_Sharp#Libraries_and_Drivers.
* You have installed the .NET Framework Runtime version 4.0 or above: https://dotnet.microsoft.com/download/dotnet-framework/net40.
* You have installed the WCF Activation Component: https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-install-and-configure-wcf-activation-components

## Installing <project_name>

To install <project_name>, follow these steps:

Windows:
```
<install_command>
```
## Using Energon

To use Energon, follow these steps:

Open Energon solution through your Visual Studio IDE and press "Start" button to execute the application.
It will appear a form in which you have to enter the code to be compiled (the largest in the left) and the parameters to be passed at run time (the smallest in the right). Once entered, click on the "Run" button to proceed with the execution of the entered code.

Since I use reflection I have imposed that the entry point will be the Main method of the Driver class (therefore these must be there).

The result of the execution or any errors during compilation will be displayed on the textBox3.

## Contributing to <project_name>
To contribute to <project_name>, follow these steps:

1. Fork this repository.
2. Create a branch: `git checkout -b <branch_name>`.
3. Make your changes and commit them: `git commit -m '<commit_message>'`
4. Push to the original branch: `git push origin <project_name>/<location>`
5. Create the pull request.

Alternatively see the GitHub documentation on [creating a pull request](https://help.github.com/en/github/collaborating-with-issues-and-pull-requests/creating-a-pull-request).

## Contributors

Thanks to the following people who have contributed to this project:

* [@dannyc84](https://github.com/dannyc84)
* Valerio Di Bernardo (https://www.linkedin.com/in/valeriodibernardo)

## Contact

If you want to contact me you can reach me at daniele.crivello84@libero.it.

## License

This project uses the following license: [<license_name>](<link>).