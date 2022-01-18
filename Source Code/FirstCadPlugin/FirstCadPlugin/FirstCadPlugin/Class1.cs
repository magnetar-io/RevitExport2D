using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Internal.DatabaseServices;
using System.IO;

namespace FirstCadPlugin
{
    public class Class1
    {
        [CommandMethod("CB")]

        public void CreateBlock()

        {

            Document doc =

              Application.DocumentManager.MdiActiveDocument;

            Database db = doc.Database;

            Editor ed = doc.Editor;



            Transaction tr =

              db.TransactionManager.StartTransaction();

            using (tr)

            {

                // Get the block table from the drawing



                BlockTable bt =

                  (BlockTable)tr.GetObject(

                    db.BlockTableId,

                    OpenMode.ForRead

                  );



                // Check the block name, to see whether it's

                // already in use



                PromptStringOptions pso =

                  new PromptStringOptions(

                    "\nEnter new block name: "

                  );

                pso.AllowSpaces = true;



                // A variable for the block's name



                string blkName = "";



                do

                {

                    PromptResult pr = ed.GetString(pso);



                    // Just return if the user cancelled

                    // (will abort the transaction as we drop out of the using

                    // statement's scope)



                    if (pr.Status != PromptStatus.OK)

                        return;



                    try

                    {

                        // Validate the provided symbol table name



                        SymbolUtilityServices.ValidateSymbolName(

                          pr.StringResult,

                          false

                        );



                        // Only set the block name if it isn't in use



                        if (bt.Has(pr.StringResult))

                            ed.WriteMessage(

                              "\nA block with this name already exists."

                            );

                        else

                            blkName = pr.StringResult;

                    }

                    catch

                    {

                        // An exception has been thrown, indicating the

                        // name is invalid



                        ed.WriteMessage(

                          "\nInvalid block name."

                        );

                    }



                } while (blkName == "");



                // Create our new block table record...



                BlockTableRecord btr = new BlockTableRecord();



                // ... and set its properties



                btr.Name = blkName;
                
                

                // Add the new block to the block table



                bt.UpgradeOpen();
                

                ObjectId btrId = bt.Add(btr);

                tr.AddNewlyCreatedDBObject(btr, true);



                // Add some lines to the block to form a square

                // (the entities belong directly to the block)



                DBObjectCollection ents = SquareOfLines(5);

                foreach (Entity ent in ents)

                {

                    btr.AppendEntity(ent);

                    tr.AddNewlyCreatedDBObject(ent, true);

                }
                // Add an attribute definition to the block
                using (AttributeDefinition acAttDef = new AttributeDefinition())
                {
                    acAttDef.Position = new Point3d(0, 0, 0);
                    acAttDef.Verifiable = true;
                    acAttDef.Tag = "Wall_Id";
                    acAttDef.TextString = "165465";
                    acAttDef.Height = 1;
                    acAttDef.Justify = AttachmentPoint.MiddleCenter;
                    acAttDef.Invisible = true;
                    btr.AppendEntity(acAttDef);
                }
                using (AttributeDefinition acAttDef = new AttributeDefinition())
                {
                    acAttDef.Position = new Point3d(0, 0, 0);
                    acAttDef.Verifiable = true;
                    acAttDef.Tag = "Wall_Type";
                    acAttDef.TextString = "W5";
                    acAttDef.Height = 1;
                    acAttDef.Justify = AttachmentPoint.MiddleCenter;
                    acAttDef.Invisible = true;
                    btr.AppendEntity(acAttDef);
                }
                using (AttributeDefinition acAttDef = new AttributeDefinition())
                {
                    acAttDef.Position = new Point3d(0, 0, 0);
                    acAttDef.Verifiable = true;
                    acAttDef.Tag = "Wall_Length";
                    acAttDef.TextString = "30";
                    acAttDef.Height = 1;
                    acAttDef.Justify = AttachmentPoint.MiddleCenter;
                    acAttDef.Invisible = true;
                    btr.AppendEntity(acAttDef);
                }
                using (AttributeDefinition acAttDef = new AttributeDefinition())
                {
                    acAttDef.Position = new Point3d(0, 0, 0);
                    acAttDef.Verifiable = true;
                    acAttDef.Tag = "Wall_Width";
                    acAttDef.TextString = "0.25";
                    acAttDef.Height = 1;
                    acAttDef.Justify = AttachmentPoint.MiddleCenter;
                    acAttDef.Invisible = true;
                    btr.AppendEntity(acAttDef);
                }
                using (AttributeDefinition acAttDef = new AttributeDefinition())
                {
                    acAttDef.Position = new Point3d(0, 0, 0);
                    acAttDef.Verifiable = true;
                    acAttDef.Tag = "Wall_Fire_Rate";
                    acAttDef.TextString = "FR5";
                    acAttDef.Height = 1;
                    acAttDef.Justify = AttachmentPoint.MiddleCenter;
                    acAttDef.Invisible = true;
                    btr.AppendEntity(acAttDef);
                }

                // Add a block reference to the model space



                /*  BlockTableRecord ms =

                    (BlockTableRecord)tr.GetObject(

                      bt[BlockTableRecord.ModelSpace],

                      OpenMode.ForWrite

                    );



                  BlockReference br =

                    new BlockReference(Point3d.Origin, btrId);



                  ms.AppendEntity(br);

                  tr.AddNewlyCreatedDBObject(br, true);
                  */




                // Commit the transaction

                // Insert the block into the current space
                var blkRecId = btr.Id;
                if (blkRecId != ObjectId.Null)
                {
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = tr.GetObject(blkRecId, OpenMode.ForRead) as BlockTableRecord;

                    // Create and insert the new block reference
                    using (BlockReference acBlkRef = new BlockReference(new Point3d(2, 2, 0), blkRecId))
                    {
                        BlockTableRecord acCurSpaceBlkTblRec;
                        acCurSpaceBlkTblRec = (BlockTableRecord)tr.GetObject(

                      bt[BlockTableRecord.ModelSpace],

                      OpenMode.ForWrite

                    );

                        acCurSpaceBlkTblRec.AppendEntity(acBlkRef);
                        tr.AddNewlyCreatedDBObject(acBlkRef, true);
                        //Pass the file path and file name to the StreamReader constructor
                        StreamReader sr = new StreamReader("C:\\Users\\h\\Desktop\\Parameters.txt");
                        //Read the first line of text
                        string line = sr.ReadLine();
                        //close the file
                        sr.Close();
                        var Parameters = GetFirstTermTwo(line);
                        // Verify block table record has attribute definitions associated with it
                        if (acBlkTblRec.HasAttributeDefinitions)
                        {
                            // Add attributes from the block table record
                            foreach (ObjectId objID in acBlkTblRec)
                            {
                                DBObject dbObj = tr.GetObject(objID, OpenMode.ForRead) as DBObject;

                                if (dbObj is AttributeDefinition)
                                {
                                    AttributeDefinition acAtt = dbObj as AttributeDefinition;

                                    if (!acAtt.Constant)
                                    {
                                        using (AttributeReference acAttRef = new AttributeReference())
                                        {
                                            acAttRef.SetAttributeFromBlock(acAtt, acBlkRef.BlockTransform);
                                            acAttRef.Position = acAtt.Position.TransformBy(acBlkRef.BlockTransform);
                                            if (acAtt.Tag == "Wall_Id")
                                            {
                                                acAttRef.TextString = Parameters[0];
                                            }
                                            else if (acAtt.Tag == "Wall_Type")
                                            {
                                                acAttRef.TextString = Parameters[1];
                                            }
                                            else if (acAtt.Tag == "Wall_Length")
                                            {
                                                acAttRef.TextString = Parameters[2];
                                            }
                                            else if (acAtt.Tag == "Wall_Width")
                                            {
                                                acAttRef.TextString = Parameters[3];
                                            }
                                            else if (acAtt.Tag == "Wall_Fire_Rate")
                                            {
                                                acAttRef.TextString = Parameters[4];
                                            }


                                            acBlkRef.AttributeCollection.AppendAttribute(acAttRef);

                                            tr.AddNewlyCreatedDBObject(acAttRef, true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                    tr.Commit();



                // Report what we've done



                ed.WriteMessage(

                  "\nCreated block named \"{0}\" containing {1} entities.",

                  blkName, ents.Count

                );

            }
     /*       Transaction trr =

  db.TransactionManager.StartTransaction();

            using (trr)
            {
                PromptResult rs = ed.GetString("square");
                ObjectId idBTR = ObjectId.Null, idBref;
                using (BlockTable btrr = db.BlockTableId.Open(OpenMode.ForRead) as BlockTable)
                {
                    if (btrr != null) idBTR = btrr[rs.StringResult];
                }
                BlockTableRecord btrRead = trr.GetObject(idBTR, OpenMode.ForRead) as BlockTableRecord;
                using (btrRead)
                {
                    var btrSpace = trr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                    BlockPropertiesTable Bp = BlockAction.Create( , IntPtr.Zero, false);
                    DBObjectCollection entss = new DBObjectCollection();
                    entss.Add(Bp);

                    foreach (Entity ent in entss)

                    {

                        btrSpace.AppendEntity(ent);

                        trr.AddNewlyCreatedDBObject(ent, true);

                    }
                }

                trr.Commit();
            }*/


        }

        public static int SelectRowNumber(ref BlockPropertiesTable bpt)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            int columns = bpt.Columns.Count;
            int rows = bpt.Rows.Count;
            int currentRow = 0, currentColumn = 0;
            ed.WriteMessage("\n");
            for (currentColumn = 0; currentColumn < columns; currentColumn++)
            {
                ed.WriteMessage("{0}; ", bpt.Columns[currentColumn].Parameter.Name);
            }
            foreach (BlockPropertiesTableRow row in bpt.Rows)
            {
                ed.WriteMessage("\n[{0}]:\t", currentRow);
                for (currentColumn = 0; currentColumn < columns; currentColumn++)
                {
                    TypedValue[] columnValue = row[currentColumn].AsArray();
                    foreach (TypedValue tpVal in columnValue)
                    {
                        ed.WriteMessage("{0}; ", tpVal.Value);
                    }
                    ed.WriteMessage("|");
                }
                currentRow++;
            }

            PromptIntegerResult res;
            string.Format("0-{0}", rows - 1);

            while ((res = ed.GetInteger(string.Format("\nSelect row number (0-{0}): ", rows - 1))).Status == PromptStatus.OK)
            {
                if (res.Value >= 0 && res.Value <= rows) return res.Value;
            }
            return -1;
        }


        private DBObjectCollection SquareOfLines(double size)

        {

            // A function to generate a set of entities for our block

            //Pass the file path and file name to the StreamReader constructor
            StreamReader sr = new StreamReader("C:\\Users\\h\\Desktop\\Points.txt");
            //Read the first line of text
            string line = sr.ReadLine();
            //close the file
            sr.Close();
            var Points=GetFirstTermTwo(line);
            

            DBObjectCollection ents = new DBObjectCollection();

            List<Point3d> pts = new List<Point3d>();
            foreach (var pt in Points)
            {
                var Coords = GetFirstTerm(pt);
                pts.Add(new Point3d(Convert.ToDouble(Coords[0]), Convert.ToDouble(Coords[1]), 0));
            }

            //int max = pts.GetUpperBound(0);
            int count = 0;
            for (int i= 0;i<((pts.Count/2)+1);i++)
            {
                if(count != pts.Count)
                {
                    Line ln = new Line(pts[count], pts[count+1]);

                    ents.Add(ln);
                }
                count=count+2;
            }



            return ents;

        }


        private List<string> GetFirstTermTwo(string field, string separator = "__S__")
        {
            List<string> x = new List<string>();
            if (!(field == null || field == ""))
            {
                foreach (var i in field.Split(new[] { "__S__" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    x.Add(i);
                }
            }

            return x;
        }

        private List<string> GetFirstTerm(string field, char separator = ',')
        {
            List<string> x = new List<string>();
            if (!(field == null || field == ","))
            {
                foreach (var i in field.Split(separator))
                {
                    x.Add(i);
                }
            }

            return x;
        }
    }
}
