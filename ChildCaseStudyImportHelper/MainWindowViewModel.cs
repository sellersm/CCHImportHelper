﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Windows;
using Ionic.Zip;
using System.Configuration;
using OCM.StringExtensions;

namespace ChildCaseStudyImporter
{
	public class MainWindowViewModel : NotifyPropertyChangedBase
	{
		#region properties

		private const string TempFolderName = "ZipTemp";

		private Dictionary<string, string> _locations;

		private List<string> _zipFileNames;
		public List<string> ZipFileNames
		{
			get
			{
				return _zipFileNames;
			}
			set
			{
				if (value != _zipFileNames)
				{
					_zipFileNames = value;
					OnPropertyChanged("ZipFileNames");
				}
			}
		}

		private string _outputCSVFileName;
		public string OutputCSVFileName
		{
			get
			{
				return _outputCSVFileName;
			}
			set
			{
				if (value != _outputCSVFileName)
				{
					_outputCSVFileName = value;
					OnPropertyChanged("OutputCSVFileName");
				}
			}
		}

		private string _locationCSVFileName;
		public string LocationCSVFileName
		{
			get
			{
				return _locationCSVFileName;
			}
			set
			{
				if (value != _locationCSVFileName)
				{
					_locationCSVFileName = value;
					OnPropertyChanged("LocationCSVFileName");
				}
			}
		}

		private bool _fixTempChildID;
		public bool FixTempChildID
		{
			get
			{
				return _fixTempChildID;
			}

			set
			{
				if (value != _fixTempChildID)
				{
					_fixTempChildID = value;
					OnPropertyChanged("FixTempChildID");
				}
			}
		 }

		// Memphis 8-13-15 added to indicate that the LocationIDs.csv file was not found.
		private bool _foundLocationCSVFile;
		public bool FoundLocationCSVFile 
		{ 
			get
			{
				return _foundLocationCSVFile;
			}
			set
			{
				_foundLocationCSVFile = value;
			}
		}



		private string _fixTempChildIDSuffix;
		public string FixTempChildIDSuffix
		{
			get
			{
				return _fixTempChildIDSuffix;
			}
			set
			{
				if (value != _fixTempChildIDSuffix)
				{
					_fixTempChildIDSuffix = value;
					OnPropertyChanged("FixTempChildIDSuffix");
				}
			}
		}

		private List<string> _childProfileUpdateValues;
		public List<string> ChildProfileUpdateValues
		{
			get
			{
				return _childProfileUpdateValues;
			}
			set
			{
				if (value != _childProfileUpdateValues)
				{
					_childProfileUpdateValues = value;
					OnPropertyChanged("ChildProfileUpdateValues");
				}
			}
		}

		private string _selectedChildProfileUpdate;
		public string SelectedChildProfileUpdate
		{
			get
			{
				return _selectedChildProfileUpdate;
			}
			set
			{
				if (value != _selectedChildProfileUpdate)
				{
					_selectedChildProfileUpdate = value;
					OnPropertyChanged("SelectedChildProfileUpdate");
				}
			}
		}

		#endregion

		public MainWindowViewModel()
		{
			OutputCSVFileName = ConfigurationManager.AppSettings.Get("DefaultOutputCSVFile");
			LocationCSVFileName = ConfigurationManager.AppSettings.Get("DefaultLocationCSVFile");

			// Memphis 8-13-2015 need to check if the LocationIDs.csv file can be accessed:
			//MessageBox.Show(LocationCSVFileName);

			if (File.Exists(LocationCSVFileName))
			{
				FoundLocationCSVFile = true;
				//MessageBox.Show("The LocationIDs.csv file exists.");
			}
			else
			{
				FoundLocationCSVFile = false;
				MessageBox.Show("Unable to get the LocationIDs.csv file! Please check that the _CRM folder is in the root of the M: drive! Contact Mark or Helpdesk if it is not, and/or you cannot find it.", "No Location File!", MessageBoxButton.OK, MessageBoxImage.Warning);
			}


			ChildProfileUpdateValues = ConfigurationManager.AppSettings.Get("ChildProfileUpdateValues").Split(',').ToList();
			SelectedChildProfileUpdate = ChildProfileUpdateValues.FirstOrDefault();
		}

