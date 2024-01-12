using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using RVNLMIS.Common;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RVNLMIS.DAC;

namespace RVNLMIS.Controllers
{
    public class StraightLinePlotController : Controller
    {

        #region ---- dynamic settings from db ----
        public double OGLbaselineHeight = 0;
        public double OGLscalingFactor = 0;
        public double SectionMinus_Y1 = 0;
        public double SectionPlus_Y2 = 0;
        public double SectionTextPlusY = 0;
        public int SectionTextWeight = 0;
        public double SectionTextRotationDegree = 0;
        public double SectionNamePlus_X = 0;
        public double SectionNamePlus_Y = 0;
        public int SectionNameWeight = 0;
        public double SecrionNameRotation = 0;
        public double EntityMinus_Y1 = 0;
        public double EntityPlus_Y2 = 0;
        public int EntityChainageCondition = 0;
        public double EntityInConditionMinus_Y1 = 0;
        public double EntityInConditionPlus_Y2 = 0;
        public double EntityTextPlusY = 0;
        public int EntityTextWeight = 0;
        public double EntityTextRotationDegree = 0;
        public double EntityNamePlus_X = 0;
        public double EntityNamePlus_Y = 0;
        public int EntityNameWeight = 0;
        public double EntityNameRotation = 0;
        public int PackageBaseYposition = 0;
        public int PackageScalingFactor = 0;
        public double SecondLineThicknessMinus_Y1 = 0;
        public double SecondLineThicknessMinus_Y2 = 0;
        public double SecondLineThickness = 0;
        public string SecondLineColor = "";
        public string SecondLIneTextColor = "";
        public double SecondLIneTextMinus_X = 0;
        public double SecondLIneTextMinus_Y = 0;
        public int SecondLIneTextWeight = 0;
        public double SecondLIneTextRotation = 0;
        public double SecondLineSectionYposition = 0;
        public double ThirdLineEntityYposition = 0;
        public double OGLLineHeight = 0;


        #region -- entity third line
        public string ThirdLineSSColor = "Green";
        public double ThirdLineSSThickness = 20;
        public double ThirdLineSSMinus_Y1 = 10;
        public double ThirdLineSSPlus_Y2 = 550;
        public double ThirdLineSSText_MinusY = 50;
        public int ThirdLineSSText_Weight = 8;
        public string ThirdLineSSText_Color = "Green";
        public double ThirdLineSSText_Rotation = 90;
        public string ThirdLineEEColor = "Green";
        public double ThirdLIneEEThickness = 10;
        public double ThirdLIneEEMinus_Y1 = 10;
        public double ThirdLIneEEPlus_Y2 = 550;
        public double ThirdLineEEText_MinusY = 50;
        public int ThirdLineEEText_Weight = 8;
        public string ThirdLineEEText_Color = "Green";
        public double ThirdLineEEText_Rotation = 90;
        public string ThirdLineSEColor = "Green";
        public double ThirdLineSEThickness = 40;
        public double ThirdLineTextYposition = 10;
        public int ThirdLineTextWeight = 8;
        public string ThirdLineTextColor = "Cyan";
        public double ThirdLineTextRotation = 0;
        #endregion

        #region -- Section second line --
        public string SecondLineMarkersTwoColor = "Green";
        public double SecondLineMarkersTwoThickness = 20;
        public double SecondLineMarkersTwoMinus_Y1 = 10;
        public double SecondLineMarkersTwoPlus_Y2 = 30;
        public double SecondLineSectionTextYposition = 30;
        public int SecondLineSectionTextWeight = 15;
        public string SecondLineSectionTextColor = "Green";
        public double SecondLineSectionTextRotation = 90;
        public double SecondLineSectionNameYposition = 10;
        public int SecondLineSectionNameWeight = 15;
        public string SecondLineSectionNameColor = "Green";
        public double SecondLineSectionNameRotation = 0;
        public string SecondLineSSColor = "Green";
        public double SecondLineSSThickness = 20;
        public double SecondLineSSMinus_Y1 = 10;
        public double SecondLineSSPlus_Y2 = 30;
        public string SecondLineEEColor = "Green";
        public double SecondLineEEThickness = 10;
        public double SecondLineEEMinus_Y1 = 10;
        public double SecondLineEEPlus_Y2 = 30;
        public double SecondLineMidSectionNameYposition = 10;
        public int SecondLineMidSectionNameWeight = 20;
        public string SecondLineMidSectionNameColor = "Cyan";
        public double SecondLineMidSectionNameRotation = 0;
        #endregion

        #region -- OGL minMax --
        public double PlotPointsOGLminX = 99999999999;
        public double PlotPointsOGLminY = 99999999999;
        public double PlotPointsOGLmaxX = 0;
        public double PlotPointsOGLmaxY = 0;

        public int GridPlottingminX = 14500;
        public int GridPlottingminY = 0;
        public int GridPlottingmaxX = 395000;
        public int GridPlottingmaxY = 1500;
        #endregion



