# UrbanMagnetometer
This project records magnetic field data from a BioMed fluxgate sensor, puts GPS-disciplined timestamps on it,
and uploads it to the Google Drive for further processing.

# Getting started

## System requirements

## Hardware

## Obtaining Google authorization credentials
How to obtain the secret key:

  - Navigate to Google Drive API wizard https://console.developers.google.com/flows/enableapi?apiid=drive
  - Click Credentials -> Create credentials -> OAuth Client ID
  - Fill out the forms
  - Once created, click "Download JSON"
  - Save the .json file as nuri_station.json into the project folder

## Deploying magnetometers

After the Google authorization credentials are obtained, build the sample logger project. Run sample logger, which will open the browser and prompt you to log into your Google account (where the data will be uploaded). Keep in  mind that by default the logger records the magnetic field at 4ksps, so the data can add up quickly.

After logging in, build the installer project. This will add the authentication information into the installer, so you won't have to log in on target machines. Run the output on the computers that will be logging the data. You will be prompted to enter the station name and the cache folder. This folder is used to store temporary files and the data that couldn't be uploaded until the internet connection is available. The installer will create two short cuts - Data Grabber and Sample Grabber.

Data Grabber is useful for long-term magnetic data recording. Sample Grabber is useful for obtaining short magnetic data samples.

## Powering Biomed magnetometer with a battery pack

## Using Data Grabber

## Using Sample Grabber

# Technical documantation

  * DataGrabber -- logs the data continuously for long periods of time
  * SampleGrabber -- records short magnetic data samples
  * GDriveFolderMerge -- merges folders with the same name in Google drive folder (should they appear)
  * Utils -- library containting the logic