		public void CreateCSV()
		{
			bool continueProcessing = ParseLocationsCSV();

			if (!continueProcessing)
				return;

			if (_zipFileNames == null || _zipFileNames.Count < 1)
			{
				MessageBox.Show("At least one zip file must be selected.");
				return;
			}

			if (Path.GetExtension(OutputCSVFileName).ToLower() != ".csv")
			{
				MessageBox.Show("The output CSV filename must end with the '.csv' extension.", "Invalid Output File Extension");
				return;
			}

			if (File.Exists(OutputCSVFileName))
			{
				var result = MessageBox.Show("The output CSV file already exists, do you want to overwrite it?", "File already exists", MessageBoxButton.YesNo);

				if (result != MessageBoxResult.Yes)
					return;
			}

			int row = 0;

			int truncationCount = 0;

			var dictionary = new Dictionary<int, Dictionary<int, string>>();

			dictionary.Add(row++, GetHeaders());

			if (Directory.Exists(TempFolderName))
				Directory.Delete(TempFolderName, true);

			if (!Directory.Exists(TempFolderName))
				Directory.CreateDirectory(TempFolderName);

			if (_zipFileNames != null)
			{
				foreach (var path in _zipFileNames)
				{
					if (!continueProcessing)
						continue;

					using (ZipFile zipFile = ZipFile.Read(path))
					{
						var entries = from e in zipFile.Entries
									  where Path.GetExtension(e.FileName).ToLower() == ".xml"
									  select e;

						foreach (var entry in entries)
						{
							if (!continueProcessing)
								continue;

							Models.ChildCaseStudy ccs = null;

							// Memphis 8-13-2015 need to trap for exception here, to identify if there's a duplicate file in this .zip file:
							try
							{
								entry.Extract(TempFolderName);
							}
							catch (ZipException zipEx)
							{
								// check the message for something like this: The file ZipTemp\SZ01-1498062742.xml already exists.
								if (zipEx.Message != null)
								{
									string exMsg = zipEx.Message.ToString();
									if ((exMsg != null || exMsg.Length > 0) && (exMsg.Contains("already exists")))
									{
										MessageBox.Show(string.Format("**WARNING***: There is a duplicate CCH form in the .zip files:{0}The duplicate CCH Form is: {1} {2}Found in this zip file: {3}", Environment.NewLine, entry.FileName,Environment.NewLine, zipFile.Name), "Duplicate CCH Form!", MessageBoxButton.OK, MessageBoxImage.Warning);
										//string.Format("first line{0}second line", Environment.NewLine);
										continueProcessing = false;
									}
								}
								else
								{
									continueProcessing = false;
									throw;
								}
								
							}
							

							XmlSerializer serializer = new XmlSerializer(typeof(Models.ChildCaseStudy));
							using (StreamReader reader = new StreamReader(Path.Combine(TempFolderName, entry.FileName)))
							{
								ccs = (Models.ChildCaseStudy)serializer.Deserialize(reader);
							}

							if (ccs != null)
							{
								if (_locations != null)
								{
									var location = _locations.Where(l => l.Key == ccs.Project.Number.ToUpper().Replace(" ","").Replace("_", "-")).FirstOrDefault();  /* Fix project id that has a space or an underscore instead of a dash */

									if (location.Value != null)
										ccs.Project.LocationID = location.Value;
									else
										continueProcessing = MessageBox.Show(string.Format("A Location ID was not found for the provided Project ID.\n\nProject ID: {0}\nFile Name: {1}\nZip File Name: {2}\n\nDo you want to continue processing?", ccs.Project.Number, entry.FileName, Path.GetFileName(zipFile.Name)), "Location ID not found", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
								}
								string fieldLengthErrors = "";

								/* Check text field lengths, because the plug-in host does not limit the text boxes */
								ccs.Father.WorksAsDetail = TrimAndCheckFieldLength(ccs.Father.WorksAsDetail, 100, ref fieldLengthErrors, "Father works as");
								ccs.Mother.WorksAsDetail = TrimAndCheckFieldLength(ccs.Mother.WorksAsDetail, 100, ref fieldLengthErrors, "Mother works as");
								ccs.CareGiver.WorksAsDetail = TrimAndCheckFieldLength(ccs.CareGiver.WorksAsDetail, 100, ref fieldLengthErrors, "Caregiver works as");
								ccs.CareGiver.RelationshipDetail = TrimAndCheckFieldLength(ccs.CareGiver.RelationshipDetail, 50, ref fieldLengthErrors, "Caregiver relationship");
								ccs.CareGiver.ReasonForDetail = TrimAndCheckFieldLength(ccs.CareGiver.ReasonForDetail, 50, ref fieldLengthErrors, "Caregiver reason");
								ccs.Housing.OtherWallsDescription = TrimAndCheckFieldLength(ccs.Housing.OtherWallsDescription, 50, ref fieldLengthErrors, "Housing wall other");
								ccs.Housing.OtherRoofDescription = TrimAndCheckFieldLength(ccs.Housing.OtherRoofDescription, 50, ref fieldLengthErrors, "Housing roofing other");
								ccs.Housing.OtherWaterDescription = TrimAndCheckFieldLength(ccs.Housing.OtherWaterDescription, 50, ref fieldLengthErrors, "Housing water source other");
								ccs.Housing.OtherCookingDescription = TrimAndCheckFieldLength(ccs.Housing.OtherCookingDescription, 50, ref fieldLengthErrors, "Housing cooking source other");
								ccs.Housing.OtherLightDescription = TrimAndCheckFieldLength(ccs.Housing.OtherLightDescription, 50, ref fieldLengthErrors, "Housing light source other");
								ccs.Housing.AreaDescription = TrimAndCheckFieldLength(ccs.Housing.AreaDescription, 1000, ref fieldLengthErrors, "Area description");
								ccs.AboutMe.FavoriteThingsToDo = TrimAndCheckFieldLength(ccs.AboutMe.FavoriteThingsToDo, 1000, ref fieldLengthErrors, "Favorite thing to do");
								ccs.AboutMe.WhenIPlayWithMyFriends = TrimAndCheckFieldLength(ccs.AboutMe.WhenIPlayWithMyFriends, 1000, ref fieldLengthErrors, "When I playing with friends");
								ccs.AboutMe.HelpsOutBy = TrimAndCheckFieldLength(ccs.AboutMe.HelpsOutBy, 1000, ref fieldLengthErrors, "When at home I help out by");
								ccs.AboutMe.AsksGod = TrimAndCheckFieldLength(ccs.AboutMe.AsksGod, 1000, ref fieldLengthErrors, "When I talk to God");
								ccs.AboutMe.WantsToBe = TrimAndCheckFieldLength(ccs.AboutMe.WantsToBe, 1000, ref fieldLengthErrors, "Someday I would like");
								ccs.AboutMe.TwoFavoriteThingsAndWhy = TrimAndCheckFieldLength(ccs.AboutMe.TwoFavoriteThingsAndWhy, 1000, ref fieldLengthErrors, "My most favorite things");
								ccs.AboutMe.AlsoEnjoys = TrimAndCheckFieldLength(ccs.AboutMe.AlsoEnjoys, 1000, ref fieldLengthErrors, "Other things the child enjoys");
								ccs.AboutMe.HowTheChildInteracts = TrimAndCheckFieldLength(ccs.AboutMe.HowTheChildInteracts, 1000, ref fieldLengthErrors, "Child's personality");
								ccs.AboutMe.PhysicalDevelopment = TrimAndCheckFieldLength(ccs.AboutMe.PhysicalDevelopment, 1000, ref fieldLengthErrors, "Physical development");
								ccs.AboutMe.SpiritualDevelopment = TrimAndCheckFieldLength(ccs.AboutMe.SpiritualDevelopment, 1000, ref fieldLengthErrors, "Spiritual development");
								ccs.School.NonAttendenceReason = TrimAndCheckFieldLength(ccs.School.NonAttendenceReason, 255, ref fieldLengthErrors, "Reason for not attending school");
								ccs.School.ClassLevel = TrimAndCheckFieldLength(ccs.School.ClassLevel, 20, ref fieldLengthErrors, "Class level");
								ccs.School.BestSubject = TrimAndCheckFieldLength(ccs.School.BestSubject, 1000, ref fieldLengthErrors, "Favorite subject");
								ccs.School.VocationalOrLifeSkill = TrimAndCheckFieldLength(ccs.School.VocationalOrLifeSkill, 1000, ref fieldLengthErrors, "Vocational or life skills");


								/* If any text box fields had to be truncated, list them in the Additional Information field because that is a memo field. */
								if (fieldLengthErrors.Length > 0)
								{
									if (string.IsNullOrEmpty(ccs.AdditionalInformation))
										ccs.AdditionalInformation = "Field truncation occurred in the following field(s):" + fieldLengthErrors;
									else
										ccs.AdditionalInformation += "\n\nField truncation occurred in the following field(s):" + fieldLengthErrors;
									
									truncationCount += 1;
								}

								ccs.CCHZipFileName = Path.GetFileNameWithoutExtension(zipFile.Name);

								dictionary.Add(row++, ConvertToDictionary(ccs));

							}
						}
					}
				}

				if (continueProcessing)
				{
					bool success = WriteCSVFile(dictionary);

					if (Directory.Exists(TempFolderName))
						Directory.Delete(TempFolderName, true);

					if (success)
						if (truncationCount > 0)
							MessageBox.Show(string.Format("Successfully created file at '{0}'\n\nNumber of cch forms added to file: {1}\nNumber of cch forms with truncation issues fixed: {2}", OutputCSVFileName, dictionary.Count - 1, truncationCount), "Success");
						else
							MessageBox.Show(string.Format("Successfully created file at '{0}'\n\nNumber of cch forms added to file: {1}", OutputCSVFileName, dictionary.Count - 1), "Success");
				}
			}
		}

		private bool WriteCSVFile(Dictionary<int, Dictionary<int, string>> dictionary)
		{
			bool success = false;

			StringBuilder csvString = new StringBuilder();

			foreach (var row in dictionary)
			{
				foreach (var column in row.Value)
				{
					csvString.Append(column.Value.FormatMultiline() + ",");
				}

				csvString.AppendLine();
			}

			try
			{
				File.WriteAllText(_outputCSVFileName, csvString.ToString());
				success = true;
			}
			catch (IOException ex)
			{
				MessageBox.Show("An error occured when attempting to create the output CSV file:\n\n" + ex.Message, "Error");
			}

			return success;
		}

		private bool ParseLocationsCSV()
		{
			bool continueProcessing = true;

			if (!string.IsNullOrWhiteSpace(LocationCSVFileName) && File.Exists(LocationCSVFileName))
			{
				_locations = new Dictionary<string, string>();

				try
				{
					using (StreamReader reader = new StreamReader(LocationCSVFileName))
					{
						string line;
						string[] cells;

						while ((line = reader.ReadLine()) != null)
						{
							cells = line.Split(',');

							if (!string.IsNullOrEmpty(cells[0]) || !string.IsNullOrEmpty(cells[1]))
							{
								if (cells[0].ToLower().Contains("projectid") && cells[1].ToLower().Contains("locationid"))
									continue;

								_locations.Add(cells[0], cells[1]);
							}
						}
					}
				}
				catch (IOException ex)
				{
					continueProcessing = MessageBox.Show(string.Format("{0}\n\nDo you want to continue processing without Location IDs?", ex.Message), "Error occurred", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
				}
			}

			return continueProcessing;
		}

		private string TrimAndCheckFieldLength(string fieldToCheck, int maxLength, ref string fieldLengthErrors, string errorCaption)
		{
			string fieldValue = "";

			if (!string.IsNullOrEmpty(fieldToCheck))
			{
				fieldValue = fieldToCheck.Trim();
				if (fieldValue.Length > maxLength)
				{
					fieldLengthErrors += "\n" + errorCaption + ": " + fieldToCheck;
					fieldValue = fieldValue.Left(maxLength);
				}
			}

			return fieldValue;
		}

		private void AddColumn(ref Dictionary<int, string> dictionary, int column, object value)
		{
			string stringValue = string.Empty;

			if (value != null)
				stringValue = value.ToString();

			stringValue = stringValue.Replace(",", string.Empty).Replace("\"", "'");

			dictionary.Add(column, stringValue);
		}

		private Dictionary<int, string> ConvertToDictionary(Models.ChildCaseStudy ccs)
		{
			var dictionary = new Dictionary<int, string>();
			int i = 0;

			AddColumn(ref dictionary, i++, "Individual Child Sponsorship");

			if (_locations != null)
			{
				// Memphis 9/3/13: force to uppercase to prevent mismatch of location codes when not all uppercase in the XML:
				//var location = _locations.Where(l => l.Key == ccs.Project.Number).FirstOrDefault();
				var location = _locations.Where(l => l.Key == ccs.Project.Number.ToUpper()).FirstOrDefault();
				AddColumn(ref dictionary, i++, location.Value);
			}
			else
			{
				AddColumn(ref dictionary, i++, string.Empty);
			}

			AddColumn(ref dictionary, i++, ccs.LastName);
			AddColumn(ref dictionary, i++, ccs.Gender);
			AddColumn(ref dictionary, i++, ccs.Father.WorksAs);
			AddColumn(ref dictionary, i++, ccs.Mother.WorksAs);
			AddColumn(ref dictionary, i++, string.Empty); //photo date
			AddColumn(ref dictionary, i++, string.Empty); //status
			AddColumn(ref dictionary, i++, ccs.AdditionalInformation);
			AddColumn(ref dictionary, i++, ccs.Housing.AreaDescription);
			AddColumn(ref dictionary, i++, ccs.School.AttendsSchool);
			AddColumn(ref dictionary, i++, ccs.Housing.BambooWalls);
			AddColumn(ref dictionary, i++, ccs.DOB.ToShortDateString());
			AddColumn(ref dictionary, i++, ccs.BirthDateAccuracy);
			AddColumn(ref dictionary, i++, ccs.Housing.BlockWalls);
			AddColumn(ref dictionary, i++, ccs.Housing.WellWater);
			AddColumn(ref dictionary, i++, ccs.CareGiver.ReasonFor);
			AddColumn(ref dictionary, i++, ccs.CareGiver.ReasonForDetail);
			AddColumn(ref dictionary, i++, ccs.CareGiver.Relationship);
			AddColumn(ref dictionary, i++, ccs.CareGiver.RelationshipDetail);
			AddColumn(ref dictionary, i++, ccs.CareGiver.WorksAs);
			AddColumn(ref dictionary, i++, ccs.CareGiver.WorksAsDetail.Left(100));
			AddColumn(ref dictionary, i++, ccs.WorkstationID);
			if (FixTempChildID)
			{
				AddColumn(ref dictionary, i++, ccs.TempChildID + FixTempChildIDSuffix);
			}
			else
			{
				AddColumn(ref dictionary, i++, ccs.TempChildID);
			}
			AddColumn(ref dictionary, i++, ccs.AboutMe.SpiritualDevelopment);
			AddColumn(ref dictionary, i++, ccs.AboutMe.PhysicalDevelopment);
			AddColumn(ref dictionary, i++, ccs.ChildLivesWith);
			AddColumn(ref dictionary, i++, ccs.Suffix);
			AddColumn(ref dictionary, i++, ccs.AboutMe.HowTheChildInteracts);
			AddColumn(ref dictionary, i++, _selectedChildProfileUpdate); //Child Profile Update
			AddColumn(ref dictionary, i++, ccs.School.ClassLevel);
			AddColumn(ref dictionary, i++, ccs.Housing.CommunityWater);
			AddColumn(ref dictionary, i++, ccs.Housing.OtherCookingDescription.OtherDescription(ccs.Housing.OtherCooking));
			AddColumn(ref dictionary, i++, string.Empty); //current completion date - should this map to completion date from xml or original?
			AddColumn(ref dictionary, i++, string.Empty); //Disability/Illness
			AddColumn(ref dictionary, i++, ccs.Housing.ElectricCooking);
			AddColumn(ref dictionary, i++, ccs.Housing.ElectrictyLight);
			AddColumn(ref dictionary, i++, ccs.Father.WorksAsDetail); //Father Works As Other
			AddColumn(ref dictionary, i++, ccs.School.BestSubject);
			AddColumn(ref dictionary, i++, ccs.FirstName);
			AddColumn(ref dictionary, i++, false); //Funded
			AddColumn(ref dictionary, i++, ccs.Housing.GasCooking);
			AddColumn(ref dictionary, i++, ccs.Housing.GeneratorLight);
			AddColumn(ref dictionary, i++, ccs.Housing.StrawRoof);
			AddColumn(ref dictionary, i++, false); //HIV positive
			AddColumn(ref dictionary, i++, string.Empty); //Child photo
			AddColumn(ref dictionary, i++, ccs.Housing.IndoorWater);
			AddColumn(ref dictionary, i++, ccs.Housing.OtherLightDescription.OtherDescription(ccs.Housing.OtherLight));
			AddColumn(ref dictionary, i++, ccs.Housing.NoneLight);
			AddColumn(ref dictionary, i++, ccs.MiddleName);
			AddColumn(ref dictionary, i++, ccs.Mother.WorksAsDetail);
			AddColumn(ref dictionary, i++, ccs.Housing.MudWalls);
			AddColumn(ref dictionary, i++, ccs.AboutMe.FavoriteThingsToDo);
			AddColumn(ref dictionary, i++, ccs.AboutMe.TwoFavoriteThingsAndWhy);
			AddColumn(ref dictionary, i++, ccs.Brothers);
			AddColumn(ref dictionary, i++, ccs.Sisters);
			AddColumn(ref dictionary, i++, ccs.Housing.LampLight);
			AddColumn(ref dictionary, i++, string.Empty); //original completion date - should this map to completion date from xml or current?
			AddColumn(ref dictionary, i++, false); //orphaned
			AddColumn(ref dictionary, i++, ccs.AboutMe.AlsoEnjoys);
			AddColumn(ref dictionary, i++, ccs.Housing.OtherWallsDescription.OtherDescription(ccs.Housing.OtherWalls));
			AddColumn(ref dictionary, i++, false); //photo stored
			AddColumn(ref dictionary, i++, string.Empty); //profile update notification
			AddColumn(ref dictionary, i++, ccs.School.NonAttendenceReason);
			AddColumn(ref dictionary, i++, ccs.Housing.RiverWater);
			AddColumn(ref dictionary, i++, ccs.Housing.OtherRoofDescription.OtherDescription(ccs.Housing.OtherRoof));
			AddColumn(ref dictionary, i++, ccs.Housing.WoodRoof);
			AddColumn(ref dictionary, i++, ccs.AboutMe.WantsToBe);
			AddColumn(ref dictionary, i++, ccs.Housing.TileRoof);
			AddColumn(ref dictionary, i++, ccs.Housing.TinRoof);
			AddColumn(ref dictionary, i++, ccs.School.VocationalOrLifeSkill);
			AddColumn(ref dictionary, i++, ccs.Housing.OtherWaterDescription.OtherDescription(ccs.Housing.OtherWater));
			AddColumn(ref dictionary, i++, ccs.AboutMe.HelpsOutBy);
			AddColumn(ref dictionary, i++, ccs.AboutMe.WhenIPlayWithMyFriends);
			AddColumn(ref dictionary, i++, ccs.AboutMe.AsksGod);
			AddColumn(ref dictionary, i++, ccs.Housing.WoodCooking);
			AddColumn(ref dictionary, i++, ccs.Housing.WoodWalls);
			AddColumn(ref dictionary, i++, ccs.CCHZipFileName);

			return dictionary;
		}

		private Dictionary<int, string> GetHeaders()
		{
			var dictionary = new Dictionary<int, string>();
			int i = 0;

			dictionary.Add(i++, "Child group");
			dictionary.Add(i++, "Location");
			dictionary.Add(i++, "Last name");
			dictionary.Add(i++, "Gender");
			dictionary.Add(i++, "Father Works As");
			dictionary.Add(i++, "Mother Works As");
			dictionary.Add(i++, "Current Photo Date");
			dictionary.Add(i++, "Child Profile Status");
			dictionary.Add(i++, "Additional Child Information");
			dictionary.Add(i++, "Area Description");
			dictionary.Add(i++, "Attending School");
			dictionary.Add(i++, "Bamboo Wall");
			dictionary.Add(i++, "Birth date");
			dictionary.Add(i++, "Birthdate Accuracy");
			dictionary.Add(i++, "Block Wall");
			dictionary.Add(i++, "Borehole/Well Water");
			dictionary.Add(i++, "Caregiver Reason");
			dictionary.Add(i++, "Caregiver Reason Other");
			dictionary.Add(i++, "Caregiver Relation");
			dictionary.Add(i++, "Caregiver Relation Other");
			dictionary.Add(i++, "Caregiver Works As");
			dictionary.Add(i++, "Caregiver Works As Other");
			dictionary.Add(i++, "CCH Entered By");
			dictionary.Add(i++, "CH CCH Tempid");
			dictionary.Add(i++, "Child faith...");
			dictionary.Add(i++, "Child health...");
			dictionary.Add(i++, "Child Lives With");
			dictionary.Add(i++, "Child Name Suffix");
			dictionary.Add(i++, "Child personality");
			dictionary.Add(i++, "Child Profile Update");
			dictionary.Add(i++, "Class Level");
			dictionary.Add(i++, "Community Tap Water");
			dictionary.Add(i++, "Cooking Source Other");
			dictionary.Add(i++, "Current Program Completion Date");
			dictionary.Add(i++, "Disability/Illness");
			dictionary.Add(i++, "Electric Stove");
			dictionary.Add(i++, "Electricity Light");
			dictionary.Add(i++, "Father Works As Other");
			dictionary.Add(i++, "Favorite Subject or Activity");
			dictionary.Add(i++, "First name");
			dictionary.Add(i++, "Funded");
			dictionary.Add(i++, "Gas Stove");
			dictionary.Add(i++, "Generator Light");
			dictionary.Add(i++, "Grass/leaves Roof");
			dictionary.Add(i++, "HIV positive");
			dictionary.Add(i++, "Image");
			dictionary.Add(i++, "Indoor Water");
			dictionary.Add(i++, "Light Source Other");
			dictionary.Add(i++, "Light-None");
			dictionary.Add(i++, "Middle name");
			dictionary.Add(i++, "Mother Works As Other");
			dictionary.Add(i++, "Mud Wall");
			dictionary.Add(i++, "My favorite thing to do when I am by myself is");
			dictionary.Add(i++, "My most favorite things");
			dictionary.Add(i++, "Number of brothers");
			dictionary.Add(i++, "Number of sisters");
			dictionary.Add(i++, "Oil Lamp Light");
			dictionary.Add(i++, "Original Program Completion Date");
			dictionary.Add(i++, "Orphaned");
			dictionary.Add(i++, "Other things the child enjoys");
			dictionary.Add(i++, "Other Wall");
			dictionary.Add(i++, "Photo Stored");
			dictionary.Add(i++, "Profile Update Notification");
			dictionary.Add(i++, "Reason for Not Attending School");
			dictionary.Add(i++, "River Water");
			dictionary.Add(i++, "Roofing Other");
			dictionary.Add(i++, "Roofing Wood Roof");
			dictionary.Add(i++, "Someday I would like");
			dictionary.Add(i++, "Tile Roof");
			dictionary.Add(i++, "Tin Roof");
			dictionary.Add(i++, "Vocational or Life Skills");
			dictionary.Add(i++, "Water Source Other");
			dictionary.Add(i++, "When at home I help out by");
			dictionary.Add(i++, "When I play with my friends we");
			dictionary.Add(i++, "When I talk to God I ask Him");
			dictionary.Add(i++, "Wood Fire");
			dictionary.Add(i++, "Wood Wall");
			dictionary.Add(i++, "CCH zip file name");
			return dictionary;
		}
	}
}