        /// <summary>
        /// Gets the setting details.
        /// </summary>
        private void GetSettingDetails()
        {
            try
            {
                SetDefaultSettings();
                using (var db = new dbRVNLMISEntities())
                {
                    var settingList = db.tblSettings.Where(o => o.IsDelete == false).ToList();
                    if (settingList != null)
                    {
                        foreach (var item in settingList)
                        {
                            switch (item.SKey)
                            {
                                case "OGLbaselineHeight":
                                    OGLbaselineHeight = Functions.ParseDouble(item.Value) == 0 ? 1000 : Functions.ParseDouble(item.Value);
                                    break;
                                case "OGLscalingFactor":
                                    OGLscalingFactor = Functions.ParseDouble(item.Value) == 0 ? 100 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SectionMinus_Y1":
                                    SectionMinus_Y1 = Functions.ParseDouble(item.Value) == 0 ? 250 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SectionPlus_Y2":
                                    SectionPlus_Y2 = Functions.ParseDouble(item.Value) == 0 ? 150 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SectionTextPlusY":
                                    SectionTextPlusY = Functions.ParseDouble(item.Value) == 0 ? 50 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SectionTextWeight":
                                    SectionTextWeight = Functions.ParseInteger(item.Value) == 0 ? 50 : Functions.ParseInteger(item.Value);
                                    break;
                                case "SectionTextRotationDegree":
                                    SectionTextRotationDegree = Functions.ParseDouble(item.Value) == 0 ? 250 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SectionNamePlus_X":
                                    SectionNamePlus_X = Functions.ParseDouble(item.Value) == 0 ? 150 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SectionNamePlus_Y":
                                    SectionNamePlus_Y = Functions.ParseDouble(item.Value) == 0 ? 50 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SectionNameWeight":
                                    SectionNameWeight = Functions.ParseInteger(item.Value) == 0 ? 50 : Functions.ParseInteger(item.Value);
                                    break;
                                case "SecrionNameRotation":
                                    SecrionNameRotation = Functions.ParseDouble(item.Value) == 0 ? 0 : Functions.ParseDouble(item.Value);
                                    break;
                                case "EntityMinus_Y1":
                                    EntityMinus_Y1 = Functions.ParseDouble(item.Value) == 0 ? 15 : Functions.ParseDouble(item.Value);
                                    break;
                                case "EntityPlus_Y2":
                                    EntityPlus_Y2 = Functions.ParseDouble(item.Value) == 0 ? 150 : Functions.ParseDouble(item.Value);
                                    break;
                                case "EntityChainageCondition":
                                    EntityChainageCondition = Functions.ParseInteger(item.Value) == 0 ? 25 : Functions.ParseInteger(item.Value);
                                    break;
                                case "EntityInConditionMinus_Y1":
                                    EntityInConditionMinus_Y1 = Functions.ParseDouble(item.Value) == 0 ? 15 : Functions.ParseDouble(item.Value);
                                    break;
                                case "EntityInConditionPlus_Y2":
                                    EntityInConditionPlus_Y2 = Functions.ParseDouble(item.Value) == 0 ? 150 : Functions.ParseDouble(item.Value);
                                    break;
                                case "EntityTextPlusY":
                                    EntityTextPlusY = Functions.ParseDouble(item.Value) == 0 ? 200 : Functions.ParseDouble(item.Value);
                                    break;
                                case "EntityTextWeight":
                                    EntityTextWeight = Functions.ParseInteger(item.Value) == 0 ? 50 : Functions.ParseInteger(item.Value);
                                    break;
                                case "EntityTextRotationDegree":
                                    EntityTextRotationDegree = Functions.ParseDouble(item.Value) == 0 ? 90 : Functions.ParseDouble(item.Value);
                                    break;
                                case "EntityNamePlus_X":
                                    EntityNamePlus_X = Functions.ParseDouble(item.Value) == 0 ? 0 : Functions.ParseDouble(item.Value);
                                    break;
                                case "EntityNamePlus_Y":
                                    EntityNamePlus_Y = Functions.ParseDouble(item.Value) == 0 ? 550 : Functions.ParseDouble(item.Value);
                                    break;
                                case "EntityNameWeight":
                                    EntityNameWeight = Functions.ParseInteger(item.Value) == 0 ? 50 : Functions.ParseInteger(item.Value);
                                    break;
                                case "EntityNameRotation":
                                    EntityNameRotation = Functions.ParseDouble(item.Value) == 0 ? 90 : Functions.ParseDouble(item.Value);
                                    break;
                                case "PackageBaseYposition":
                                    PackageBaseYposition = Functions.ParseInteger(item.Value) == 0 ? 1000 : Functions.ParseInteger(item.Value);
                                    break;
                                case "PackageScalingFactor":
                                    PackageScalingFactor = Functions.ParseInteger(item.Value) == 0 ? 100 : Functions.ParseInteger(item.Value);
                                    break;
                                case "SecondLineThicknessMinus_Y1":
                                    SecondLineThicknessMinus_Y1 = Functions.ParseDouble(item.Value) == 0 ? 350 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineThickness":
                                    SecondLineThickness = Functions.ParseInteger(item.Value) == 0 ? 100 : Functions.ParseInteger(item.Value);
                                    break;
                                case "SecondLineThicknessMinus_Y2":
                                    SecondLineThicknessMinus_Y2 = Functions.ParseDouble(item.Value) == 0 ? 350 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLIneTextMinus_X":
                                    SecondLIneTextMinus_X = Functions.ParseDouble(item.Value) == 0 ? 100 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLIneTextMinus_Y":
                                    SecondLIneTextMinus_Y = Functions.ParseDouble(item.Value) == 0 ? 350 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLIneTextWeight":
                                    SecondLIneTextWeight = Functions.ParseInteger(item.Value) == 0 ? 18 : Functions.ParseInteger(item.Value);
                                    break;
                                case "SecondLIneTextRotation":
                                    SecondLIneTextRotation = Functions.ParseDouble(item.Value) == 0 ? 0 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineSectionYposition":
                                    SecondLineSectionYposition = Functions.ParseDouble(item.Value) == 0 ? 350 : Functions.ParseDouble(item.Value);
                                    break;
                                case "ThirdLineEntityYposition":
                                    ThirdLineEntityYposition = Functions.ParseDouble(item.Value) == 0 ? 550 : Functions.ParseDouble(item.Value);
                                    break;
                                case "OGLLineHeight":
                                    OGLLineHeight = Functions.ParseDouble(item.Value) == 0 ? 300 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineColor":
                                    SecondLineColor = Convert.ToString(item.Value) == string.Empty ? "#FF2C2C" : Convert.ToString(item.Value);
                                    break;
                                case "SecondLIneTextColor":
                                    SecondLIneTextColor = Convert.ToString(item.Value) == string.Empty ? "#FF2C2C" : Convert.ToString(item.Value);
                                    break;

                                //Entity third line

                                case "ThirdLineSSColor":
                                    ThirdLineSSColor = Convert.ToString(item.Value) == string.Empty ? "Green" : Convert.ToString(item.Value);
                                    break;
                                case "ThirdLineSSThickness":
                                    ThirdLineSSThickness = Functions.ParseDouble(item.Value) == 0 ? 20 : Functions.ParseDouble(item.Value);
                                    break;
                                case "ThirdLineSSMinus_Y1":
                                    ThirdLineSSMinus_Y1 = Functions.ParseDouble(item.Value) == 0 ? 10 : Functions.ParseDouble(item.Value);
                                    break;
                                case "ThirdLineSSPlus_Y2":
                                    ThirdLineSSPlus_Y2 = Functions.ParseDouble(item.Value) == 0 ? 550 : Functions.ParseDouble(item.Value);
                                    break;
                                case "ThirdLineSSText_MinusY":
                                    ThirdLineSSText_MinusY = Functions.ParseDouble(item.Value) == 0 ? 50 : Functions.ParseDouble(item.Value);
                                    break;
                                case "ThirdLineSSText_Weight":
                                    ThirdLineSSText_Weight = Functions.ParseInteger(item.Value) == 0 ? 8 : Functions.ParseInteger(item.Value);
                                    break;
                                case "ThirdLineSSText_Color":
                                    ThirdLineSSText_Color = Convert.ToString(item.Value) == string.Empty ? "Green" : Convert.ToString(item.Value);
                                    break;
                                case "ThirdLineEEColor":
                                    ThirdLineEEColor = Convert.ToString(item.Value) == string.Empty ? "Green" : Convert.ToString(item.Value);
                                    break;
                                case "ThirdLineSSText_Rotation":
                                    ThirdLineSSText_Rotation = Functions.ParseDouble(item.Value) == 0 ? 90 : Functions.ParseDouble(item.Value);
                                    break;
                                case "ThirdLIneEEThickness":
                                    ThirdLIneEEThickness = Functions.ParseDouble(item.Value) == 0 ? 10 : Functions.ParseDouble(item.Value);
                                    break;
                                case "ThirdLIneEEMinus_Y1":
                                    ThirdLIneEEMinus_Y1 = Functions.ParseDouble(item.Value) == 0 ? 10 : Functions.ParseDouble(item.Value);
                                    break;
                                case "ThirdLIneEEPlus_Y2":
                                    ThirdLIneEEPlus_Y2 = Functions.ParseDouble(item.Value) == 0 ? 550 : Functions.ParseDouble(item.Value);
                                    break;
                                case "ThirdLineEEText_MinusY":
                                    ThirdLineEEText_MinusY = Functions.ParseDouble(item.Value) == 0 ? 50 : Functions.ParseDouble(item.Value);
                                    break;
                                case "ThirdLineEEText_Weight":
                                    ThirdLineEEText_Weight = Functions.ParseInteger(item.Value) == 0 ? 8 : Functions.ParseInteger(item.Value);
                                    break;
                                case "ThirdLineEEText_Color":
                                    ThirdLineEEText_Color = Convert.ToString(item.Value) == string.Empty ? "Green" : Convert.ToString(item.Value);
                                    break;
                                case "ThirdLineSEColor":
                                    ThirdLineSEColor = Convert.ToString(item.Value) == string.Empty ? "Green" : Convert.ToString(item.Value);
                                    break;
                                case "ThirdLineEEText_Rotation":
                                    ThirdLineEEText_Rotation = Functions.ParseDouble(item.Value) == 0 ? 90 : Functions.ParseDouble(item.Value);
                                    break;
                                case "ThirdLineSEThickness":
                                    ThirdLineSEThickness = Functions.ParseDouble(item.Value) == 0 ? 40 : Functions.ParseDouble(item.Value);
                                    break;
                                case "ThirdLineTextYposition":
                                    ThirdLineTextYposition = Functions.ParseDouble(item.Value) == 0 ? 10 : Functions.ParseDouble(item.Value);
                                    break;
                                case "ThirdLineTextWeight":
                                    ThirdLineTextWeight = Functions.ParseInteger(item.Value) == 0 ? 8 : Functions.ParseInteger(item.Value);
                                    break;
                                case "ThirdLineTextColor":
                                    ThirdLineTextColor = Convert.ToString(item.Value) == string.Empty ? "Cyan" : Convert.ToString(item.Value);
                                    break;
                                case "ThirdLineTextRotation":
                                    ThirdLineTextRotation = Functions.ParseDouble(item.Value) == 0 ? 0 : Functions.ParseDouble(item.Value);
                                    break;
                                // second line
                                case "SecondLineMarkersTwoColor":
                                    SecondLineMarkersTwoColor = Convert.ToString(item.Value) == string.Empty ? "Green" : Convert.ToString(item.Value);
                                    break;
                                case "SecondLineMarkersTwoThickness":
                                    SecondLineMarkersTwoThickness = Functions.ParseDouble(item.Value) == 0 ? 20 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineMarkersTwoMinus_Y1":
                                    SecondLineMarkersTwoMinus_Y1 = Functions.ParseDouble(item.Value) == 0 ? 10 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineMarkersTwoPlus_Y2":
                                    SecondLineMarkersTwoPlus_Y2 = Functions.ParseDouble(item.Value) == 0 ? 30 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineSectionTextYposition":
                                    SecondLineSectionTextYposition = Functions.ParseDouble(item.Value) == 0 ? 30 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineSectionTextWeight":
                                    SecondLineSectionTextWeight = Functions.ParseInteger(item.Value) == 0 ? 15 : Functions.ParseInteger(item.Value);
                                    break;
                                case "SecondLineSectionTextColor":
                                    SecondLineSectionTextColor = Convert.ToString(item.Value) == string.Empty ? "Green" : Convert.ToString(item.Value);
                                    break;
                                case "SecondLineSectionTextRotation":
                                    SecondLineSectionTextRotation = Functions.ParseDouble(item.Value) == 0 ? 90 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineSectionNameYposition":
                                    SecondLineSectionNameYposition = Functions.ParseDouble(item.Value) == 0 ? 10 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineSectionNameWeight":
                                    SecondLineSectionNameWeight = Functions.ParseInteger(item.Value) == 0 ? 15 : Functions.ParseInteger(item.Value);
                                    break;
                                case "SecondLineSectionNameColor":
                                    SecondLineSectionNameColor = Convert.ToString(item.Value) == string.Empty ? "Green" : Convert.ToString(item.Value);
                                    break;
                                case "SecondLineSectionNameRotation":
                                    SecondLineSectionNameRotation = Functions.ParseDouble(item.Value) == 0 ? 0 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineSSColor":
                                    SecondLineSSColor = Convert.ToString(item.Value) == string.Empty ? "Green" : Convert.ToString(item.Value);
                                    break;
                                case "SecondLineSSThickness":
                                    SecondLineSSThickness = Functions.ParseDouble(item.Value) == 0 ? 20 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineSSMinus_Y1":
                                    SecondLineSSMinus_Y1 = Functions.ParseDouble(item.Value) == 0 ? 10 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineSSPlus_Y2":
                                    SecondLineSSPlus_Y2 = Functions.ParseDouble(item.Value) == 0 ? 30 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineEEColor":
                                    SecondLineEEColor = Convert.ToString(item.Value) == string.Empty ? "Green" : Convert.ToString(item.Value);
                                    break;
                                case "SecondLineEEThickness":
                                    SecondLineEEThickness = Functions.ParseDouble(item.Value) == 0 ? 10 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineEEMinus_Y1":
                                    SecondLineEEMinus_Y1 = Functions.ParseDouble(item.Value) == 0 ? 10 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineEEPlus_Y2":
                                    SecondLineEEPlus_Y2 = Functions.ParseDouble(item.Value) == 0 ? 30 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineMidSectionNameYposition":
                                    SecondLineMidSectionNameYposition = Functions.ParseDouble(item.Value) == 0 ? 10 : Functions.ParseDouble(item.Value);
                                    break;
                                case "SecondLineMidSectionNameWeight":
                                    SecondLineMidSectionNameWeight = Functions.ParseInteger(item.Value) == 0 ? 15 : Functions.ParseInteger(item.Value);
                                    break;
                                case "SecondLineMidSectionNameColor":
                                    SecondLineMidSectionNameColor = Convert.ToString(item.Value) == string.Empty ? "Cyan" : Convert.ToString(item.Value);
                                    break;
                                case "SecondLineMidSectionNameRotation":
                                    SecondLineMidSectionNameRotation = Functions.ParseDouble(item.Value) == 0 ? 0 : Functions.ParseDouble(item.Value);
                                    break;
                                case "PlotPointsOGLminX":
                                    PlotPointsOGLminX = Functions.ParseDouble(item.Value) == 0 ? 99999999999 : Functions.ParseDouble(item.Value);
                                    break;
                                case "PlotPointsOGLminY":
                                    PlotPointsOGLminY = Functions.ParseDouble(item.Value) == 0 ? 99999999999 : Functions.ParseDouble(item.Value);
                                    break;
                                case "PlotPointsOGLmaxX":
                                    PlotPointsOGLmaxX = Functions.ParseDouble(item.Value) == 0 ? 0 : Functions.ParseDouble(item.Value);
                                    break;
                                case "PlotPointsOGLmaxY":
                                    PlotPointsOGLmaxY = Functions.ParseDouble(item.Value) == 0 ? 0 : Functions.ParseDouble(item.Value);
                                    break;
                                case "GridPlottingminX":
                                    GridPlottingminX = Functions.ParseInteger(item.Value) == 0 ? 14500 : Functions.ParseInteger(item.Value);
                                    break;
                                case "GridPlottingminY":
                                    GridPlottingminY = Functions.ParseInteger(item.Value) == 0 ? 0 : Functions.ParseInteger(item.Value);
                                    break;
                                case "GridPlottingmaxX":
                                    GridPlottingmaxX = Functions.ParseInteger(item.Value) == 0 ? 395000 : Functions.ParseInteger(item.Value);
                                    break;
                                case "GridPlottingmaxY":
                                    GridPlottingmaxY = Functions.ParseInteger(item.Value) == 0 ? 1500 : Functions.ParseInteger(item.Value);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        SetDefaultSettings();
                    }
                }
            }
            catch (Exception ex) { SetDefaultSettings(); }
        }

        /// <summary>
        /// Sets the default settings.
        /// </summary>
        private void SetDefaultSettings()
        {
            #region ----base line ----
            OGLLineHeight = 300;
            OGLbaselineHeight = 1000;
            OGLscalingFactor = 100;
            SectionMinus_Y1 = 250;
            SectionPlus_Y2 = 150;
            SectionTextPlusY = 50;
            SectionTextWeight = 50;
            SectionTextRotationDegree = 90;
            SectionNamePlus_X = 50;
            SectionNamePlus_Y = 50;
            SectionNameWeight = 50;
            SecrionNameRotation = 0;
            EntityMinus_Y1 = 15;
            EntityPlus_Y2 = 150;
            EntityChainageCondition = 25;
            EntityInConditionMinus_Y1 = 15;
            EntityInConditionPlus_Y2 = 150;
            EntityTextPlusY = 200;
            EntityTextWeight = 50;
            EntityTextRotationDegree = 90;
            EntityNamePlus_X = 0;
            EntityNamePlus_Y = 550;
            EntityNameWeight = 50;
            EntityNameRotation = 90;
            PackageBaseYposition = 1000;
            PackageScalingFactor = 100;
            SecondLineThicknessMinus_Y1 = 350;
            SecondLineThicknessMinus_Y2 = 350;
            SecondLineThickness = 60;
            SecondLineColor = "#FF2C2C";
            SecondLIneTextColor = "#FF2C2C";
            SecondLIneTextMinus_X = 100;
            SecondLIneTextMinus_Y = 350;
            SecondLIneTextWeight = 18;
            SecondLIneTextRotation = 0;
            SecondLineSectionYposition = 350;
            ThirdLineEntityYposition = 550;
            #endregion
            #region -- entity third line --
            ThirdLineSSColor = "Green";
            ThirdLineSSThickness = 20;
            ThirdLineSSMinus_Y1 = 10;
            ThirdLineSSPlus_Y2 = 550;
            ThirdLineSSText_MinusY = 50;
            ThirdLineSSText_Weight = 8;
            ThirdLineSSText_Color = "Green";
            ThirdLineSSText_Rotation = 90;
            ThirdLineEEColor = "Green";
            ThirdLIneEEThickness = 10;
            ThirdLIneEEMinus_Y1 = 10;
            ThirdLIneEEPlus_Y2 = 550;
            ThirdLineEEText_MinusY = 50;
            ThirdLineEEText_Weight = 8;
            ThirdLineEEText_Color = "Green";
            ThirdLineEEText_Rotation = 90;
            ThirdLineSEColor = "Green";
            ThirdLineSEThickness = 40;
            ThirdLineTextYposition = 10;
            ThirdLineTextWeight = 8;
            ThirdLineTextColor = "Cyan";
            ThirdLineTextRotation = 0;
            #endregion

            #region -- section second line --
            SecondLineMarkersTwoColor = "Green";
            SecondLineMarkersTwoThickness = 20;
            SecondLineMarkersTwoMinus_Y1 = 10;
            SecondLineMarkersTwoPlus_Y2 = 30;
            SecondLineSectionTextYposition = 30;
            SecondLineSectionTextWeight = 15;
            SecondLineSectionTextColor = "Green";
            SecondLineSectionTextRotation = 90;
            SecondLineSectionNameYposition = 10;
            SecondLineSectionNameWeight = 15;
            SecondLineSectionNameColor = "Green";
            SecondLineSectionNameRotation = 0;
            SecondLineSSColor = "Green";
            SecondLineSSThickness = 20;
            SecondLineSSMinus_Y1 = 10;
            SecondLineSSPlus_Y2 = 30;
            SecondLineEEColor = "Green";
            SecondLineEEThickness = 10;
            SecondLineEEMinus_Y1 = 10;
            SecondLineEEPlus_Y2 = 30;
            SecondLineMidSectionNameYposition = 10;
            SecondLineMidSectionNameWeight = 20;
            SecondLineMidSectionNameColor = "Cyan";
            SecondLineMidSectionNameRotation = 0;
            #endregion

            #region -- OGL minMax --
            PlotPointsOGLminX = 99999999999;
            PlotPointsOGLminY = 99999999999;
            PlotPointsOGLmaxX = 0;
            PlotPointsOGLmaxY = 0;

            GridPlottingminX = 14500;
            GridPlottingminY = 0;
            GridPlottingmaxX = 395000;
            GridPlottingmaxY = 1500;
            #endregion

        }

        #endregion


        // GET: StraightLinePlot
        public ActionResult Index()
        {
            AutoCadViewerModel obj = new AutoCadViewerModel();
            if (Session["UserData"] != null)
            {
                var objUserM = (UserModel)Session["UserData"];
                obj.PackageId = objUserM.RoleTableID;
                return View(obj);
            }
            else
            {
                return RedirectToAction("Index", "Login");

            }


        }


        /// <summary>
        /// Load starightt line chart
        /// </summary>
        /// <param name="oModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadStraightLinePlot(AutoCadViewerModel oModel)
        {
            try
            {
                if (oModel.PackageId != 0)
                {
                    string _Plot = string.Empty;

                    _Plot = Baseline(oModel);
                    oModel.FileName = _Plot;
                    return Json(oModel);
                }
                else
                {
                    return Json(1);
                }
            }
            catch (Exception)
            {

                return Json("Exception");
            }
        }

        private string Baseline(AutoCadViewerModel oModel)
        {
            GetSettingDetails();
            int _packageId = oModel.PackageId;
            //Get Section Details
            DataTable dtSection = new DataTable();
            dtSection = GetSections(_packageId);

            //CREATE FILES
            DxfDocument dxfDocument = new DxfDocument();


            //CREATE LAYER
            Layer layer = new Layer("Layer-01");
            dxfDocument.Layers.Add(layer);
            layer.Lineweight = Lineweight.W30;
            string color = Convert.ToString("Green");
            ApplyColorToLayer(layer, color);


            //CREATE LAYER
            Layer layer2 = new Layer("Layer-02");
            dxfDocument.Layers.Add(layer2);
            layer2.Lineweight = Lineweight.W13;
            string color2 = Convert.ToString("Red");
            ApplyColorToLayer(layer2, color2);

            //CREATE LAYER
            Layer layer3 = new Layer("Layer-03");
            dxfDocument.Layers.Add(layer3);
            layer3.Lineweight = Lineweight.W20;
            string color3 = Convert.ToString("RED");
            ApplyColorToLayer(layer3, color3);

            //------------------------CREATE GRAYSCALE GRID------------------------------------
            GridPlotting(dxfDocument);

            double sectionStart = 0;
            double sectionEnd = 0;
            int count = dtSection.Rows.Count;

            sectionStart = Convert.ToDouble(dtSection.Rows[0]["StartChainage"].ToString().Trim().Replace("+", "").ToString());
            sectionEnd = Convert.ToDouble(dtSection.Rows[count - 1]["EndChainage"].ToString().Trim().Replace("+", "").ToString());

            //GET OGL
            DataTable dtOGL = GetOGL(_packageId);
            double topHeight = Convert.ToDouble(dtOGL.Compute("min([Height])", string.Empty));

            //FACTOR FOR OGL PRINTING
            double baselineHeight = OGLbaselineHeight;

            double scalingFactor = OGLscalingFactor;
            double lineHeight = baselineHeight - OGLLineHeight;

            topHeight = topHeight > 0 ? 0 : topHeight;

            double totalHeight = (lineHeight + (topHeight * scalingFactor));
            //  double scalingFactorForOGL = (-1 * topHeight * scalingFactor);


            //DEFINE BASELINE START
            for (int i = 0; i < dtSection.Rows.Count; i++)
            {
                string startChainage = dtSection.Rows[i]["StartChainage"].ToString().Trim();
                string endChainage = dtSection.Rows[i]["EndChainage"].ToString().Trim();

                double startX = Convert.ToDouble(startChainage.Replace("+", "").ToString());
                double endX = Convert.ToDouble(endChainage.Replace("+", "").ToString());

                //MainLine
                CreateLineWithLayer(dxfDocument, layer, startX, baselineHeight, endX, baselineHeight);

                //Main Line Two
                CreateLineWithLayer(dxfDocument, layer, startX, totalHeight, endX, totalHeight);

                //MainLine - End Chainage Vertical Marks
                CreateLineWithLayer(dxfDocument, layer2, endX, baselineHeight - SectionMinus_Y1, endX, baselineHeight + SectionPlus_Y2);
                CreateText(dxfDocument, endChainage, endX, baselineHeight + SectionTextPlusY, SectionTextWeight, layer2, SectionTextRotationDegree);

                if (i == 0)
                {
                    CreateLineWithLayer(dxfDocument, layer2, startX, baselineHeight - SectionMinus_Y1, startX, baselineHeight + SectionPlus_Y2);
                    CreateText(dxfDocument, startChainage, startX, baselineHeight + SectionTextPlusY, SectionTextWeight, layer2, SectionTextRotationDegree);
                }
                //Print Section Name
                string strSectionName = dtSection.Rows[i]["SectionName"].ToString().Trim();
                CreateText(dxfDocument, strSectionName, startX + SectionNamePlus_X, baselineHeight + SectionNamePlus_Y, SectionNameWeight, layer2, SecrionNameRotation);
            }

            //Get Entities
            DataTable dtEntities = new DataTable();
            dtEntities = GetEntities(_packageId);
            for (int i = 0; i < dtEntities.Rows.Count; i++)
            {
                string startChainage = dtEntities.Rows[i]["StartChainage"].ToString().Trim();
                string endChainage = dtEntities.Rows[i]["EndChainage"].ToString().Trim();

                double startX = Convert.ToDouble(startChainage.Replace("+", "").ToString());
                double endX = Convert.ToDouble(endChainage.Replace("+", "").ToString());


                //MainLine - End Chainage Vertical Marks
                CreateLineWithLayer(dxfDocument, layer3, startX, baselineHeight - EntityMinus_Y1, startX, baselineHeight + EntityPlus_Y2);

                if ((endX - startX) > EntityChainageCondition)
                    CreateLineWithLayer(dxfDocument, layer3, endX, baselineHeight - EntityInConditionMinus_Y1, endX, baselineHeight + EntityInConditionPlus_Y2);

                CreateText(dxfDocument, startChainage, startX, baselineHeight + EntityTextPlusY, EntityTextWeight, layer3, EntityTextRotationDegree);


                //Print Entity Name
                string strEntityName = dtEntities.Rows[i]["EntityName"].ToString().Trim();
                CreateText(dxfDocument, strEntityName, startX + EntityNamePlus_X, baselineHeight + EntityNamePlus_Y, EntityNameWeight, layer3, EntityNameRotation);
            }

            //PRINT VERTICAL LINE AT NEAREST HUNDRED CHAINAGE
            List<double> listHundred = new List<double>();


            double ans = sectionStart % 100;
            sectionStart = sectionStart + (100 - ans);
            listHundred.Add(sectionStart);
            // this section have static numbers not from settiings
            do
            {
                sectionStart += 100;
                listHundred.Add(sectionStart);
                CreateLineWithLayer(dxfDocument, layer3, sectionStart, baselineHeight - 250, sectionStart, baselineHeight);
                CreateText(dxfDocument, sectionStart.ToString(), sectionStart, baselineHeight - 250, 15, layer3, 90);
            } while (sectionStart < sectionEnd);

            //PLOT OGL
            //for (int i = 0; i < dtOGL.Rows.Count - 1; i++)
            //{
            //    double xOne = Convert.ToDouble(dtOGL.Rows[i]["Chainage"].ToString().Trim().Replace("+", "").ToString());
            //    double heightOne = Convert.ToDouble(dtOGL.Rows[i]["Height"].ToString().Trim().Replace("+", "").ToString());

            //    double xTwo = Convert.ToDouble(dtOGL.Rows[i + 1]["Chainage"].ToString().Trim().Replace("+", "").ToString());
            //    double heightTwo = Convert.ToDouble(dtOGL.Rows[i + 1]["Height"].ToString().Trim().Replace("+", "").ToString());

            //    CreateLineWithLayer(dxfDocument, layer3, xOne, totalHeight - (heightOne * scalingFactor), xTwo, totalHeight - (heightTwo * scalingFactor));
            //}


            //------------------------PRINT ACTIVITY LAYERS WITH OGL ------------------------------------
            //DataTable dtPlot = new DataTable();
            //dtPlot = CrossSectionCalculations(_packageId);
            //PlotActivityLayers(dtPlot, dxfDocument, scalingFactor);

            //------------------------PRINT ACTIVITY LAYERS WITH OGL CONSIDERING FRL AS STRAIGHT LINE------------------------------------


            DataTable dtStraightPlot = new DataTable();
            dtStraightPlot = StraightLineCalculations(lineHeight, scalingFactor, _packageId);
            PlotActivityLayersWithStraightLine(dtStraightPlot, dxfDocument);

            //------------------------------------------------------------

            DataTable dtPackage = new DataTable();
            dtPackage = GetPackageDetails(_packageId);
            int _BaseYPosition = PackageBaseYposition;
            int _ScalingFactor = PackageScalingFactor;
            //Get Start chinage and End Chainage of Package
            string strStartChainage = dtPackage.Rows[0]["StartChainage"].ToString();
            string strEndChainage = dtPackage.Rows[0]["EndChainage"].ToString();
            int intStartChainge = Functions.ParseInteger(strStartChainage.Replace("+", ""));
            int intEndChainage = Functions.ParseInteger(strEndChainage.Replace("+", ""));
            DataTable dtPlotPoints = new DataTable();
            List<double> listMinMax = GetActualPlotPointsOGL(dxfDocument, _packageId, _BaseYPosition, _ScalingFactor, strStartChainage, strEndChainage);

            #region -------------------- Plot Section and Yard Line ---------------------
            //Print Second Line 
            CreateLineWithColorThickness(dxfDocument, SecondLineColor, SecondLineThickness, intStartChainge, listMinMax[1] - SecondLineThicknessMinus_Y1, intEndChainage, listMinMax[1] - SecondLineThicknessMinus_Y2, "Chainage Line Two");
            //Plot Vertical lines with chainage text Start and End only - False in this case
            //PlotChinageWithMarkers(dxfDocument, _startChainage, _endChainage, listMinMax[1] - 350, false);
            CreateTextWithColor(dxfDocument, "Sections", intStartChainge - SecondLIneTextMinus_X, listMinMax[1] - SecondLIneTextMinus_Y, SecondLIneTextWeight, SecondLIneTextColor, SecondLIneTextRotation, "Line-Name");
            #endregion

            #region ------------------------ Plot Sections on Second Line --------------------

            PlotSectionsOnSecondLine(_packageId, intStartChainge, intEndChainage, dxfDocument, listMinMax[1] - SecondLineSectionYposition);
            #endregion

            #region ----------------------- Plot Entities on Third Line

            PlotEntitiesOnThirdLine(_packageId, intStartChainge, intEndChainage, dxfDocument, listMinMax[1] - ThirdLineEntityYposition);

            #endregion


            #region --------------------- SAVE FILE and REDIRECT ---------------------
            //CREATE FOLDER WITH PACKAGE NAME
            Functions.CreateIfMissing(Server.MapPath(@"~/Uploads/StripChart/" + _packageId));
            //CREATE DOCUMENT NAME HERE 
            string obj = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            // Guid obj = Guid.NewGuid();
            string newDoc = "PKG-" + _packageId + "_" + obj + ".dxf";
            string newDxfFile = Server.MapPath(@"~/Uploads/StripChart/" + _packageId + "/" + newDoc);
            //SAVE GENERATED  AUTOCAD FDRAWING            

            dxfDocument.Save(newDxfFile);

            return newDoc;
            #endregion
        }



        private void PlotEntitiesOnThirdLine(int packageId, int startChainage, int endChainage, DxfDocument dxfDocument, double YPosition)
        {
            DataTable dtEntities = new DataTable();
            dtEntities = GetEntitiesBetweenChainage(packageId, startChainage, endChainage);
            int rowCount = dtEntities.Rows.Count;
            for (int i = 0; i < rowCount; i++)
            {
                int Center = (Functions.ParseInteger(dtEntities.Rows[i]["StartChainage"].ToString()) + Functions.ParseInteger(dtEntities.Rows[i]["EndChainage"].ToString())) / 2;

                CreateLineWithColorThickness(dxfDocument, ThirdLineSSColor, ThirdLineSSThickness, Functions.ParseInteger(dtEntities.Rows[i]["StartChainage"].ToString()),
                   YPosition - ThirdLineSSMinus_Y1, Functions.ParseInteger(dtEntities.Rows[i]["StartChainage"].ToString()), YPosition + ThirdLineSSPlus_Y2, "Markers-Three");

                CreateTextWithColor(dxfDocument, IntToStringChainage(Functions.ParseInteger(dtEntities.Rows[i]["StartChainage"].ToString())),
                    Functions.ParseInteger(dtEntities.Rows[i]["StartChainage"].ToString()), YPosition - ThirdLineSSText_MinusY, ThirdLineSSText_Weight, ThirdLineSSText_Color, ThirdLineSSText_Rotation, "Entity Text");

                CreateLineWithColorThickness(dxfDocument, ThirdLineEEColor, ThirdLIneEEThickness, Functions.ParseInteger(dtEntities.Rows[i]["EndChainage"].ToString()),
                    YPosition - ThirdLIneEEMinus_Y1, Functions.ParseInteger(dtEntities.Rows[i]["EndChainage"].ToString()), YPosition + ThirdLIneEEPlus_Y2, "Markers-Three");

                CreateTextWithColor(dxfDocument, IntToStringChainage(Functions.ParseInteger(dtEntities.Rows[i]["EndChainage"].ToString())),
                   Functions.ParseInteger(dtEntities.Rows[i]["EndChainage"].ToString()), YPosition - ThirdLineEEText_MinusY, ThirdLineEEText_Weight, ThirdLineEEText_Color, ThirdLineEEText_Rotation, "Entity Text");

                //Entity Horizontal Line
                CreateLineWithColorThickness(dxfDocument, ThirdLineSEColor, ThirdLineSEThickness, Functions.ParseInteger(dtEntities.Rows[i]["StartChainage"].ToString()),
                  YPosition, Functions.ParseInteger(dtEntities.Rows[i]["EndChainage"].ToString()), YPosition, "Entity Line");


                //Entity Name
                CreateTextWithColor(dxfDocument, dtEntities.Rows[i]["EntityName"].ToString(), Center, YPosition + ThirdLineTextYposition, ThirdLineTextWeight, ThirdLineTextColor, ThirdLineTextRotation, "Entity Text");
            }
        }

        private void PlotSectionsOnSecondLine(int packageId, int startChainage, int endChainage, DxfDocument dxfDocument, double YPosition)
        {
            DataTable dtSection = new DataTable();
            dtSection = GetSectionsBetweenChainage(packageId, startChainage, endChainage);
            int rowCount = dtSection.Rows.Count;

            if (rowCount > 1)
            {
                for (int i = 0; i < rowCount; i++)
                {

                    if (i == 0)
                    {
                        int Center = (startChainage + Functions.ParseInteger(dtSection.Rows[i]["EndChainage"].ToString())) / 2;
                        CreateLineWithColorThickness(dxfDocument, SecondLineMarkersTwoColor, SecondLineMarkersTwoThickness, startChainage, YPosition - SecondLineMarkersTwoMinus_Y1, startChainage, YPosition + SecondLineMarkersTwoPlus_Y2, "Markers-Two");

                        CreateTextWithColor(dxfDocument, IntToStringChainage(startChainage), startChainage, YPosition + SecondLineSectionTextYposition, SecondLineSectionTextWeight,
                            SecondLineSectionTextColor, SecondLineSectionTextRotation, "Section Text");

                        //Section Name
                        CreateTextWithColor(dxfDocument, dtSection.Rows[i]["SectionName"].ToString(), Center, YPosition + SecondLineSectionNameYposition, SecondLineSectionNameWeight, SecondLineSectionNameColor, SecondLineSectionNameRotation, "Section Text");
                    }
                    else if (i == rowCount - 1)
                    {
                        int Center = (endChainage + Functions.ParseInteger(dtSection.Rows[i]["StartChainage"].ToString())) / 2;

                        CreateLineWithColorThickness(dxfDocument, SecondLineMarkersTwoColor, SecondLineMarkersTwoThickness, endChainage, YPosition - SecondLineMarkersTwoMinus_Y1, endChainage, YPosition + SecondLineMarkersTwoPlus_Y2, "Markers-Two");

                        CreateTextWithColor(dxfDocument, IntToStringChainage(endChainage), endChainage, YPosition + SecondLineSectionTextYposition, SecondLineSectionTextWeight,
                            SecondLineSectionTextColor, SecondLineSectionTextRotation, "Section Text");

                        //Section Name
                        CreateTextWithColor(dxfDocument, dtSection.Rows[i]["SectionName"].ToString(), Center, YPosition + SecondLineSectionNameYposition, SecondLineSectionNameWeight, SecondLineSectionNameColor, SecondLineSectionNameRotation, "Section Text");
                    }
                    else
                    {
                        int Center = (Functions.ParseInteger(dtSection.Rows[i]["StartChainage"].ToString()) + Functions.ParseInteger(dtSection.Rows[i]["EndChainage"].ToString())) / 2;

                        CreateLineWithColorThickness(dxfDocument, SecondLineSSColor, SecondLineSSThickness, Functions.ParseInteger(dtSection.Rows[i]["StartChainage"].ToString()),
                           YPosition - SecondLineSSMinus_Y1, Functions.ParseInteger(dtSection.Rows[i]["StartChainage"].ToString()), YPosition + SecondLineSSPlus_Y2, "Markers-Two");

                        CreateTextWithColor(dxfDocument, IntToStringChainage(Functions.ParseInteger(dtSection.Rows[i]["StartChainage"].ToString())),
                            Functions.ParseInteger(dtSection.Rows[i]["StartChainage"].ToString()), YPosition + SecondLineSectionTextYposition, SecondLineSectionTextWeight,
                            SecondLineSectionTextColor, SecondLineSectionTextRotation, "Section Text");

                        CreateLineWithColorThickness(dxfDocument, SecondLineEEColor, SecondLineEEThickness, Functions.ParseInteger(dtSection.Rows[i]["EndChainage"].ToString()),
                            YPosition - SecondLineEEMinus_Y1, Functions.ParseInteger(dtSection.Rows[i]["EndChainage"].ToString()), YPosition + SecondLineEEPlus_Y2, "Markers-Two");

                        CreateTextWithColor(dxfDocument, IntToStringChainage(Functions.ParseInteger(dtSection.Rows[i]["EndChainage"].ToString())),
                           Functions.ParseInteger(dtSection.Rows[i]["EndChainage"].ToString()), YPosition + SecondLineSectionTextYposition, SecondLineSectionTextWeight,
                           SecondLineSectionTextColor, SecondLineSectionTextRotation, "Section Text");


                        //Section Name
                        CreateTextWithColor(dxfDocument, dtSection.Rows[i]["SectionName"].ToString(), Center, YPosition + SecondLineMidSectionNameYposition, SecondLineMidSectionNameWeight, SecondLineMidSectionNameColor, SecondLineMidSectionNameRotation, "Section Text");
                    }
                }
            }

        }

        private DataTable GetSectionsBetweenChainage(int packageId, int startChainage, int endChainage)
        {
            string ConnectionString = GlobalVariables.ConnectionString;
            List<SqlParameter> paramlist = new List<SqlParameter>();

            paramlist.Add(new SqlParameter("@PackageId", packageId));
            paramlist.Add(new SqlParameter("@StartChainage", (startChainage)));
            paramlist.Add(new SqlParameter("@EndChainage", (endChainage)));

            DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, CommandType.StoredProcedure, "GetSectionsBetweenChainage", paramlist.ToArray());
            return dsDataset.Tables[0];
        }

        private DataTable GetEntitiesBetweenChainage(int packageId, int startChainage, int endChainage)
        {
            string ConnectionString = GlobalVariables.ConnectionString;
            List<SqlParameter> paramlist = new List<SqlParameter>();

            paramlist.Add(new SqlParameter("@PackageId", packageId));
            paramlist.Add(new SqlParameter("@StartChainage", (startChainage)));
            paramlist.Add(new SqlParameter("@EndChainage", (endChainage)));

            DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, CommandType.StoredProcedure, "GetEntitiesBetweenChainage", paramlist.ToArray());
            return dsDataset.Tables[0];
        }


