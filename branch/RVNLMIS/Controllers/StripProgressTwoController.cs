using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Objects;
using netDxf.Tables;
using netDxf.Units;
using RVNLMIS.Common;
using RVNLMIS.Common.ActionFilters;
using RVNLMIS.DAC;
using RVNLMIS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Web.Mvc;

namespace RVNLMIS.Controllers
{
    [HandleError]
    [Authorize]
    [SessionAuthorize]
    public class StripProgressTwoController : Controller
    {
        public string IpAddress = "";
        public ActionResult Index()
        {
            IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(IpAddress))
            {
                IpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
            int UserID = ((UserModel)Session["UserData"]).UserId;
            int k = Functions.SaveUserLog(pkgId, "Generate Strip Chart", "View", UserID, IpAddress, "NA");
            var objUserM = (UserModel)Session["UserData"];

            AutoCadViewerModel obj = new AutoCadViewerModel();
            obj.PackageId = objUserM.RoleTableID;
            return View(obj);
        }

        public JsonResult GenerateProgressLoadFile(AutoCadViewerModel oModel)
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
                    _Plot = PlotActivityLines(oModel, _Chainage.Item1, _Chainage.Item2);
                    oModel.FileName = _Plot;
                    IpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(IpAddress))
                    {
                        IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }
                    int pkgId = ((UserModel)Session["UserData"]).RoleTableID;
                    int UserID = ((UserModel)Session["UserData"]).UserId;
                    int k = Functions.SaveUserLog(pkgId, "Generate Strip Chart", "Generate Progress File", UserID, IpAddress, "NA");
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


        public string GetSeetingsByKey(string Skey)
        {
            string result = string.Empty;
            using (var db = new dbRVNLMISEntities())
            {
                //var res = db.tblSettings.Where(r => r.SKey == Skey).Select(p => new { p.Value }).FirstOrDefault().ToString();
                var res = db.tblSettings.FirstOrDefault(r => r.SKey == Skey).Value;
                result = res.ToString();
            }
            return result;
        }

