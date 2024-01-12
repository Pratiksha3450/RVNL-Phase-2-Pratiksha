using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Data;

namespace RVNLMIS.Controllers
{
    public class AutocadController : Controller
    {
        // GET: Autocad
        public ActionResult Index()
        {
            Baseline();
            return View();
        }

        private void Baseline()
        {
            //Get Section Details
            DataTable dtSection = new DataTable();
            dtSection = GetSections();

            //CREATE FILES
            DxfDocument dxfDocument = new DxfDocument();

            //CREATE LAYER
            Layer layer = new Layer("Layer-01");
            dxfDocument.Layers.Add(layer);
            layer.Lineweight = Lineweight.W5;
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
            layer3.Lineweight = Lineweight.W13;
            string color3 = Convert.ToString("Cyan");
            ApplyColorToLayer(layer3, color3);

            double sectionStart = 0;
            double sectionEnd = 0;
            int count = dtSection.Rows.Count;

            sectionStart = Convert.ToDouble(dtSection.Rows[0]["StartChainage"].ToString().Trim().Replace("+", "").ToString());
            sectionEnd = Convert.ToDouble(dtSection.Rows[count - 1]["EndChainage"].ToString().Trim().Replace("+", "").ToString());

            //GET OGL
            DataTable dtOGL = GetOGL();
            double topHeight = Convert.ToDouble(dtOGL.Compute("min([Height])", string.Empty));
            //FACTOR FOR OGL PRINTING
            double baselineHeight = 1000;

            double scalingFactor = 200;
            double lineHeight = baselineHeight - 300;

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
                CreateLine(dxfDocument, layer, startX, baselineHeight, endX, baselineHeight);

                //Main Line Two
                CreateLine(dxfDocument, layer, startX, totalHeight, endX, totalHeight);

                //MainLine - End Chainage Vertical Marks
                CreateLine(dxfDocument, layer2, endX, baselineHeight - 250, endX, baselineHeight + 150);
                CreateText(dxfDocument, endChainage, endX, baselineHeight + 50, 50, layer2, 90);

                if (i == 0)
                {
                    CreateLine(dxfDocument, layer2, startX, baselineHeight - 250, startX, baselineHeight + 150);
                    CreateText(dxfDocument, startChainage, startX, baselineHeight + 50, 50, layer2, 90);
                }
                //Print Section Name
                string strSectionName = dtSection.Rows[i]["SectionName"].ToString().Trim();
                CreateText(dxfDocument, strSectionName, startX + 50, baselineHeight + 50, 50, layer2, 0);
            }


            //Get Entities
            DataTable dtEntities = new DataTable();
            dtEntities = GetEntities();
            for (int i = 0; i < dtEntities.Rows.Count; i++)
            {
                string startChainage = dtEntities.Rows[i]["StartChainage"].ToString().Trim();
                string endChainage = dtEntities.Rows[i]["EndChainage"].ToString().Trim();

                double startX = Convert.ToDouble(startChainage.Replace("+", "").ToString());
                double endX = Convert.ToDouble(endChainage.Replace("+", "").ToString());


                //MainLine - End Chainage Vertical Marks
                CreateLine(dxfDocument, layer3, startX, baselineHeight - 15, startX, baselineHeight + 150);

                if ((endX - startX) > 25)
                    CreateLine(dxfDocument, layer3, endX, baselineHeight - 15, endX, baselineHeight + 150);

                CreateText(dxfDocument, startChainage, startX, baselineHeight + 200, 50, layer3, 90);


                //Print Entity Name
                string strEntityName = dtEntities.Rows[i]["EntityName"].ToString().Trim();
                CreateText(dxfDocument, strEntityName, startX, baselineHeight + 550, 50, layer3, 90);
            }

            //PRINT VERTICAL LINE AT NEAREST HUNDRED CHAINAGE
            List<double> listHundred = new List<double>();


