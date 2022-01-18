#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Linq;
#endregion

namespace Exporter
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        #region ExportViewUtils
        public static ICollection<ViewType> GetExportableViewTypes()
        {
            return new ViewType[]
            {
            ViewType.FloorPlan,
            ViewType.CeilingPlan,
            ViewType.Section,
            ViewType.Elevation,
            ViewType.Detail,
            ViewType.AreaPlan,
            ViewType.EngineeringPlan
            };
        }

        bool isExportableView(Autodesk.Revit.DB.View view)
        {
            if (!view.CanBePrinted || view.IsTemplate)
                return false;

            ICollection<ViewType> exportableTypes = GetExportableViewTypes();
            if (!exportableTypes.Contains(view.ViewType))
                return false;

            return true;
        }

        private static void ExportView(Autodesk.Revit.DB.View exportableView,
                                       DisplayStyle displayStyle,
                                       bool includeGeometricObjects,
                                       bool export2DIncludingAnnotationObjects,
                                       bool export2DGeometricObjectsIncludingPatternLines,
                                       out IList<XYZ> points,
                                       out ResultsSummary resultsSummary)
        {
            TessellatedGeomAndText2DExportContext context = new TessellatedGeomAndText2DExportContext(out points);
            CustomExporter exporter = new CustomExporter(exportableView.Document, context);
            exporter.IncludeGeometricObjects = includeGeometricObjects;
            exporter.Export2DIncludingAnnotationObjects = export2DIncludingAnnotationObjects;
            exporter.Export2DGeometricObjectsIncludingPatternLines = export2DGeometricObjectsIncludingPatternLines;
            exporter.ShouldStopOnError = true;
            exporter.Export(exportableView);
            exporter.Dispose();

            resultsSummary = new ResultsSummary();
            resultsSummary.numElements = context.NumElements;
            resultsSummary.numTexts = context.NumTexts;
            resultsSummary.texts = context.Texts;
        }
        #endregion

        #region ResultsUtils
        /// <summary>
        /// Class that aggregates the results of the export.
        /// </summary>
        class ResultsSummary
        {
            public int numElements { get; set; }
            public int numTexts { get; set; }
            public string texts { get; set; }

            public ResultsSummary()
            {
            }
        }

        /// <summary>
        /// Displays the results from a run of path of travel creation using a TaskDialog.
        /// </summary>
        /// <param name="resultsSummary"></param>
        private static void ShowResults(ResultsSummary resultsSummary)
        {
            TaskDialog td = new TaskDialog("Results of 2D export");
            td.MainInstruction = String.Format("2D exporter exported {0} elements", resultsSummary.numElements);
            String details = String.Format("There were {0} text nodes exported.\n\n",
                                            resultsSummary.numTexts);

            if (resultsSummary.numTexts > 0 && resultsSummary.texts.Length > 0)
                details += "Exported text nodes:\n" + resultsSummary.texts;

            td.MainContent = details;

            td.Show();


        }
        #endregion

        /// <summary>
        /// Implement this method as an external command for Revit.
        /// </summary>
        /// <param name="commandData">An object that is passed to the external application 
        /// which contains data related to the command, 
        /// such as the application object and active view.</param>
        /// <param name="message">A message that can be set by the external application 
        /// which will be displayed if a failure or cancellation is returned by 
        /// the external command.</param>
        /// <param name="elements">A set of elements to which the external application 
        /// can add elements that are to be highlighted in case of failure or cancellation.</param>
        /// <returns>Return the status of the external command. 
        /// A result of Succeeded means that the API external method functioned as expected. 
        /// Cancelled can be used to signify that the user cancelled the external operation 
        /// at some point. Failure should be returned if the application is unable to proceed with 
        /// the operation.</returns>
        public virtual Result Execute(ExternalCommandData commandData
            , ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;
                Autodesk.Revit.DB.View activeView = uiDoc.ActiveView;
                if (!isExportableView(activeView))
                {
                    TaskDialog td = new TaskDialog("Cannot export view.");
                    td.MainInstruction = String.Format("Only plans, elevations and sections can be exported.");

                    td.Show();

                    return Result.Succeeded;
                }

                using (Export2DView exportForm = new Export2DView())
                {
                    if (DialogResult.OK == exportForm.ShowDialog())
                    {
                        IList<XYZ> points = null;
                        ResultsSummary resSummary = null;
                        ExportView(activeView,
                                   activeView.DisplayStyle /*display with current display style*/,
                                   true /* always export some geometry */,
                                   exportForm.ViewExportOptions.ExportAnnotationObjects,
                                   exportForm.ViewExportOptions.ExportPatternLines,
                                   out points,
                                   out resSummary);

                        Utilities.displayExport(activeView, points);

                        string PointsString = "";
                        int count = 0;
                        foreach(var p in points)
                        {
                            if (count == 0)
                            {
                                PointsString = p.X.ToString()+","+ p.Y.ToString() + "," + p.Z.ToString();
                            }
                            else
                            {
                                PointsString = PointsString + "__S__"+ p.X.ToString() + "," + p.Y.ToString() + "," + p.Z.ToString();
                            }
                            count++;
                        }
                        //Pass the filepath and filename to the StreamWriter Constructor
                        StreamWriter sw = new StreamWriter("C:\\Users\\h\\Desktop\\Points.txt");
                        //Write a line of text
                        sw.WriteLine(PointsString);
                        //Close the file
                        sw.Close();


                        var wall = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().FirstOrDefault();
                        //Pass the filepath and filename to the StreamWriter Constructor
                        string WID = wall.Id.ToString();
                        string wallty = (wall as Wall).WallType.Name;
                        string wallLength = wall.LookupParameter("Length").AsDouble().ToString();
                        string wallWidth = (wall as Wall).WallType.LookupParameter("Width").AsDouble().ToString();
                        string wallFire = wall.LookupParameter("Fire Rate").AsString();
                        string ParametersString = WID + "__S__" + wallty + "__S__" + wallLength + "__S__" + wallWidth + "__S__" + wallFire;
                        StreamWriter sww = new StreamWriter("C:\\Users\\h\\Desktop\\Parameters.txt");
                        //Write a line of text
                        sww.WriteLine(ParametersString);
                        //Close the file
                        sww.Close();

                        ShowResults(resSummary);
                    }
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }

}
