# UrbanMagnetometer
This project records magnetic field data from a BioMed fluxgate sensor, puts GPS-disciplined timestamps on it,
and uploads it to the Google Drive for further processing.

# Getting started

## System requirements

This program was developed using .NET framework v4.5.2, and was running properly on Windows 8 and Windows 10 systems.

System requirements for .NET framework 4.5 can be found here - https://msdn.microsoft.com/en-us/library/8z6watww(v=vs.110).aspx

If you don't have 4.5 redistributable, you can download one from Microsoft website - https://www.microsoft.com/en-us/download/details.aspx?id=42642

I've been using these ASUS X551 laptop to operate four synchronized stations - https://www.amazon.com/15-6-inch-Celeron-2-16GHz-Processor-Windows/dp/B00L49X8E6

## Hardware
Each station was built with the following devices:

  * ASUS X551 laptop (any computer satisfying the system requirements).
  * Biomed eFM3A Fluxgate magnetometer
  * Garmin 18x LVC GPS (any NMEA GPS with 1 PPS output)
  * SerialIO SIO-U232-59 (RS232 to USB converter with +5V on DB9 pin 9)

## Biomed-eMains-eFM-x.dll
In toder to to inteface the magnetometer, the project uses a .NET wrapper that can be found here https://github.com/lenazh/Biomed-eMains-eFM-x . The wrapper encapsulates the eFM-x API.dll functions into an object. **The wrapper uses customized version of eFM-x API.dll that behaves differently from the one coming with the device (as of 01/2016).**

## Obtaining Google authorization credentials
This program stores data in Google Drive to make collaboration easy, and ensure the computer doesn't run out of space. In order to function properly, you need to first obtain the authorization credential that the program will use to log in to Google Drive.

How to obtain the secret key:

  - Navigate to Google Drive API wizard https://console.developers.google.com/flows/enableapi?apiid=drive
  - Click Credentials -> Create credentials -> OAuth Client ID
  - Fill out the forms
  - Once created, click "Download JSON"
  - Save the .json file as nuri_station.json into the project folder

## Deploying magnetometers

After the Google authorization credentials are obtained, build the sample logger project. Run sample logger, which will open the browser and prompt you to log into your Google account (where the data will be uploaded). Keep in  mind that by default the logger records the magnetic field at 4ksps, so the data can add up quickly.

After logging in, build the installer project. This will add the authentication information into the installer, so you won't have to log in on target machines. Run the output on the computers that will be logging the data. You will be prompted to enter the station name and the cache folder. This folder is used to store temporary files and the data that couldn't be uploaded until the internet connection is available. The installer will create two shortcuts - Data Grabber and Sample Grabber.

Data Grabber is useful for long-term magnetic data recording. Sample Grabber is useful for obtaining short magnetic data samples.

## Powering Biomed magnetometer with a battery pack

## Using Data Grabber

## Using Sample Grabber

# Technical documantation

  * DataGrabber - logs the data continuously for long periods of time
  * SampleGrabber - records short magnetic data samples
  * GDriveFolderMerge - merges folders with the same name in Google drive folder (should they appear)
  * Utils - library containting the logic
