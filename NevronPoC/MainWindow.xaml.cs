using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NevronPoC
{
    public enum BlisPointTypes : int
    {
        NULL = 0,
        Lap,
        GapIn,
        GapOut,
        USAL,
        Invalid,
        NoLine,
        SingleEdge
    }

    public class ViewModel
    {
        public List<Data> Data { get; private set; }
        public List<BlisDispoSeries> SeriesList { get; private set; }

        public ViewModel ( )
        {
            Random rand = new Random ( );

            Data = new List<Data> ( Enumerable.Range ( 0, 10000 ).Select ( o => new Data ( ) { X = (float)( rand.NextDouble ( ) * 1000 ), Y = (float)( rand.NextDouble ( ) * 1000 ), Type = BlisPointTypes.GapOut } ) );

            SeriesList = new List<BlisDispoSeries> ( ) { new BlisDispoSeries ( "asdf", System.Drawing.Color.Green, o => new Nevron.Chart.NDataPoint ( o.X, o.Y ), BlisPointTypes.GapOut ) };
        }
    }

    public class Data
    {
        public float X { get; set; }
        public float Y { get; set; }

        public BlisPointTypes Type { get; set; }
    }

    public class Series<T, TData>
    {
        public Series ( string name, System.Drawing.Color color, Func<T, TData> dataFunction, string longName = null )
        {
            Name = name;
            Color = color;
            DataFunction = dataFunction;
            LongName = longName;
        }

        public string Name { get; private set; }
        public string LongName { get; private set; }
        public System.Drawing.Color Color { get; private set; }
        public Func<T, TData> DataFunction { get; private set; }
    }

    public class BlisDispoSeries : Series<Data, global::Nevron.Chart.NDataPoint>
    {
        public BlisDispoSeries ( string name, System.Drawing.Color color, Func<Data, global::Nevron.Chart.NDataPoint> dataFunction, BlisPointTypes type, string longName = null )
            : base ( name, color, dataFunction, longName )
        {
            Type = type;
        }

        public BlisPointTypes Type { get; private set; }
    }

    public partial class MainWindow : Window
    {
        private Nevron.Chart.NCartesianChart m_chart;
        private Nevron.Chart.NRangeSelection m_rs;
        private ViewModel ViewModel;

        private readonly Nevron.Chart.NMarkerStyle m_highLightMarkerStyle = new Nevron.Chart.NMarkerStyle
        {
            PointShape = Nevron.Chart.PointShape.Bar,
            Width = new Nevron.GraphicsCore.NLength ( 4.0f ),
            Height = new Nevron.GraphicsCore.NLength ( 4.0f ),
            FillStyle = new Nevron.GraphicsCore.NColorFillStyle ( System.Drawing.Color.Yellow ),
            BorderStyle = new Nevron.GraphicsCore.NStrokeStyle ( System.Drawing.Color.Black ),
            Visible = true
        };

        public MainWindow ( )
        {
            InitializeComponent ( );

            ViewModel = new NevronPoC.ViewModel ( );
            InitChart ( );
            SetData ( );
        }

        private Nevron.Chart.NPointSeries [] m_seriesList;
        //        private readonly Dictionary<BlisPointTypes, FullSeries> m_seriesByPointType = new Dictionary<BlisPointTypes, FullSeries> ( );

        private void SetData ( )
        {
            InitializeAxis ( false );
            CreateSeries ( );

            AddData ( ViewModel.Data, false );
            Console.WriteLine ( "Added " + ViewModel.Data.Count + " points." );
            Console.WriteLine ( ViewModel.Data.Count );
            var data = m_chart.Series [ 0 ].GetDataSeries ( Nevron.Chart.DataSeriesMask.All, Nevron.Chart.DataSeriesMask.None, false );

            Console.WriteLine ( data.Count );
            m_chart.Refresh ( );
        }

        private void CreateSeries ( )
        {
            m_seriesList = new Nevron.Chart.NPointSeries [ ViewModel.SeriesList.Count ];

            for ( int i = 0 ; i < ViewModel.SeriesList.Count ; i++ )
            {
                var seriesModel = ViewModel.SeriesList [ i ];
                var series = new Nevron.Chart.NPointSeries ( );
                m_seriesList [ i ] = series;
                //m_seriesByPointType [ seriesModel.Type ] = new FullSeries ( series, seriesModel );

                series.DataLabelStyle.Visible = false;
                series.PointShape = Nevron.Chart.PointShape.Bar;
                series.BorderStyle.Width = new Nevron.GraphicsCore.NLength ( 0 );
                series.Size = new Nevron.GraphicsCore.NLength ( 2f );
                series.UseXValues = true;
                series.FillStyle = new Nevron.GraphicsCore.NColorFillStyle ( seriesModel.Color );
                series.Name = seriesModel.Name;
                series.Visible = true;

                //TODO: copied over and seems suspicious
                //If there is existing data make sure every point is reset
                int dataPointCount = series.GetDataPointCount ( );
                if ( dataPointCount > 0 )
                {
                    Nevron.Chart.NMarkerStyle ms = new Nevron.Chart.NMarkerStyle { Visible = false };

                    for ( int ii = 0 ; ii < dataPointCount ; ii++ )
                    {
                        if ( series.FillStyles.Count <= dataPointCount )
                            series.FillStyles [ ii ] = new Nevron.GraphicsCore.NColorFillStyle ( seriesModel.Color );
                        if ( series.MarkerStyles.Count <= dataPointCount )
                            series.MarkerStyles [ ii ] = ms;
                    }
                }

                m_chart.Series.Add ( series );
            }
        }

        private void InitChart ( )
        {
            //Core.Nevron.License.Ensure ( );

            m_chart = new Nevron.Chart.NCartesianChart ( );
            m_chart.Margins = new Nevron.GraphicsCore.NMarginsL ( 0 );
            m_chart.Padding = new Nevron.GraphicsCore.NMarginsL ( 0 );
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

            chartContainer.Controller.Tools.Clear ( );
            chartContainer.Controller.Tools.Add ( new NevronPlanChartMultiPointSelectTool ( chartContainer, m_chart, m_highLightMarkerStyle ) );
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

                xAxis.View = new Nevron.Chart.NContentAxisView ( );

                // setup Y axis
                Nevron.Chart.NLinearScaleConfigurator scaleY = new Nevron.Chart.NLinearScaleConfigurator ( );
                scaleY.MajorGridStyle.ShowAtWalls = new [] { Nevron.Chart.ChartWallType.Back };
                scaleY.MajorGridStyle.LineStyle.Pattern = Nevron.GraphicsCore.LinePattern.Dot;
                yAxis.ScaleConfigurator = scaleY;

                xAxis.View = new Nevron.Chart.NContentAxisView ( );
            }

            xAxis.SynchronizeScaleWithConfigurator = true;
            xAxis.InvalidateScale ( );
            xAxis.UpdateScale ( );

            yAxis.SynchronizeScaleWithConfigurator = true;
            yAxis.InvalidateScale ( );
            yAxis.UpdateScale ( );

        }

        private void AddData ( IEnumerable<Data> datas, bool reset = true )
        {
            if ( reset )
            {
                for ( int j = 0 ; j < m_chart.Series.Count ; j++ )
                {
                    ( (Nevron.Chart.NPointSeries)m_chart.Series [ j ] ).ClearDataPoints ( );
                }
            }

            var series = (Nevron.Chart.NPointSeries)m_chart.Series [ 0 ];
            foreach ( var data in datas )
            {
                var dataPoint = new Nevron.Chart.NDataPoint ( data.X, data.Y );
                series.AddDataPoint ( dataPoint );
            }
        }
    }

    [Serializable]
    public class NevronPlanChartMultiPointSelectTool : Nevron.Chart.Windows.NDataZoomTool
    {
        private readonly Nevron.Chart.Wpf.NChartControl m_chartContainer;

        [Nevron.Reflection.NReferenceField, NonSerialized]
        private Nevron.Chart.NChartBase m_ChartControl;

        [Nevron.Reflection.NReferenceField, NonSerialized]
        private Nevron.Chart.NRangeSelection m_RangeSelection;

        [Nevron.Reflection.NReferenceField, NonSerialized]
        private List<long> m_SelectedData = new List<long> ( );

        [Nevron.Reflection.NReferenceField, NonSerialized]
        private Nevron.Chart.NCartesianChart m_SelectedChart;

        [Nevron.Reflection.NReferenceField, NonSerialized]
        private readonly Nevron.Chart.NMarkerStyle m_highLightMarkerStyle;

        /// <summary>
        /// Event used to communicate which points were selected back to the UI
        /// </summary>
        //public event EventHandler<EventArgs<List<Data>>> PassSelectedGraphPoints = delegate { }; //Assign an empty delegate to avoid !Null checks


        public NevronPlanChartMultiPointSelectTool ( Nevron.Chart.Wpf.NChartControl chartContainer, Nevron.Chart.NCartesianChart chartControl, Nevron.Chart.NMarkerStyle highLightMarkerStyle )
        {
            m_chartContainer = chartContainer;
            m_ChartControl = chartControl;
            m_highLightMarkerStyle = highLightMarkerStyle;
            m_SelectedChart = chartControl;

            ZoomInFillStyle = new Nevron.GraphicsCore.NColorFillStyle ( System.Drawing.Color.FromArgb ( 125, System.Drawing.Color.Navy ) );
            ZoomInBorderStyle.Color = System.Drawing.Color.FromArgb ( 125, System.Drawing.Color.Navy );
            this.AlwaysZoomIn = true;
        }

        private bool m_notFirst = false;

        public override void OnEndDrag ( object sender, Nevron.Chart.Windows.NMouseEventArgs e )
        {
            m_SelectedChart.Document.Lock ( );
            List<Data> entities = new List<Data> ( );
            Data entity;

            if ( UpdateRangeSelections ( ref e ) )
            {
                if ( m_SelectedChart == null || m_SelectedChart.RangeSelections.Count == 0 )
                {
                    Console.WriteLine ( "No Range Selections for Chart" );
                    return; //TODO: Error handling
                }

                var rs = (Nevron.Chart.NRangeSelection)m_SelectedChart.RangeSelections [ 0 ]; // = new Nevron.Chart.NRangeSelection ( );
                rs.Visible = false;

                Nevron.GraphicsCore.NRange1DD rangeX = rs.HorizontalAxisRange;
                Nevron.GraphicsCore.NRange1DD rangeY = rs.VerticalAxisRange;

                rangeX.Normalize ( );
                rangeY.Normalize ( );

                m_notFirst = true;

                int selectCount = 0;
                int notSelectedCount = 0;

                StringBuilder sb = new StringBuilder ( );

                sb.AppendLine ( "Range: " + rangeX.Begin + ", " + rangeY.Begin + " to " + rangeX.End + "," + rangeY.End );
                for ( int j = 0 ; j < m_SelectedChart.Series.Count ; j++ )
                {
                    var series = m_SelectedChart.Series [ j ] as Nevron.Chart.NPointSeries;
                    if ( series != null )
                    {
                        Nevron.Chart.NPointSeries pointSeries = series;

                        int count = pointSeries.GetDataPointCount ( );

                        for ( int i = 0 ; i < count ; i++ )
                        {
                            if ( rangeX.Contains ( (double)pointSeries.XValues [ i ] ) &&
                                rangeY.Contains ( (double)pointSeries.Values [ i ] ) )
                            {
                                sb.AppendLine ( pointSeries.XValues [ i ] + " x " + pointSeries.Values [ i ] );
                                pointSeries.MarkerStyles [ i ] = m_highLightMarkerStyle;

                                selectCount++;
                            }
                            else
                            {
                                pointSeries.FillStyles [ i ] = null; //Remove marker if not selected
                                pointSeries.MarkerStyles [ i ] = null;

                                //NOTE: NEVRON LOOK HERE
                                // a small percent do not "take" for reasons unknown.  So we just try real hard and they go through.
                                int cnt = 0;
                                while ( pointSeries.MarkerStyles [ i ] != null )
                                {
                                    System.Threading.Thread.Sleep ( 1 );
                                    Console.Write ( "." );
                                    pointSeries.MarkerStyles [ i ] = null;
                                    cnt++;
                                    if ( cnt > 100 )
                                        System.Diagnostics.Debugger.Break ( );
                                }
                                if ( cnt > 0 )
                                    Console.WriteLine ( i + " : " + cnt );
                                notSelectedCount++;
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine ( "OnEndDrag, " + selectCount + " points selected, " + notSelectedCount + " points NOT selected\r\n\ttotal: " + ( selectCount + notSelectedCount ) );
                //System.IO.File.WriteAllText ( @"C:\temp\point_select\Selected_" + DateTime.Now.ToOADate ( ) + ".txt", sb.ToString ( ) );
            }

            m_SelectedChart.Document.Unlock ( );

            this.Deactivate ( );

            m_ChartControl.Refresh ( );
            m_chartContainer.Refresh ( );

            //Kick the event to pass the list of selected items to subscriber. This assumes there is only one subscriber for now.
            //PassSelectedGraphPoints ( this, new EventArgs<List<BlisDispoCompositeEntity>> ( entities ) );
        }

        private void Reset ( )
        {
            for ( int j = 0 ; j < m_SelectedChart.Series.Count ; j++ )
            {
                var series = m_SelectedChart.Series [ j ] as Nevron.Chart.NPointSeries;
                if ( series != null )
                {
                    Nevron.Chart.NPointSeries pointSeries = series;

                    int count = pointSeries.GetDataPointCount ( );
                    for ( int i = 0 ; i < count ; i++ )
                    {
                        pointSeries.FillStyles [ i ] = null; //Remove marker if not selected
                        pointSeries.MarkerStyles [ i ] = null;
                    }
                }
            }
        }
    }
}