﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using ReservoirVisualisationProject.Enums;
using ReservoirVisualisationProject.Extensions;
using ReservoirVisualisationProject.Models;
using ReservoirVisualisationProject.Models.Readers.Excel;
using MahApps.Metro.Controls.Dialogs;
using OfficeOpenXml;

namespace ReservoirVisualisationProject.ViewModels.Readers.Excel
{
    public class ExcelPathReaderViewModel : ExcelReaderViewModelBase
    {
        #region fields

        #region injected fields

        #endregion injected fields

        private string _nameText;

        private string _xColumnHeading; //Easting
        private bool? _xColumnFound;

        private string _yColumnHeading; //TVD
        private bool? _yColumnFound;

        private string _zColumnHeading; //Northing
        private bool? _zColumnFound;

        #endregion fields

        #region properties

        public string NameText
        {
            get => _nameText;
            set { _nameText = value; RaisePropertyChanged(); }
        }

        public string XColumnHeading
        {
            get { return _xColumnHeading; }
            set
            {
                _xColumnHeading = value?.ToUpper();
                XColumnFound = HeadingValidation(_xColumnHeading);
                RaisePropertyChanged();
            }
        }

        public bool? XColumnFound
        {
            get { return _xColumnFound; }
            set { _xColumnFound = value; RaisePropertyChanged(() => XColumnFound); }
        }

        public string YColumnHeading
        {
            get { return _yColumnHeading; }
            set
            {
                _yColumnHeading = value?.ToUpper();
                YColumnFound = HeadingValidation(_yColumnHeading);
                RaisePropertyChanged();
            }
        }

        public bool? YColumnFound
        {
            get { return _yColumnFound; }
            set { _yColumnFound = value; RaisePropertyChanged(() => YColumnFound); }
        }

        public string ZColumnHeading
        {
            get { return _zColumnHeading; }
            set
            {
                _zColumnHeading = value?.ToUpper();
                ZColumnFound = HeadingValidation(_zColumnHeading);
                RaisePropertyChanged();
            }
        }

        public bool? ZColumnFound
        {
            get { return _zColumnFound; }
            set { _zColumnFound = value; RaisePropertyChanged(() => ZColumnFound); }
        }

        #endregion properties

        #region constructor

        public ExcelPathReaderViewModel(IDialogCoordinator dialogCoordinator) : base(dialogCoordinator)
        {
            DialogHostIdentifier = "ExcelPathDialogHost";
        }

        #endregion constructor

        #region methods

        protected override void SetupMessengerInstance()
        {
            MessengerInstance.Register<FlyoutMessageModel>(this, (FileTypeEnum.Excel, WellDataTypeEnum.Path), LoadExcelFileCallback);
        }

        protected override void ResetProperties()
        {
            base.ResetProperties();
            NameText = null;
            XColumnHeading = null;
            YColumnHeading = null;
            ZColumnHeading = null;
        }

        #region command methods

        protected override bool CanReadExcelFileAction()
        {
            if (XColumnFound != null && YColumnFound != null && ZColumnFound != null && !string.IsNullOrWhiteSpace(NameText))
            {
                return (XColumnFound.Value && YColumnFound.Value && ZColumnFound.Value);
            }

            return false;
        }

        protected override async void ReadExcelFileAction()
        {
            ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Please wait...", "Finding headings...");
            progressDialogController.Maximum = 100;

            await Task.Run(() =>
            {
                IEnumerable<ExcelRangeBase> xColumn = _excelUsedRange.Single(cell => cell.Address == XColumnHeading);

                progressDialogController.SetProgress(33);

                IEnumerable<ExcelRangeBase> yColumn = _excelUsedRange.Single(cell => cell.Address == YColumnHeading);

                progressDialogController.SetProgress(66);

                IEnumerable<ExcelRangeBase> zColumn = _excelUsedRange.Single(cell => cell.Address == ZColumnHeading);

                progressDialogController.SetMessage("Headings found...");
                progressDialogController.SetProgress(100);

                List<Point3D> tubePath = new List<Point3D>();

                int numberOfRows = _excelWorksheet.Dimension.Rows;

                progressDialogController.SetMessage("Beginning to read Worksheet...");
                progressDialogController.SetProgress(0);
                progressDialogController.Maximum = numberOfRows;

                for (int currentRow = xColumn.First().Start.Row; currentRow < numberOfRows; currentRow++)
                {
                    object xCellValue = _excelWorksheet.Cells[currentRow, xColumn.First().Start.Column].Value;
                    object yCellValue = _excelWorksheet.Cells[currentRow, yColumn.First().Start.Column].Value;
                    object zCellValue = _excelWorksheet.Cells[currentRow, zColumn.First().Start.Column].Value;

                    if (xCellValue.IsNumeric() && yCellValue.IsNumeric() && zCellValue.IsNumeric() && FilterRow(currentRow))
                    {
                        double x = (double) xCellValue;
                        double y = (double) yCellValue;
                        double z = (double) zCellValue;

                        tubePath.Add(new Point3D(x, y, z));
                    }

                    progressDialogController.SetMessage($"Reading row {currentRow} of {numberOfRows}...");
                    progressDialogController.SetProgress(currentRow);
                }

                WellModel wellModel = new WellModel
                {
                    ID = 0,
                    Name = NameText,
                    Path = new ObservableCollection<Point3D>(tubePath)
                };

                MessengerInstance.Send(wellModel, MessageTokenStrings.AddPathToManager);
            });

            await progressDialogController.CloseAsync();
        }

        #endregion command methods

        #endregion methods

    }
}
