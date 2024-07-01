# SNP42XComm

This repository contains .NET projects for demonstrating how to communicate with parking lock SNP421/SNP422 produced by SANICA, 株式会社サニカ, and modeling behavior of the lock:  

* SanicaSNP42X -- API for communicating with SNP42X(SNP421/SNP422) parking lock.
* SanicaSNP42XDemo -- A project to demonstrat API SanicaSNP42X usage.
* SanicaSNP42XConsole -- A console project to demonstrat API SanicaSNP42X usage.
* SNP42XSimulator -- An emulator used to replace the real SNP42X lock during development.
* WebSimulateSNP42X -- An emulator based on SNP42XSimulator and can be configured and controlled via webservice.

## Goal
The goal of this repository is to develop a simulator that could be used to simulate SNP42X parking lock to facilitate development. Developers can assume different situations on SNP42X and perform simulations without real device.

## Benefit
1. Speed up the development process
   * Developer doesn't need to worry about problems caused by physical devices and save the time for operating physical devices.
   * Easy to extend and simulate failed conditions of parking lot to discover key problems.
   * Facilitate regression test.
2. Help develop various SNP42X API for different programming languages
   * WebSimulateSNP42X can be controlled via webservice that is supported by most languages like C/C++, Java, Python, Go ... etc. and can work on Mac, Linux and Windows.

## Development
Refer to README in the projects.

## To do
1. Make sure how Loop count, Off base count, On level count, Off level count and On base count work and simulate these numbers.
2. Collect more regular and irregular conditions of SNP42X and the related data to make the simulation more realistic. 


