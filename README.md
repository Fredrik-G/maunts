# maunts - aka see how unlucky you are

##### Personal-use application for looking up interesting (mounts) World of Warcraft stuff.

I created this small application for me and my friends to lookup our character progress, and specifically mount progress. The application is created as a console application, written in python, a GUI application written in C# and using WPF. And lastly as a spreadsheet using Google Apps Scripts and made for use with Google Spreadsheet.

The maunts application will lookup given characters and check for their progress on given bosses using the public WoW API. The application will then summarize the total number of kills and calculate the user's RNG for each boss. (RNG = Random Number Generator, aka this checks how lucky you are.)

### Screenshots
### Console Application
![Console Application](/../Screenshots/maunts_-_Console_Application.png?raw=true "Console Application")
### GUI Application
![GUI Application](/../Screenshots/maunts_-_GUI.png?raw=true "GUI Application")
### Spreadsheet
![Spreadsheet Application](/../Screenshots/maunts_-_Spreadsheet.png?raw=true "Spreadsheet Application")


Creating this application taught me several interesting techniques:
* Using an API to read data.
* Using JSON to process data.
* Creating a GUI using Windows Presentation Foundation (WPF) and XML/XAML-style designing.
* Creating scripts using Javascript / Google Apps Script.
* Using scripts to load data into a spreadsheet.
