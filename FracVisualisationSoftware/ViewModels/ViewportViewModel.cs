﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using FracVisualisationSoftware.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using HelixToolkit.Wpf;
using MahApps.Metro.Controls.Dialogs;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;

namespace FracVisualisationSoftware.ViewModels
{
    public class ViewportViewModel : ViewModelBase
    {
        #region fields

        #region injected fields

        private IDialogCoordinator _dialogCoordinator;

        #endregion injected fields

        private Point3DCollection _tubePath;

        private double _tubeLength;
        private double _tubeDiameter;

        #endregion fields

        #region properties

        public double TubeLength
        {
            get => _tubeLength;
            set { _tubeLength = value; RaisePropertyChanged(); }
        }

        public double TubeDiameter
        {
            get => _tubeDiameter;
            set { _tubeDiameter = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<Visual3D> ViewportObjects { get; set; }

        public Camera ViewPortCamera { get; set; }

        #endregion properties

        #region constructor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public ViewportViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            ViewportObjects = new ObservableCollection<Visual3D>();

            _tubePath = new Point3DCollection();

            TubeDiameter = 5;

            HelixViewport3DLoadedCommand = new RelayCommand<HelixViewport3D>(HelixViewport3DLoadedAction); //TODO Get reference to the ViewPort so we can manipulate the camera

            MessengerInstance.Register<BoreholeModel>(this, "Borehole Data Added",AddBoreholeMessageCallback);
        }

        #endregion constructor

        #region commands

        public ICommand HelixViewport3DLoadedCommand { get; }

        #endregion commands 

        #region methods

        #region command methods

        private void HelixViewport3DLoadedAction(HelixViewport3D viewPort)
        {
            ViewPortCamera.Position = new Point3D(0,0,0);
        }

        #endregion command methods

        #region event methods

        private void AddBoreholeMessageCallback(BoreholeModel boreholeModel)
        {
            if (boreholeModel == null || !boreholeModel.TubePath.Any())
                return;

            _tubePath.Dispatcher?.Invoke(() =>
            {

                _tubePath = new Point3DCollection(boreholeModel.TubePath);
                //ViewportObjects.Clear();

                ViewportObjects.Add(new SunLight());

                ViewportObjects.Add(new TubeVisual3D { AddCaps = true, Path = _tubePath, Diameter = TubeDiameter });

                _tubePath.Clear();
            });
        }

        #endregion event methods

        #endregion methods

    }
}