        private string PlotActivityLines(AutoCadViewerModel oModel, int startChainage, int endChainage)
        {

            double _HORIZ_SCALING_FACTOR = 1;
            double _CHAINAGE_FACTOR = 1;
            int _BASELINE, _ACTIVITY_THICKNESS, pointY, _packageId = oModel.PackageId, _VERTICAL_MARKERS;

            #region  -------------- Get Settings ------------
            if (string.IsNullOrEmpty(oModel.Scale))
            {
                _HORIZ_SCALING_FACTOR = Functions.ParseDouble(GetSeetingsByKey("HORIZ_SCALING_FACTOR"));
            }
            else
            {
                _HORIZ_SCALING_FACTOR = Functions.ParseInteger(oModel.Scale);
            }


            _CHAINAGE_FACTOR = _HORIZ_SCALING_FACTOR / 100;
            _HORIZ_SCALING_FACTOR = 100 / _HORIZ_SCALING_FACTOR;


            _BASELINE = pointY = Functions.ParseInteger(GetSeetingsByKey("BASELINE")); ;
            _ACTIVITY_THICKNESS = Functions.ParseInteger(GetSeetingsByKey("ACTIVITY_THICKNESS")); ; ;

            _VERTICAL_MARKERS = 0;

            #endregion

            #region ----------- CREATE FILE -----------------
            //CREATE AutoCAD Document 
            DxfDocument dxfDocument = new DxfDocument();





            #endregion



            #region ------------------ Plot Package ----------------------
            using (var db = new dbRVNLMISEntities())
            {
                tblPackage objPackage = db.tblPackages.Where(r => r.PackageId == oModel.PackageId).FirstOrDefault();

                double x1, y1, x2, y2, x3, y3, x4, y4 = 0;
                x1 = x4 = startChainage * _HORIZ_SCALING_FACTOR;
                x2 = x3 = endChainage * _HORIZ_SCALING_FACTOR;
                y1 = y2 = pointY;
                y3 = y4 = pointY - _ACTIVITY_THICKNESS;//TODO: change the thickness later

                CreateRectangle(dxfDocument, "LightGray", Lineweight.W25, x1, y1, x2, y2, x3, y3, x4, y4, "Package");
                CreateTextWithColor(dxfDocument,
                        string.Concat(objPackage.PackageCode, " - ", objPackage.PackageName),
                        x1 + 10,
                        pointY - 35,
                        20,
                        "DarkGray",
                        0,
                        "Package");

                pointY -= _ACTIVITY_THICKNESS;
            }

            #endregion

            #region ------------------ Plot Sections --------------------

            using (var db = new dbRVNLMISEntities())
            {
                List<viewStripChartSection> listSections = db.viewStripChartSections.Where(r => r.PackageId == oModel.PackageId && r.StartChainage <= endChainage && r.EndChainage >= startChainage).OrderBy(s => s.StartChainage).ToList();
                foreach (viewStripChartSection section in listSections)
                {
                    double x1, y1, x2, y2, x3, y3, x4, y4, sectionStart, sectionEnd;
                    int stChain = Functions.ParseInteger(section.StartChainage.ToString());
                    int enChain = Functions.ParseInteger(section.EndChainage.ToString());

                    sectionStart = stChain * _HORIZ_SCALING_FACTOR;
                    sectionEnd = enChain * _HORIZ_SCALING_FACTOR;
                    x1 = x4 = sectionStart <= startChainage * _HORIZ_SCALING_FACTOR ? startChainage * _HORIZ_SCALING_FACTOR : sectionStart;
                    x2 = x3 = sectionEnd >= endChainage * _HORIZ_SCALING_FACTOR ? endChainage * _HORIZ_SCALING_FACTOR : sectionEnd;
                    y1 = y2 = pointY;
                    y3 = y4 = pointY - _ACTIVITY_THICKNESS;//TODO: change the thickness later

                    CreateRectangle(dxfDocument, "LightGray", Lineweight.W25, x1, y1, x2, y2, x3, y3, x4, y4, "Section");
                    CreateTextWithColor(dxfDocument,
                            string.Concat(section.SectionCode, " - ", section.SectionName, " (", IntToStringChainage(stChain), " to ", IntToStringChainage(enChain), ")"),
                            x1 + 10,
                            pointY - 35,
                            20,
                            "DarkGray",
                            0,
                            "Section");
                }
                pointY -= _ACTIVITY_THICKNESS;
            }
            //PlotSectionsOnSecondLine(_packageId, startChainage, endChainage, dxfDocument, pointY);
            #endregion

            #region ------------------ Plot Entities ------------------------

            using (var db = new dbRVNLMISEntities())
            {
                List<viewStripChartEntity> listEntities = db.viewStripChartEntities.Where(r => r.PackageId == oModel.PackageId && (r.EntityType == "Yard" || r.EntityType == "Mid section") && r.StartChainage <= endChainage && r.EndChainage >= startChainage).OrderBy(s => s.StartChainage).ToList();
                foreach (viewStripChartEntity entity in listEntities)
                {
                    double x1, y1, x2, y2, x3, y3, x4, y4, entityStart, entityEnd;
                    int stChain = Functions.ParseInteger(entity.StartChainage.ToString());
                    int enChain = Functions.ParseInteger(entity.EndChainage.ToString());


                    entityStart = stChain * _HORIZ_SCALING_FACTOR;
                    entityEnd = enChain * _HORIZ_SCALING_FACTOR;
                    x1 = x4 = entityStart <= startChainage * _HORIZ_SCALING_FACTOR ? startChainage * _HORIZ_SCALING_FACTOR : entityStart;
                    x2 = x3 = entityEnd >= endChainage * _HORIZ_SCALING_FACTOR ? endChainage * _HORIZ_SCALING_FACTOR : entityEnd;
                    y1 = y2 = pointY;
                    y3 = y4 = pointY - _ACTIVITY_THICKNESS;//TODO: change the thickness later

                    CreateRectangle(dxfDocument, "LightGray", Lineweight.W25, x1, y1, x2, y2, x3, y3, x4, y4, "Entity");
                    CreateTextWithColor(dxfDocument,
                            string.Concat(entity.EntityCode, " - ", entity.EntityName, " (", IntToStringChainage(stChain), " to ", IntToStringChainage(enChain), ")"),
                            x1 + 10,
                            pointY - 35,
                            20,
                            "DarkGray",
                            0,
                            "Entity");
                }
                pointY -= _ACTIVITY_THICKNESS;
            }

            #endregion

            #region ------------------ Plot Chainage Text ------------------
            ////Plot Vertical lines with chainage text
            {
                double x1, y1, x2, y2, x3, y3, x4, y4 = 0;
                x1 = x4 = startChainage * _HORIZ_SCALING_FACTOR;
                x2 = x3 = endChainage * _HORIZ_SCALING_FACTOR;
                y1 = y2 = pointY;
                y3 = y4 = pointY - 120;//TODO: change the thickness later

                CreateRectangle(dxfDocument, "LightGray", Lineweight.W25, x1, y1, x2, y2, x3, y3, x4, y4, "Package");

                pointY -= 120;
            }

            PlotChinageWithMarkers(dxfDocument, startChainage, endChainage, pointY, true, _HORIZ_SCALING_FACTOR, _CHAINAGE_FACTOR);
            _VERTICAL_MARKERS = pointY;
            pointY -= 200;

            #endregion

            #region ---------------- Plot Activities and Hatch -------------

            using (var db = new dbRVNLMISEntities())
            {
                int _NEWBASELINE = pointY;


                #region --------------------- PRINT ACTIVITY LINES ----------------------
                List<viewStripActivityTwo> listActivity = db.viewStripActivityTwoes.Where(r => r.PackageId == oModel.PackageId).OrderBy(s => s.Sequence).ToList();
                //List<stripGetActivitesInSquence_Result> listActivity = db.stripGetActivitesInSquence(oModel.PackageId).ToList();

                for (int i = 0; i < listActivity.Count; i++)
                {
                    CreateTextWithColor(dxfDocument,
                       listActivity[i].ActivityName,
                       (startChainage * _HORIZ_SCALING_FACTOR) - 250,
                       pointY - 26,
                       16,
                       listActivity[i].Color,
                       0,
                       "Activity Names");

                    CreateTextWithColor(dxfDocument,
                        listActivity[i].ActivityName,
                        (endChainage * _HORIZ_SCALING_FACTOR) + 10,
                        pointY - 26,
                        16,
                        listActivity[i].Color,
                        0,
                        "Activity Names");

                    CreateLineWithColorThickness(dxfDocument,
                        listActivity[i].Color,
                        Lineweight.W15,
                        startChainage * _HORIZ_SCALING_FACTOR,
                        pointY,
                        endChainage * _HORIZ_SCALING_FACTOR,
                        pointY,
                        "Activity Lines");

                    pointY = pointY - _ACTIVITY_THICKNESS;//TODO: set the 100 dynamically from settings

                }
                #endregion

                #region ----------------- Get progress details and PRINT with HATCH ---------------------



                List<viewStripProgress> listProgress = new List<viewStripProgress>();
                listProgress = db.viewStripProgresses.Where(r => r.PackageId == oModel.PackageId && r.StartChainage <= endChainage && r.EndChainage >= startChainage).OrderBy(s => s.Sequence).ToList();

                for (int i = 0; i < listProgress.Count; i++)
                {
                    double start = Functions.ParseDouble(listProgress[i].StartChainage.ToString()) * _HORIZ_SCALING_FACTOR;
                    double end = Functions.ParseDouble(listProgress[i].EndChainage.ToString()) * _HORIZ_SCALING_FACTOR;

                    double p1X, p4X;
                    p1X = p4X = start < startChainage * _HORIZ_SCALING_FACTOR ? startChainage * _HORIZ_SCALING_FACTOR : start;

                    double p2X, p3X;
                    p2X = p3X = end > endChainage * _HORIZ_SCALING_FACTOR ? endChainage * _HORIZ_SCALING_FACTOR : end;

                    double p1Y, p2Y, p3Y, p4Y;

                    string plotColor = listProgress[i].Color;

                    p1Y = p2Y = _NEWBASELINE - ((Functions.ParseInteger(listProgress[i].Sequence.ToString()) - 1) * _ACTIVITY_THICKNESS);
                    p3Y = p4Y = p1Y - _ACTIVITY_THICKNESS;

                    LwPolyline poly = new LwPolyline();
                    poly.Vertexes.Add(new LwPolylineVertex(p1X, p1Y));
                    poly.Vertexes.Add(new LwPolylineVertex(p2X, p2Y));
                    poly.Vertexes.Add(new LwPolylineVertex(p3X, p3Y));
                    poly.Vertexes.Add(new LwPolylineVertex(p4X, p4Y));
                    poly.IsClosed = true;

                    HatchBoundaryPath boundary = new HatchBoundaryPath(new List<EntityObject> { poly });
                    HatchPattern pattern = new HatchPattern("Progress");// = status == "Completed" ? HatchPattern.Solid : HatchPattern.Line;

                    switch (listProgress[i].Status.ToLower())
                    {
                        case "completed":
                            pattern = HatchPattern.Solid;
                            break;
                        case "in progress":
                            pattern = HatchPattern.Line;
                            pattern.Scale = 50;
                            pattern.Angle = 45;
                            break;
                        case "exception":
                            pattern = HatchPattern.Line;
                            pattern.Scale = 25;
                            pattern.Angle = 90;
                            break;
                    }

                    Hatch hatch = new Hatch(pattern, true);
                    hatch.Color = GetAciColor(plotColor);

                    hatch.BoundaryPaths.Add(boundary);
                    dxfDocument.AddEntity(hatch);
                }
                #endregion
            }


            #endregion

            #region ------------ vertical chainage lines ------------
            PlotChainageVerticalLines(dxfDocument, startChainage, endChainage, _VERTICAL_MARKERS, pointY, true, _HORIZ_SCALING_FACTOR, _CHAINAGE_FACTOR);
            #endregion

            #region ------------------ Plot Chainage Text ------------------
            ////Plot Vertical lines with chainage text
            {
                double x1, y1, x2, y2, x3, y3, x4, y4 = 0;
                x1 = x4 = startChainage * _HORIZ_SCALING_FACTOR;
                x2 = x3 = endChainage * _HORIZ_SCALING_FACTOR;
                y1 = y2 = pointY;
                y3 = y4 = pointY - 120;//TODO: change the thickness later

                CreateRectangle(dxfDocument, "LightGray", Lineweight.W25, x1, y1, x2, y2, x3, y3, x4, y4, "Package");

                pointY -= 120;
            }
            PlotChinageWithMarkers(dxfDocument, startChainage, endChainage, pointY, true, _HORIZ_SCALING_FACTOR, _CHAINAGE_FACTOR);
            #endregion

            #region ------------------ Plot Structure Entities ------------------------
            pointY -= 160;

            using (var db = new dbRVNLMISEntities())
            {
                //List<viewStripChartEntity> listEntities = db.viewStripChartEntities.Where(r => r.PackageId == oModel.PackageId && (r.EntityType == "Yard" || r.EntityType == "Block section") && r.StartChainage <= endChainage && r.EndChainage >= startChainage).OrderBy(s => s.StartChainage).ToList();
                List<viewStripChartEntity> listEntities = db.viewStripChartEntities.Where(r => r.PackageId == oModel.PackageId && (r.EntityType != "Yard" && r.EntityType != "Mid section") && r.StartChainage <= endChainage && r.EndChainage >= startChainage).OrderBy(s => s.StartChainage).ToList();
                foreach (viewStripChartEntity entity in listEntities)
                {
                    double x1, y1, x2, y2, x3, y3, x4, y4, sectionStart, sectionEnd;

                    sectionStart = Functions.ParseDouble(entity.StartChainage.ToString()) * _HORIZ_SCALING_FACTOR;
                    sectionEnd = Functions.ParseDouble(entity.EndChainage.ToString()) * _HORIZ_SCALING_FACTOR;

                    x1 = x4 = sectionStart <= startChainage * _HORIZ_SCALING_FACTOR ? startChainage * _HORIZ_SCALING_FACTOR : sectionStart;
                    x2 = x3 = sectionEnd >= endChainage * _HORIZ_SCALING_FACTOR ? endChainage * _HORIZ_SCALING_FACTOR : sectionEnd;
                    y1 = y2 = pointY;
                    y3 = y4 = pointY - 50;//TODO: change the thickness later

                    CreateRectangle(dxfDocument, "LightGray", Lineweight.W25, x1, y1, x2, y2, x3, y3, x4, y4, "Entity");
                    CreateTextWithColor(dxfDocument,
                            string.Concat(entity.EntityName/*, " (", IntToStringChainage(sectionStart), " to ", IntToStringChainage(sectionEnd), ")"*/),
                            x1 + 10,
                            pointY - 100,
                            8,
                            "LightGray",
                            90,
                            "Entity");
                }
                pointY -= 50;
            }

            //PlotEntitiesOnThirdLine(_packageId, startChainage, endChainage, dxfDocument, pointY);

            #endregion

            #region -------------- Vertical Lines ---------------------

            CreateLineWithColorThickness(dxfDocument,
                        "LightGray",
                        Lineweight.W0,
                        startChainage * _HORIZ_SCALING_FACTOR,
                        _BASELINE,
                        startChainage * _HORIZ_SCALING_FACTOR,
                        pointY,
                        "Block");

            CreateLineWithColorThickness(dxfDocument,
                        "LightGray",
                        Lineweight.W0,
                        endChainage * _HORIZ_SCALING_FACTOR,
                        _BASELINE,
                        endChainage * _HORIZ_SCALING_FACTOR,
                        pointY,
                        "Block");

            #endregion

            #region -------------- Outer Box --------------------- 
            {
                double x1, y1, x2, y2, x3, y3, x4, y4;

                x1 = x4 = (startChainage * _HORIZ_SCALING_FACTOR) - 300;
                x2 = x3 = (endChainage * _HORIZ_SCALING_FACTOR) + 300;
                y1 = y2 = _BASELINE;
                y3 = y4 = pointY;//TODO: change the thickness later

                CreateRectangle(dxfDocument, "LightGray", Lineweight.W5, x1, y1, x2, y2, x3, y3, x4, y4, "Outer Box");
            }
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


        private  void WriteImage(DxfDocument dxfDocument)
        {

            try
            {
                ImageDefinition imageDefinition = new ImageDefinition(Server.MapPath(@"\Uploads\StripChart\train.jpg"));



                Image image = new Image(imageDefinition, Vector3.Zero, 10, 10);
                image.IsVisible = true;
                image.DisplayOptions = ImageDisplayFlags.ShowImage;


                XData xdata1 = new XData(new ApplicationRegistry("netDxf"));

                xdata1.XDataRecord.Add(new XDataRecord(XDataCode.String, "xData image position"));
                xdata1.XDataRecord.Add(XDataRecord.OpenControlString);
                xdata1.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionX, image.Position.X));
                xdata1.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionY, image.Position.Y));
                xdata1.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionZ, image.Position.Z));
                xdata1.XDataRecord.Add(XDataRecord.CloseControlString);
                image.XData.Add(xdata1);

                Block block = new Block("ImageBlock");
                block.Entities.Add(image);

                Insert insert = new Insert(block, new Vector3(0, 100, 0));



               // dxfDocument.AddEntity(image);

                dxfDocument.AddEntity(insert);

                //dxf.RemoveEntity(image2);
                //dxf.Save("image2.dxf");

            }
            catch (Exception ex) 
            {

                throw;
            }

          
        }


        private string AddTrainImage(DxfDocument dxfDocument,double x,double y)
        {
            try
            {
                ImageDefinition imageDef = new ImageDefinition( "TrainImage", @"~\Uploads\StripChart\train.jpg");
                imageDef.ResolutionUnits = ImageResolutionUnits.Centimeters;
                //double width = imageDef.Width / imageDef.HorizontalResolution;
                //double height = imageDef.Height / imageDef.VerticalResolution;

                Image image = new Image(imageDef, new Vector2(x, y), 834, 291);
                image.Rotation = 0;
                //double xx = width / 2;
                //double yy = height /2;

                //ClippingBoundary clip = new ClippingBoundary(xx, yy, 2 * xx, 2 * yy);
                //image.ClippingBoundary = clip;


                image.DisplayOptions = ImageDisplayFlags.ShowImage;
                image.Layer = new Layer("MyImage");
                dxfDocument.AddEntity(image);
                return "";
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }

        private void PlotChainageVerticalLines(DxfDocument dxfDocument, double startChainage, double endChainage, double fromY, double toY, bool isHundred, double _HORIZ_SCALING_FACTOR, double _CHAINAGE_FACTOR)
        {

            if (isHundred)
            {
                double ans = startChainage % (100 * _CHAINAGE_FACTOR);
                startChainage = (int)(startChainage + ((100 * _CHAINAGE_FACTOR) - ans));
                do
                {
                    CreateLineWithColorThickness(dxfDocument, "#E0E0E0", Lineweight.W0, startChainage * _HORIZ_SCALING_FACTOR, fromY, startChainage * _HORIZ_SCALING_FACTOR, toY, "Vertical Chainage Lines");

                    startChainage += (100 * _CHAINAGE_FACTOR);
                } while (startChainage <= endChainage);
            }

        }

        private void PlotChinageWithMarkers(DxfDocument dxfDocument, double startChainage, double endChainage, double YPosition, bool isHundred, double _HORIZ_SCALING_FACTOR, double _CHAINAGE_FACTOR)
        {
            //PRINT VERTICAL LINE AT NEAREST HUNDRED CHAINAGE
            CreateLineWithColorThickness(dxfDocument, "#E0E0E0", Lineweight.W0, startChainage * _HORIZ_SCALING_FACTOR, YPosition, startChainage * _HORIZ_SCALING_FACTOR, YPosition + 5, "Markers-One");
            CreateTextWithColor(dxfDocument, IntToStringChainage(Convert.ToInt32(startChainage)), startChainage * _HORIZ_SCALING_FACTOR, YPosition + 10, 14, "LightGray", 90, "Chainage Text");

            if (isHundred)
            {
                double ans = (startChainage) % (100 * _CHAINAGE_FACTOR);
                startChainage = (int)((startChainage) + (100 * _CHAINAGE_FACTOR) - ans);
                do
                {
                    CreateLineWithColorThickness(dxfDocument, "#E0E0E0", Lineweight.W0, startChainage * _HORIZ_SCALING_FACTOR, YPosition, startChainage * _HORIZ_SCALING_FACTOR, YPosition + 5, "Markers-One");
                    CreateTextWithColor(dxfDocument, IntToStringChainage(Convert.ToInt32(startChainage)), startChainage * _HORIZ_SCALING_FACTOR, YPosition + 10, 14, "LightGray", 90, "Chainage Text");
                    startChainage += (100 * _CHAINAGE_FACTOR);
                } while (startChainage <= endChainage);
            }

            if (startChainage != endChainage)
            {
                CreateLineWithColorThickness(dxfDocument, "#E0E0E0", Lineweight.W0, endChainage * _HORIZ_SCALING_FACTOR, YPosition, endChainage * _HORIZ_SCALING_FACTOR, YPosition + 5, "Markers-One");
                CreateTextWithColor(dxfDocument, IntToStringChainage(Convert.ToInt32(endChainage)), endChainage * _HORIZ_SCALING_FACTOR, YPosition + 10, 14, "LightGray", 90, "Chainage Text");
            }
        }

        public string IntToStringChainage(int chainage)
        {
            return chainage.ToString("D6").Insert(chainage.ToString("D6").Length - 3, "+");
        }


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

            int countHrz = (int)((maxY - minY) / _scalingFactor);
            for (int i = 0; i <= countHrz; i++)
            {
                CreateLineWithColorThickness(dxfDocument, "#e6e6e6", Lineweight.W0, minX, minY, maxX, minY, "Grid-Horizontal");
                minY = minY + _scalingFactor;
            }

            minX = RoundValueToLower100(_minX);
            minY = RoundValueToLower100(_minY);
            maxX = RoundValueToNext100(_maxX);
            maxY = RoundValueToNext100(_maxY);

            int countVert = (int)((maxX - minX) / _scalingFactor);
            for (int i = 0; i <= countVert; i++)
            {
                CreateLineWithColorThickness(dxfDocument, "#e6e6e6", Lineweight.W0, minX, minY, minX, maxY, "Grid-Vertical");
                minX = minX + _scalingFactor;
            }

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

        private void CreateTextWithColor(DxfDocument document, string text, double x, double y, int weight, string color, double RotationDegree, string LayerName)
        {
            Layer layer = new Layer(LayerName);
            document.Layers.Add(layer);
            TextStyle style = new TextStyle("True type font", "Arial.ttf");
            //create text-element for the start point and add to the dxf
            netDxf.Entities.Text strText = new netDxf.Entities.Text(text, new Vector2(x, y), weight);
            strText.Color = GetAciColor(color);
            strText.Rotation = RotationDegree;
            strText.Layer = layer;
            strText.Lineweight = Lineweight.W15;
            strText.Style = style;
            document.AddEntity(strText);


        }



        private void CreateLineWithColorThickness(DxfDocument dxfDocument, string color, Lineweight thickness, double x1, double y1, double x2, double y2, string LayerName)
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
            lwPolyline.Lineweight = thickness;
            lwPolyline.Linetype = lineType;
            lwPolyline.Layer = layer;

            dxfDocument.AddEntity(lwPolyline);
        }


        private void CreateRectangle(DxfDocument dxfDocument, string color, Lineweight thickness, double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, string LayerName)
        {
            Layer layer = new Layer(LayerName);
            dxfDocument.Layers.Add(layer);

            List<PolylineVertex> lwPolylineVertices = new List<PolylineVertex>();
            PolylineVertex lwVertex1 = new PolylineVertex();
            PolylineVertex lwVertex2 = new PolylineVertex();
            lwPolylineVertices.Add(new PolylineVertex(x1, y1, 0));
            lwPolylineVertices.Add(new PolylineVertex(x2, y2, 0));
            lwPolylineVertices.Add(new PolylineVertex(x3, y3, 0));
            lwPolylineVertices.Add(new PolylineVertex(x4, y4, 0));
            Polyline lwPolyline = new Polyline(lwPolylineVertices);
            lwPolyline.Color = GetAciColor(color);
            lwPolyline.Lineweight = thickness;

            lwPolyline.Layer = layer;
            lwPolyline.IsClosed = true;

            dxfDocument.AddEntity(lwPolyline);
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

                return new Tuple<int, int, string>(StartC, EndC, _Status);

            }
            catch (Exception)
            {
                return new Tuple<int, int, string>(0, 0, "Exception");
            }
        }




        // End Class
    }
}