using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NevronZoom
{
    public partial class MainWindow
    {
        private Nevron.Chart.NCartesianChart m_chart;
        private Nevron.Chart.NPointSeries [] m_seriesList;

        public MainWindow ( )
        {
            InitializeComponent ( );

            InitChart ( );
            InitializeAxis ( false );
            CreateSeries ( );
            AddData ( );
        }

        private void InitChart ( )
        {
            //Core.Nevron.License.Ensure ( );

            chartContainer.BackgroundStyle.FrameStyle.Visible = false;

            m_chart = new Nevron.Chart.NCartesianChart ( );
            m_chart.Margins = new Nevron.GraphicsCore.NMarginsL ( 0 );
            m_chart.Padding = new Nevron.GraphicsCore.NMarginsL ( 0 );
            m_chart.DockMode = Nevron.Chart.PanelDockMode.Fill;
            m_chart.DockMargins = new Nevron.GraphicsCore.NMarginsL ( 0 );
            m_chart.Location = new Nevron.GraphicsCore.NPointL ( 0, 0 );
            m_chart.BoundsMode = Nevron.GraphicsCore.BoundsMode.Stretch;

            chartContainer.Charts.Add ( m_chart );

            m_chart.Projection.SetPredefinedProjection ( Nevron.GraphicsCore.PredefinedProjection.OrthogonalHorizontalLeft );
            m_chart.BoundsMode = Nevron.GraphicsCore.BoundsMode.Fit;
            m_chart.UsePlotAspect = true;

            //m_chart.RangeSelections.Add ( new Nevron.Chart.NRangeSelection ( ) );
            //chartContainer.Controller.Tools.Add ( new NevronPlanChartMultiPointSelectTool ( m_chart ) );

            Nevron.Chart.NRangeSelection rs = new Nevron.Chart.NRangeSelection ( m_chart.Axis ( Nevron.Chart.StandardAxis.PrimaryX ).AxisId, m_chart.Axis ( Nevron.Chart.StandardAxis.PrimaryY ).AxisId );
            m_chart.RangeSelections.Clear ( );
            m_chart.RangeSelections.Add ( rs );

            chartContainer.Controller.Selection.Clear ( );
            chartContainer.Controller.Selection.Add ( m_chart );

            // ensure the chart is selected or mouse zoom will never work
            chartContainer.Controller.Selection.SelectedObjects.Add ( chartContainer.Charts [ 0 ] );

            chartContainer.Controller.Tools.Clear ( );

            chartContainer.Controller.Tools.Add ( new Nevron.Chart.Windows.NSelectorTool { Focus = true } );
            chartContainer.Controller.Tools.Add ( new Nevron.Chart.Windows.NDataZoomTool
            {
                WheelZoomAtMouse = true,
                AnimateZooming = true,
                AnimationTime = 333,
                AnimationDurationType = Nevron.Chart.AnimationDurationType.MaxTime,
                BeginDragMouseCommand = new Nevron.Chart.Windows.NMouseCommand ( Nevron.Chart.Windows.MouseAction.Wheel, Nevron.Chart.Windows.MouseButton.Middle, 0 ),
                //ZoomStep = 16
            } );
        }

        private void CreateSeries ( )
        {
            m_seriesList = new Nevron.Chart.NPointSeries [ 3 ];

            for ( int i = 0 ; i < 3 ; i++ )
            {
                //var seriesModel = ViewModel.SeriesList [ i ];
                var series = new Nevron.Chart.NPointSeries ( );
                m_seriesList [ i ] = series;
                //m_seriesByPointType [ seriesModel.Type ] = new FullSeries ( series, seriesModel );

                series.DataLabelStyle.Visible = false;
                series.PointShape = Nevron.Chart.PointShape.Bar;
                series.BorderStyle.Width = new Nevron.GraphicsCore.NLength ( 0 );
                series.Size = new Nevron.GraphicsCore.NLength ( 2f );
                series.UseXValues = true;
                series.FillStyle = new Nevron.GraphicsCore.NColorFillStyle ( i % 3 == 0 ? System.Drawing.Color.Red : i % 2 == 0 ? System.Drawing.Color.Green : System.Drawing.Color.Blue );
                series.Name = "Series " + i;
                series.Visible = true;

                m_chart.Series.Add ( series );
            }
        }

        private void InitializeAxis ( bool reset )
        {
            var xAxis = m_chart.Axis ( Nevron.Chart.StandardAxis.PrimaryX );
            xAxis.Scale.Reset ( );
            xAxis.PagingView.Reset ( );

            var yAxis = m_chart.Axis ( Nevron.Chart.StandardAxis.PrimaryY );
            yAxis.Scale.Reset ( );
            yAxis.PagingView.Reset ( );

            if ( !reset )
            {
                // setup X axis
                Nevron.Chart.NLinearScaleConfigurator scaleX = new Nevron.Chart.NLinearScaleConfigurator ( );
                scaleX.MajorGridStyle.ShowAtWalls = new [] { Nevron.Chart.ChartWallType.Back };
                scaleX.MajorGridStyle.LineStyle.Pattern = Nevron.GraphicsCore.LinePattern.Dot;
                xAxis.ScaleConfigurator = scaleX;

                //OLD did this:
                //Nevron.Chart.NNumericAxisPagingView xPagingView = new Nevron.Chart.NNumericAxisPagingView ( new Nevron.GraphicsCore.NRange1DD ( -50, 10200 ) );
                //xPagingView.MinPageLength = 1.0;
                //xAxis.PagingView = xPagingView;
                //xAxis.View = new Nevron.Chart.NRangeAxisView ( new Nevron.GraphicsCore.NRange1DD ( 0, 200 ) );

                // this fits proper to content
                xAxis.View = new Nevron.Chart.NContentAxisView ( );

                // setup Y axis
                Nevron.Chart.NLinearScaleConfigurator scaleY = new Nevron.Chart.NLinearScaleConfigurator ( );
                scaleY.MajorGridStyle.ShowAtWalls = new [] { Nevron.Chart.ChartWallType.Back };
                scaleY.MajorGridStyle.LineStyle.Pattern = Nevron.GraphicsCore.LinePattern.Dot;
                yAxis.ScaleConfigurator = scaleY;

                //TODO: Make paging view size configurable
                //Nevron.Chart.NNumericAxisPagingView yPagingView = new Nevron.Chart.NNumericAxisPagingView ( new Nevron.GraphicsCore.NRange1DD ( -50, 1600 ) );
                //yPagingView.MinPageLength = 1.0;
                //yAxis.PagingView = yPagingView;
                //yAxis.View = new Nevron.Chart.NRangeAxisView ( new Nevron.GraphicsCore.NRange1DD ( 0, 200 ) );
                xAxis.View = new Nevron.Chart.NContentAxisView ( );
            }

            xAxis.SynchronizeScaleWithConfigurator = true;
            xAxis.InvalidateScale ( );
            xAxis.UpdateScale ( );

            yAxis.SynchronizeScaleWithConfigurator = true;
            yAxis.InvalidateScale ( );
            yAxis.UpdateScale ( );

        }

        private void AddData ( )
        {
            var lines = System.IO.File.ReadLines ( "Data.csv" );
            foreach ( var line in lines )
            {
                var parts = line.Split ( ',' );
                if ( parts.Length != 3 )
                    throw new Exception ( "Invalid data format: " + line );

                int series = int.Parse ( parts.Last ( ) ) - 1;
                var nevronSeries = m_seriesList [ series % m_seriesList.Length ];
                nevronSeries.AddDataPoint ( new Nevron.Chart.NDataPoint ( double.Parse ( parts [ 0 ] ), double.Parse ( parts [ 1 ] ) ) );
            }
        }
    }
}