            double ans = sectionStart % 100;
            sectionStart = sectionStart + (100 - ans);
            listHundred.Add(sectionStart);

            do
            {
                sectionStart += 100;
                listHundred.Add(sectionStart);
                CreateLine(dxfDocument, layer3, sectionStart, baselineHeight - 250, sectionStart, baselineHeight);
                CreateText(dxfDocument, sectionStart.ToString(), sectionStart, baselineHeight - 250, 15, layer3, 90);
            } while (sectionStart < sectionEnd);





            //PLOT OGL
            for (int i = 0; i < dtOGL.Rows.Count - 1; i++)
            {
                double xOne = Convert.ToDouble(dtOGL.Rows[i]["Chainage"].ToString().Trim().Replace("+", "").ToString());
                double heightOne = Convert.ToDouble(dtOGL.Rows[i]["Height"].ToString().Trim().Replace("+", "").ToString());

                double xTwo = Convert.ToDouble(dtOGL.Rows[i + 1]["Chainage"].ToString().Trim().Replace("+", "").ToString());
                double heightTwo = Convert.ToDouble(dtOGL.Rows[i + 1]["Height"].ToString().Trim().Replace("+", "").ToString());

                CreateLine(dxfDocument, layer3, xOne, totalHeight - (heightOne * scalingFactor), xTwo, totalHeight - (heightTwo * scalingFactor));
            }

            //CREATE DOCUMENT NAME HERE 
            string newDxfFile = Server.MapPath(@"../Uploads/" + "test123.dxf");
            //SAVE GENERATED  AUTOCAD FDRAWING
            dxfDocument.Save(newDxfFile);

           // lblMessage.Text = "File Generation Completed";
            //SAVE IT IN SESSION SO THAT THE LATEST DRAWING IS OPENED
            Session["NEW-DRAWING"] = newDxfFile;

            //TRANSFER IT TO CAD VIEWER
            //Server.Transfer("cadviewer.aspx");
        }

        private DataTable GetOGL()
        {
            string ConnectionString = "Data Source=188.138.1.54,9999; database=dbDevPBI;User ID=userDev;Password=Asdlkj@123";
            DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, "SELECT [PackageId],[Chainage],[OGL],[FRL] ,[FormLvl] ,[FRL]-[OGL] AS Height FROM [dbDevPBI].[dbo].[tblPkgChngLvl] WHERE PackageId = 3 order by chainage ");
            return dsDataset.Tables[0];
        }

        private DataTable GetEntities()
        {
            string ConnectionString = "Data Source=188.138.1.54,9999; database=dbDevPBI;User ID=userDev;Password=Asdlkj@123";
            DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, "SELECT [EntityName],[EntityType],[StartChainage],[EndChainage] FROM [dbDevPBI].[dbo].[tblMasterEntity] WHERE PackageId = 3 ");
            return dsDataset.Tables[0];
        }

        private void CreateText(DxfDocument document, string text, double x, double y, int weight, Layer layer, double RotationDegree)
        {
            //create text-element for the start point and add to the dxf
            netDxf.Entities.Text strText = new netDxf.Entities.Text(text, new Vector2(x, y), weight);
            strText.Layer = layer;
            strText.Rotation = RotationDegree;
            document.AddEntity(strText);
        }

        private void CreateLine(DxfDocument document, Layer layer, double x1, double y1, double x2, double y2)
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

        private DataTable GetSections()
        {
            string ConnectionString = "Data Source=188.138.1.54,9999; database=dbDevPBI;User ID=userDev;Password=Asdlkj@123";
            DataSet dsDataset = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, "SELECT [SectionID],[ProjectId] ,[PackageId],[SectionName],[SectionCode],[StartChainage],[EndChainage],[IsDeleted],[Length] FROM [dbDevPBI].[dbo].[tblSection] WHERE PackageId = 3 ");
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
           // lblMessage.Text = "File Generation Completed";
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

    }
}