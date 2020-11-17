using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Globalization;

public static class CDecisionLogger
{
	// Member variables
	private static string m_outputPath = Application.dataPath + "\\Logging\\Combat_";
	private static int m_combatCounter = 1;


	// Methods
	public static void Init()
	{
		m_outputPath = Application.dataPath + "\\Logging\\Combat_" + m_combatCounter + "\\";
		Directory.CreateDirectory(m_outputPath);
		m_combatCounter++;

		foreach (CCombatParticipant participant in CCombatSystem.GetInstance().GetBothTeamsAsOneList())
		{
			if (participant.GetEntity().GetIsControlledByAI())
			{
				CreateEntityFile(participant);
			}
		}
	}


	private static void CreateEntityFile(CCombatParticipant participant)
	{
		using (FileStream fileStream = File.Create(m_outputPath + participant.GetEntity().name + "_DecisionLog.csv"))
		{
			byte[] header = new UTF8Encoding(true).GetBytes("");
			fileStream.Write(header, 0, header.Length);
		}

		using (FileStream fileStream = File.Open(m_outputPath + participant.GetEntity().name + "_DecisionLog.csv", FileMode.Truncate))
		{
			string fileString = string.Format("Participant: {0}\r\nActions: {1};\r\n", participant.GetEntity().name, participant.GetEntity().GetEntityActions().Count);

			for (int i = 0; i < participant.GetEntity().GetEntityActions().Count; i++)
			{
				fileString += string.Format("{0}. Action:;{1}\r\n", (i+1), participant.GetEntity().GetEntityActions()[i]);

				for (int j = 0; j < participant.GetEntity().GetEntityActions()[i].GetConsiderationCount(); j++)
				{
					fileString += string.Format(";{0}.{1}. Consideration:; {2}\r\n", (i+1), (j+1), participant.GetEntity().GetEntityActions()[i].GetConsiderationAt(j).GetType().ToString().Substring(1));
				}

				fileString += "\r\n";
			}

			fileString += "\r\n";
			
			byte[] stringToWrite = new UTF8Encoding(true).GetBytes(fileString);
			fileStream.Write(stringToWrite, 0, stringToWrite.Length);

			fileStream.Close();
		}
	}

	public static void ResetEntityFile(CCombatParticipant participant)
	{
		CreateEntityFile(participant);
	}


	public static void LogDecisionStart(CCombatParticipant participant)
	{
		if (!File.Exists(m_outputPath + participant.GetEntity().name + "_DecisionLog.csv"))
		{
			CreateEntityFile(participant);
		}

		using (FileStream fileStream = File.Open(m_outputPath + participant.GetEntity().name + "_DecisionLog.csv", FileMode.Append))
		{
			string currentTime = System.DateTime.Now.ToString(new CultureInfo("de-DE"));
			string fileString = string.Format("Decision Making of Participant \"{0}\" started;Gauge Position: {1};;Time: {2}", participant.GetEntity().name, participant.GetGaugePosition(), currentTime);

			fileString += "\r\n";

			byte[] stringToWrite = new UTF8Encoding(true).GetBytes(fileString);
			fileStream.Write(stringToWrite, 0, stringToWrite.Length);

			fileStream.Close();
		}
	}


	public static void LogDecisionEnd(CCombatParticipant participant)
	{
		using (FileStream fileStream = File.Open(m_outputPath + participant.GetEntity().name + "_DecisionLog.csv", FileMode.Append))
		{
			string currentTime = System.DateTime.Now.ToString(new CultureInfo("de-DE"));
			string fileString = string.Format("Decision Making of Participant \"{0}\" ended; at {1}", participant.GetEntity().name, currentTime);

			fileString += "\r\n\r\n\r\n";

			byte[] stringToWrite = new UTF8Encoding(true).GetBytes(fileString);
			fileStream.Write(stringToWrite, 0, stringToWrite.Length);

			fileStream.Close();
		}
	}


