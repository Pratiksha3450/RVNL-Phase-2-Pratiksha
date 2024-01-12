using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using static RVNLMIS.Controllers.EnggDrawingController;
using System.Data;
using System.Data.SqlClient;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [SessionAuthorize]
    public class AutocadDocViewerController : Controller
    {
        #region ---- dynamic settings veriables and load methods ----
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

        #region ---- curve line chart ----
        public string CurveChainageLineOneColor = "Cyan";
        public double CurveChainageLineOneThickness = 30;
        public double CurveChainageLineOneMinus_Y1 = 200;
        public double CurveChainageLineOneMinus_Y2 = 200;
        public double CurveChainageLineOneMinus_Yposition = 200;
        public double CurveChainageLineOneText_X = 160;
        public double CurveChainageLineOneText_Y = 200;
        public int CurveChainageLineOneText_Weight = 18;
        public string CurveChainageLineOneText_Color = "Cyan";
        public double CurveChainageLineOneText_Rotation = 0;

        public string CurvePlotChainageMarkerOne_Color = "Red";
        public double CurvePlotChainageMarkerOne_Thikness = 10;
        public double CurvePlotChainageMarkerOne_MinusY1 = 5;
        public double CurvePlotChainageMarkerOne_PlusY2 = 5;
        public double CurvePlotChainageMarkerOneText_Yposition = 10;
        public int CurvePlotChainageMarkerOneText_Weight = 15;
        public string CurvePlotChainageMarkerOneText_Color = "Cyan";
        public double CurvePlotChainageMarkerOneText_Rotation = 90;
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
                                // -- curve chart setting --
                                case "CurveChainageLineOneColor":
                                    CurveChainageLineOneColor = Convert.ToString(item.Value) == string.Empty ? "Cyan" : Convert.ToString(item.Value);
                                    break;
                                case "CurveChainageLineOneThickness":
                                    CurveChainageLineOneThickness = Functions.ParseDouble(item.Value) == 0 ? 30 : Functions.ParseDouble(item.Value);
                                    break;
                                case "CurveChainageLineOneMinus_Y1":
                                    CurveChainageLineOneMinus_Y1 = Functions.ParseDouble(item.Value) == 0 ? 200 : Functions.ParseDouble(item.Value);
                                    break;
                                case "CurveChainageLineOneMinus_Y2":
                                    CurveChainageLineOneMinus_Y2 = Functions.ParseDouble(item.Value) == 0 ? 200 : Functions.ParseDouble(item.Value);
                                    break;
                                case "CurveChainageLineOneMinus_Yposition":
                                    CurveChainageLineOneMinus_Yposition = Functions.ParseDouble(item.Value) == 0 ? 200 : Functions.ParseDouble(item.Value);
                                    break;
                                case "CurveChainageLineOneText_Weight":
                                    CurveChainageLineOneText_Weight = Functions.ParseInteger(item.Value) == 0 ? 18 : Functions.ParseInteger(item.Value);
                                    break;
                                case "CurveChainageLineOneText_Color":
                                    CurveChainageLineOneText_Color = Convert.ToString(item.Value) == string.Empty ? "Cyan" : Convert.ToString(item.Value);
                                    break;
                                case "CurveChainageLineOneText_X":
                                    CurveChainageLineOneText_X = Functions.ParseDouble(item.Value) == 0 ? 160 : Functions.ParseDouble(item.Value);
                                    break;
                                case "CurveChainageLineOneText_Y":
                                    CurveChainageLineOneText_Y = Functions.ParseDouble(item.Value) == 0 ? 200 : Functions.ParseDouble(item.Value);
                                    break;
                                case "CurveChainageLineOneText_Rotation":
                                    CurveChainageLineOneText_Rotation = Functions.ParseDouble(item.Value) == 0 ? 0 : Functions.ParseDouble(item.Value);
                                    break;
                                case "CurvePlotChainageMarkerOne_Color":
                                    CurvePlotChainageMarkerOne_Color = Convert.ToString(item.Value) == string.Empty ? "Red" : Convert.ToString(item.Value);
                                    break;
                                case "CurvePlotChainageMarkerOne_Thikness":
                                    CurvePlotChainageMarkerOne_Thikness = Functions.ParseDouble(item.Value) == 0 ? 10 : Functions.ParseDouble(item.Value);
                                    break;
                                case "CurvePlotChainageMarkerOne_MinusY1":
                                    CurvePlotChainageMarkerOne_MinusY1 = Functions.ParseDouble(item.Value) == 0 ? 5 : Functions.ParseDouble(item.Value);
                                    break;
                                case "CurvePlotChainageMarkerOne_PlusY2":
                                    CurvePlotChainageMarkerOne_PlusY2 = Functions.ParseDouble(item.Value) == 0 ? 5 : Functions.ParseDouble(item.Value);
                                    break;
                                case "CurvePlotChainageMarkerOneText_Yposition":
                                    CurvePlotChainageMarkerOneText_Yposition = Functions.ParseDouble(item.Value) == 0 ? 15 : Functions.ParseDouble(item.Value);
                                    break;
                                case "CurvePlotChainageMarkerOneText_Weight":
                                    CurvePlotChainageMarkerOneText_Weight = Functions.ParseInteger(item.Value) == 0 ? 15 : Functions.ParseInteger(item.Value);
                                    break;
                                case "CurvePlotChainageMarkerOneText_Color":
                                    CurvePlotChainageMarkerOneText_Color = Convert.ToString(item.Value) == string.Empty ? "Cyan" : Convert.ToString(item.Value);
                                    break;
                                case "CurvePlotChainageMarkerOneText_Rotation":
                                    CurvePlotChainageMarkerOneText_Rotation = Functions.ParseDouble(item.Value) == 0 ? 90 : Functions.ParseDouble(item.Value);
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

            #region ---- curve line chart ----
            CurveChainageLineOneColor = "Cyan";
            CurveChainageLineOneThickness = 30;
            CurveChainageLineOneMinus_Y1 = 200;
            CurveChainageLineOneMinus_Y2 = 200;
            CurveChainageLineOneMinus_Yposition = 200;
            CurveChainageLineOneText_X = 160;
            CurveChainageLineOneText_Y = 200;
            CurveChainageLineOneText_Weight = 18;
            CurveChainageLineOneText_Color = "Cyan";
            CurveChainageLineOneText_Rotation = 0;

            CurvePlotChainageMarkerOne_Color = "Red";
            CurvePlotChainageMarkerOne_Thikness = 10;
            CurvePlotChainageMarkerOne_MinusY1 = 5;
            CurvePlotChainageMarkerOne_PlusY2 = 5;
            CurvePlotChainageMarkerOneText_Yposition = 10;
            CurvePlotChainageMarkerOneText_Weight = 15;
            CurvePlotChainageMarkerOneText_Color = "Cyan";
            CurvePlotChainageMarkerOneText_Rotation = 90;
            #endregion

        }
        #endregion

        // GET: AutocadDocViewer
        [PageAccessFilter]
        public ActionResult Index()
        {
            var objUserM = (UserModel)Session["UserData"];

            AutoCadViewerModel obj = new AutoCadViewerModel();
            obj.PackageId = objUserM.RoleTableID;
            return View(obj);
        }

        #region ---- View Load events ----
        /// <summary>
        /// Gets the file list.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public JsonResult GetFileList(string text)
        {
            DirectoryInfo directory = new DirectoryInfo(Server.MapPath("~/Uploads/NewDXF"));
            var dd = new SelectList(Directory.EnumerateFiles(Server.MapPath("~/Uploads/NewDXF")));
            var filesListing = directory.GetFiles().Select(x => new SelectListItem
            {
                Value = x.Name,
                Text = x.Name
            }).ToList(); ;
            return Json(filesListing, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Binds the entity DRP values.
        /// </summary>
        /// <param name="pkgId">The PKG identifier.</param>
        /// <returns></returns>
        public JsonResult BindEntityDrpValues(int? pkgId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                try
                {
                    var entityDrpList = (from e in dbContext.tblMasterEntities
                                         where e.PackageId == pkgId && e.SectionID != 0 && e.IsDelete == false
                                         select new { e }).AsEnumerable().Select(s => new EntityDropdownOptions()
                                         {
                                             Id = s.e.EntityID,
                                             Name = s.e.EntityCode + " " + s.e.EntityName
                                         }).ToList();
                    return Json(entityDrpList, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.InnerException, JsonRequestBehavior.AllowGet);
                }
            }
        }

        /// <summary>
        /// Gets the sections by package.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public JsonResult Get_SectionsByPackage(int? id)
        {
            List<SectionModel> _SectionList = new List<SectionModel>();
            try
            {
                using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
                {
                    _SectionList = (from e in dbContext.tblSections
                                    where (e.PackageId == id && e.IsDeleted == false)
                                    select new SectionModel
                                    {
                                        SectionId = e.SectionID,
                                        SectionName = e.SectionCode + " - " + e.SectionName
                                    }).ToList();
                    return Json(_SectionList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(_SectionList, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region ---- Get Chainage + Validate it ----
        /// <summary>
        /// Gets the chainage.
        /// get Start and end chainage according to conditions
        /// </summary>
        /// <param name="oModel">The o model.</param>
        /// <returns></returns>
        private Tuple<int, int, string> GetChainage(AutoCadViewerModel oModel)
        {
            try
            {
                int StartC = 0, EndC = 0;
                string _Status = string.Empty;
                int entityStartC = Functions.RepalceCharacter(oModel.StartChainage);
                int entityEndC = Functions.RepalceCharacter(oModel.EndChainage);

                using (var db = new dbRVNLMISEntities())
                {
                    if (oModel.EntityId != 0)
                    {
                        var entity = db.tblMasterEntities.Where(s => s.EntityID == oModel.EntityId).FirstOrDefault();
                        StartC = Functions.RepalceCharacter(entity.StartChainage);
                        EndC = Functions.RepalceCharacter(entity.EndChainage);
                    }
                    else if (oModel.SectionId != 0)
                    {
                        var sectionObj = db.tblSections.Where(s => s.SectionID == oModel.SectionId && s.IsDeleted == false).FirstOrDefault();
                        StartC = Functions.RepalceCharacter(sectionObj.StartChainage);
                        EndC = Functions.RepalceCharacter(sectionObj.EndChainage);
                    }
                    else
                    {

                        if (entityEndC != 0 && entityStartC != 0)
                        {
                            StartC = entityStartC;
                            EndC = entityEndC;
                        }
                        else
                        {
                            var pakg = db.tblPackages.Where(s => s.PackageId == oModel.PackageId && s.IsDeleted == false).FirstOrDefault();
                            StartC = Functions.RepalceCharacter(pakg.StartChainage);
                            EndC = Functions.RepalceCharacter(pakg.EndChainage);
                        }

                    }
                }
                //if (entityEndC != 0 && entityStartC != 0)
                //{
                //    if ((entityStartC >= StartC && entityStartC <= EndC) && (entityEndC >= StartC && entityEndC <= EndC))
                //    {
                //        StartC = entityStartC;
                //        EndC = entityEndC;
                //    }
                //    else
                //    {
                //        _Status = "Invalid Chainage";
                //    }
                //}

                return new Tuple<int, int, string>(StartC, EndC, _Status);

            }
            catch (Exception ex)
            {
                return new Tuple<int, int, string>(0, 0, "Exception");
            }

        }

        #endregion

        #region ---- Straight Line ----
        [HttpPost]
        public JsonResult SaveAndLoadStraightLineFile(AutoCadViewerModel oModel)
        {
            try
            {
                if (oModel.PackageId != 0)
                {
                    string _Plot = string.Empty;
                    _ = new Tuple<int, int, string>(0, 0, "");
                    Tuple<int, int, string> _Chainage = GetChainage(oModel); //  check for exception / chainage validation / get start end chainage
                    if (_Chainage.Item3 != string.Empty)
                    {
                        return Json(_Chainage.Item3);
                    }
                    _Plot = Baseline(oModel, _Chainage.Item1, _Chainage.Item2);
                    oModel.FileName = _Plot;
                    return Json(oModel);
                }
                else
                {
                    return Json(1);
                }
            }
            catch (Exception ex)
            {

                return Json("Exception");
            }

        }


        private string Baseline(AutoCadViewerModel oModel, int _startChainage, int _endChainage)
        {
            GetSettingDetails();
            int _packageId = oModel.PackageId;
            //Get Section Details
            DataTable dtSection = new DataTable();
            // dtSection = GetSections(_packageId); //-- old
            dtSection = GetSectionsBetweenChainage(_packageId, _startChainage, _endChainage);
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
            // GridPlotting(dxfDocument);//old

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

                string startChainage = string.Empty;
                string endChainage = string.Empty;
                if (Functions.ParseInteger(dtSection.Rows[i]["StartChainage"].ToString().Replace("+", "").Trim()) < _startChainage)
                {
                    startChainage = _startChainage.ToString();
                }
                else
                {
                    startChainage = dtSection.Rows[i]["StartChainage"].ToString().Trim();
                }
                if (Functions.ParseInteger(dtSection.Rows[i]["EndChainage"].ToString().Replace("+", "").Trim()) > _startChainage)
                {
                    endChainage = _startChainage.ToString();
                }
                else
                {
                    endChainage = dtSection.Rows[i]["EndChainage"].ToString().Trim();
                }
                
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
            //dtEntities = GetEntities(_packageId); //old
            dtEntities = GetEntitiesBetweenChainage(_packageId, _startChainage, _endChainage); //new

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

            // added 2 new line 
            sectionStart = Convert.ToDouble(_startChainage);
            sectionEnd = Convert.ToDouble(_endChainage);

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

            #region --- commented section ----
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
            #endregion

            //DataTable dtPackage = new DataTable();
            //dtPackage = GetPackageDetails(_packageId);
            int _BaseYPosition = PackageBaseYposition;
            int _ScalingFactor = PackageScalingFactor;
            //Get Start chinage and End Chainage of Package
            string strStartChainage = IntToStringChainage(_startChainage); //dtPackage.Rows[0]["StartChainage"].ToString();
            string strEndChainage = IntToStringChainage(_endChainage);// dtPackage.Rows[0]["EndChainage"].ToString();
            int intStartChainge = Functions.ParseInteger(strStartChainage.Replace("+", ""));
            int intEndChainage = Functions.ParseInteger(strEndChainage.Replace("+", ""));

            DataTable dtStraightPlot = new DataTable();

            //dtStraightPlot = StraightLineCalculations(lineHeight, scalingFactor, _packageId); //old
            dtStraightPlot = StraightLineCalculations(lineHeight, scalingFactor, _packageId, strStartChainage, strEndChainage); //new
            if (dtStraightPlot.Rows.Count > 0) // added new 
            {
                PlotActivityLayersWithStraightLine(dtStraightPlot, dxfDocument);
            }


            //------------------------------------------------------------


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

        private List<double> GetActualPlotPointsOGL(DxfDocument dxfDocument, int _packageId, int _baseYPosition, int _scalingFactor, string _startChainage, string _endChainage)
        {
            int intStartChainge = Functions.ParseInteger(_startChainage.Replace("+", ""));
            int intEndChainage = Functions.ParseInteger(_endChainage.Replace("+", ""));

            //Step 1 Get OGL, FRL and Cross Section Id, Name for chainage.
            //Get OGL (Original Ground Level), FRL(Finish Rail Level), CROSS Section Name, Height = FRL - OGL
            //Height will be -ve if OGL is above Line  cutting is needed in that case
            DataTable dtOGLFRL = new DataTable();
            //dtOGLFRL = GetOGLFRLforPackage(_packageId, _startChainage, _endChainage);//old
            dtOGLFRL = GetOGLFRLforPackage(_packageId, intStartChainge, intEndChainage);//new

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

            List<double> listMinMax = GridPlotting(dxfDocument, minX,
             minY * _scalingFactor,
               maxX,
               maxY * _scalingFactor,
               _scalingFactor);
            //List<double> listMinMax = new List<double>();
            //listMinMax.Add(RoundValueToLower100(minX));
            //listMinMax.Add(RoundValueToLower100(minY));
            //listMinMax.Add(RoundValueToNext100(maxX));
            //listMinMax.Add(RoundValueToNext100(maxY));
            // PlotActivityLayers(dtOGLFRL, dxfDocument, _scalingFactor);
            return listMinMax;
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

            for (int i = 1; i < dtPlotPoints.Rows.Count; i++)//old           
            {

                double startX = Functions.ParseDouble(dtPlotPoints.Rows[i]["StartX"].ToString());
                double startY = Functions.ParseDouble(dtPlotPoints.Rows[i]["StartY"].ToString());
                double endX = Functions.ParseDouble(dtPlotPoints.Rows[i]["EndX"].ToString());
                double endY = Functions.ParseDouble(dtPlotPoints.Rows[i]["EndY"].ToString());

                if (startX == 0 && startY == 0 && endX == 0 && endY == 0)
                {
                    if (i == dtPlotPoints.Rows.Count) //added new condition
                    {
                        CreateText(dxfDocument, dtPlotPoints.Rows[i]["ScActName"].ToString(),
                        Functions.ParseDouble(dtPlotPoints.Rows[i]["StartX"].ToString()) - 80,
                       Functions.ParseDouble(dtPlotPoints.Rows[i]["StartY"].ToString()),
                        12,
                        layer,
                        0);
                    }
                    else
                    {
                        CreateText(dxfDocument, dtPlotPoints.Rows[i]["ScActName"].ToString(),
                        Functions.ParseDouble(dtPlotPoints.Rows[i + 1]["StartX"].ToString()) - 80,
                       Functions.ParseDouble(dtPlotPoints.Rows[i + 1]["StartY"].ToString()),
                        12,
                        layer,
                        0);
                    }
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

        #region ---- PlotActivityLayers function commented for straight line functions ----
        //public void PlotActivityLayers(DataTable dtPlotPoints, DxfDocument dxfDocument, double scalingFactor)
        //{
        //    //CREATE FILES
        //    //DxfDocument dxfDocument = new DxfDocument();

        //    //CREATE LAYER
        //    Layer layer = new Layer("Layer-01");
        //    dxfDocument.Layers.Add(layer);
        //    layer.Lineweight = Lineweight.W5;
        //    string color = Convert.ToString("Green");
        //    ApplyColorToLayer(layer, color);

        //    for (int i = 1; i < dtPlotPoints.Rows.Count; i++)
        //    {

        //        double startX = Functions.ParseDouble(dtPlotPoints.Rows[i]["StartX"].ToString());
        //        double startY = Functions.ParseDouble(dtPlotPoints.Rows[i]["StartY"].ToString());
        //        double endX = Functions.ParseDouble(dtPlotPoints.Rows[i]["EndX"].ToString());
        //        double endY = Functions.ParseDouble(dtPlotPoints.Rows[i]["EndY"].ToString());

        //        if (startX == 0 && startY == 0 && endX == 0 && endY == 0)
        //        {
        //            CreateText(dxfDocument, dtPlotPoints.Rows[i]["ScActName"].ToString(),
        //                Functions.ParseDouble(dtPlotPoints.Rows[i + 1]["StartX"].ToString()) - 80,
        //               Functions.ParseDouble(dtPlotPoints.Rows[i + 1]["StartY"].ToString()),
        //                12,
        //                layer,
        //                0);
        //        }
        //        else
        //        {
        //            CreateLineWithColorThickness(dxfDocument,
        //                dtPlotPoints.Rows[i]["PlotColour"].ToString(),
        //                Functions.ParseDouble(dtPlotPoints.Rows[i]["PlotThk"].ToString()),
        //                 startX,
        //                scalingFactor * startY,
        //                  endX,
        //                scalingFactor * endY);
        //        }

        //    }

        //    //CREATE DOCUMENT NAME HERE 
        //    //string newDxfFile = Server.MapPath(@"../NewDXF/" + "test123.dxf");
        //    //SAVE GENERATED  AUTOCAD FDRAWING
        //    //dxfDocument.Save(newDxfFile);

        //    //lblMessage.Text = "File Generation Completed";
        //    //SAVE IT IN SESSION SO THAT THE LATEST DRAWING IS OPENED
        //    //Session["NEW-DRAWING"] = newDxfFile;

        //    //TRANSFER IT TO CAD VIEWER
        //    // Server.Transfer("cadviewer.aspx");
        //}
        #endregion

        //private DataTable StraightLineCalculations(double lineHeight, double scalingFactor, int packageId) //old
        private DataTable StraightLineCalculations(double lineHeight, double scalingFactor, int packageId, string strStartChainage, string strEndChainage) // added 2 last param in new
        {
            int PackageId = packageId;
            //DataTable dtPackage = new DataTable();     //old commented
            //dtPackage = GetPackageDetails(PackageId);
            //Get Start chinage and End Chainage of Package  
            //string strStartChainage = dtPackage.Rows[0]["StartChainage"].ToString();
            //string strEndChainage = dtPackage.Rows[0]["EndChainage"].ToString();
            int intStartChainge = Functions.ParseInteger(strStartChainage.Replace("+", ""));
            int intEndChainage = Functions.ParseInteger(strEndChainage.Replace("+", ""));

            //Step 1 Get OGL, FRL and Cross Section Id, Name for chainage.
            //Get OGL (Original Ground Level), FRL(Finish Rail Level), CROSS Section Name, Height = FRL - OGL
            //Height will be -ve if OGL is above Line  cutting is needed in that case
            DataTable dtOGLFRL = new DataTable();
            //dtOGLFRL = GetOGLFRLforPackage(PackageId, strStartChainage, strEndChainage);//old
            dtOGLFRL = GetOGLFRLforPackage(PackageId, intStartChainge, intEndChainage);

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

            if (dtOGLFRL.Rows.Count > 0) // added new 
            {
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
            }




            // PlotActivityLayers(dtOGLFRL);

            return dtOGLFRL;
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


        #endregion

        #endregion

        #region ---- Curve line code ----

        [HttpPost]
        public JsonResult SaveAndLoadCurveFile(AutoCadViewerModel oModel)
        {
            try
            {

                if (oModel.PackageId != 0)
                {
                    if ((!string.IsNullOrEmpty(oModel.StartChainage) && string.IsNullOrEmpty(oModel.EndChainage)) || (string.IsNullOrEmpty(oModel.StartChainage) && !string.IsNullOrEmpty(oModel.EndChainage)))
                    {
                        return Json("nullChainage");
                    }
                    else
                    {
                        // get all veriable settiing from db
                        GetSettingDetails();
                        oModel.FileName = "Jojila-Tunnel-Option-03.dxf";
                        _ = new Tuple<int, int, string>(0, 0, "");
                        Tuple<int, int, string> _Chainage = GetChainage(oModel); //  check for exception / chainage validation / get start end chainage
                        if (_Chainage.Item3 != string.Empty)
                        {
                            return Json(_Chainage.Item3);
                        }

                        int _PackageId = oModel.PackageId;
                        int _BaseYPosition = PackageBaseYposition;
                        int _ScalingFactor = PackageScalingFactor;
                        // _Chainage = new Tuple<int, int, string>(146060, 150000, "");

                        string newImage = GenerateStripChartForPackage(oModel.PackageId, _BaseYPosition, _ScalingFactor, _Chainage.Item1, _Chainage.Item2);
                        oModel.FileName = newImage;
                        return Json(oModel);
                    }
                }
                else
                {
                    return Json(1);
                }
            }
            catch (Exception ex)
            {
                return Json("Exception");
            }

        }


        private string GenerateStripChartForPackage(int _packageId, int _baseYPosition, int _scalingFactor, int _startChainage, int _endChainage)
        {

            #region ----------- CREATE FILE -----------------
            //CREATE AutoCAD Document 
            DxfDocument dxfDocument = new DxfDocument();
            #endregion

            #region ----------------- PRINT ACTIVITY LAYERS WITH ORIGINAL VALUES  ------------------

            //------------------------PRINT ACTIVITY LAYERS WITH OGL ------------------------------------
            DataTable dtPlotPoints = new DataTable();
            List<double> listMinMax = GetActualPlotPointsOGL(dxfDocument, _packageId, _baseYPosition, _scalingFactor, _startChainage, _endChainage);

            #endregion

            #region ------------------- Plot Chainage line with Chainage at every 100 ------------------

            //PRINT A LINE BELOW THE PLOT GRID FOR CHAINAGE
            CreateLineWithColorThickness(dxfDocument, CurveChainageLineOneColor, CurveChainageLineOneThickness, _startChainage, listMinMax[1] - CurveChainageLineOneMinus_Y1, _endChainage, listMinMax[1] - CurveChainageLineOneMinus_Y2, "Chainage Line One");

            //Plot Vertical lines with chainage text
            PlotChinageWithMarkers(dxfDocument, _startChainage, _endChainage, listMinMax[1] - CurveChainageLineOneMinus_Yposition, true);

            CreateTextWithColor(dxfDocument, "Chainages", _startChainage - CurveChainageLineOneText_X, listMinMax[1] - CurveChainageLineOneText_Y, CurveChainageLineOneText_Weight, CurveChainageLineOneText_Color, CurveChainageLineOneText_Rotation, "Line-Name");
            #endregion


            #region -------------------- Plot Section and Yard Line ---------------------
            //Print Second Line 
            CreateLineWithColorThickness(dxfDocument, SecondLineColor, SecondLineThickness, _startChainage, listMinMax[1] - SecondLineThicknessMinus_Y1, _endChainage, listMinMax[1] - SecondLineThicknessMinus_Y2, "Chainage Line Two");
            //Plot Vertical lines with chainage text Start and End only - False in this case
            //PlotChinageWithMarkers(dxfDocument, _startChainage, _endChainage, listMinMax[1] - 350, false);
            CreateTextWithColor(dxfDocument, "Sections", _startChainage - SecondLIneTextMinus_X, listMinMax[1] - SecondLIneTextMinus_Y, SecondLIneTextWeight, SecondLIneTextColor, SecondLIneTextRotation, "Line-Name");
            #endregion

            #region ------------------------ Plot Sections on Second Line --------------------

            PlotSectionsOnSecondLine(_packageId, _startChainage, _endChainage, dxfDocument, listMinMax[1] - SecondLineSectionYposition);
            #endregion

            #region ----------------------- Plot Entities on Third Line

            PlotEntitiesOnThirdLine(_packageId, _startChainage, _endChainage, dxfDocument, listMinMax[1] - ThirdLineEntityYposition);

            #endregion

            #region --------------------- SAVE FILE and REDIRECT ---------------------
            //CREATE FOLDER WITH PACKAGE NAME
            Functions.CreateIfMissing(Server.MapPath(@"~/Uploads/StripChart/" + _packageId));
            //CREATE DOCUMENT NAME HERE 
            string obj = string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now);
            // Guid obj = Guid.NewGuid();
            string newDoc = "PKG-" + _packageId + "_" + obj + ".dxf";
            string newDxfFile = Server.MapPath(@"~/Uploads/StripChart/" + _packageId + "/" + newDoc);
            //SAVE GENERATED  AUTOCAD FDRAWING

            Functions.DeleteFilesInFolder(Server.MapPath(@"~/Uploads/StripChart/" + _packageId), false);

            dxfDocument.Save(newDxfFile);

            return newDoc;
            #endregion
        }



        private void PlotChinageWithMarkers(DxfDocument dxfDocument, int startChainage, int endChainage, double YPosition, bool isHundred)
        {
            //PRINT VERTICAL LINE AT NEAREST HUNDRED CHAINAGE
            CreateLineWithColorThickness(dxfDocument, CurvePlotChainageMarkerOne_Color, CurvePlotChainageMarkerOne_Thikness, startChainage, YPosition - CurvePlotChainageMarkerOne_MinusY1, startChainage, YPosition + CurvePlotChainageMarkerOne_PlusY2, "Markers-One");
            CreateTextWithColor(dxfDocument, IntToStringChainage(startChainage), startChainage, YPosition + CurvePlotChainageMarkerOneText_Yposition, CurvePlotChainageMarkerOneText_Weight, CurvePlotChainageMarkerOneText_Color, CurvePlotChainageMarkerOneText_Rotation, "Chainage Text");

            if (isHundred)
            {
                double ans = startChainage % 100;
                startChainage = (int)(startChainage + (100 - ans));
                do
                {
                    CreateLineWithColorThickness(dxfDocument, CurvePlotChainageMarkerOne_Color, CurvePlotChainageMarkerOne_Thikness, startChainage, YPosition - CurvePlotChainageMarkerOne_MinusY1, startChainage, YPosition + CurvePlotChainageMarkerOne_PlusY2, "Markers-One");
                    CreateTextWithColor(dxfDocument, IntToStringChainage(startChainage), startChainage, YPosition + CurvePlotChainageMarkerOneText_Yposition, CurvePlotChainageMarkerOneText_Weight, CurvePlotChainageMarkerOneText_Color, CurvePlotChainageMarkerOneText_Rotation, "Chainage Text");
                    startChainage += 100;
                } while (startChainage <= endChainage);
            }

            if (startChainage != endChainage)
            {
                CreateLineWithColorThickness(dxfDocument, CurvePlotChainageMarkerOne_Color, CurvePlotChainageMarkerOne_Thikness, endChainage, YPosition - CurvePlotChainageMarkerOne_MinusY1, endChainage, YPosition + CurvePlotChainageMarkerOne_PlusY2, "Markers-One");
                CreateTextWithColor(dxfDocument, IntToStringChainage(endChainage), endChainage, YPosition + CurvePlotChainageMarkerOneText_Yposition, CurvePlotChainageMarkerOneText_Weight, CurvePlotChainageMarkerOneText_Color, CurvePlotChainageMarkerOneText_Rotation, "Chainage Text");
            }
        }



        private List<double> GetActualPlotPointsOGL(DxfDocument dxfDocument, int _packageId, int _baseYPosition, int _scalingFactor, int _startChainage, int _endChainage)
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


            List<double> listMinMax = GridPlotting(dxfDocument, minX,
              minY * _scalingFactor,
                maxX,
                maxY * _scalingFactor,
                _scalingFactor);

            PlotActivityLayers(dtOGLFRL, dxfDocument, _scalingFactor);
            return listMinMax;
        }

        public void PlotActivityLayers(DataTable dtPlotPoints, DxfDocument dxfDocument, double _scalingFactor)
        {

            for (int i = 1; i < dtPlotPoints.Rows.Count; i++)
            {

                double startX = Functions.ParseDouble(dtPlotPoints.Rows[i]["StartX"].ToString());
                double startY = Functions.ParseDouble(dtPlotPoints.Rows[i]["StartY"].ToString());
                double endX = Functions.ParseDouble(dtPlotPoints.Rows[i]["EndX"].ToString());
                double endY = Functions.ParseDouble(dtPlotPoints.Rows[i]["EndY"].ToString());

                if (startX == 0 && startY == 0 && endX == 0 && endY == 0)
                {
                    CreateTextWithColor(dxfDocument, dtPlotPoints.Rows[i]["ScActName"].ToString(),
                        Functions.ParseDouble(dtPlotPoints.Rows[i + 1]["StartX"].ToString()) - 80,
                      _scalingFactor * Functions.ParseDouble(dtPlotPoints.Rows[i + 1]["StartY"].ToString()),
                        12,
                        dtPlotPoints.Rows[i + 1]["PlotColour"].ToString(),
                        0,
                        "Activity Text");
                }
                else
                {
                    CreateLineWithColorThickness(dxfDocument,
                        dtPlotPoints.Rows[i]["PlotColour"].ToString(),
                        Functions.ParseDouble(dtPlotPoints.Rows[i]["PlotThk"].ToString()),
                         startX,
                        _scalingFactor * startY,
                          endX,
                        _scalingFactor * endY,
                        dtPlotPoints.Rows[i]["ScActName"].ToString());
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

        private DataTable GetOGLFRLforPackage(int packageId, int startChainage, int endChainage)
        {
            string ConnectionString = GlobalVariables.ConnectionString;
            List<SqlParameter> paramlist = new List<SqlParameter>();

            paramlist.Add(new SqlParameter("@PackageId", packageId));
            paramlist.Add(new SqlParameter("@StartChainage", (startChainage)));
            paramlist.Add(new SqlParameter("@EndChainage", (endChainage)));

            DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, CommandType.StoredProcedure, "GetOGLFRLforPackage", paramlist.ToArray());
            return dsDataset.Tables[0];
        }


        #region -------------  TEMP FUNCTIONS ---------
        private DataTable GetPackageDetails(object packageId)
        {
            string ConnectionString = GlobalVariables.ConnectionString;
            DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, "SELECT * FROM [dbDevPBI].[dbo].[tblPackages] WHERE PackageId = " + packageId + " ");
            return dsDataset.Tables[0];
        }
        #endregion


        private static double RoundValueToNext100(double value)
        {
            return (Math.Ceiling(value / 100) * 100);
        }


        private static double RoundValueToLower100(double value)
        {
            return (value - (value % 100));

        }
        //PLOT A GRAY COLORED GRID
        private List<double> GridPlotting(DxfDocument dxfDocument, double _minX, double _minY, double _maxX, double _maxY, int _scalingFactor)
        {
            double minX = RoundValueToLower100(_minX);
            double minY = RoundValueToLower100(_minY);
            double maxX = RoundValueToNext100(_maxX);
            double maxY = RoundValueToNext100(_maxY);

            List<double> listMinMax = new List<double>();
            listMinMax.Add(minX);
            listMinMax.Add(minY);
            listMinMax.Add(maxX);
            listMinMax.Add(maxY);

            int countHrz = (int)((maxY - minY) / 100);
            for (int i = 0; i <= countHrz; i++)
            {
                CreateLineWithColorThickness(dxfDocument, "#e6e6e6", 1, minX, minY, maxX, minY, "Grid-Horizontal");
                minY = minY + 100;
            }

            minX = RoundValueToLower100(_minX);
            minY = RoundValueToLower100(_minY);
            maxX = RoundValueToNext100(_maxX);
            maxY = RoundValueToNext100(_maxY);

            int countVert = (int)((maxX - minX) / 100);
            for (int i = 0; i <= countVert; i++)
            {
                CreateLineWithColorThickness(dxfDocument, "#e6e6e6", 1, minX, minY, minX, maxY, "Grid-Vertical");
                minX = minX + 100;
            }

            return listMinMax;
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

        #region ------------- Helper Functions ------------------
        //Returns double value for chainage Ex: 146+960 Out: 146960
        private double ChainageStringToInteger(string strChainage)
        {
            return Functions.ParseDouble(strChainage.Replace("+", ""));
        }

        public string IntToStringChainage(int chainage)
        {
            return chainage.ToString("D6").Insert(chainage.ToString("D6").Length - 3, "+");
        }

        public AciColor GetAciColor(string ColorName)
        {
            if (ColorName.StartsWith("#") && ColorName.Length == 7)
            {
                ColorName = ColorName.Remove(0, 1);
                byte r = Convert.ToByte(ColorName.Substring(0, 2), 16);
                byte g = Convert.ToByte(ColorName.Substring(2, 2), 16);
                byte b = Convert.ToByte(ColorName.Substring(4, 2), 16);

                AciColor color = new AciColor(r, g, b);
                return color;
            }
            else
            {
                switch (ColorName)
                {
                    case "Red": return AciColor.Red;
                    case "Yellow": return AciColor.Yellow;
                    case "Green": return AciColor.Green;
                    case "Blue": return AciColor.Blue;
                    case "Cyan": return AciColor.Cyan;
                    case "DarkGray": return AciColor.DarkGray;
                    case "LightGray": return AciColor.LightGray;
                    case "Magenta": return AciColor.Magenta;
                    default: return AciColor.Cyan;
                }
            }
        }


        #endregion

        #endregion


    }

}