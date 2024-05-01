using Harmony;
using LobotomyBaseMod;
using System;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace CosmosJerry
{
    class Harmony_Patch
    {
		private static string path;
		public static Sprite JerryEncounter;
		public Harmony_Patch()
		{
			try
			{
				path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
				HarmonyInstance harmonyInstance = HarmonyInstance.Create("Lobotomy.arangMk2.CosmosJerry");

				MethodInfo method = typeof(Harmony_Patch).GetMethod("AgentModel_HorrorDamage", AccessTools.all);
				harmonyInstance.Patch(typeof(AgentModel).GetMethod("HorrorDamage", AccessTools.all), new HarmonyMethod(method), null, null);
				method = typeof(Harmony_Patch).GetMethod("AgentUI_EncounterActivate", AccessTools.all);
				harmonyInstance.Patch(typeof(InGameUI.AgentUI).GetMethod("EncounterActivate", AccessTools.all), new HarmonyMethod(method), null, null);

				MakeHorrorIcon();
			}
			catch (Exception ex)
			{
				File.WriteAllText(Application.dataPath + "/BaseMods/CosmosJerryError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
			}
		}

		//Patch method for change fear damage
		//恐怖ダメージを変更するためのパッチ用メソッド
		public static bool AgentModel_HorrorDamage(UnitModel target,AgentModel __instance)
        {
			
			try
			{
				if (target is CreatureModel && (target as CreatureModel).script is CosmosJerry)
				{
					CreatureModel creature = target as CreatureModel;
					if (!creature.IsEscaped())
					{
						int num = target.GetRiskLevel() - __instance.level-1;
						if (num < -1)
						{
							__instance.GetUnit().agentUI.EncounterActivate(-52);
							__instance.GetUnit().SpeechHorrorLyric(0);
							return false;
						}else 
                        {
                            if (num < 0)
                            {
								num = 0;
                            }
							__instance.GetUnit().agentUI.EncounterActivate(num);
							__instance.GetUnit().SpeechHorrorLyric(0);
							return false;
                        }


					}
					//もし恐怖ダメージを入れたい場合はここでダメージ処理も書こう
				}
			}catch(Exception ex)
            {
				WriteLog(ex);
            }
			return true;
        }

		//Patch method for changing icons and text
		//恐怖レベルのアイコンとテキストを変更するためのパッチ用メソッド
		public static bool AgentUI_EncounterActivate(int level, ref InGameUI.AgentUI __instance)
		{
			if (level == -52)
            {
				
				try
				{
					
					FieldInfo fieldInfo = __instance.GetType().GetField("currentHorrorLevel", BindingFlags.NonPublic | BindingFlags.Instance);
					if (fieldInfo.GetValue(__instance) != null)
					{

						fieldInfo.SetValue(__instance, 0);
					}
					
					string text = GetCustomEncounterText();
					Sprite sprite = JerryEncounter;
					Color color = new Color(0.988f, 0.455f, 0.918f);
					__instance.EncounterIcon.sprite = sprite;
					Graphic encounterText = __instance.EncounterText;
					Color color2 = color;
					__instance.EncounterIcon.color = color2;
					encounterText.color = color2;
					__instance.EncounterText.text = text;

					
					if (__instance.UIAnim != null)
					{
						__instance.UIAnim.SetBool("Damage", true);
					}

					
					FieldInfo fieldInfo2 = __instance.GetType().GetField("_encounterTimer",AccessTools.all);
					;
						if (fieldInfo2.GetValue(__instance)!=null)
						{
						
							object timerObj = fieldInfo2.GetValue(__instance);
							if (timerObj is Timer timer)
							{
							
								timer.StartTimer(2f);
							}
							else
							{
							// *A depressed face because things aren't going well*
							WriteLog("(._. )");
							}
						}
					__instance.EncounterActiveControl.SetActive(true);
				}catch(Exception ex)
                {
					WriteLog(ex);
                }
				
					return false;
			}

			return true;
		}

		//icon set
		//アイコンを用意
		public static void MakeHorrorIcon()
        {
			try
			{
				Texture2D texture2D = new Texture2D(2, 2);
				texture2D.LoadImage(File.ReadAllBytes(path + "/Sprite/JerryEncounter.png"));
				JerryEncounter = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f));
			}catch(Exception ex)
            {
				WriteLog(ex.Message);
            }
		}

		//It's a hassle, so it's better to use the Localize function
		//めんどくさいのでLocalizeの機能を使った方が良いです
		public static string GetCustomEncounterText()
		{

			string result = "Relieved";
			try
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(path + "/xml");
				string currentLanguage = GlobalGameManager.instance.GetCurrentLanguage();
				bool flag = File.Exists(directoryInfo.FullName + "/" + currentLanguage + ".xml");
				if (flag)
				{
					string path2 = directoryInfo.FullName + "/" + currentLanguage + ".xml";
					result = GetXmlString(path2);
				}
			}catch(Exception ex)
            {
				WriteLog(ex.Message);
            }

			return result;
		}

		public static string GetXmlString(string path)
		{
			string result = "null";
			XmlDocument xmlDocument = new XmlDocument();
			using (XmlReader xmlReader = XmlReader.Create(path))
			{
				while (xmlReader.Read())
				{
					bool flag = xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "JerryHorror";
					bool flag2 = flag;
					if (flag2)
					{
						bool flag3 = xmlReader.Read() && xmlReader.NodeType == XmlNodeType.Text;
						bool flag4 = flag3;
						if (flag4)
						{
							result = xmlReader.Value;
						}
					}
				}
			}
			return result;
		}

		//I used it to output error logs to Log and to confirm that processing was successful.
		//Logにエラーログを出したり、処理の成功を確認したりするために使用
		public static void WriteLog(string str)
		{
			using (StreamWriter streamWriter = new StreamWriter(ModDebug.LogFilePath, true))
			{
				streamWriter.WriteLine(str);
			}
		}
		public static void WriteLog(Exception str)
		{
			using (StreamWriter streamWriter = new StreamWriter(ModDebug.LogFilePath, true))
			{
				streamWriter.WriteLine(str+ Environment.NewLine + str.StackTrace);
			}
		}

	}


}