	public static void LogAction(CAction action, float finalScore, CAIContext context, CCombatParticipant participant)
	{
		using (FileStream fileStream = File.Open(m_outputPath + participant.GetEntity().name + "_DecisionLog.csv", FileMode.Append))
		{
			string fileString = string.Format("Score-Calculation of Action: {0};-> {1};using;\"{2}\";on;{3}.\r\n", 
				action.GetName(), context.GetExecutorAsParticipant().GetEntity().name, context.GetAbilityToExecute().GetAbilityName(), context.GetTarget().name);

			for (int i = 0; i < action.GetConsiderationCount(); i++)
			{
				fileString += string.Format("{0}:;{1};\r\n", action.GetConsiderationAt(i).GetName(), action.GetConsiderationAt(i).ToString());
			}

			fileString += string.Format("Action Score (Compensation: \"{0}\"):;{1}", CUtilityAiSystem.GetInstance().PrintActionScoreCompensation(), finalScore);

			fileString += "\r\n\r\n";

			byte[] stringToWrite = new UTF8Encoding(true).GetBytes(fileString);
			fileStream.Write(stringToWrite, 0, stringToWrite.Length);

			fileStream.Close();
		}
	}


	public static void LogActionOptions(List<CUtilityAiSystem.CActionOption> actionOptions, CCombatParticipant participant)
	{
		using (FileStream fileStream = File.Open(m_outputPath + participant.GetEntity().name + "_DecisionLog.csv", FileMode.Append))
		{
			string fileString = string.Format("Final List of Actionscores of {0}:\r\n;Actionscore;Manipulation;Result Score;Ability Name; Target\r\n", participant.GetEntity().name);

			foreach (CUtilityAiSystem.CActionOption actionOption in actionOptions)
			{
				fileString += string.Format("; {0};{1};{2};{3};{4}\r\n", 
					actionOption.GetActionScore(),actionOption.GetManipulation(), actionOption.GetFinalActionScore(), actionOption.GetAbility().GetAbilityName(), actionOption.GetTarget().name);
			}

			fileString += "\r\n";

			byte[] stringToWrite = new UTF8Encoding(true).GetBytes(fileString);
			fileStream.Write(stringToWrite, 0, stringToWrite.Length);

			fileStream.Close();
		}
	}


	public static void LogChosenOption(CUtilityAiSystem.CActionOption chosenOption, CUtilityAiSystem.DecisionHeuristic decisionHeuristic, CCombatParticipant participant)
	{
		using (FileStream fileStream = File.Open(m_outputPath + participant.GetEntity().name + "_DecisionLog.csv", FileMode.Append))
		{
			string fileString = string.Format("{0} (used Heuristic: {1}) decides for; {2};{3};{4}\r\n\r\n\r\n",
				participant.GetEntity().name, decisionHeuristic.ToString(), chosenOption.GetActionScore(), chosenOption.GetAbility().GetAbilityName(), chosenOption.GetTarget().name); ;

			byte[] stringToWrite = new UTF8Encoding(true).GetBytes(fileString);
			fileStream.Write(stringToWrite, 0, stringToWrite.Length);

			fileStream.Close();
		}
	}


	public static void ExportAllLogs()
	{
		string[] username = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\');

		string targetDirectory = string.Format("C:\\Users\\{0}\\Desktop\\Logging", username[username.Length - 1]);
		if (!Directory.Exists(targetDirectory))
		{
			Directory.CreateDirectory(targetDirectory);
		}

		string consoleLogPathAndFile = string.Format("C:\\Users\\{0}\\AppData\\Locallow\\DefaultCompany\\Masterarbeit_UtilityBasedAI\\output_log.txt", username[username.Length - 1]);
		string consoleLogTargetPathAndFile = string.Format("C:\\Users\\{0}\\Desktop\\Logging\\ConsoleLog.txt", username[username.Length - 1]);
		File.Copy(consoleLogPathAndFile, consoleLogTargetPathAndFile, true);

		string[] directories = Directory.GetDirectories(Application.dataPath + "\\Logging");

		foreach (string directoryPath in directories)
		{
			string[] pathElements = directoryPath.Split('\\');
			string folderName = pathElements[pathElements.Length - 1];
			if (!Directory.Exists(targetDirectory + "\\" + folderName))
			{
				Directory.CreateDirectory(targetDirectory + "\\" + folderName);
			}
			
			string[] files = Directory.GetFiles(Application.dataPath + "\\Logging\\" + folderName);
			foreach (string filePath in files)
			{
				string[] filePathElements = filePath.Split('\\');
				string fileName = filePathElements[filePathElements.Length - 1];
				File.Copy(filePath, targetDirectory + "\\" + folderName + "\\" + fileName);
			}
		}
	}	
}