using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using RevitAPITrainingLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lab_6_1
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public List<DuctType> DuctTypes { get; }
        public DuctType SelectedDuctType { get; set; }
        public List<Level> Levels { get; }
        public Level SelectedLevel { get; set; }
        public List<XYZ> Points { get; set; }
        public double Offset { get; set; }
        public DelegateCommand SaveCommand { get; }


        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            DuctTypes = DuctUtils.GetDuctType(commandData);
            Levels = LevelsUtils.GetLevels(commandData);
            Points = SelectionUtils.GetPoints(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
            Offset = 0;
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Points.Count < 2 || SelectedDuctType == null || SelectedLevel == null)
            {
                return;
            }
            MEPSystemType systemType = new FilteredElementCollector(doc)
    .OfClass(typeof(MEPSystemType))
    .Cast<MEPSystemType>()
    .FirstOrDefault(m => m.SystemClassification == MEPSystemClassification.SupplyAir);
            using (var ts = new Transaction(doc, "Create duct"))
            {
                ts.Start();
                for (int i = 0; i < Points.Count; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }
                    XYZ previousPoint = Points[i - 1];
                    XYZ currentPoint = Points[i];
                    Duct duct =  Duct.Create(doc, systemType.Id, SelectedDuctType.Id, SelectedLevel.Id, previousPoint, currentPoint);
                    Parameter offsetParameter = duct.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM);
                    offsetParameter.Set(UnitUtils.ConvertToInternalUnits(Offset, UnitTypeId.Millimeters));
                }

                ts.Commit();
            }

            RaiseCloseRequest();
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