        private List<double> GetActualPlotPointsOGL(DxfDocument dxfDocument, int _packageId, int _baseYPosition, int _scalingFactor, string _startChainage, string _endChainage)
        {
            //Step 1 Get OGL, FRL and Cross Section Id, Name for chainage.
            //Get OGL (Original Ground Level), FRL(Finish Rail Level), CROSS Section Name, Height = FRL - OGL
            //Height will be -ve if OGL is above Line  cutting is needed in that case
            DataTable dtOGLFRL = new DataTable();
            dtOGLFRL = GetOGLFRLforPackage(_packageId, _startChainage, _endChainage);

            for (int i = 0; i < dtOGLFRL.Rows.Count; i++)
            {
                int SequenceNo = Functions.ParseInteger(dtOGLFRL.Rows[i]["SeqNo"].ToString());
                double TotalThickness = Functions.ParseDouble(dtOGLFRL.Rows[i]["TotalThk"].ToString());
                double FRL = Functions.ParseDouble(dtOGLFRL.Rows[i]["FRL"].ToString());
                bool isLayer = Convert.ToBoolean(dtOGLFRL.Rows[i]["Layer"].ToString());
                double OGL = Functions.ParseDouble(dtOGLFRL.Rows[i]["OGL"].ToString());
                double ActivityTopLevel = 0;
                double ActivityBottomLevel = 0;


                if (SequenceNo == 1)
                {
                    dtOGLFRL.Rows[i]["ActTopLevel"] = FRL;
                    dtOGLFRL.Rows[i]["ActBottomLevel"] = FRL - TotalThickness;
                }
                else if (i != 0)
                {
                    dtOGLFRL.Rows[i]["ActTopLevel"] = dtOGLFRL.Rows[i - 1]["ActBottomLevel"].ToString();
                    ActivityTopLevel = Functions.ParseDouble(dtOGLFRL.Rows[i]["ActTopLevel"].ToString());
                    dtOGLFRL.Rows[i]["ActBottomLevel"] = TotalThickness != 0 ? (ActivityTopLevel - TotalThickness) : OGL;
                    ActivityBottomLevel = Functions.ParseDouble(dtOGLFRL.Rows[i]["ActBottomLevel"].ToString());
                }

                if (isLayer)
                {
                    dtOGLFRL.Rows[i]["LayerCount"] = Math.Ceiling((ActivityTopLevel - ActivityBottomLevel) / Functions.ParseDouble(dtOGLFRL.Rows[i]["MaxLayerThk"].ToString()));
                }
            }

            dtOGLFRL = dtOGLFRL.AsEnumerable().OrderBy(c => c["SeqNo"]).ThenBy(c => c["Chainage"]).CopyToDataTable();

            double minX = PlotPointsOGLminX;
            double minY = PlotPointsOGLminY;
            double maxX = PlotPointsOGLmaxX;
            double maxY = PlotPointsOGLmaxY;

            for (int i = 0; i < dtOGLFRL.Rows.Count; i++)
            {
                if (i >= 1)
                {
                    int prevSequence = Functions.ParseInteger(dtOGLFRL.Rows[i - 1]["SeqNo"].ToString());
                    int currentSequence = Functions.ParseInteger(dtOGLFRL.Rows[i]["SeqNo"].ToString());

                    if (prevSequence == currentSequence)
                    {
                        double StartX = ChainageStringToInteger(dtOGLFRL.Rows[i - 1]["Chainage"].ToString());
                        double StartY = ChainageStringToInteger(dtOGLFRL.Rows[i - 1]["ActBottomLevel"].ToString());
                        double EndX = ChainageStringToInteger(dtOGLFRL.Rows[i]["Chainage"].ToString());
                        double EndY = ChainageStringToInteger(dtOGLFRL.Rows[i]["ActBottomLevel"].ToString());

                        dtOGLFRL.Rows[i]["StartX"] = Functions.ParseDouble(StartX.ToString());
                        dtOGLFRL.Rows[i]["StartY"] = Functions.ParseDouble(StartY.ToString());
                        dtOGLFRL.Rows[i]["EndX"] = Functions.ParseDouble(EndX.ToString());
                        dtOGLFRL.Rows[i]["EndY"] = Functions.ParseDouble(EndY.ToString());

                        minX = (StartX < minX && StartX != 0) ? StartX : minX;
                        minY = (StartY < minY && StartY != 0) ? StartY : minY;

                        maxX = (EndX > maxX) ? EndX : maxX;
                        maxY = (EndY > maxY) ? EndY : maxY;
                    }
                }
            }


            List<double> listMinMax = new List<double>();
            listMinMax.Add(RoundValueToLower100(minX));
            listMinMax.Add(RoundValueToLower100(minY));
            listMinMax.Add(RoundValueToNext100(maxX));
            listMinMax.Add(RoundValueToNext100(maxY));
            // PlotActivityLayers(dtOGLFRL, dxfDocument, _scalingFactor);
            return listMinMax;
        }
        private static double RoundValueToNext100(double value)
        {
            return (Math.Ceiling(value / 100) * 100);
        }


        private static double RoundValueToLower100(double value)
        {
            return (value - (value % 100));

        }


        private DataTable GetEntities(int packageId)
        {
            string ConnectionString = GlobalVariables.ConnectionString;
            string _queryString = "SELECT [EntityName],[EntityType],[StartChainage],[EndChainage] FROM tblMasterEntity WHERE PackageId = " + packageId;
            DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, _queryString);
            return dsDataset.Tables[0];
        }

        private DataTable GetSections(int packageId)
        {
            string ConnectionString = GlobalVariables.ConnectionString;
            string _queryString = "SELECT [SectionID],[ProjectId] ,[PackageId],[SectionName],[SectionCode],[StartChainage],[EndChainage],[IsDeleted],[Length] FROM tblSection WHERE PackageId = " + packageId;
            DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, _queryString);
            return dsDataset.Tables[0];
        }

        private static void ApplyColorToLayer(Layer layer, string color)
        {
            switch (color)
            {
                case "Red":
                    layer.Color = AciColor.Red;
                    break;
                case "Yellow":
                    layer.Color = AciColor.Yellow;
                    break;
                case "Green":
                    layer.Color = AciColor.Green;
                    break;

                case "Blue":
                    layer.Color = AciColor.Blue;
                    break;

                case "Cyan":
                    layer.Color = AciColor.Cyan;
                    break;

                case "DarkGray":
                    layer.Color = AciColor.DarkGray;
                    break;

                case "LightGray":
                    layer.Color = AciColor.LightGray;
                    break;

                case "Magenta":
                    layer.Color = AciColor.Magenta;
                    break;
            }
        }

        private void GridPlotting(DxfDocument dxfDocument)
        {

            int minX = GridPlottingminX;
            int minY = GridPlottingminY;
            int maxX = GridPlottingmaxX;
            int maxY = GridPlottingmaxY;

            for (int i = 1; i < ((maxY - minY) / 100); i++)
            {
                CreateLineWithColorThickness(dxfDocument, "LightGray", 1, minX, minY, maxX, minY);
                minY = minY + 100;
                maxY = maxY + 100;
            }

            minX = GridPlottingminX;
            minY = GridPlottingminY;
            maxX = GridPlottingmaxX;
            maxY = GridPlottingmaxY;

            for (int i = 1; i < ((maxX - minX) / 100); i++)
            {
                CreateLineWithColorThickness(dxfDocument, "LightGray", 1, minX, minY, minX, maxY);
                minX = minX + 100;
                maxX = maxX + 100;
            }
        }

        private void CreateLineWithColorThickness(DxfDocument document, string color, double thickness, double x1, double y1, double x2, double y2)
        {
            //List<LwPolylineVertex> lwPolylineVertices = new List<LwPolylineVertex>();
            //LwPolylineVertex lwVertex1 = new LwPolylineVertex(x1, y1);
            //LwPolylineVertex lwVertex2 = new LwPolylineVertex(x2, y2);
            //lwPolylineVertices.Add(lwVertex1);
            //lwPolylineVertices.Add(lwVertex2);
            //LwPolyline lwPolyline = new LwPolyline(lwPolylineVertices);
            //lwPolyline.Color = GetAciColor(color);
            //lwPolyline.Thickness = thickness;
            //document.AddEntity(lwPolyline);

            List<PolylineVertex> lwPolylineVertices = new List<PolylineVertex>();
            PolylineVertex lwVertex1 = new PolylineVertex();
            PolylineVertex lwVertex2 = new PolylineVertex();
            lwPolylineVertices.Add(new PolylineVertex(x1, y1, 0));
            lwPolylineVertices.Add(new PolylineVertex(x2, y2, 0));
            Polyline lwPolyline = new Polyline(lwPolylineVertices);
            lwPolyline.Color = GetAciColor(color);
            lwPolyline.Lineweight = Lineweight.W25;
            document.AddEntity(lwPolyline);
        }

        private AciColor GetAciColor(string color)
        {
            switch (color)
            {
                case "Red":
                    return AciColor.Red;

                case "Yellow":
                    return AciColor.Yellow;

                case "Green":
                    return AciColor.Green;


                case "Blue":
                    return AciColor.Blue;


                case "Cyan":
                    return AciColor.Cyan;


                case "DarkGray":
                    return AciColor.DarkGray;


                case "LightGray":
                    return AciColor.LightGray;


                case "Magenta":
                    return AciColor.Magenta;
                default: return AciColor.Cyan;


            }
        }

        private DataTable GetOGL(int packageId)
        {
            string ConnectionString = GlobalVariables.ConnectionString;
            string _queryString = "SELECT [PackageId],[Chainage],[OGL],[FRL] ,[FormLvl] ,[FRL]-[OGL] AS Height FROM tblPkgChngLvl WHERE PackageId = " + packageId + " order by chainage ";
            DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, _queryString);
            return dsDataset.Tables[0];
        }

        private void CreateLineWithLayer(DxfDocument document, Layer layer, double x1, double y1, double x2, double y2)
        {
            List<LwPolylineVertex> lwPolylineVertices = new List<LwPolylineVertex>();
            LwPolylineVertex lwVertex1 = new LwPolylineVertex(x1, y1);
            LwPolylineVertex lwVertex2 = new LwPolylineVertex(x2, y2);
            lwPolylineVertices.Add(lwVertex1);
            lwPolylineVertices.Add(lwVertex2);
            LwPolyline lwPolyline = new LwPolyline(lwPolylineVertices);
            lwPolyline.Layer = layer;

            document.AddEntity(lwPolyline);
        }

        private void CreateText(DxfDocument document, string text, double x, double y, int weight, Layer layer, double RotationDegree)
        {
            //create text-element for the start point and add to the dxf
            netDxf.Entities.Text strText = new netDxf.Entities.Text(text, new Vector2(x, y), weight);
            strText.Layer = layer;
            strText.Rotation = RotationDegree;
            document.AddEntity(strText);
        }

        private DataTable CrossSectionCalculations(int packageId)
        {
            int PackageId = packageId;
            DataTable dtPackage = new DataTable();
            dtPackage = GetPackageDetails(PackageId);

            //Get Start chinage and End Chainage of Package
            string strStartChainage = dtPackage.Rows[0]["StartChainage"].ToString();
            string strEndChainage = dtPackage.Rows[0]["EndChainage"].ToString();
            int intStartChainge = Functions.ParseInteger(strStartChainage.Replace("+", ""));
            int intEndChainage = Functions.ParseInteger(strEndChainage.Replace("+", ""));

            //Step 1 Get OGL, FRL and Cross Section Id, Name for chainage.
            //Get OGL (Original Ground Level), FRL(Finish Rail Level), CROSS Section Name, Height = FRL - OGL
            //Height will be -ve if OGL is above Line  cutting is needed in that case
            DataTable dtOGLFRL = new DataTable();
            dtOGLFRL = GetOGLFRLforPackage(PackageId, strStartChainage, strEndChainage);

            for (int i = 0; i < dtOGLFRL.Rows.Count; i++)
            {
                int SequenceNo = Functions.ParseInteger(dtOGLFRL.Rows[i]["SeqNo"].ToString());
                double TotalThickness = Functions.ParseDouble(dtOGLFRL.Rows[i]["TotalThk"].ToString());
                double FRL = Functions.ParseDouble(dtOGLFRL.Rows[i]["FRL"].ToString());
                bool isLayer = Convert.ToBoolean(dtOGLFRL.Rows[i]["Layer"].ToString());
                double OGL = Functions.ParseDouble(dtOGLFRL.Rows[i]["OGL"].ToString());
                double ActivityTopLevel = 0;
                double ActivityBottomLevel = 0;


                if (SequenceNo == 1)
                {
                    dtOGLFRL.Rows[i]["ActTopLevel"] = FRL;
                    dtOGLFRL.Rows[i]["ActBottomLevel"] = FRL - TotalThickness;
                }
                else if (i != 0)
                {
                    dtOGLFRL.Rows[i]["ActTopLevel"] = dtOGLFRL.Rows[i - 1]["ActBottomLevel"].ToString();
                    ActivityTopLevel = Functions.ParseDouble(dtOGLFRL.Rows[i]["ActTopLevel"].ToString());
                    dtOGLFRL.Rows[i]["ActBottomLevel"] = TotalThickness != 0 ? (ActivityTopLevel - TotalThickness) : OGL;
                    ActivityBottomLevel = Functions.ParseDouble(dtOGLFRL.Rows[i]["ActBottomLevel"].ToString());
                }

                if (isLayer)
                {
                    dtOGLFRL.Rows[i]["LayerCount"] = Math.Ceiling((ActivityTopLevel - ActivityBottomLevel) / Functions.ParseDouble(dtOGLFRL.Rows[i]["MaxLayerThk"].ToString()));
                }
            }

            dtOGLFRL = dtOGLFRL.AsEnumerable().OrderBy(c => c["SeqNo"]).ThenBy(c => c["Chainage"]).CopyToDataTable();

            for (int i = 0; i < dtOGLFRL.Rows.Count; i++)
            {
                if (i >= 1)
                {
                    int prevSequence = Functions.ParseInteger(dtOGLFRL.Rows[i - 1]["SeqNo"].ToString());
                    int currentSequence = Functions.ParseInteger(dtOGLFRL.Rows[i]["SeqNo"].ToString());

                    if (prevSequence == currentSequence)
                    {
                        dtOGLFRL.Rows[i]["StartX"] = Functions.ParseDouble(dtOGLFRL.Rows[i - 1]["Chainage"].ToString().Replace("+", ""));
                        dtOGLFRL.Rows[i]["StartY"] = Functions.ParseDouble(dtOGLFRL.Rows[i - 1]["ActBottomLevel"].ToString());
                        dtOGLFRL.Rows[i]["EndX"] = Functions.ParseDouble(dtOGLFRL.Rows[i]["Chainage"].ToString().Replace("+", ""));
                        dtOGLFRL.Rows[i]["EndY"] = Functions.ParseDouble(dtOGLFRL.Rows[i]["ActBottomLevel"].ToString());
                    }
                }
            }

            // PlotActivityLayers(dtOGLFRL);

            return dtOGLFRL;
        }

        private DataTable GetPackageDetails(int packageId)
        {
            string ConnectionString = GlobalVariables.ConnectionString;
            DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, "SELECT * FROM tblPackages WHERE PackageId = " + packageId + " ");
            return dsDataset.Tables[0];
        }

        private DataTable GetOGLFRLforPackage(int packageId, string intStartChainge, string intEndChainage)
        {
            string ConnectionString = GlobalVariables.ConnectionString;
            List<SqlParameter> paramlist = new List<SqlParameter>();

            paramlist.Add(new SqlParameter("@PackageId", packageId));
            paramlist.Add(new SqlParameter("@StartChainage", intStartChainge));
            paramlist.Add(new SqlParameter("@EndChainage", intEndChainage));

            DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, CommandType.StoredProcedure, "GetOGLFRLforPackage", paramlist.ToArray());
            return dsDataset.Tables[0];
        }

        private void PlotActivityLayersWithStraightLine(DataTable dtPlotPoints, DxfDocument dxfDocument)
        {
            //CREATE FILES
            //DxfDocument dxfDocument = new DxfDocument();

            //CREATE LAYER
            Layer layer = new Layer("Layer-01");
            dxfDocument.Layers.Add(layer);
            layer.Lineweight = Lineweight.W5;
            string color = Convert.ToString("Green");
            ApplyColorToLayer(layer, color);

            for (int i = 1; i < dtPlotPoints.Rows.Count; i++)
            {

                double startX = Functions.ParseDouble(dtPlotPoints.Rows[i]["StartX"].ToString());
                double startY = Functions.ParseDouble(dtPlotPoints.Rows[i]["StartY"].ToString());
                double endX = Functions.ParseDouble(dtPlotPoints.Rows[i]["EndX"].ToString());
                double endY = Functions.ParseDouble(dtPlotPoints.Rows[i]["EndY"].ToString());

                if (startX == 0 && startY == 0 && endX == 0 && endY == 0)
                {
                    CreateText(dxfDocument, dtPlotPoints.Rows[i]["ScActName"].ToString(),
                        Functions.ParseDouble(dtPlotPoints.Rows[i + 1]["StartX"].ToString()) - 80,
                       Functions.ParseDouble(dtPlotPoints.Rows[i + 1]["StartY"].ToString()),
                        12,
                        layer,
                        0);
                }
                else
                {
                    CreateLineWithColorThickness(dxfDocument,
                        dtPlotPoints.Rows[i]["PlotColour"].ToString(),
                        Functions.ParseDouble(dtPlotPoints.Rows[i]["PlotThk"].ToString()),
                         startX,
                         startY,
                          endX,
                         endY);
                }

            }

        }

        public void PlotActivityLayers(DataTable dtPlotPoints, DxfDocument dxfDocument, double scalingFactor)
        {
            //CREATE FILES
            //DxfDocument dxfDocument = new DxfDocument();

            //CREATE LAYER
            Layer layer = new Layer("Layer-01");
            dxfDocument.Layers.Add(layer);
            layer.Lineweight = Lineweight.W5;
            string color = Convert.ToString("Green");
            ApplyColorToLayer(layer, color);

            for (int i = 1; i < dtPlotPoints.Rows.Count; i++)
            {

                double startX = Functions.ParseDouble(dtPlotPoints.Rows[i]["StartX"].ToString());
                double startY = Functions.ParseDouble(dtPlotPoints.Rows[i]["StartY"].ToString());
                double endX = Functions.ParseDouble(dtPlotPoints.Rows[i]["EndX"].ToString());
                double endY = Functions.ParseDouble(dtPlotPoints.Rows[i]["EndY"].ToString());

                if (startX == 0 && startY == 0 && endX == 0 && endY == 0)
                {
                    CreateText(dxfDocument, dtPlotPoints.Rows[i]["ScActName"].ToString(),
                        Functions.ParseDouble(dtPlotPoints.Rows[i + 1]["StartX"].ToString()) - 80,
                       Functions.ParseDouble(dtPlotPoints.Rows[i + 1]["StartY"].ToString()),
                        12,
                        layer,
                        0);
                }
                else
                {
                    CreateLineWithColorThickness(dxfDocument,
                        dtPlotPoints.Rows[i]["PlotColour"].ToString(),
                        Functions.ParseDouble(dtPlotPoints.Rows[i]["PlotThk"].ToString()),
                         startX,
                        scalingFactor * startY,
                          endX,
                        scalingFactor * endY);
                }

            }

            //CREATE DOCUMENT NAME HERE 
            //string newDxfFile = Server.MapPath(@"../NewDXF/" + "test123.dxf");
            //SAVE GENERATED  AUTOCAD FDRAWING
            //dxfDocument.Save(newDxfFile);

            //lblMessage.Text = "File Generation Completed";
            //SAVE IT IN SESSION SO THAT THE LATEST DRAWING IS OPENED
            //Session["NEW-DRAWING"] = newDxfFile;

            //TRANSFER IT TO CAD VIEWER
            // Server.Transfer("cadviewer.aspx");
        }

        private DataTable StraightLineCalculations(double lineHeight, double scalingFactor, int packageId)
        {
            int PackageId = packageId;
            DataTable dtPackage = new DataTable();
            dtPackage = GetPackageDetails(PackageId);

            //Get Start chinage and End Chainage of Package
            string strStartChainage = dtPackage.Rows[0]["StartChainage"].ToString();
            string strEndChainage = dtPackage.Rows[0]["EndChainage"].ToString();
            int intStartChainge = Functions.ParseInteger(strStartChainage.Replace("+", ""));
            int intEndChainage = Functions.ParseInteger(strEndChainage.Replace("+", ""));

            //Step 1 Get OGL, FRL and Cross Section Id, Name for chainage.
            //Get OGL (Original Ground Level), FRL(Finish Rail Level), CROSS Section Name, Height = FRL - OGL
            //Height will be -ve if OGL is above Line  cutting is needed in that case
            DataTable dtOGLFRL = new DataTable();
            dtOGLFRL = GetOGLFRLforPackage(PackageId, strStartChainage, strEndChainage);

            for (int i = 0; i < dtOGLFRL.Rows.Count; i++)
            {
                int SequenceNo = Functions.ParseInteger(dtOGLFRL.Rows[i]["SeqNo"].ToString());
                double TotalThickness = Functions.ParseDouble(dtOGLFRL.Rows[i]["TotalThk"].ToString());
                double FRL = Functions.ParseDouble(dtOGLFRL.Rows[i]["FRL"].ToString());
                bool isLayer = Convert.ToBoolean(dtOGLFRL.Rows[i]["Layer"].ToString());
                double OGL = Functions.ParseDouble(dtOGLFRL.Rows[i]["OGL"].ToString());
                double Height = Functions.ParseDouble(dtOGLFRL.Rows[i]["Height"].ToString());
                double ActivityTopLevel = 0;
                double ActivityBottomLevel = 0;

                double NewFRL = lineHeight;
                double NewOGL = NewFRL - (Height * scalingFactor);


                if (SequenceNo == 1)
                {
                    dtOGLFRL.Rows[i]["ActTopLevel"] = NewFRL;
                    dtOGLFRL.Rows[i]["ActBottomLevel"] = NewFRL - (TotalThickness * scalingFactor);
                }
                else if (i != 0)
                {
                    dtOGLFRL.Rows[i]["ActTopLevel"] = dtOGLFRL.Rows[i - 1]["ActBottomLevel"].ToString();
                    ActivityTopLevel = Functions.ParseDouble(dtOGLFRL.Rows[i]["ActTopLevel"].ToString());
                    dtOGLFRL.Rows[i]["ActBottomLevel"] = TotalThickness != 0 ? (ActivityTopLevel - (TotalThickness * scalingFactor)) : NewOGL;
                    ActivityBottomLevel = Functions.ParseDouble(dtOGLFRL.Rows[i]["ActBottomLevel"].ToString());
                }

                if (isLayer)
                {
                    dtOGLFRL.Rows[i]["LayerCount"] = Math.Ceiling((ActivityTopLevel - ActivityBottomLevel) / Functions.ParseDouble(dtOGLFRL.Rows[i]["MaxLayerThk"].ToString()));
                }
            }

            dtOGLFRL = dtOGLFRL.AsEnumerable().OrderBy(c => c["SeqNo"]).ThenBy(c => c["Chainage"]).CopyToDataTable();

            for (int i = 0; i < dtOGLFRL.Rows.Count; i++)
            {
                if (i >= 1)
                {
                    int prevSequence = Functions.ParseInteger(dtOGLFRL.Rows[i - 1]["SeqNo"].ToString());
                    int currentSequence = Functions.ParseInteger(dtOGLFRL.Rows[i]["SeqNo"].ToString());

                    if (prevSequence == currentSequence)
                    {
                        dtOGLFRL.Rows[i]["StartX"] = Functions.ParseDouble(dtOGLFRL.Rows[i - 1]["Chainage"].ToString().Replace("+", ""));
                        dtOGLFRL.Rows[i]["StartY"] = Functions.ParseDouble(dtOGLFRL.Rows[i - 1]["ActBottomLevel"].ToString());
                        dtOGLFRL.Rows[i]["EndX"] = Functions.ParseDouble(dtOGLFRL.Rows[i]["Chainage"].ToString().Replace("+", ""));
                        dtOGLFRL.Rows[i]["EndY"] = Functions.ParseDouble(dtOGLFRL.Rows[i]["ActBottomLevel"].ToString());
                    }
                }
            }

            // PlotActivityLayers(dtOGLFRL);

            return dtOGLFRL;
        }

        private void CreateLineWithColorThickness(DxfDocument dxfDocument, string color, double thickness, double x1, double y1, double x2, double y2, string LayerName)
        {
            Layer layer = new Layer(LayerName);
            dxfDocument.Layers.Add(layer);

            Linetype lineType = new Linetype("Dashed");
            dxfDocument.Linetypes.Add(lineType);

            List<PolylineVertex> lwPolylineVertices = new List<PolylineVertex>();
            PolylineVertex lwVertex1 = new PolylineVertex();
            PolylineVertex lwVertex2 = new PolylineVertex();
            lwPolylineVertices.Add(new PolylineVertex(x1, y1, 0));
            lwPolylineVertices.Add(new PolylineVertex(x2, y2, 0));
            Polyline lwPolyline = new Polyline(lwPolylineVertices);
            lwPolyline.Color = GetAciColor(color);
            lwPolyline.Lineweight = Lineweight.W25;
            lwPolyline.Linetype = lineType;
            lwPolyline.Layer = layer;

            dxfDocument.AddEntity(lwPolyline);
        }

        private void CreateTextWithColor(DxfDocument document, string text, double x, double y, int weight, string color, double RotationDegree, string LayerName)
        {
            Layer layer = new Layer(LayerName);
            document.Layers.Add(layer);

            //create text-element for the start point and add to the dxf
            netDxf.Entities.Text strText = new netDxf.Entities.Text(text, new Vector2(x, y), weight);
            strText.Color = GetAciColor(color);
            strText.Rotation = RotationDegree;
            strText.Layer = layer;
            document.AddEntity(strText);
        }


        private double ChainageStringToInteger(string strChainage)
        {
            return Functions.ParseDouble(strChainage.Replace("+", ""));
        }

        #region ---- Not in use ----
        private void CreateBaseline()
        {
            DxfDocument dxfDocs = new DxfDocument();

            //create layer for that activity-code
            Layer layer = new Layer("Layer-01");
            dxfDocs.Layers.Add(layer);

            layer.Lineweight = Lineweight.W5;

            string color = Convert.ToString("Red");

            //pl 64,43574 w   66,43574 

            ApplyColorToLayer(layer, color);

            for (int i = 0; i < 67100; i = i + 100)
            {

                //get start-point for that face-code
                double text_x, text_y;
                string startString = Convert.ToString("pl " + (i).ToString() + ",0 w   0,0000");
                ParsePoints(startString, out text_x, out text_y);

                //create text-element for the start point and add to the dxf
                netDxf.Entities.Text faceCodeText = new netDxf.Entities.Text("146+960", new Vector2(text_x, text_y), 50);
                faceCodeText.Layer = layer;
                faceCodeText.Rotation = 90;
                dxfDocs.AddEntity(faceCodeText);

                //get start-point for that face-code
                double text_x1, text_y1;
                string startString1 = Convert.ToString("pl " + (i).ToString() + ",0 w   0,0000");
                ParsePoints(startString1, out text_x1, out text_y1);

                //create text-element for the start point and add to the dxf
                netDxf.Entities.Text faceCodeText1 = new netDxf.Entities.Text("|", new Vector2(text_x1, text_y1), 1);
                faceCodeText1.Layer = layer;
                dxfDocs.AddEntity(faceCodeText1);
            }

            double R1x, R1y, R2x, R2y;
            string value1 = Convert.ToString("pl 0,1000 w   67100,1000");
            ParsePoints(value1, out R1x, out R1y, out R2x, out R2y);

            List<PolylineVertex> polylineVertices = new List<PolylineVertex>();
            polylineVertices.Add(new PolylineVertex(R1x, R1y, 0));
            polylineVertices.Add(new PolylineVertex(R2x, R2y, 0));
            Polyline plLine = new Polyline(polylineVertices);
            Layer layer1 = new Layer("Layer-02");
            layer1.Lineweight = Lineweight.W60;
            string color1 = Convert.ToString("Cyan");
            ApplyColorToLayer(layer1, color1);
            plLine.Layer = layer1;
            dxfDocs.AddEntity(plLine);
            //----------------------------
            double p1x, p1y, p2x, p2y;
            string value = Convert.ToString("pl 0,0 w   67100,0");
            ParsePoints(value, out p1x, out p1y, out p2x, out p2y);

            LwPolylineVertex lwVertex1 = new LwPolylineVertex(p1x, p1y);
            LwPolylineVertex lwVertex2 = new LwPolylineVertex(p2x, p2y);
            List<LwPolylineVertex> lwPolylineVertices = new List<LwPolylineVertex>();
            lwPolylineVertices.Add(lwVertex1);
            lwPolylineVertices.Add(lwVertex2);
            LwPolyline lwPolyline = new LwPolyline(lwPolylineVertices);
            lwPolyline.SetConstantWidth(0.2);

            lwPolyline.Layer = layer;
            dxfDocs.AddEntity(lwPolyline);
            string newDxfFile = Server.MapPath(@"../NewDXF/" + "test123.dxf");

            dxfDocs.Save(newDxfFile);
            //lblMessage.Text = "File Generation Completed";
            Session["NEW-DRAWING"] = newDxfFile;

            Server.Transfer("cadviewer.aspx");
        }
        private static void ParsePoints(string value, out double p1x, out double p1y, out double p2x, out double p2y)
        {
            value = value.Replace(" ", String.Empty);
            string[] arrValues = value.Split(new string[] { ",", "pl", "w" }, StringSplitOptions.RemoveEmptyEntries);

            p1x = Convert.ToDouble(arrValues[0]);
            p1y = Convert.ToDouble(arrValues[1]);
            //double width = Convert.ToDouble(arrValues[4]);
            p2x = Convert.ToDouble(arrValues[2]);
            p2y = Convert.ToDouble(arrValues[3]);
        }

        private static void ParsePoints(string value, out double p1x, out double p1y)
        {
            value = value.Replace(" ", String.Empty);
            string[] arrValues = value.Split(new string[] { ",", "pl", "w" }, StringSplitOptions.RemoveEmptyEntries);

            p1x = Convert.ToDouble(arrValues[0]);
            p1y = Convert.ToDouble(arrValues[1]);
        }

        private DataTable GetActivityDetailByCrossSection(int packageId, int crossSectionId)
        {
            string ConnectionString = GlobalVariables.ConnectionString;
            List<SqlParameter> paramlist = new List<SqlParameter>();

            paramlist.Add(new SqlParameter("@PackageId", packageId));
            paramlist.Add(new SqlParameter("@CrossSectionId", crossSectionId));

            DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, CommandType.StoredProcedure, "GetActivityDetailsByChainage", paramlist.ToArray());
            return dsDataset.Tables[0];
        }

        private DataTable GetOGLFRLforChainage(int packageId, string chainage)
        {
            string ConnectionString = GlobalVariables.ConnectionString;
            List<SqlParameter> paramlist = new List<SqlParameter>();

            paramlist.Add(new SqlParameter("@PackageId", packageId));
            paramlist.Add(new SqlParameter("@Chinage", chainage));

            DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, CommandType.StoredProcedure, "GetOGLFRLByChainage", paramlist.ToArray());
            return dsDataset.Tables[0];
        }

        public string IntToStringChainage(int chainage)
        {
            return chainage.ToString("D6").Insert(chainage.ToString("D6").Length - 3, "+");
        }

        #endregion


    }
}